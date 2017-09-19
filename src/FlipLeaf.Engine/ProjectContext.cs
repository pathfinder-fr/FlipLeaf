namespace FlipLeaf
{
    /// <summary>
    /// Represents the context for a documentation project.
    /// </summary>
    public class ProjectContext
    {
        /// <summary>
        /// Gets or sets the settings for the site.
        /// </summary>
        public SiteSettings Settings { get; set; }

        /// <summary>
        /// Gets or sets the path where the project files are stored.
        /// </summary>
        public string Path { get; set; }
    }
}
