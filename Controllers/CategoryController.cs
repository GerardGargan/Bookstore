﻿using Bookstore.Data;
using Bookstore.Models;
using Microsoft.AspNetCore.Mvc;

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
    }
}
