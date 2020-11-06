using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using FlipLeaf.Markup;
using FlipLeaf.Storage;
using FlipLeaf.Templating;
using FlipLeaf.Website;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Primitives;

namespace FlipLeaf.Pages._Manage
{
    public class EditModel : PageModel
    {
        private readonly IGitRepository _git;
        private readonly IWebsiteIdentity _website;
        private readonly IDocumentStore _docStore;
        private readonly IFileSystem _fileSystem;
        private readonly IYamlMarkup _yaml;

        public EditModel(
            IFileSystem fileSystem,
            IYamlMarkup yaml,
            IGitRepository git,
            IWebsiteIdentity website,
            IDocumentStore docStore)
        {
            _git = git;
            _website = website;
            _docStore = docStore;
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

            if (!TryLoadTemplate(template, file, out var yamlHeader, out var templateName, out var formTemplate, out var content)
                || formTemplate == null)
            {
                if (form || template != null)
                {
                    return BadRequest("This file does not supports form editing. No template or invalid template specified");
                }

                return this.RedirectToPage("EditRaw", new { path });
            }

            var formValues = new Dictionary<string, StringValues>();
            foreach (var yamlItem in yamlHeader)
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
            this.TemplateName = templateName;
            this.FormTemplate = formTemplate;
            this.PageContent = content;

            return Page();
        }

        public IActionResult OnPost(string path)
        {
            var user = _website.GetCurrentUser();
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

            if (!TryLoadTemplate(this.TemplateName, file, out var yamlHeader, out _, out var formTemplate, out _)
                || formTemplate == null)
            {
                return BadRequest();
            }

            using (var writer = new StringWriter())
            {
                writer.WriteLine("---");
                writer.WriteLine($"{KnownFields.Template}: {this.TemplateName}");

                foreach (var field in formTemplate.Fields)
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

            var websiteUser = _website.GetWebsiteUser();
            _git.Commit(user, websiteUser, path, this.Comment);
            _git.PullPush(websiteUser);

            return RedirectToPage("Show", new { path });
        }

        private bool TryLoadTemplate(
            string? templateName,
            IStorageItem file,
            out HeaderFieldDictionary yamlHeader,
            out string? loadedTemplateName,
            out FormTemplate? formTemplate,
            out string content)
        {
            formTemplate = null;
            loadedTemplateName = null;
            content = string.Empty;

            if (_fileSystem.FileExists(file) && templateName == null)
            {
                // read raw content
                content = _fileSystem.ReadAllText(file);

                // parse YAML header
                yamlHeader = _yaml.ParseHeader(content, out content);

                if (yamlHeader.TryGetValue(KnownFields.Template, out var templateNameObj))
                {
                    templateName = templateNameObj as string;
                }
            }
            else
            {
                yamlHeader = new HeaderFieldDictionary();
            }

            if (templateName == null)
            {
                return false;
            }

            // try load template
            var templateDoc = _docStore.Get<Docs.Template>(templateName);
            if (templateDoc == null)
            {
                return false;
            }

            // parse template
            formTemplate = templateDoc.FormTemplate;
            loadedTemplateName = templateDoc.Name;
            return true;
        }
    }
}
