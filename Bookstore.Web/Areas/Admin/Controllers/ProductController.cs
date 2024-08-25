using Bookstore.DataAccess.Repository.IRepository;
using Bookstore.Models;
using Bookstore.Models.ViewModels;
using Bookstore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace BookstoreWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
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
                productVM.Product = _unitOfWork.Product.Get(x => x.Id == id, includeProperties: "ProductImages");
                return View(productVM);
            }

        }

        public IActionResult DeleteImage(int ImageId)
        {
            ProductImage image = _unitOfWork.ProductImage.Get(x => x.Id == ImageId);
            int productId = image.ProductId;
            if (image != null)
            {
                // delete from file system

                if (!string.IsNullOrEmpty(image.ImageUrl))
                {
                    var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, image.ImageUrl.TrimStart('\\'));
                    if(System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                // delete from db
                _unitOfWork.ProductImage.Remove(image);
                _unitOfWork.Save();
            }
            TempData["success"] = "Image deleted successfully";
            return RedirectToAction("Upsert", new { id = productId });
        }

        [HttpPost]
        public IActionResult Upsert(ProductVM productVM, List<IFormFile> files)
        {
            if (ModelState.IsValid)
            {
                if (productVM.Product.Id == 0)
                {
                    // handle create
                    _unitOfWork.Product.Add(productVM.Product);
                    TempData["Success"] = "Product added successfully";
                }
                else
                {
                    // handle update
                    _unitOfWork.Product.Update(productVM.Product);
                    TempData["Success"] = "Product updated successfully";
                }
                _unitOfWork.Save();


                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if(files != null)
                {

                    foreach(IFormFile file in files)
                    {
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        string productPath = @"images\products\product-"+ productVM.Product.Id;
                        string finalPath = Path.Combine(wwwRootPath, productPath);

                        if(!Directory.Exists(finalPath))
                        {
                            Directory.CreateDirectory(finalPath);
                        }

                        using (var fileStream = new FileStream(Path.Combine(finalPath, fileName), FileMode.Create))
                        {
                            file.CopyTo(fileStream);
                        }

                        ProductImage productImage = new ProductImage()
                        {
                            ProductId = productVM.Product.Id,
                            ImageUrl = @"\" + productPath + @"\" + fileName
                        };

                        if(productVM.Product.ProductImages == null)
                        {
                            productVM.Product.ProductImages = new List<ProductImage>();
                        }

                        productVM.Product.ProductImages.Add(productImage);

                    }

                    _unitOfWork.Product.Update(productVM.Product);
                    _unitOfWork.Save();

                }

                
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

            Product productToDelete = _unitOfWork.Product.Get(x => x.Id == id, includeProperties: "ProductImages");

            if(productToDelete == null)
            {
                return Json(new { success = false, message = "Product not found" });
            }

            string productPath = @"images\products\product-" + id;
            string finalPath = Path.Combine(_webHostEnvironment.WebRootPath, productPath);

            if(Directory.Exists(finalPath))
            {
                string[] files = Directory.GetFiles(finalPath);

                foreach(var file in files)
                {
                    System.IO.File.Delete(file);
                
                }

                Directory.Delete(finalPath);
            }

            _unitOfWork.Product.Remove(productToDelete);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Product deleted" });
        }

        #endregion
    }

}
