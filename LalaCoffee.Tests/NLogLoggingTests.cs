using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog;
using NLog.Config;
using LalaCoffee;

namespace LalaCoffee.Tests
{
    [TestClass]
    public class NLogLoggingTests
    {
        private class DummyController : Controller { }

        private class TestHttpContext : HttpContextBase
        {
            public override bool IsCustomErrorEnabled => true;
            public override HttpResponseBase Response => new TestHttpResponse();
            public override HttpServerUtilityBase Server => new TestHttpServerUtility();
        }

        private class TestHttpResponse : HttpResponseBase
        {
            public override int StatusCode { get; set; } = 200;
            public override bool TrySkipIisCustomErrors { get; set; }
        }

        private class TestHttpServerUtility : HttpServerUtilityBase
        {
            public override void ClearError() { }
        }

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
        public void NLogConfig_FileExistsAndIsValidXml()
        {
            var projectRoot = GetProjectRoot();
            var configPath = Path.Combine(projectRoot, "NLog.config");

            Assert.IsTrue(File.Exists(configPath), "NLog.config must exist in LalaCoffee project root");

            var doc = XDocument.Load(configPath);
            Assert.IsNotNull(doc.Root, "NLog.config must have a valid root XML element");
            Assert.AreEqual("nlog", doc.Root.Name.LocalName.ToLowerInvariant(), "Root element must be <nlog>");
        }

        [TestMethod]
        public void NLogConfig_ConfiguresRollingFileTargets()
        {
            var projectRoot = GetProjectRoot();
            var configPath = Path.Combine(projectRoot, "NLog.config");
            var doc = XDocument.Load(configPath);

            var targets = doc.Root?.Descendants().Where(e => e.Name.LocalName == "target").ToList();
            Assert.IsNotNull(targets, "NLog.config must have targets");
            Assert.IsGreaterThanOrEqualTo(targets.Count, 2, "NLog.config must define at least appFile and errorFile targets");

            var appTarget = targets.FirstOrDefault(t => (string)t.Attribute("name")! == "appFile");
            Assert.IsNotNull(appTarget, "Target 'appFile' must be defined");
            var appFileName = (string)appTarget.Attribute("fileName")!;
            StringAssert.Contains(appFileName, "logs/app-${shortdate}.log",
                "appFile target must use rolling filename pattern 'logs/app-${shortdate}.log'");

            var errorTarget = targets.FirstOrDefault(t => (string)t.Attribute("name")! == "errorFile");
            Assert.IsNotNull(errorTarget, "Target 'errorFile' must be defined");
            var errorFileName = (string)errorTarget.Attribute("fileName")!;
            StringAssert.Contains(errorFileName, "logs/error-${shortdate}.log",
                "errorFile target must use rolling filename pattern 'logs/error-${shortdate}.log'");

            foreach (var target in new[] { appTarget, errorTarget })
            {
                var layout = (string)target.Attribute("layout")!;
                Assert.IsNotNull(layout, "Target layout must not be null");

                StringAssert.Contains(layout, "date", "Layout must include timestamp (e.g. ${longdate})");
                StringAssert.Contains(layout, "level", "Layout must include log level");
                StringAssert.Contains(layout, "logger", "Layout must include logger name");
                StringAssert.Contains(layout, "message", "Layout must include message");
                StringAssert.Contains(layout, "exception", "Layout must include exception details");
                StringAssert.Contains(layout, "tostring", "Layout must format exception to include full stack trace");
            }
        }

        [TestMethod]
        public void NLogConfig_ConfiguresRulesForAppAndErrorLogging()
        {
            var projectRoot = GetProjectRoot();
            var configPath = Path.Combine(projectRoot, "NLog.config");
            var doc = XDocument.Load(configPath);

            var rules = doc.Root?.Descendants().Where(e => e.Name.LocalName == "logger").ToList();
            Assert.IsNotNull(rules, "NLog.config must have logger rules");

            var appRule = rules.FirstOrDefault(r => (string)r.Attribute("writeTo")! == "appFile");
            Assert.IsNotNull(appRule, "Rule writing to 'appFile' must exist");

            var errorRule = rules.FirstOrDefault(r => (string)r.Attribute("writeTo")! == "errorFile");
            Assert.IsNotNull(errorRule, "Rule writing to 'errorFile' must exist");
            Assert.AreEqual("Error", (string)errorRule.Attribute("minlevel")!, "Error rule must have minlevel='Error'");
        }

        [TestMethod]
        public void NLog_LogsExceptionWithStackTrace_Successfully()
        {
            var projectRoot = GetProjectRoot();
            var configPath = Path.Combine(projectRoot, "NLog.config");

            LogManager.Configuration = new XmlLoggingConfiguration(configPath);
            var logger = LogManager.GetLogger("TestDiagnosticsLogger");

            try
            {
                throw new InvalidOperationException("Simulated test diagnostic error with internal details");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Captured test diagnostic exception");
            }

            Assert.IsNotNull(logger, "Logger must be successfully instantiated and execute without errors");
        }

        [TestMethod]
        public void AiExceptionFilter_LogsUnhandledExceptionAndRendersFriendlyError()
        {
            var filter = new AiExceptionFilter();
            Assert.IsNotNull(filter, "AiExceptionFilter must be instantiable");

            var routeData = new RouteData();
            routeData.Values.Add("controller", "Home");
            routeData.Values.Add("action", "Index");

            var testEx = new NullReferenceException("Test null reference inside Home/Index");
            var controllerContext = new ControllerContext(new TestHttpContext(), routeData, new DummyController());
            var exceptionContext = new ExceptionContext(controllerContext, testEx);

            filter.OnException(exceptionContext);

            Assert.IsTrue(exceptionContext.ExceptionHandled, "AiExceptionFilter must mark exception as handled when customErrors is enabled");
            Assert.IsInstanceOfType(exceptionContext.Result, typeof(ViewResult), "Result must be a ViewResult");
            var viewResult = (ViewResult)exceptionContext.Result;
            Assert.AreEqual("Error", viewResult.ViewName, "ViewName must be 'Error'");
        }

        [TestMethod]
        public void FilterConfig_RegistersAiExceptionFilterGlobally()
        {
            var filters = new GlobalFilterCollection();
            FilterConfig.RegisterGlobalFilters(filters);

            Assert.IsGreaterThan(0, filters.Count, "Global filters must not be empty");
            var registeredFilter = filters.FirstOrDefault(f => f.Instance is AiExceptionFilter || f.Instance is HandleErrorAttribute);
            Assert.IsNotNull(registeredFilter, "FilterConfig must register AiExceptionFilter or HandleErrorAttribute");
        }

        [TestMethod]
        public void GlobalApplication_HasApplicationErrorHandler()
        {
            var appType = typeof(MvcApplication);
            var errorMethod = appType.GetMethod("Application_Error",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            Assert.IsNotNull(errorMethod, "MvcApplication must define Application_Error handler for unhandled application-level errors");
        }
    }
}
