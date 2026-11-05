using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using LalaCoffee.Areas.Admin.Controllers;
using LalaCoffee.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LalaCoffee.Tests
{
    [TestClass]
    public class CsrfProtectionTests
    {
        [TestMethod]
        public void Startup_CookieAuthentication_LoginPath_ShouldBeAccountLogin()
        {
            var configMethod = typeof(Startup).GetMethod("Configuration");
            Assert.IsNotNull(configMethod, "Startup must contain a 'Configuration' method.");

            var strings = GetStringLiterals(configMethod);
            Assert.Contains("/Account/Login", strings, $"Startup.Configuration must configure LoginPath to '/Account/Login'. Found string literals: [{string.Join(", ", strings)}]");
            Assert.DoesNotContain("/Home/Index", strings, "Startup.Configuration must not have LoginPath set to '/Home/Index'.");
        }

        private static List<string> GetStringLiterals(MethodInfo method)
        {
            var list = new List<string>();
            var body = method.GetMethodBody();
            if (body == null) return list;

            byte[] il = body.GetILAsByteArray();
            int i = 0;
            while (i < il.Length)
            {
                byte opCode = il[i];
                if (opCode == 0x72) // OpCodes.Ldstr
                {
                    int token = BitConverter.ToInt32(il, i + 1);
                    try
                    {
                        string s = method.Module.ResolveString(token);
                        list.Add(s);
                    }
                    catch { }
                    i += 5;
                }
                else
                {
                    i++;
                }
            }
            return list;
        }

        [TestMethod]
        public void AccountController_HttpPostActions_MustHaveValidateAntiForgeryToken()
        {
            AssertAllHttpPostActionsHaveValidateAntiForgeryToken(typeof(AccountController));

            // Verify explicitly required methods exist and are protected
            AssertMethodHasHttpPostAndAntiForgeryToken(typeof(AccountController), "Login");
            AssertMethodHasHttpPostAndAntiForgeryToken(typeof(AccountController), "Register");
            AssertMethodHasHttpPostAndAntiForgeryToken(typeof(AccountController), "LogOff");
        }

        [TestMethod]
        public void OrderController_CheckOutPost_MustHaveValidateAntiForgeryToken()
        {
            AssertMethodHasHttpPostAndAntiForgeryToken(typeof(OrderController), "CheckOut");
        }

        [TestMethod]
        public void AdminControllers_AllHttpPostActions_MustHaveValidateAntiForgeryToken()
        {
            var adminControllerTypes = typeof(ManageProductController).Assembly
                .GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && typeof(Controller).IsAssignableFrom(t)
                            && t.Namespace != null && t.Namespace.StartsWith("LalaCoffee.Areas.Admin.Controllers"))
                .ToList();

            Assert.IsGreaterThanOrEqualTo(7, adminControllerTypes.Count, $"Expected at least 7 Admin controllers, found {adminControllerTypes.Count}.");

            var unprotectedActions = new List<string>();

            foreach (var controller in adminControllerTypes)
            {
                var methods = controller.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Where(m => !m.IsSpecialName && m.GetCustomAttribute<HttpPostAttribute>() != null)
                    .ToList();

                foreach (var method in methods)
                {
                    var antiForgery = method.GetCustomAttribute<ValidateAntiForgeryTokenAttribute>();
                    if (antiForgery == null)
                    {
                        unprotectedActions.Add($"{controller.Name}.{method.Name}");
                    }
                }
            }

            Assert.IsEmpty(
                unprotectedActions,
                $"The following Admin [HttpPost] actions are missing [ValidateAntiForgeryToken]: {string.Join(", ", unprotectedActions)}"
            );
        }

        [TestMethod]
        public void ExternalPaymentCallbacks_MustNotHaveValidateAntiForgeryToken()
        {
            var paymentControllerType = typeof(PayController);
            var methods = paymentControllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.Name == "MomoProcessing" || m.Name == "VNPayProcessing")
                .ToList();

            Assert.IsNotEmpty(methods, "Payment callback actions should exist on PayController.");

            foreach (var method in methods)
            {
                var antiForgery = method.GetCustomAttribute<ValidateAntiForgeryTokenAttribute>();
                Assert.IsNull(
                    antiForgery,
                    $"External payment webhook '{method.Name}' must NOT have [ValidateAntiForgeryToken] attribute."
                );
            }
        }

        private static void AssertAllHttpPostActionsHaveValidateAntiForgeryToken(Type controllerType)
        {
            var httpPostMethods = controllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => !m.IsSpecialName && m.GetCustomAttribute<HttpPostAttribute>() != null)
                .ToList();

            Assert.IsNotEmpty(httpPostMethods, $"Controller '{controllerType.Name}' must have at least one [HttpPost] action.");

            foreach (var method in httpPostMethods)
            {
                var antiForgery = method.GetCustomAttribute<ValidateAntiForgeryTokenAttribute>();
                Assert.IsNotNull(
                    antiForgery,
                    $"Action '{controllerType.Name}.{method.Name}' has [HttpPost] but is missing [ValidateAntiForgeryToken]."
                );
            }
        }

        private static void AssertMethodHasHttpPostAndAntiForgeryToken(Type controllerType, string methodName)
        {
            var method = controllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .FirstOrDefault(m => m.Name.Equals(methodName, StringComparison.OrdinalIgnoreCase) && m.GetCustomAttribute<HttpPostAttribute>() != null);

            Assert.IsNotNull(method, $"Expected [HttpPost] method '{methodName}' on controller '{controllerType.Name}' was not found.");

            var antiForgery = method.GetCustomAttribute<ValidateAntiForgeryTokenAttribute>();
            Assert.IsNotNull(antiForgery, $"Method '{controllerType.Name}.{method.Name}' must have [ValidateAntiForgeryToken] attribute.");
        }
    }
}
