using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using LalaCoffee.Data;
using LalaCoffee.Models;
using LalaCoffee.Services;

namespace LalaCoffee.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<PropertyProduct> _propertyProductRepository;
        private readonly IRepository<OptionProduct> _optionProductRepository;
        private readonly ICartService _cartService;

        public CartController()
            : this(null, null, null, null, null)
        {
        }

        public CartController(
            ICartService cartService,
            IRepository<Product> productRepository = null,
            IRepository<PropertyProduct> propertyProductRepository = null,
            IRepository<OptionProduct> optionProductRepository = null,
            ApplicationDbContext context = null)
        {
            _context = context ?? new ApplicationDbContext();
            _productRepository = productRepository ?? new Repository<Product>(_context);
            _propertyProductRepository = propertyProductRepository ?? new Repository<PropertyProduct>(_context);
            _optionProductRepository = optionProductRepository ?? new Repository<OptionProduct>(_context);
            _cartService = cartService ?? new CartService();
        }

        public ActionResult Index()
        {
            var cart = _cartService.GetCartItems();
            return View(cart);
        }

        [HttpPost]
        public ActionResult AddToCart(int productId, int quantity, List<int> optionProductIds, int propertyId)
        {
            var product = _productRepository.GetById(productId);
            var propertyProduct = _propertyProductRepository.GetById(propertyId);
            List<OptionProduct> optionProducts = new List<OptionProduct>();
            if (optionProductIds != null)
            {
                foreach (var optionProductId in optionProductIds)
                {
                    optionProducts.Add(_optionProductRepository.GetById(optionProductId));
                }
            }
            if (quantity <= 0)
            {
                return Json(new { status = false, message = "Số lượng không hợp lệ" });
            }
            var item = new CartItem
            {
                ProductId = productId,
                Quantity = quantity,
                ProductName = product != null ? product.Name : string.Empty,
                Price = product != null ? product.Price : 0,
                Options = optionProducts,
                PropertyProduct = propertyProduct
            };
            _cartService.AddToCart(item);
            return Json(new { status = true, message = "Thêm vào giỏ hàng thành công" });
        }

        [HttpPost]
        public JsonResult UpdateCart(Guid itemId, int quantity)
        {
            if (!ModelState.IsValid || quantity < 1)
            {
                return Json(new { success = false, message = "Số lượng không hợp lệ" });
            }

            var cart = _cartService.GetCartItems();
            var item = cart.FirstOrDefault(x => x.Id == itemId);
            if (item == null)
            {
                return Json(new { success = false, message = "Item not found" });
            }

            item.Quantity = quantity;
            _cartService.UpdateCart(cart);

            return Json(new
            {
                success = true,
                quantity = item.Quantity,
                subtotal = _cartService.GetCartTotal(new[] { item }).ToString("N0") + "đ",
                total = _cartService.GetCartTotal(cart).ToString("N0") + "đ",
                totalQuantity = cart.Sum(x => x.Quantity)
            });
        }

        [HttpGet]
        public JsonResult GetTotal()
        {
            int total = 0;
            var cart = _cartService.GetCartItems();
            foreach (var item in cart)
            {
                total += item.Quantity;
            }
            return Json(new { success = true, total = total }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult RemoveFromCart(Guid cartItemId)
        {
            _cartService.RemoveFromCart(cartItemId);
            return Json(new { success = true });
        }

        [HttpPost]
        public ActionResult ClearCart()
        {
            _cartService.ClearCart();
            return Json(new { success = true });
        }
    }
}
