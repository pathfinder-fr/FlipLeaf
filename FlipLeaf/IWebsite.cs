namespace FlipLeaf
{
    public interface IWebsite
    {
        IUser GetCurrentUser();

        IUser GetWebsiteUser();
    }

    public sealed class DefaultWebsite : IWebsite
    {
        public IUser GetCurrentUser() => DefaultUser.Anonymous;

        public IUser GetWebsiteUser() => DefaultUser.Anonymous;
    }
}
