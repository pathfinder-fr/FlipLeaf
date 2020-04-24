namespace FlipLeaf.Website
{

    public sealed class WebsiteIdentity : IWebsiteIdentity
    {
        public IUser GetCurrentUser() => DefaultUser.Anonymous;

        public IUser GetWebsiteUser() => DefaultUser.Anonymous;
    }
}
