using Bookstore.Data;
using Bookstore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _db;
        public CategoryController(ApplicationDbContext db) {
            _db = db;
        }

        public IActionResult Index()
        {
            List<Category> objCategoryList = _db.Categories.OrderBy(x => x.DisplayOrder).ToList();
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
            _db.Categories.Add(category);
            _db.SaveChanges();
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
            Category? categoryFromDb = _db.Categories.AsNoTracking().Where(x => x.Id == id).FirstOrDefault();

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
                _db.Categories.Update(category);
                _db.SaveChanges();
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

            Category? categoryFromDb = _db.Categories.Where(x => x.Id == id).FirstOrDefault();

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

            Category? categoryToDelete = _db.Categories.Where(x => x.Id == id).FirstOrDefault();

            if(categoryToDelete == null)
            {
                return NotFound();
            }

            _db.Categories.Remove(categoryToDelete);
            _db.SaveChanges();

            return RedirectToAction("Index","Category");
        }
    }
}
