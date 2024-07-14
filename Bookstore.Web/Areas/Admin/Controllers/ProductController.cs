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
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            List<Product> productList = _unitOfWork.Product.GetAll().ToList();
            
            return View(productList);
        }

        public IActionResult Upsert(int? id)
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

            if(id == null || id == 0)
            {
                // create
            return View(productVM);
            } else
            {
                //update
                productVM.Product = _unitOfWork.Product.Get(x => x.Id == id);
                return View(productVM);
            }

        }

        [HttpPost]
        public IActionResult Upsert(ProductVM productVM, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if(file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = Path.Combine(wwwRootPath, @"images\product");

                    using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }

                    productVM.Product.ImageUrl = @"\images\product\" + fileName;

                }

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
