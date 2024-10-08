using Bookstore.DataAccess.Repository.IRepository;
using Bookstore.Models;
using Bookstore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace BookstoreWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            
            IEnumerable<Product> products = _unitOfWork.Product.GetAll(includeProperties: "category,ProductImages");
            return View(products);
        }

        public IActionResult Details(int productId)
        {
            ShoppingCart cart = new ShoppingCart() {
                Product = _unitOfWork.Product.Get(x => x.Id == productId, includeProperties: "category,ProductImages"),
                Quantity = 1,
                ProductId = productId
            };
            return View(cart);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            // get the user id
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            shoppingCart.UserId = userId;

            ShoppingCart cartFromDb = _unitOfWork.ShoppingCart.Get(x => x.UserId == userId && x.ProductId == shoppingCart.ProductId);

            if(cartFromDb != null)
            {
                // cart exists with this product already, update
                cartFromDb.Quantity += shoppingCart.Quantity;
                _unitOfWork.ShoppingCart.Update(cartFromDb);
                _unitOfWork.Save();
            } else
            {
                // add new cart
                _unitOfWork.ShoppingCart.Add(shoppingCart);
                _unitOfWork.Save();
                HttpContext.Session.SetInt32(SD.SessionCart, 
                _unitOfWork.ShoppingCart.GetAll(x => x.UserId == userId).Count());
            }


            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }

    }
}
