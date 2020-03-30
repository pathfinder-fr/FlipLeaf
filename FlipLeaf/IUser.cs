namespace FlipLeaf
{
    public interface IUser
    {
        string Name { get; }

        string Email { get; }
    }

    public class DefaultUser : IUser
    {
        public static readonly IUser Anonymous = new DefaultUser(@"anonymous", @"anonymous@example.org");

        public DefaultUser(string name, string email)
        {
            Name = name;
            Email = email;
        }

        public string Name { get; }

        public string Email { get; }
    }
}
