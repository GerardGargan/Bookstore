using Bookstore.DataAccess.Repository.IRepository;
using Bookstore.Models;
using Bookstore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookstoreWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
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

        [HttpPost]
        public IActionResult Upsert(Company company)
        {
            if(ModelState.IsValid)
            {
                if(company.Id == 0)
                {
                    //create
                    _unitOfWork.Company.Add(company);
                    TempData["success"] = "Company added";
                } else
                {
                    //update
                    _unitOfWork.Company.Update(company);
                    TempData["success"] = "Company updated";
                }
                _unitOfWork.Save();
                return RedirectToAction("Index");
            }
            else
            {
                return View(company);
            }
        }

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            List<Company> companyList = _unitOfWork.Company.GetAll().ToList();

            return Json(new { data = companyList });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            if(id == null || id == 0)
            {
                return Json(new { success = false, message = "Invalid company id"});
            } else
            {
                Company companyToDelete = _unitOfWork.Company.Get(x => x.Id == id);
                if(companyToDelete == null)
                {
                    return Json(new { success = false, message = "Company does not exist" }); 
                } else
                {
                    _unitOfWork.Company.Remove(companyToDelete);
                    _unitOfWork.Save();
                    return Json(new { success = true, message = "Company deleted" });
                }
            }
        }

        #endregion

    }
}
