using Bookstore.DataAccess.Data;
using Bookstore.DataAccess.Repository.IRepository;
using Bookstore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookstore.DataAccess.Repository
{
    public class CompanyRepository : Repository<Company>, ICompanyRepository
    {
        private readonly ApplicationDbContext _db;

        public CompanyRepository(ApplicationDbContext db) : base(db) 
        {
            _db = db;
        }
        public void Update(Company company)
        {
            var companyFromDb = _db.Companies.Where(x => x.Id == company.Id);

            if(companyFromDb != null)
            {
                _db.Companies.Update(company);
            }
        }
    }
}
