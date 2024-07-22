using Bookstore.DataAccess.Repository.IRepository;
using Bookstore.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

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
            IEnumerable<Product> products = _unitOfWork.Product.GetAll(includeProperties: "category");
            return View(products);
        }

        public IActionResult Details(int id)
        {
            ShoppingCart cart = new ShoppingCart() {
                Product = _unitOfWork.Product.Get(x => x.Id == id, includeProperties: "category"),
                Quantity = 1,
                ProductId = id
            };
            return View(cart);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
