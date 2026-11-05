using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LalaCoffee.Models;
using LalaCoffee.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LalaCoffee.Tests
{
    [TestClass]
    public class OrderCalculationTests
    {
        [TestMethod]
        public void CalculateUnitPrice_BaseOnly_ReturnsBasePrice()
        {
            decimal unitPrice = OrderCalculator.CalculateUnitPrice(35000m, 0m, null);
            Assert.AreEqual(35000m, unitPrice);
        }

        [TestMethod]
        public void CalculateUnitPrice_WithSizeAndToppings_ReturnsSum()
        {
            var toppings = new List<decimal> { 5000m, 7000m };
            decimal unitPrice = OrderCalculator.CalculateUnitPrice(35000m, 10000m, toppings);

            // 35000 + 10000 + 5000 + 7000 = 57000
            Assert.AreEqual(57000m, unitPrice);
        }

        [TestMethod]
        public void CalculateLineTotal_SingleQuantity_ReturnsUnitPrice()
        {
            var toppings = new List<decimal> { 5000m };
            decimal lineTotal = OrderCalculator.CalculateLineTotal(30000m, 5000m, toppings, 1);

            // (30000 + 5000 + 5000) * 1 = 40000
            Assert.AreEqual(40000m, lineTotal);
        }

        [TestMethod]
        public void CalculateLineTotal_MultipleQuantity_ReturnsMultipliedTotal()
        {
            var toppings = new List<decimal> { 5000m };
            int quantity = 5;
            decimal lineTotal = OrderCalculator.CalculateLineTotal(30000m, 5000m, toppings, quantity);

            // (30000 + 5000 + 5000) * 5 = 200000
            Assert.AreEqual(200000m, lineTotal);
            // Verify it does NOT equal single-item total (the previous bug)
            Assert.AreNotEqual(40000m, lineTotal);
        }

        [TestMethod]
        public void CalculateLineTotal_LargeQuantity_ReturnsAccurateAmount()
        {
            var toppings = new List<decimal> { 7000m, 5000m };
            int quantity = 25;
            decimal lineTotal = OrderCalculator.CalculateLineTotal(45000m, 8000m, toppings, quantity);

            // unit = 45000 + 8000 + 7000 + 5000 = 65000; total = 65000 * 25 = 1625000
            Assert.AreEqual(1625000m, lineTotal);
        }

        [TestMethod]
        public void CalculateLineTotal_ZeroQuantity_ReturnsZero()
        {
            decimal lineTotal = OrderCalculator.CalculateLineTotal(50000m, 10000m, new[] { 5000m }, 0);
            Assert.AreEqual(0m, lineTotal);
        }

        [TestMethod]
        public void CalculateLineTotal_NullOptions_DoesNotThrow()
        {
            decimal lineTotal = OrderCalculator.CalculateLineTotal(30000m, 5000m, null, 3);
            // (30000 + 5000) * 3 = 105000
            Assert.AreEqual(105000m, lineTotal);
        }

        [TestMethod]
        public void CalculateCartTotal_MultipleDistinctItems_ReturnsAccurateGrandTotal()
        {
            var cart = new List<CartItem>
            {
                // Item 1: 3 cups of Ca phe sua da (30k base + 8k size L + 5k jelly + 5k boba = 48k/cup * 3 = 144k)
                new CartItem
                {
                    ProductId = 1,
                    ProductName = "Cà phê sữa đá",
                    Price = 30000m,
                    Quantity = 3,
                    PropertyProduct = new PropertyProduct { Id = 1, Name = "Size L", Price = 8000m },
                    Options = new List<OptionProduct>
                    {
                        new OptionProduct { Id = 1, Name = "Thạch cà phê", Price = 5000m },
                        new OptionProduct { Id = 2, Name = "Trân châu trắng", Price = 5000m }
                    }
                },
                // Item 2: 2 cups of Tra dao (45k base + 0k size M + no options = 45k/cup * 2 = 90k)
                new CartItem
                {
                    ProductId = 2,
                    ProductName = "Trà đào cam sả",
                    Price = 45000m,
                    Quantity = 2,
                    PropertyProduct = new PropertyProduct { Id = 2, Name = "Size M", Price = 0m },
                    Options = new List<OptionProduct>()
                },
                // Item 3: 4 cups of Bac xiu (32k base + 8k size L + 10k cheese foam = 50k/cup * 4 = 200k)
                new CartItem
                {
                    ProductId = 3,
                    ProductName = "Bạc xỉu",
                    Price = 32000m,
                    Quantity = 4,
                    PropertyProduct = new PropertyProduct { Id = 3, Name = "Size L", Price = 8000m },
                    Options = new List<OptionProduct>
                    {
                        new OptionProduct { Id = 3, Name = "Kem cheese", Price = 10000m }
                    }
                }
            };

            decimal grandTotal = OrderCalculator.CalculateCartTotal(cart);

            // 144000 + 90000 + 200000 = 434000
            Assert.AreEqual(434000m, grandTotal);

            // Buggy total (omitting quantity): 48000 + 45000 + 50000 = 143000
            Assert.AreNotEqual(143000m, grandTotal);
        }

        [TestMethod]
        public void CalculateCartTotal_EmptyOrNullCart_ReturnsZero()
        {
            Assert.AreEqual(0m, OrderCalculator.CalculateCartTotal(null));
            Assert.AreEqual(0m, OrderCalculator.CalculateCartTotal(new List<CartItem>()));
        }

        [TestMethod]
        public void CartManager_GetCartTotal_MatchesOrderCalculator()
        {
            var cartManager = new CartManager();
            var cart = new List<CartItem>
            {
                new CartItem
                {
                    ProductId = 1,
                    ProductName = "Cà phê đen",
                    Price = 25000m,
                    Quantity = 3,
                    PropertyProduct = new PropertyProduct { Id = 1, Name = "Size L", Price = 5000m },
                    Options = new List<OptionProduct>
                    {
                        new OptionProduct { Id = 1, Name = "Boba", Price = 5000m }
                    }
                }
            };

            decimal cartTotal = cartManager.GetCartTotal(cart);
            decimal calcTotal = OrderCalculator.CalculateCartTotal(cart);

            // (25000 + 5000 + 5000) * 3 = 105000
            Assert.AreEqual(105000m, cartTotal);
            Assert.AreEqual(105000m, calcTotal);
        }

        [TestMethod]
        public void Order_InitialState_PendingAndUnpaid()
        {
            var order = new Order
            {
                Paid = 0,
                Status = "Pending",
                Code = "TEST12345678"
            };

            Assert.AreEqual(0, order.Paid);
            Assert.AreEqual("Pending", order.Status);
            Assert.AreEqual("TEST12345678", order.Code);
        }

        [TestMethod]
        public void SourceAudit_OrderController_DoesNotHaveBuggyTotalAssignmentWithoutQuantity()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string solutionDir = Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\.."));
            string orderControllerPath = Path.Combine(solutionDir, @"LalaCoffee\Controllers\OrderController.cs");

            if (!File.Exists(orderControllerPath))
            {
                orderControllerPath = @"D:\UniCafe-wt-05\LalaCoffee\Controllers\OrderController.cs";
            }

            Assert.IsTrue(File.Exists(orderControllerPath), $"OrderController.cs not found at {orderControllerPath}");

            string content = File.ReadAllText(orderControllerPath);

            // Ensure orderDetail.Total = itemTotal without quantity is not present
            Assert.DoesNotContain("orderDetail.Total = itemTotal;", content,
                "OrderController.cs should not assign orderDetail.Total = itemTotal without multiplying by Quantity!");

            // Ensure order is added to DB and not commented out
            Assert.DoesNotContain("//Add(order);", content,
                "OrderController.cs must not have //Add(order); commented out!");

            Assert.Contains("Context.Orders.Add(order)", content,
                "OrderController.cs must persist order to database!");
        }

        [TestMethod]
        public void SourceAudit_PayController_DoesNotHaveBuggyTotalAssignmentWithoutQuantity()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string solutionDir = Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\.."));
            string payControllerPath = Path.Combine(solutionDir, @"LalaCoffee\Controllers\PayController.cs");

            if (!File.Exists(payControllerPath))
            {
                payControllerPath = @"D:\UniCafe-wt-05\LalaCoffee\Controllers\PayController.cs";
            }

            Assert.IsTrue(File.Exists(payControllerPath), $"PayController.cs not found at {payControllerPath}");

            string content = File.ReadAllText(payControllerPath);

            Assert.DoesNotContain("orderDetail.Total = itemTotal;", content,
                "PayController.cs should not assign orderDetail.Total = itemTotal without multiplying by Quantity!");

            // Ensure PayController looks up order in DB
            Assert.Contains("Context.Orders.FirstOrDefault", content,
                "PayController.cs must query Context.Orders.FirstOrDefault by orderCode instead of relying solely on Session!");
        }
    }
}
