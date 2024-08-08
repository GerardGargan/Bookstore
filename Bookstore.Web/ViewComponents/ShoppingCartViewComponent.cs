using Bookstore.DataAccess.Repository.IRepository;
using Bookstore.Utility;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookstoreWeb.ViewComponents
{
    public class ShoppingCartViewComponent : ViewComponent
    {
        private readonly IUnitOfWork _unitOfWork;
        public ShoppingCartViewComponent(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            ClaimsIdentity userClaim = (ClaimsIdentity)User.Identity;
            var claim = userClaim.FindFirst(ClaimTypes.NameIdentifier);


            if (claim != null)
            {
                if (HttpContext.Session.GetInt32(SD.SessionCart) == null)
                {
                    HttpContext.Session.SetInt32(SD.SessionCart, _unitOfWork.ShoppingCart.GetAll(x => x.UserId == claim.Value).Count());   
                }
                // set the session for the cart
                return View(HttpContext.Session.GetInt32(SD.SessionCart));
            } else
            {
                HttpContext.Session.Clear();
                return View(0);
            }
        }
    }
}
