using System;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using LalaCoffee.Areas.Admin.Controllers;
using LalaCoffee.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LalaCoffee.Tests
{
    [TestClass]
    public class PublicEndpointsSecurityTests
    {
        private static readonly string[] ProhibitedMethodNames = { "Create", "Edit", "Delete" };

        [TestMethod]
        public void CategoryController_ShouldNotHavePublicCrudMethods()
        {
            AssertControllerHasNoProhibitedMethods(typeof(CategoryController));
        }

        [TestMethod]
        public void OptionProductController_ShouldNotHavePublicCrudMethods()
        {
            AssertControllerHasNoProhibitedMethods(typeof(OptionProductController));
        }

        [TestMethod]
        public void PropertyProductController_ShouldNotHavePublicCrudMethods()
        {
            AssertControllerHasNoProhibitedMethods(typeof(PropertyProductController));
        }

        [TestMethod]
        public void AdminControllers_ShouldBeProtectedWithAuthorizeAdminRole()
        {
            Type[] adminControllers = new[]
            {
                typeof(ManageCategoryController),
                typeof(ManageOptionProductController),
                typeof(ManagePropertyProductController)
            };

            foreach (var controller in adminControllers)
            {
                var authAttr = controller.GetCustomAttribute<AuthorizeAttribute>(inherit: true);
                Assert.IsNotNull(authAttr, $"{controller.Name} must have [Authorize] attribute.");
                Assert.AreEqual("Admin", authAttr.Roles, $"{controller.Name} [Authorize] must require Roles = 'Admin'.");
            }
        }

        private static void AssertControllerHasNoProhibitedMethods(Type controllerType)
        {
            var declaredMethods = controllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => !m.IsSpecialName)
                .Select(m => m.Name)
                .Distinct()
                .ToList();

            foreach (var prohibited in ProhibitedMethodNames)
            {
                bool hasProhibitedMethod = declaredMethods.Contains(prohibited, StringComparer.OrdinalIgnoreCase);
                Assert.IsFalse(
                    hasProhibitedMethod,
                    $"Public controller '{controllerType.Name}' must NOT contain '{prohibited}' action method."
                );
            }
        }
    }
}
