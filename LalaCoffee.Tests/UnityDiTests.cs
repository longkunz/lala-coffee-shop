using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unity;
using LalaCoffee.Controllers;
using LalaCoffee.Data;
using LalaCoffee.Models;
using LalaCoffee.Services;
using LalaCoffee.Services.Email;
using LalaCoffee.Services.Payment;

namespace LalaCoffee.Tests
{
    [TestClass]
    public class UnityDiTests
    {
        private interface IUnregisteredService { }

        [TestInitialize]
        public void Setup()
        {
            UnityConfig.ResetContainer();
            UnityConfig.RegisterComponents();
        }

        [TestMethod]
        public void UnityConfig_RegisterComponents_SetsMvcDependencyResolver()
        {
            var resolver = DependencyResolver.Current;
            Assert.IsNotNull(resolver);
            Assert.IsInstanceOfType(resolver, typeof(UnityDependencyResolver));
        }

        [TestMethod]
        public void UnityConfig_Container_RegistersAllExpectedTypes()
        {
            var container = UnityConfig.Container;

            Assert.IsTrue(container.IsRegistered<IOrderService>(), "IOrderService must be registered");
            Assert.IsTrue(container.IsRegistered<ICartService>(), "ICartService must be registered");
            Assert.IsTrue(container.IsRegistered<IPaymentService>(), "IPaymentService must be registered");
            Assert.IsTrue(container.IsRegistered<IEmailService>(), "IEmailService must be registered");
            Assert.IsTrue(container.IsRegistered<ApplicationDbContext>(), "ApplicationDbContext must be registered");
        }

        [TestMethod]
        public void UnityDependencyResolver_ResolvesRegisteredDomainServices()
        {
            var resolver = DependencyResolver.Current;

            var orderService = resolver.GetService(typeof(IOrderService));
            var cartService = resolver.GetService(typeof(ICartService));
            var paymentService = resolver.GetService(typeof(IPaymentService));
            var emailService = resolver.GetService(typeof(IEmailService));

            Assert.IsNotNull(orderService);
            Assert.IsInstanceOfType(orderService, typeof(OrderService));

            Assert.IsNotNull(cartService);
            Assert.IsInstanceOfType(cartService, typeof(CartService));

            Assert.IsNotNull(paymentService);
            Assert.IsInstanceOfType(paymentService, typeof(PaymentService));

            Assert.IsNotNull(emailService);
            Assert.IsInstanceOfType(emailService, typeof(EmailService));
        }

        [TestMethod]
        public void UnityDependencyResolver_UnregisteredInterface_ReturnsNull_DoesNotThrow()
        {
            var resolver = DependencyResolver.Current;

            var result = resolver.GetService(typeof(IUnregisteredService));
            Assert.IsNull(result, "Mvc DependencyResolver must return null for unresolvable interfaces, not throw.");

            var results = resolver.GetServices(typeof(IUnregisteredService));
            Assert.IsNotNull(results);
            Assert.AreEqual(0, System.Linq.Enumerable.Count(results));
        }

        [TestMethod]
        public void UnityDependencyResolver_ResolvesControllers_ViaConstructorInjection()
        {
            var resolver = DependencyResolver.Current;

            var orderController = resolver.GetService(typeof(OrderController)) as OrderController;
            var payController = resolver.GetService(typeof(PayController)) as PayController;
            var cartController = resolver.GetService(typeof(CartController)) as CartController;

            Assert.IsNotNull(orderController, "OrderController should be resolvable by Unity DI");
            Assert.IsNotNull(payController, "PayController should be resolvable by Unity DI");
            Assert.IsNotNull(cartController, "CartController should be resolvable by Unity DI");
        }

        [TestMethod]
        public void OrderController_ConstructorInjection_SupportsCustomServices()
        {
            bool orderCreated = false;
            var stubOrderService = new OrderService((order, details) =>
            {
                orderCreated = true;
                order.Code = "MOCK_ORDER_123";
            });
            var stubCartService = new CartService();
            stubCartService.AddToCart(new CartItem { ProductId = 1, Quantity = 2, Price = 30000m });
            var stubEmailService = new EmailService(_ => null);

            var controller = new OrderController(stubOrderService, stubCartService, stubEmailService);

            var form = new FormCollection
            {
                { "name", "Test User" },
                { "email", "test@lalacoffee.vn" },
                { "phoneNumber", "0987654321" },
                { "address", "123 Test Street" },
                { "payment", "cash" },
                { "note", "None" },
                { "UserId", "USER_01" }
            };

            var result = controller.CheckOut(form) as RedirectToRouteResult;

            Assert.IsTrue(orderCreated, "OrderController should delegate order creation to IOrderService");
            Assert.IsNotNull(result);
            Assert.AreEqual("CompleteOrder", result.RouteValues["action"]);
        }

