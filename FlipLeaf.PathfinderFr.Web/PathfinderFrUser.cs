using System;
using FlipLeaf;

namespace PathfinderFr
{
    public sealed class PathfinderFrUser : IUser
    {
        public static readonly PathfinderFrUser SiteUser = new PathfinderFrUser(-1, "site", Guid.Empty, Array.Empty<string>(), false, null);

        private const int Size = 60;

        public PathfinderFrUser(int userId, string userName, Guid providerKey, string[] roles, bool hasAvatarImage, string avatarUrl)
        {
            UserId = userId;
            UserName = userName;
            ProviderKey = providerKey;
            Roles = roles;
            AvatarUrl = ResolveAvatarUrl(userId, hasAvatarImage, avatarUrl);
        }

        string IUser.Name => UserName;

        string IUser.Email => $"{UserName.ToLowerInvariant()}@pathfinder-fr.org";

        public int UserId { get; }

        public string UserName { get; }

        public Guid ProviderKey { get; }

        public string[] Roles { get; }

        public string AvatarUrl { get; }

        private string ResolveAvatarUrl(int userId, bool hasImage, string url)
        {
            if (hasImage)
            {
                return $"/Forum/resource.ashx?u={userId}";
            }
            else if (!string.IsNullOrEmpty(url))
            {
                // force HTTPS
                if (url.StartsWith("http://www.pathfinder-fr.org", StringComparison.OrdinalIgnoreCase))
                {
                    url = "https://" + url.Substring(7);
                }

                return $"/Forum/resource.ashx?url={Uri.EscapeDataString(url)}&width={Size}&height={Size}";
            }
            else
            {
                return @"/images/avatars/0t.jpg";
            }
        }
    }
}
