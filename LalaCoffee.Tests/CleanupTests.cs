using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LalaCoffee.Tests
{
    [TestClass]
    public class CleanupTests
    {
        [TestMethod]
        public void DeadAndConflictFiles_DoNotExistInProjectDirectory()
        {
            var testAssemblyDir = Path.GetDirectoryName(typeof(CleanupTests).Assembly.Location)
                ?? AppDomain.CurrentDomain.BaseDirectory;
            var projectDir = Path.GetFullPath(Path.Combine(testAssemblyDir, @"..\..\..\..\LalaCoffee"));

            Assert.IsTrue(Directory.Exists(projectDir), $"Project directory should exist at: {projectDir}");

            var deadFiles = new[]
            {
                Path.Combine(projectDir, @"Areas\Admin\Controllers\ManageCategoryController_BACKUP_273.cs"),
                Path.Combine(projectDir, @"Areas\Admin\Controllers\ManageCategoryController_BASE_273.cs"),
                Path.Combine(projectDir, @"Areas\Admin\Controllers\ManageCategoryController_LOCAL_273.cs"),
                Path.Combine(projectDir, @"Areas\Admin\Controllers\ManageCategoryController_REMOTE_273.cs"),
                Path.Combine(projectDir, @"Controllers\CollectionController.cs"),
                Path.Combine(projectDir, @"Views\Collection\Index.cshtml"),
                Path.Combine(projectDir, @"Models\User.cs"),
                Path.Combine(projectDir, @"Models\Role.cs")
            };

            var existingFiles = deadFiles.Where(File.Exists).ToList();
            Assert.IsEmpty(existingFiles,
                $"Dead or conflict files still exist:\n{string.Join(Environment.NewLine, existingFiles)}");

            var deadDirs = new[]
            {
                Path.Combine(projectDir, @"Views\Collection")
            };
            var existingDirs = deadDirs.Where(Directory.Exists).ToList();
            Assert.IsEmpty(existingDirs,
                $"Dead directories still exist:\n{string.Join(Environment.NewLine, existingDirs)}");
        }

        [TestMethod]
        public void AccountController_IsInControllersNamespace()
        {
            // Verify compile-time type resolution
            Assert.AreEqual("LalaCoffee.Controllers", typeof(LalaCoffee.Controllers.AccountController).Namespace);

            // Verify via reflection scanning
            var accountControllerType = typeof(LalaCoffee.MvcApplication).Assembly
                .GetTypes()
                .FirstOrDefault(t => t.Name == "AccountController");

            Assert.IsNotNull(accountControllerType, "AccountController must exist in assembly.");
            Assert.AreEqual("LalaCoffee.Controllers", accountControllerType.Namespace,
                "AccountController should have LalaCoffee.Controllers namespace.");
        }
    }
}
