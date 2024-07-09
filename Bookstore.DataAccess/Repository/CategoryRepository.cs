using Bookstore.DataAccess.Data;
using Bookstore.DataAccess.Repository.IRepository;
using Bookstore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Bookstore.DataAccess.Repository
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
       private readonly ApplicationDbContext _db;
        public CategoryRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Save()
        {
            _db.SaveChanges();
        }

        public void Update(Category category)
        {
            _db.Categories.Update(category);
        }
    }
}
