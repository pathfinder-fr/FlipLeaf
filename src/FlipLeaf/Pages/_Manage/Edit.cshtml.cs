using System.ComponentModel.DataAnnotations;
using FlipLeaf.Markup;
using FlipLeaf.Storage;
using FlipLeaf.Templating;
using FlipLeaf.Website;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Primitives;

namespace FlipLeaf.Pages.Manage
{
    public class EditModel : PageModel
    {
        private readonly IGitRepository _git;
        private readonly IWebsiteIdentity _websiteIdentity;
        private readonly IFormTemplateManager _formTemplateManager;
        private readonly IFileSystem _fileSystem;
        private readonly IYamlMarkup _yaml;

        public EditModel(
            IFileSystem fileSystem,
            IYamlMarkup yaml,
            IGitRepository git,
            IWebsiteIdentity websiteIdentity,
            IFormTemplateManager formTemplateManager)
        {
            _git = git;
            _websiteIdentity = websiteIdentity;
            _formTemplateManager = formTemplateManager;
            _fileSystem = fileSystem;
            _yaml = yaml;
            Path = string.Empty;
        }

        public string Path { get; set; }

        public IDictionary<string, StringValues> Form { get; set; } = new Dictionary<string, StringValues>();

        public FormTemplate FormTemplate { get; set; } = FormTemplate.Default;

        [Display]
        [BindProperty]
        public string? PageContent { get; set; }

        [BindProperty]
        public string? TemplateName { get; set; }

        [Display]
        [BindProperty]
        public string? Comment { get; set; }

        [BindProperty]
        public string? Action { get; set; }

        public IActionResult OnGet(string path, bool form = false, string? template = null)
        {
            var file = _fileSystem.GetItem(path);
            if (file == null)
            {
                return NotFound();
            }

            // form edition reserved to markdown files
            if (!file.IsMarkdown())
            {
                return this.RedirectToPage("EditRaw", new { path });
            }

            if (!_formTemplateManager.TryLoadTemplate(
                template, 
                file, 
                out var templatePage)
                || templatePage == null)
            {
                if (form || template != null)
                {
                    return BadRequest("This file does not supports form editing. No template or invalid template specified");
                }

                return this.RedirectToPage("EditRaw", new { path });
            }

            var formValues = new Dictionary<string, StringValues>();
            foreach (var yamlItem in templatePage.PageHeader)
            {
                if (yamlItem.Value is string yamlString)
                {
                    formValues[yamlItem.Key] = new StringValues(yamlString);
                }
                else if (yamlItem.Value is List<object> yamlArray)
                {
                    formValues[yamlItem.Key] = new StringValues(yamlArray.Select(x => x.ToString()).ToArray());
                }
            }

            this.Path = path;
            this.Form = formValues;
            this.TemplateName = templatePage.Name;
            this.FormTemplate = templatePage.FormTemplate;
            this.PageContent = templatePage.PageContent;

            return Page();
        }

        public IActionResult OnPost(string path)
        {
            var user = _websiteIdentity.GetCurrentUser();
            if (user == null)
            {
                return Unauthorized();
            }

            var file = _fileSystem.GetItem(path);
            if (file == null)
            {
                return NotFound();
            }

            if (!file.IsMarkdown() || string.IsNullOrEmpty(this.TemplateName))
            {
                return BadRequest();
            }

            if (!_formTemplateManager.TryLoadTemplate(this.TemplateName, file, out var templatePage)
                || templatePage == null)
            {
                return BadRequest();
            }

            using (var writer = new StringWriter())
            {
                writer.WriteLine("---");
                writer.WriteLine($"{KnownFields.Template}: {this.TemplateName}");

                foreach (var field in templatePage.FormTemplate.Fields)
                {
                    if (field.Id == null) continue;
                    if (!Request.Form.TryGetValue($"Fields.{field.Id}", out var formValues))
                    {
                        formValues = StringValues.Empty;
                    }

                    switch (field.Type)
                    {
                        case FormTemplateFieldType.Text:
                        case FormTemplateFieldType.Choice:
                        case FormTemplateFieldType.MultiCheckBox:
                            _yaml.WriteHeaderValue(writer, field.Id, formValues, field.DefaultValue?.ToString());
                            break;
                    }
                }

                writer.WriteLine("---");

                writer.Write(PageContent);

                _fileSystem.WriteAllText(file, writer.ToString());
            }

            var websiteUser = _websiteIdentity.GetWebsiteUser();
            _git.Commit(user, websiteUser, path, this.Comment);
            _git.PullPush(websiteUser);

            return RedirectToPage("Show", new { path });
        }
    }
}
