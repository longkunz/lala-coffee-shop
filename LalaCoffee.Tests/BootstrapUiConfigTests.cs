using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System.Web.Optimization;
using LalaCoffee;

namespace LalaCoffee.Tests
{
    [TestClass]
    public class BootstrapUiConfigTests
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

        [TestMethod]
        public void LibManConfig_FileExistsAndIsValidJson()
        {
            var projectRoot = GetProjectRoot();
            var libmanPath = Path.Combine(projectRoot, "libman.json");

            Assert.IsTrue(File.Exists(libmanPath), "libman.json must exist in LalaCoffee project root");

            var jsonContent = File.ReadAllText(libmanPath);
            var json = JObject.Parse(jsonContent);

            Assert.IsNotNull(json["version"], "libman.json must specify version");
            Assert.AreEqual("1.0", (string)json["version"]!);
            Assert.IsNotNull(json["defaultProvider"], "libman.json must specify defaultProvider");
        }

        [TestMethod]
        public void LibManConfig_ManagesBootstrap5Library()
        {
            var projectRoot = GetProjectRoot();
            var libmanPath = Path.Combine(projectRoot, "libman.json");

            var jsonContent = File.ReadAllText(libmanPath);
            var json = JObject.Parse(jsonContent);

            var libraries = json["libraries"] as JArray;
            Assert.IsNotNull(libraries, "libman.json must have libraries array");
            Assert.IsGreaterThan(0, libraries.Count, "libraries array must not be empty");

            var bootstrapLib = libraries.FirstOrDefault(l =>
                l["library"] != null && ((string)l["library"]!).StartsWith("bootstrap@5", StringComparison.OrdinalIgnoreCase));

            Assert.IsNotNull(bootstrapLib, "libman.json must contain bootstrap@5.x library");
            Assert.AreEqual("Content/lib/bootstrap/", (string)bootstrapLib["destination"]!);

            var files = (bootstrapLib["files"] as JArray)?.Select(f => (string)f!).ToList();
            Assert.IsNotNull(files, "files array must be present for bootstrap");
            Assert.IsTrue(files.Any(f => f.Contains("bootstrap.min.css")), "bootstrap.min.css must be specified");
            Assert.IsTrue(files.Any(f => f.Contains("bootstrap.bundle.min.js")), "bootstrap.bundle.min.js must be specified");
        }

        [TestMethod]
        public void Bootstrap5Assets_ExistInFileSystemAndAreNonEmpty()
        {
            var projectRoot = GetProjectRoot();
            var cssPath = Path.Combine(projectRoot, "Content", "lib", "bootstrap", "dist", "css", "bootstrap.min.css");
            var jsPath = Path.Combine(projectRoot, "Content", "lib", "bootstrap", "dist", "js", "bootstrap.bundle.min.js");

            Assert.IsTrue(File.Exists(cssPath), "bootstrap.min.css must exist at: " + cssPath);
            Assert.IsTrue(File.Exists(jsPath), "bootstrap.bundle.min.js must exist at: " + jsPath);

            var cssInfo = new FileInfo(cssPath);
            var jsInfo = new FileInfo(jsPath);

            Assert.IsGreaterThan(50000L, cssInfo.Length, "bootstrap.min.css should be a valid compiled css file (> 50KB)");
            Assert.IsGreaterThan(50000L, jsInfo.Length, "bootstrap.bundle.min.js should be a valid compiled js bundle (> 50KB)");
        }

        [TestMethod]
        public void Bootstrap5Assets_ContainValidBootstrap5Signatures()
        {
            var projectRoot = GetProjectRoot();
            var cssPath = Path.Combine(projectRoot, "Content", "lib", "bootstrap", "dist", "css", "bootstrap.min.css");
            var jsPath = Path.Combine(projectRoot, "Content", "lib", "bootstrap", "dist", "js", "bootstrap.bundle.min.js");

            var cssContent = File.ReadAllText(cssPath);
            var jsContent = File.ReadAllText(jsPath);

            StringAssert.Contains(cssContent, "Bootstrap", "bootstrap.min.css must contain Bootstrap signature");
            StringAssert.Contains(cssContent, "v5.", "bootstrap.min.css must be Bootstrap version 5");
            StringAssert.Contains(jsContent, "Bootstrap", "bootstrap.bundle.min.js must contain Bootstrap signature");
            StringAssert.Contains(jsContent, "v5.", "bootstrap.bundle.min.js must be Bootstrap version 5");
        }

        [TestMethod]
        public void BundleConfig_RegistersBootstrap5Assets()
        {
            var bundles = new BundleCollection();
            BundleConfig.RegisterBundles(bundles);

            var bootstrapScriptBundle = bundles.GetBundleFor("~/bundles/bootstrap");
            Assert.IsNotNull(bootstrapScriptBundle, "~/bundles/bootstrap bundle must be registered");

            var cssBundle = bundles.GetBundleFor("~/Content/css");
            Assert.IsNotNull(cssBundle, "~/Content/css bundle must be registered");

            var projectRoot = GetProjectRoot();
            var bundleConfigFile = Path.Combine(projectRoot, "App_Start", "BundleConfig.cs");
            var content = File.ReadAllText(bundleConfigFile);

            StringAssert.Contains(content, "Content/lib/bootstrap/dist/js/bootstrap.bundle.min.js",
                "BundleConfig must reference Bootstrap 5 bundle js");
            StringAssert.Contains(content, "Content/lib/bootstrap/dist/css/bootstrap.min.css",
                "BundleConfig must reference Bootstrap 5 minified css");
        }

