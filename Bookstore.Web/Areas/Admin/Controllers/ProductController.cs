using Bookstore.DataAccess.Repository.IRepository;
using Bookstore.Models;
using Bookstore.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace BookstoreWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ProductController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            List<Product> productList = _unitOfWork.Product.GetAll().ToList();
            
            return View(productList);
        }

        public IActionResult Create()
        {

            ProductVM productVM = new ProductVM()
            {
                Product = new Product(),
                CategoryList = _unitOfWork.Category
                .GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString(),
                })
            };

            return View(productVM);
        }

        [HttpPost]
        public IActionResult Create(ProductVM productVM)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Product.Add(productVM.Product);
                _unitOfWork.Save();
                TempData["Success"] = "Product added successfully";
                return RedirectToAction("Index", "Product");
            } else
            {
                //Need to generate the category list for re-rendering
                productVM.CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                });
            }

            return View(productVM);
        }

        public IActionResult Edit(int? id)
        {
            if(id == null || id <= 0)
            {
                TempData["Error"] = "Product not found";
                return RedirectToAction("Index", "Product");
            }

            Product product = _unitOfWork.Product.Get(x => x.Id == id);
            if(product == null)
            {
                TempData["Error"] = "Product not found";
                return RedirectToAction("Index", "Product");
            }

            return View(product);
        }

        [HttpPost]
        public IActionResult Edit(Product product)
        {
            if(ModelState.IsValid)
            {
                _unitOfWork.Product.Update(product);
                _unitOfWork.Save();
                TempData["Success"] = "Product updated";
                return RedirectToAction("index", "product");
            }

            return View();
        }

        public IActionResult Delete(int? id)
        {
            if(id == null || id <= 0)
            {
                TempData["Error"] = "Product not found";
                return RedirectToAction("Index", "Product");
            }

            Product? product = _unitOfWork.Product.Get(x => x.Id == id);

            if(product == null)
            {
                TempData["Error"] = "Product not found";
                return RedirectToAction("Index", "Product");
            }

            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePOST(int? id)
        {
            if(id == null || id <= 0)
            {
                TempData["Error"] = "Invalid product";
                return RedirectToAction("Index", "Product");
            }

            Product? product = _unitOfWork.Product.Get(x => x.Id == id);
            if(product == null)
            {
                TempData["Error"] = "Invalid product";
                return RedirectToAction("Index", "Product");
            }

            _unitOfWork.Product.Remove(product);
            _unitOfWork.Save();
            TempData["Success"] = "Successfully deleted product";
            return RedirectToAction("Index", "Product");
        }
    }
}
