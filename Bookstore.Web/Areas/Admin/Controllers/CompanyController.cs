using Bookstore.DataAccess.Repository.IRepository;
using Bookstore.Models;
using Microsoft.AspNetCore.Mvc;

namespace BookstoreWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            List<Company> companyList = _unitOfWork.Company.GetAll().ToList();

            return View(companyList);
        }

        public IActionResult Upsert(int? id)
        {
            if (id == null || id == 0)
            {
                Company emptyCompany = new Company();
                return View(emptyCompany);
            }

            Company company = _unitOfWork.Company.Get(x => x.Id == id);

            if (company == null)
            {
                Company emptyCompany = new Company();
                return View(emptyCompany);
            }

            return View(company);
        }

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            List<Company> companyList = _unitOfWork.Company.GetAll().ToList();

            return Json(new { data = companyList });
        }

        #endregion

    }
}
