using System;
using System.Collections.Concurrent;
using System.Linq;
using Dapper;
using FlipLeaf;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using PathfinderFr.Compatibility;

namespace PathfinderFr
{
    public interface IPathfinderFrWebsite : IWebsite
    {
        PathfinderFrUser GetCurrentPathfinderFrUser();

        PathfinderFrUser GetUser(string userName);
    }

    public class PathfinderFrWebsite : IPathfinderFrWebsite
    {
        private static readonly ConcurrentDictionary<string, PathfinderFrUser> _users = new ConcurrentDictionary<string, PathfinderFrUser>(StringComparer.OrdinalIgnoreCase);
        private readonly string _connectionString;
        private readonly string _cookieName;
        private readonly AspNetMachineKeySection _section;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PathfinderFrWebsite(PathfinderFrSettings settings, IHttpContextAccessor httpContextAccessor)
        {
            _connectionString = settings.ConnectionString;
            _cookieName = settings.AuthCookieName;
            _section = new AspNetMachineKeySection("AES", settings.DecryptionKey, "SHA1", settings.ValidationKey); _httpContextAccessor = httpContextAccessor;
        }

        IUser IWebsite.GetCurrentUser() => GetCurrentPathfinderFrUser();

        public IUser GetWebsiteUser() => PathfinderFrUser.SiteUser;

        public PathfinderFrUser GetCurrentPathfinderFrUser() => GetUser(GetCurrentUserName(_httpContextAccessor.HttpContext.Request));

        public PathfinderFrUser GetUser(string userName)
        {
            if (string.IsNullOrEmpty(userName))
            {
                return null;
            }

            return _users.GetOrAdd(userName, GetUserCore);
        }

        private string GetCurrentUserName(HttpRequest request)
        {
            AspNetFormsTicketAuthentication ticket = null;
            var ticketText = request.Cookies[_cookieName];
            if (string.IsNullOrEmpty(ticketText))
            {
                return null;
            }

            byte[] ticketData;
            try
            {
                ticketData = _section.DecryptData(ticketText);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Impossible de déchiffrer le ticket {ticketText}", ex);
            }

            if (_section.VerifyHashedData(ticketData))
            {
                var len = ticketData.Length - _section.KeySize;
                ticket = AspNetFormsTicketAuthentication.Deserialize(ticketData, len);
            }

            return ticket?.Name;
        }

        private PathfinderFrUser GetUserCore(string userName)
        {
            using var conn = new SqlConnection(_connectionString);

            conn.Open();

            var yafUser = conn
                .Query<YafUser>(@"SELECT TOP 1 UserID, Name, cast(ProviderUserKey as uniqueidentifier) as ProviderUserKey, CAST(CASE WHEN AvatarImage IS NOT NULL THEN 1 ELSE 0 END AS BIT) AS [HasAvatarImage], Avatar AS AvatarUrl FROM yaf_User WHERE Name = @name", new { name = userName })
                .FirstOrDefault();


            var roles = conn.
                Query<string>(@"SELECT r.RoleName FROM aspnet_Roles r JOIN aspnet_UsersInRoles ur ON r.RoleId = ur.RoleId WHERE ur.UserId = @userId", new { userId = yafUser.ProviderUserKey })
                .ToArray();

            return new PathfinderFrUser(yafUser.UserID, yafUser.Name, yafUser.ProviderUserKey, roles, yafUser.HasAvatarImage, yafUser.AvatarUrl);
        }

        private class YafUser
        {
            public int UserID { get; set; }

            public string Name { get; set; }

            public Guid ProviderUserKey { get; set; }

            public bool HasAvatarImage { get; set; }

            public string AvatarUrl { get; set; }
        }
    }
}
