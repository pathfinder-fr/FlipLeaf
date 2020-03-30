using System;
using System.IO;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FlipLeaf.Mvc
{
    [HtmlTargetElement("user-menu")]
    public class UserMenuTagHelper : TagHelper
    {
        private readonly IPathfinderFrWebsite _websitePlatform;

        public UserMenuTagHelper(IPathfinderFrWebsite websitePlatform)
        {
            this._websitePlatform = websitePlatform;
        }

        public string Section { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var avatarImgSrc = @"/images/avatars/0t.jpg";
            var userName = "Invité";
            var userNameLink = @"/Forum/login";
            var roles = Array.Empty<string>();

            var user = _websitePlatform.GetCurrentPathfinderFrUser();
            if (user != null)
            {
                avatarImgSrc = user.AvatarUrl;
                userName = user.UserName;
                userNameLink = @"/Forum/yaf_cp_profile.aspx";
                roles = user.Roles;
            }

            using (var writer = new StringWriter())
            {
                writer.Write("<img class=\"UserMenuAvatar\" src=\"");
                writer.Write(avatarImgSrc);
                writer.WriteLine("\" style=\"width:60px;height:60px;border-width:0\" />");

                writer.WriteLine("<ul class=\"menu\">");
                writer.WriteLine($"<li><a href=\"{userNameLink}\">Bienvenue, {userName} !</a></li>");
                if (user != null)
                {
                    writer.WriteLine($"<li><a href=\"/Forum/default.aspx?g=cp_pm\">Messages privés</a></li>");
                    writer.WriteLine($"<li><a href=\"/Forum/logout\">Déconnexion</a></li>");

                    string targetRole = null;
                    switch (Section)
                    {
                        case "Forum":
                            targetRole = "ForumAdministrator";
                            break;

                        case "Wiki":
                            targetRole = "WikiAdministrator";
                            break;

                        case "Blog":
                            targetRole = "BlogAdministrator";
                            break;
                    }

                    var hasAdminRole = false;
                    for (var i = 0; i < roles.Length; i++)
                    {
                        var role = roles[i];
                        if (role == "Administrators" || role == targetRole)
                        {
                            hasAdminRole = true;
                            break;
                        }
                    }

                    if (hasAdminRole)
                    {
                        string adminLink;
                        switch (Section)
                        {
                            case "Forum":
                                adminLink = "/Forum/default.aspx?g=admin_admin";
                                break;

                            case "Blog":
                                adminLink = "/Blog/admin/dashboard.aspx";
                                break;

                            case "Wiki":
                            default:
                                adminLink = "/Wiki/AdminHome.aspx";
                                break;
                        }

                        if (adminLink != null)
                        {
                            writer.WriteLine($"<li><a href=\"{adminLink}\">Admin {Section}</a></li>");
                        }
                    }
                }
                else
                {
                    writer.WriteLine("<li><a href=\"/Forum/login\">Connexion</a></li>");
                    writer.WriteLine("<li><a href=\"/Forum/register\">Inscription</a></li>");
                }

                writer.WriteLine("</ul>");

                output.TagName = "div";
                output.Attributes.Add("id", "userpanel");
                output.Content.SetHtmlContent(writer.ToString());
            }
        }
    }
}
