using Bookstore.DataAccess.Data;
using Bookstore.DataAccess.Repository.IRepository;
using Bookstore.Models;
using Bookstore.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookstoreWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UserController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _db;

        public UserController(IUnitOfWork unitOfWork, ApplicationDbContext db)
        {
            _unitOfWork = unitOfWork;
            _db = db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult RoleManagement(string userId)
        {
            ApplicationUser user = _unitOfWork.User.Get(x => x.Id == userId);
            
            if(user == null || userId == null)
            {
                return NotFound();
            }
            var roleId = _db.UserRoles.FirstOrDefault(x => x.UserId == userId).RoleId;
            var roleName = _db.Roles.FirstOrDefault(x => x.Id == roleId).Name;

            user.Role = roleName;


            UserRoleVM userRoleVM = new UserRoleVM()
            {
                User = user,
                RoleList = _db.Roles.Select(x => new SelectListItem()
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                }).ToList(),
                RoleId = roleId
            };

            return View(userRoleVM);
        }

        #region API

        [HttpGet]
        public IActionResult GetAll()
        {
            List<ApplicationUser> allUsers = _unitOfWork.User.GetAll(includeProperties: "Company").ToList();
            // get all roles and user roles
            var userRoles = _db.UserRoles.ToList();
            var roles = _db.Roles.ToList();

            foreach(var user in allUsers)
            {
                var roleId = userRoles.FirstOrDefault(x => x.UserId == user.Id).RoleId;
                user.Role = roles.FirstOrDefault(x => x.Id == roleId).Name;

                if(user.Company == null)
                {
                    user.Company = new Company() { Name = "" };
                }
            }

            return Json(new { data = allUsers });
        }

        [HttpPost]
        public IActionResult LockUnlock([FromBody] string id)
        {
            ApplicationUser user = _db.ApplicationUsers.FirstOrDefault(x => x.Id == id);

            if(user == null)
            {
                return Json(new { success = false, message = "Error while Locking/Unlocking" });
            }

            if (user.LockoutEnd != null && user.LockoutEnd > DateTime.Now)
            {
                // user is currently locked, we will unlock them
                user.LockoutEnd = DateTime.Now;
            } else
            {
                // lock the user
                user.LockoutEnd = DateTime.Now.AddYears(1000);
            }

            _db.SaveChanges();

            return Json(new { success = true, message = "User updated" });
        }

        #endregion

    }
}
