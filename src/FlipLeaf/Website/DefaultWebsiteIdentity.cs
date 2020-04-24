namespace FlipLeaf.Website
{

    public sealed class DefaultWebsiteIdentity : IWebsiteIdentity
    {
        public IUser GetCurrentUser() => DefaultUser.Anonymous;

        public IUser GetWebsiteUser() => DefaultUser.Anonymous;
    }
}
