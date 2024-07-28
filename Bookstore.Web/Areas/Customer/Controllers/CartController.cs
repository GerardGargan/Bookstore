using Bookstore.DataAccess.Repository.IRepository;
using Bookstore.Models;
using Bookstore.Models.ViewModels;
using Bookstore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookstoreWeb.Areas.Customer.Controllers
{
    [Area("customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
		[BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }

        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;   
        }
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM = new()
            {
                ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(x => x.UserId == userId, includeProperties: "Product"),
                OrderHeader = new()
            };

            foreach(var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += cart.Price * cart.Quantity;
            }
            
            return View(ShoppingCartVM);
        }

        public IActionResult Plus(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(x => x.Id == cartId);
            cartFromDb.Quantity +=1;
            _unitOfWork.ShoppingCart.Update(cartFromDb);
            _unitOfWork.Save();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(x => x.Id == cartId);
            if(cartFromDb.Quantity <= 1)
            {
                //remove
                _unitOfWork.ShoppingCart.Remove(cartFromDb);
            } else
            {
                //decrement and update
                cartFromDb.Quantity -= 1;
                _unitOfWork.ShoppingCart.Update(cartFromDb);
            }
            _unitOfWork.Save();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(x => x.Id == cartId);
           
            _unitOfWork.ShoppingCart.Remove(cartFromDb);
            _unitOfWork.Save();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM = new ShoppingCartVM()
            {
                ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(x => x.UserId == userId, includeProperties: "Product"),
                OrderHeader = new OrderHeader()
            };

            foreach(var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Quantity);
            }

            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.User.Get(x => x.Id == userId);

            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
            ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
            ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
            ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
            ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.County;
            ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;

            return View(ShoppingCartVM); 
        }

        [HttpPost]
        [ActionName("Summary")]
        public IActionResult SummaryPOST()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM.ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(x => x.UserId == userId, includeProperties: "Product");
            ShoppingCartVM.OrderHeader.OrderDate = System.DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId = userId;
            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.User.Get(x => x.Id == userId);

            foreach(var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Quantity);
                
            }

            if(ShoppingCartVM.OrderHeader.ApplicationUser.CompanyId.GetValueOrDefault() == 0)
            {
				// it is a regular customer account - need to capture payment
				ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
				ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
			} else
            {
                // company user - defer payment
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
            }
            //create the order header

            _unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
            _unitOfWork.Save();

            // create order details records

            foreach(var cart in ShoppingCartVM.ShoppingCartList)
            {
                OrderDetail orderDetail = new()
                {
                    ProductId = cart.ProductId,
                    OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
                    Price = cart.Price,
                    Quantity = cart.Quantity
                };
                _unitOfWork.OrderDetail.Add(orderDetail);
                _unitOfWork.Save();
            }

            
            return null;
        }

        private double GetPriceBasedOnQuantity(ShoppingCart shoppingCart)
        {
            if(shoppingCart.Quantity <= 50)
            {
                return shoppingCart.Product.Price;
            } else if(shoppingCart.Quantity <= 100)
            {
                return shoppingCart.Product.Price50;
            } else 
            {
                return shoppingCart.Product.Price100;
            }
        }
    }
}
