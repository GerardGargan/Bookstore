using Bookstore.DataAccess.Data;
using Bookstore.DataAccess.Repository.IRepository;
using Bookstore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ICategoryRepository _categoryRepo;
        public CategoryController(ICategoryRepository categoryRepo) {
            _categoryRepo = categoryRepo;
        }

        public IActionResult Index()
        {
            List<Category> objCategoryList = _categoryRepo.GetAll().ToList();
            return View(objCategoryList);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Category category)
        {
            if(category.Name!=null && category.Name.ToLower() == "test")
            {
                ModelState.AddModelError("", "Test is not allowed as a category name");
            }

            if(ModelState.IsValid)
            {
            _categoryRepo.Add(category);
                _categoryRepo.Save();
                TempData["success"] = "Category added successfully";
            return RedirectToAction("Index","Category");
            }

            return View();
        }

        public IActionResult Edit(int? id)
        {
            if(id == null || id ==0)
            {
                return NotFound();
            }
            Category? categoryFromDb = _categoryRepo.Get(x => x.Id == id);

            if(categoryFromDb == null)
            {
                return NotFound();
            }

            return View(categoryFromDb);
        }

        [HttpPost]
        public IActionResult Edit(Category category)
        {
            if(ModelState.IsValid)
            {
                _categoryRepo.Update(category);
                _categoryRepo.Save();
                TempData["success"] = "Category updated successfully";
                return RedirectToAction("Index","Category");
            }

            return View();
        }

        public IActionResult Delete(int? id)
        {
            if(id == null || id ==0)
            {
                return NotFound();
            }

            Category? categoryFromDb = _categoryRepo.Get(x => x.Id == id);

            if(categoryFromDb==null)
            {
                return NotFound();
            }

            return View(categoryFromDb);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePOST(int? id)
        {
            if(id==null || id ==0)
            {
                return NotFound();
            }

            Category? categoryToDelete = _categoryRepo.Get(x => x.Id == id);

            if(categoryToDelete == null)
            {
                return NotFound();
            }

            _categoryRepo.Remove(categoryToDelete);
            _categoryRepo.Save();
            TempData["success"] = "Category deleted successfully";

            return RedirectToAction("Index","Category");
        }
    }
}
