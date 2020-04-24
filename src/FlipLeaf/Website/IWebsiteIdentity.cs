namespace FlipLeaf.Website
{
    public interface IWebsiteIdentity
    {
        IUser GetCurrentUser();

        IUser GetWebsiteUser();
    }
}
