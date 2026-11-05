using System;
using System.IO;
using System.Web.Mvc;
using LalaCoffee.Areas.Admin.Controllers;
using LalaCoffee.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LalaCoffee.Tests
{
    [TestClass]
    public class AdminProductEditTests
    {
        private static string GetProjectRoot()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var dir = new DirectoryInfo(baseDir);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "LalaCoffee.sln")))
            {
                dir = dir.Parent;
            }

            if (dir == null)
            {
                throw new InvalidOperationException("Could not locate solution root from: " + baseDir);
            }

            return Path.Combine(dir.FullName, "LalaCoffee");
        }

        private static string GetViewPath(string area, string controller, string viewName)
        {
            var projectDir = GetProjectRoot();
            return Path.Combine(projectDir, "Areas", area, "Views", controller, viewName);
        }

        [TestMethod]
        public void ManageProduct_EditView_ShouldNotContainUnsafeConvertToInt32Status()
        {
            var viewPath = GetViewPath("Admin", "ManageProduct", "Edit.cshtml");
            Assert.IsTrue(File.Exists(viewPath), $"View file not found at {viewPath}");

            var content = File.ReadAllText(viewPath);
            Assert.DoesNotContain("Convert.ToInt32(Model.Status)", content, 
                "ManageProduct/Edit.cshtml must not use Convert.ToInt32(Model.Status) because Status is a string (e.g. 'Show', 'Active') which throws FormatException.");
            Assert.Contains("isActive", content, "ManageProduct/Edit.cshtml should calculate isActive safely.");
            Assert.Contains("type=\"number\"", content, "ManageProduct/Edit.cshtml price input should use type='number'.");
        }

        [TestMethod]
        public void AdminViews_AllEditViews_ShouldNotContainUnsafeConvertToInt32Status()
        {
            string[] controllers = { "ManageCategory", "ManageOptionProduct", "ManagePropertyProduct" };
            foreach (var controller in controllers)
            {
                var viewPath = GetViewPath("Admin", controller, "Edit.cshtml");
                if (File.Exists(viewPath))
                {
                    var content = File.ReadAllText(viewPath);
                    Assert.DoesNotContain("Convert.ToInt32(Model.Status)", content, 
                        $"{controller}/Edit.cshtml must not use Convert.ToInt32(Model.Status).");
                }
            }
        }

        [TestMethod]
        public void ManageProduct_EditView_PriceInput_ShouldBeFormattedWithoutThousandsSeparator()
        {
            var viewPath = GetViewPath("Admin", "ManageProduct", "Edit.cshtml");
            var content = File.ReadAllText(viewPath);
            Assert.DoesNotContain("Model.Price.ToString(\"N0\")", content, 
                "ManageProduct/Edit.cshtml should not format price input value with 'N0' to prevent culture decimal/group separator parsing conflicts.");
        }
    }
}
