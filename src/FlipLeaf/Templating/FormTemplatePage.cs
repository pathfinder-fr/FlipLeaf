namespace FlipLeaf.Templating
{
    public class FormTemplatePage
    {
        public FormTemplatePage(string name, FormTemplate template, HeaderFieldDictionary pageHeader, string pageContent)
        {
            this.Name = name;
            this.FormTemplate = template;
            this.PageHeader = pageHeader;
            this.PageContent = pageContent;
        }

        public string Name { get; }

        public FormTemplate FormTemplate { get; }

        public HeaderFieldDictionary PageHeader { get; }

        public string PageContent { get; }
    }
}