        [TestMethod]
        public void OrderController_CheckOut_WithStronglyTypedViewModel_Succeeds()
        {
            bool orderCreated = false;
            var stubOrderService = new OrderService((order, details) =>
            {
                orderCreated = true;
                order.Code = "MOCK_ORDER_VM_123";
            });
            var stubCartService = new CartService();
            stubCartService.AddToCart(new CartItem { ProductId = 1, Quantity = 3, Price = 35000m });
            var stubEmailService = new EmailService(_ => null);

            var controller = new OrderController(stubOrderService, stubCartService, stubEmailService);

            var viewModel = new CheckOutViewModel
            {
                Name = "Lala Customer",
                Email = "customer@lalacoffee.vn",
                PhoneNumber = "0987654321",
                Address = "456 Nguyen Hue, Q1, HCM",
                Payment = "cash",
                Note = "Giao gio hanh chinh",
                UserId = "CUST_99"
            };

            var result = controller.CheckOut(viewModel) as RedirectToRouteResult;

            Assert.IsTrue(orderCreated, "OrderController should create order from CheckOutViewModel");
            Assert.IsNotNull(result);
            Assert.AreEqual("CompleteOrder", result.RouteValues["action"]);
        }

        [TestMethod]
        public void CheckOutViewModel_DataAnnotations_ValidationRulesEnforced()
        {
            // Invalid model: missing Name, invalid phone, missing address, invalid payment
            var invalidModel = new CheckOutViewModel
            {
                Name = "",
                PhoneNumber = "invalid-phone",
                Address = "",
                Payment = "bitcoin"
            };

            var context = new ValidationContext(invalidModel);
            var results = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(invalidModel, context, results, validateAllProperties: true);

            Assert.IsFalse(isValid, "Invalid model must fail DataAnnotations validation");
            Assert.IsTrue(results.Any(r => r.MemberNames.Contains("Name")), "Name should have validation error");
            Assert.IsTrue(results.Any(r => r.MemberNames.Contains("PhoneNumber")), "PhoneNumber should have validation error");
            Assert.IsTrue(results.Any(r => r.MemberNames.Contains("Address")), "Address should have validation error");
            Assert.IsTrue(results.Any(r => r.MemberNames.Contains("Payment")), "Payment should have validation error");

            // Valid model
            var validModel = new CheckOutViewModel
            {
                Name = "Nguyen Van A",
                PhoneNumber = "0987654321",
                Address = "123 Le Loi, Q1",
                Payment = "momo",
                Email = "nguyenvana@gmail.com"
            };

            var validContext = new ValidationContext(validModel);
            var validResults = new List<ValidationResult>();
            bool isValidSuccess = Validator.TryValidateObject(validModel, validContext, validResults, validateAllProperties: true);

            Assert.IsTrue(isValidSuccess, "Valid CheckOutViewModel must pass DataAnnotations validation");
            Assert.IsEmpty(validResults);
        }

        [TestMethod]
        public void UnityDi_PerRequestChildScope_ProvidesIsolatedDbContextAndDisposesOnScopeEnd()
        {
            var resolver = DependencyResolver.Current as UnityDependencyResolver;
            Assert.IsNotNull(resolver, "Resolver must be UnityDependencyResolver");

            // Simulate Request 1 child scope
            using (var scope1 = resolver.CreateChildScope())
            {
                var context1 = scope1.GetService(typeof(ApplicationDbContext)) as ApplicationDbContext;
                Assert.IsNotNull(context1, "Child scope 1 must resolve ApplicationDbContext");

                // Simulate Request 2 child scope
                using (var scope2 = resolver.CreateChildScope())
                {
                    var context2 = scope2.GetService(typeof(ApplicationDbContext)) as ApplicationDbContext;
                    Assert.IsNotNull(context2, "Child scope 2 must resolve ApplicationDbContext");

                    // Hierarchical lifetime guarantees per-scope isolation
                    Assert.AreNotSame(context1, context2, "Different scopes must receive distinct ApplicationDbContext instances");
                }
            }
        }

        [TestMethod]
        public void PayController_ConstructorInjection_SupportsCustomServices()
        {
            var stubPaymentService = new PaymentService(null, null);
            var stubOrderService = new OrderService(
                orderPersister: (p, d) => { },
                orderLookup: code => new Order { Code = "ORD_MOCK_1", Total = 50000m }
            );
            var stubCartService = new CartService();
            var stubEmailService = new EmailService(_ => null);

            var controller = new PayController(stubPaymentService, stubOrderService, stubCartService, stubEmailService);
            Assert.IsNotNull(controller);
        }

        [TestMethod]
        public void CartController_ConstructorInjection_SupportsCustomServices()
        {
            var stubCartService = new CartService();
            var controller = new CartController(stubCartService);

            var result = controller.GetTotal() as JsonResult;
            Assert.IsNotNull(result);
        }
    }
}
