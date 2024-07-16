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
            List<Product> productList = _unitOfWork.Product.GetAll(includeProperties: "category").ToList();
            
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

                    if (!string.IsNullOrEmpty(productVM.Product.ImageUrl))
                    {
                        // delete the old image, new image has been provided
                        var oldImagePath = Path.Combine(wwwRootPath, productVM.Product.ImageUrl.TrimStart('\\'));

                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }

                    productVM.Product.ImageUrl = @"\images\product\" + fileName;

                }

                if(productVM.Product.Id == 0)
                {
                    // handle create
                    _unitOfWork.Product.Add(productVM.Product);
                    TempData["Success"] = "Product added successfully";
                } else
                {
                    // handle update
                    _unitOfWork.Product.Update(productVM.Product);
                    TempData["Success"] = "Product updated successfully";
                }
                _unitOfWork.Save();
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


        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            List<Product> allProducts = _unitOfWork.Product.GetAll(includeProperties: "category").ToList();

            return Json(new { data = allProducts });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            if(id == null || id == 0)
            {
                return Json(new { success = false, message = "Invalid id" });
            }

            Product productToDelete = _unitOfWork.Product.Get(x => x.Id == id);

            if(productToDelete == null)
            {
                return Json(new { success = false, message = "Product not found" });
            }

            // delete any existing image

            string imagePathToDelete = Path.Combine(_webHostEnvironment.WebRootPath, productToDelete.ImageUrl.TrimStart('\\'));

            if (System.IO.File.Exists(imagePathToDelete))
            {
                System.IO.File.Delete(imagePathToDelete);
            }

            // delete the product record

            _unitOfWork.Product.Remove(productToDelete);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Product deleted" });
        }

        #endregion
    }

}
