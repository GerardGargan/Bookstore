using Bookstore.DataAccess.Repository.IRepository;
using Bookstore.Models;
using Microsoft.AspNetCore.Mvc;

namespace BookstoreWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UserController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        #region

        [HttpGet]
        public IActionResult GetAll()
        {
            List<ApplicationUser> allUsers = _unitOfWork.User.GetAll().ToList();
            return Json(new { data = allUsers });
        }

        #endregion

    }
}
