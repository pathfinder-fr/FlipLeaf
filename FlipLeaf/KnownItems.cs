namespace FlipLeaf
{
    public static class KnownFields
    {
        public const string Layout = "layout";
        public const string Template = "template";
        public const string ContentType = "contentType";
        public const string Title = "title";
        public const string Collection = "collection";
    }

    public static class KnownVariables
    {
        public const string Layout = "layout";
        public const string Site = "site";
        public const string Page = "page";
    }

    public enum FamilyFolder
    {
        None,
        Layouts,
        Includes,
        Templates,
        Data
    }

    public static class KnownFolders
    {
        public const string Layouts = "_layouts";
        public const string Templates = "_templates";
        public const string Includes = "_includes";
        public const string Data = "_data";
    }

    public static class KnownFiles
    {
        public const string DefaultDocument = "index.html";
    }
}
