using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using LalaCoffee.Models;

namespace LalaCoffee.Helpers
{
    public static class MenuHelper
    {
        public static string RenderMenuHtml(IEnumerable<Category> allCategories, string host)
        {
            if (allCategories == null)
            {
                return string.Empty;
            }

            var categoryList = allCategories.ToList();
            if (categoryList.Count == 0)
            {
                return string.Empty;
            }

            var parentCategories = categoryList.Where(c => c.ParentId == 0).ToList();
            var subCategoryLookup = categoryList.Where(c => c.ParentId > 0).ToLookup(c => c.ParentId);

            StringBuilder result = new StringBuilder();
            foreach (var category in parentCategories)
            {
                result.Append("<li class='lv2_title'>");
                result.AppendFormat("<a href='{0}/Category/Details/{1}'>{2}</a>", host, category.Slug, category.Name);
                result.Append("<ul class='menu_child_lv3'>");

                var subCategories = subCategoryLookup[category.Id].ToList();
                if (subCategories.Count > 0)
                {
                    foreach (var subCategory in subCategories)
                    {
                        result.AppendFormat("<li class='lv3_title'><a href='{0}/Category/Details/{1}'>{2}</a></li>", host, subCategory.Slug, subCategory.Name);
                    }
                }

                result.Append("</ul></li>");
            }

            return result.ToString();
        }

        public static MvcHtmlString Menu(this HtmlHelper helper)
        {
            var requestUrl = HttpContext.Current?.Request?.Url;
            string hostUrl = requestUrl?.Authority ?? "";
            string scheme = requestUrl?.Scheme ?? "http";
            string host = string.IsNullOrEmpty(hostUrl) ? "" : scheme + "://" + hostUrl;

            using (var context = new ApplicationDbContext())
            {
                var allCategories = context.Categories.ToList();
                var html = RenderMenuHtml(allCategories, host);
                return new MvcHtmlString(html);
            }
        }
    }
}
