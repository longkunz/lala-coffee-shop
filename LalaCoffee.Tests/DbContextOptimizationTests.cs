using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using LalaCoffee.Areas.Admin.Controllers;
using LalaCoffee.Controllers;
using LalaCoffee.Helpers;
using LalaCoffee.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LalaCoffee.Tests
{
    [TestClass]
    public class DbContextOptimizationTests
    {
        private static string GetProjectRootDirectory()
        {
            var testAssemblyDir = Path.GetDirectoryName(typeof(DbContextOptimizationTests).Assembly.Location)
                ?? AppDomain.CurrentDomain.BaseDirectory;
            return Path.GetFullPath(Path.Combine(testAssemblyDir, @"..\..\..\..\LalaCoffee"));
        }

        #region Reflection & Lifecycle Tests

        [TestMethod]
        public void BaseController_OverridesDispose_WithDisposingParameter()
        {
            var method = typeof(BaseController<>).GetMethod(
                "Dispose",
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new[] { typeof(bool) },
                null);

            Assert.IsNotNull(method, "BaseController<T> must implement protected override void Dispose(bool disposing).");
            Assert.AreEqual(typeof(BaseController<>), method.DeclaringType,
                "Dispose(bool) must be declared/overridden directly on BaseController<T>.");
            Assert.IsTrue(method.IsFamily, "Dispose(bool) in BaseController<T> must be protected.");
            Assert.IsTrue(method.IsVirtual, "Dispose(bool) in BaseController<T> must be virtual/override.");
        }

        [TestMethod]
        public void AccountController_OverridesDispose_WithDisposingParameter()
        {
            var method = typeof(AccountController).GetMethod(
                "Dispose",
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new[] { typeof(bool) },
                null);

            Assert.IsNotNull(method, "AccountController must implement protected override void Dispose(bool disposing).");
            Assert.AreEqual(typeof(AccountController), method.DeclaringType,
                "Dispose(bool) must be declared/overridden directly on AccountController.");
            Assert.IsTrue(method.IsFamily, "Dispose(bool) in AccountController must be protected.");
        }

        #endregion

        #region Static Code Audit Tests

        [TestMethod]
        public void OrderController_CheckOut_DoesNotCallSaveChangesInsideForeach()
        {
            var projectDir = GetProjectRootDirectory();
            var orderControllerPath = Path.Combine(projectDir, @"Controllers\OrderController.cs");
            Assert.IsTrue(File.Exists(orderControllerPath), $"File should exist: {orderControllerPath}");

            var code = File.ReadAllText(orderControllerPath);

            // Match foreach blocks: foreach (...) { ... }
            var foreachRegex = new Regex(@"foreach\s*\([^\)]+\)\s*\{(?<body>[^}]*)\}", RegexOptions.Singleline);
            var matches = foreachRegex.Matches(code);

            foreach (Match match in matches)
            {
                var body = match.Groups["body"].Value;
                Assert.DoesNotContain(
                    "SaveChanges()",
                    body,
                    "OrderController.cs must NOT call SaveChanges() inside any foreach loop. SaveChanges() must be called once after loop."
                );
            }
        }

        [TestMethod]
        public void ManageRoleController_SetRole_DoesNotQueryDatabaseInsideLoop()
        {
            var projectDir = GetProjectRootDirectory();
            var manageRoleControllerPath = Path.Combine(projectDir, @"Areas\Admin\Controllers\ManageRoleController.cs");
            Assert.IsTrue(File.Exists(manageRoleControllerPath), $"File should exist: {manageRoleControllerPath}");

            var code = File.ReadAllText(manageRoleControllerPath);

            // Extract the SetRole GET action method
            var setRoleMethodRegex = new Regex(@"public\s+ActionResult\s+SetRole\s*\(\s*\)\s*\{(?<body>.*?return\s+View\s*\(.*?\);\s*\}*)", RegexOptions.Singleline);
            var match = setRoleMethodRegex.Match(code);
            Assert.IsTrue(match.Success, "ManageRoleController must have public ActionResult SetRole().");

            var setRoleBody = match.Groups["body"].Value;

            // Inside SetRole GET, there should NOT be a foreach loop that queries Context
            var foreachInsideSetRole = new Regex(@"foreach\s*\([^\)]+\)\s*\{(?<body>[^}]*)\}", RegexOptions.Singleline);
            var loopMatches = foreachInsideSetRole.Matches(setRoleBody);

            foreach (Match loopMatch in loopMatches)
            {
                var loopBody = loopMatch.Groups["body"].Value;
                Assert.DoesNotContain(
                    "Context.",
                    loopBody,
                    "ManageRoleController.SetRole() must not perform database queries inside a foreach loop (N+1 query issue)."
                );
                Assert.DoesNotContain(
                    "Context.Set<",
                    loopBody,
                    "ManageRoleController.SetRole() must not perform database queries inside a foreach loop (N+1 query issue)."
                );
            }
        }

        #endregion

        #region MenuHelper Category Tree In-Memory Tests

        [TestMethod]
        public void MenuHelper_RenderMenuHtml_BuildsHierarchicalHtml_WithoutDatabaseQuery()
        {
            var categories = new List<Category>
            {
                new Category { Id = 1, Name = "Cà phê", Slug = "ca-phe", ParentId = 0 },
                new Category { Id = 2, Name = "Cà phê Đen", Slug = "ca-phe-den", ParentId = 1 },
                new Category { Id = 3, Name = "Cà phê Sữa", Slug = "ca-phe-sua", ParentId = 1 },
                new Category { Id = 4, Name = "Trà trái cây", Slug = "tra-trai-cay", ParentId = 0 },
                new Category { Id = 5, Name = "Trà đào", Slug = "tra-dao", ParentId = 4 },
                new Category { Id = 6, Name = "Bánh ngọt", Slug = "banh-ngot", ParentId = 0 } // No children
            };

            var host = "https://lalacoffee.vn";
            var html = MenuHelper.RenderMenuHtml(categories, host);

            Assert.IsFalse(string.IsNullOrWhiteSpace(html), "Generated HTML must not be empty.");

            // Verify parent categories exist as lv2_title
            StringAssert.Contains(html, "<li class='lv2_title'><a href='https://lalacoffee.vn/Collections/ca-phe'>Cà phê</a>");
            StringAssert.Contains(html, "<li class='lv2_title'><a href='https://lalacoffee.vn/Collections/tra-trai-cay'>Trà trái cây</a>");
            StringAssert.Contains(html, "<li class='lv2_title'><a href='https://lalacoffee.vn/Collections/banh-ngot'>Bánh ngọt</a>");

            // Verify sub-categories exist inside menu_child_lv3
            StringAssert.Contains(html, "<ul class='menu_child_lv3'>");
            StringAssert.Contains(html, "<li class='lv3_title'><a href='https://lalacoffee.vn/Collections/ca-phe-den'>Cà phê Đen</a></li>");
            StringAssert.Contains(html, "<li class='lv3_title'><a href='https://lalacoffee.vn/Collections/ca-phe-sua'>Cà phê Sữa</a></li>");
            StringAssert.Contains(html, "<li class='lv3_title'><a href='https://lalacoffee.vn/Collections/tra-dao'>Trà đào</a></li>");

            // Verify Bánh ngọt closes properly
            Assert.Contains("</ul></li>", html);
        }

        [TestMethod]
        public void MenuHelper_RenderMenuHtml_HandlesEmptyAndNullGracefully()
        {
            var emptyResult = MenuHelper.RenderMenuHtml(new List<Category>(), "https://lalacoffee.vn");
            Assert.AreEqual(string.Empty, emptyResult);

            var nullResult = MenuHelper.RenderMenuHtml(null, "https://lalacoffee.vn");
            Assert.AreEqual(string.Empty, nullResult);
        }

        [TestMethod]
        public void MenuHelper_RenderMenuHtml_HandlesOrphanedSubCategoriesGracefully()
        {
            // Category whose ParentId does not point to an existing parent
            var categories = new List<Category>
            {
                new Category { Id = 1, Name = "Cà phê", Slug = "ca-phe", ParentId = 0 },
                new Category { Id = 99, Name = "Mồ côi", Slug = "mo-coi", ParentId = 999 }
            };

            var html = MenuHelper.RenderMenuHtml(categories, "https://lalacoffee.vn");

            // Should render the valid parent, without throwing exception for orphaned child
            StringAssert.Contains(html, "Cà phê");
            Assert.DoesNotContain("mo-coi", html, "Orphaned subcategory without valid parent should not render under non-existent parent.");
        }

        #endregion
    }
}
