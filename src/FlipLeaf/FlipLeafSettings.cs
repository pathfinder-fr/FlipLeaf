namespace FlipLeaf
{
    public class FlipLeafSettings
    {
        /// <summary>
        /// Configured directory where the files are stored.
        /// This path is normalized when the app is started, and replaced with a full absolute path to the source directory.
        /// </summary>
        public string SourcePath { get; set; } = ".content";

        public string BaseUrl { get; set; } = string.Empty;

        public bool GitEnabled { get; set; } = true;

        public string? GitOrigin { get; set; }

        public string GitBranch { get; set; } = @"master";

        public string? GitUsername { get; set; }

        public string? GitPassword { get; set; }

        /// <summary>
        /// Indicates that the default <see cref="Website.IWebsiteIdentity"/> must
        /// </summary>
        public bool CustomIdentity { get; set; }
    }
}
