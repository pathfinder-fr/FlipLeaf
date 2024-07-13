namespace FlipLeaf.Templating
{
    public class FormTemplatePage
    {
        public FormTemplatePage(string name, FormTemplate template, HeaderFieldDictionary pageHeader, string pageContent)
        {
            Name = name;
            FormTemplate = template;
            PageHeader = pageHeader;
            PageContent = pageContent;
        }

        public string Name { get; }

        public FormTemplate FormTemplate { get; }

        public HeaderFieldDictionary PageHeader { get; }

        public string PageContent { get; }
    }
}