        [TestMethod]
        public void LayoutViews_ReferenceBootstrap5Assets()
        {
            var projectRoot = GetProjectRoot();
            var userLayoutPath = Path.Combine(projectRoot, "Views", "Shared", "_Layout_User.cshtml");
            Assert.IsTrue(File.Exists(userLayoutPath), "_Layout_User.cshtml must exist");

            var userLayoutContent = File.ReadAllText(userLayoutPath);
            StringAssert.Contains(userLayoutContent, "Content/lib/bootstrap/dist/css/bootstrap.min.css",
                "_Layout_User.cshtml must reference local Bootstrap 5 css");
            StringAssert.Contains(userLayoutContent, "Content/lib/bootstrap/dist/js/bootstrap.bundle.min.js",
                "_Layout_User.cshtml must reference local Bootstrap 5 bundle js");

            var defaultLayoutPath = Path.Combine(projectRoot, "Views", "Shared", "_Layout.cshtml");
            Assert.IsTrue(File.Exists(defaultLayoutPath), "_Layout.cshtml must exist");

            var defaultLayoutContent = File.ReadAllText(defaultLayoutPath);
            StringAssert.Contains(defaultLayoutContent, "data-bs-toggle=\"collapse\"",
                "_Layout.cshtml must use Bootstrap 5 data-bs-toggle for responsive navigation");
            StringAssert.Contains(defaultLayoutContent, "data-bs-target=\".navbar-collapse\"",
                "_Layout.cshtml must use Bootstrap 5 data-bs-target for responsive navigation");
        }

        [TestMethod]
        public void ClientViews_UseBootstrap5DataAttributes()
        {
            var projectRoot = GetProjectRoot();
            var detailsPath = Path.Combine(projectRoot, "Views", "Product", "Details.cshtml");
            Assert.IsTrue(File.Exists(detailsPath), "Details.cshtml must exist");

            var detailsContent = File.ReadAllText(detailsPath);
            StringAssert.Contains(detailsContent, "data-bs-toggle=\"tab\"",
                "Product Details view must use Bootstrap 5 data-bs-toggle for tabs");
        }

        [TestMethod]
        public void WebConfig_ConfiguresSafeProductionSettingsAndCustomErrors()
        {
            var projectRoot = GetProjectRoot();
            var webConfigPath = Path.Combine(projectRoot, "Web.config");
            Assert.IsTrue(File.Exists(webConfigPath), "Web.config must exist");

            var doc = XDocument.Load(webConfigPath);
            var systemWeb = doc.Root?.Element("system.web");
            Assert.IsNotNull(systemWeb, "<system.web> section must exist in Web.config");

            var compilation = systemWeb.Element("compilation");
            Assert.IsNotNull(compilation, "<compilation> element must exist");
            Assert.AreEqual("false", (string)compilation.Attribute("debug")!,
                "Compilation debug attribute must be 'false' for safe production operation");

            var customErrors = systemWeb.Element("customErrors");
            Assert.IsNotNull(customErrors, "<customErrors> element must exist in <system.web>");
            Assert.AreEqual("RemoteOnly", (string)customErrors.Attribute("mode")!,
                "customErrors mode must be 'RemoteOnly'");
            Assert.AreEqual("~/Shared/Error", (string)customErrors.Attribute("defaultRedirect")!,
                "customErrors defaultRedirect must be '~/Shared/Error'");
        }

        [TestMethod]
        public void ErrorPage_HidesDatabaseDetailsAndStackTraces()
        {
            var projectRoot = GetProjectRoot();
            var errorViewPath = Path.Combine(projectRoot, "Views", "Shared", "Error.cshtml");
            Assert.IsTrue(File.Exists(errorViewPath), "Views/Shared/Error.cshtml must exist");

            var content = File.ReadAllText(errorViewPath);

            StringAssert.DoesNotMatch(content, new System.Text.RegularExpressions.Regex("@Model\\.Exception\\.StackTrace"), "Error.cshtml must not expose Model.Exception.StackTrace");
            StringAssert.DoesNotMatch(content, new System.Text.RegularExpressions.Regex("@Model\\.Exception\\.Message"), "Error.cshtml must not expose raw Model.Exception.Message");
            StringAssert.DoesNotMatch(content, new System.Text.RegularExpressions.Regex("Server\\.GetLastError"), "Error.cshtml must not directly render Server.GetLastError");
            StringAssert.DoesNotMatch(content, new System.Text.RegularExpressions.Regex("SqlException"), "Error.cshtml must not expose SqlException");
            StringAssert.DoesNotMatch(content, new System.Text.RegularExpressions.Regex("connectionString"), "Error.cshtml must not expose database connection string");

            StringAssert.Contains(content, "LalaCoffee", "Error.cshtml must display LalaCoffee branding");
            StringAssert.Contains(content, "Về Trang Chủ", "Error.cshtml must provide a friendly home navigation link");
        }
    }
}
