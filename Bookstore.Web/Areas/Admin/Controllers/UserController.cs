using Bookstore.DataAccess.Data;
using Bookstore.DataAccess.Repository.IRepository;
using Bookstore.Models;
using Bookstore.Models.ViewModels;
using Bookstore.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

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
                CompanyList = _db.Companies.Select(x => new SelectListItem()
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                }),
                RoleId = roleId
            };

            return View(userRoleVM);
        }

        [HttpPost]
        [ActionName("RoleManagement")]
        public IActionResult RoleManagementPOST(UserRoleVM userRoleVM)
        {
            ApplicationUser user = _db.ApplicationUsers.Where(x => x.Id == userRoleVM.User.Id).FirstOrDefault();
           
            if(user == null)
            {
                TempData["error"] = "Cannot find user";
                RedirectToAction(nameof(Index));
            }
            var userRoleCurrent = _db.UserRoles.Where(x => x.UserId == user.Id).FirstOrDefault();

            // check if the user is a company user and they have selected a company
            var newRoleName = _db.Roles.Where(x => x.Id == userRoleVM.RoleId).FirstOrDefault().Name;
            if (newRoleName == SD.Role_Company && userRoleVM.User.CompanyId == null)
            {
                userRoleVM.CompanyList = _db.Companies.Select(x => new SelectListItem()
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                });
                userRoleVM.RoleList = _db.Roles.Select(x => new SelectListItem()
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                }).ToList();
                userRoleVM.RoleId = userRoleCurrent.RoleId;
                TempData["error"] = "Please select a company";
                return View(userRoleVM);
            }

            // remove current userRole record

            _db.UserRoles.Remove(userRoleCurrent);

            // create a new user role record
            var newUserRole = new IdentityUserRole<string>()
            {
                RoleId = userRoleVM.RoleId,
                UserId = user.Id
            };

            // update company
            if(newRoleName == SD.Role_Company)
            {
                user.CompanyId = userRoleVM.User.CompanyId;
            } else
            {
                user.CompanyId = null;
            }

            _db.UserRoles.Add(newUserRole);
            _db.SaveChanges();
            TempData["success"] = "User updated successfully";
            return RedirectToAction(nameof(Index));
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
