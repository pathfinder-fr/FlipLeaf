using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PathfinderFr.Docs;
using PathfinderFr.Markup;

namespace PathfinderFr.Pages._Render
{
    public class CategoryModel : PageModel
    {
        private readonly IWiki _wiki;

        public CategoryModel(IWiki wiki)
        {
            _wiki = wiki;
        }

        public WikiName Name { get; private set; }

        public IEnumerable<CategoryPageModel> Pages { get; private set; }

        public IActionResult OnGet(string name = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                Name = null;
                Pages = _wiki.GetAllCategories()
                    .Select(p => new CategoryPageModel { Title = p.Name, Path = $"category.html?name={p.FullName}" });
            }
            else
            {
                Name = new WikiName(name);
                Pages = _wiki.GetCategoryPages(Name)
                    .Select(n => _wiki.GetPage(n))
                    .Where(p => p != null)
                    .Select(p => new CategoryPageModel { Title = p.Title, Path = p.Name.FullName + ".html" });
            }

            return Page();
        }
    }

    public class CategoryPageModel
    {
        public string Title { get; set; }

        public string Path { get; set; }
    }
}
