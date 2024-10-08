﻿using Bookstore.DataAccess.Repository.IRepository;
using Bookstore.Models;
using Bookstore.Models.ViewModels;
using Bookstore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGeneration.DotNet;
using Stripe;
using Stripe.Checkout;
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

			IEnumerable<ProductImage> productImages = _unitOfWork.ProductImage.GetAll();

			foreach (var cart in ShoppingCartVM.ShoppingCartList)
			{
				cart.Product.ProductImages = productImages.Where(x => x.ProductId == cart.Product.Id).ToList();
				cart.Price = GetPriceBasedOnQuantity(cart);
				ShoppingCartVM.OrderHeader.OrderTotal += cart.Price * cart.Quantity;
			}

			return View(ShoppingCartVM);
		}

		public IActionResult Plus(int cartId)
		{
			var cartFromDb = _unitOfWork.ShoppingCart.Get(x => x.Id == cartId);
			cartFromDb.Quantity += 1;
			_unitOfWork.ShoppingCart.Update(cartFromDb);
			_unitOfWork.Save();

			return RedirectToAction(nameof(Index));
		}

		public IActionResult Minus(int cartId)
		{
			var cartFromDb = _unitOfWork.ShoppingCart.Get(x => x.Id == cartId, tracked: true);
			if (cartFromDb.Quantity <= 1)
			{
				//remove
                HttpContext.Session.SetInt32(SD.SessionCart, _unitOfWork.ShoppingCart.GetAll(x => x.UserId == cartFromDb.UserId).Count() - 1);
				_unitOfWork.ShoppingCart.Remove(cartFromDb);

            }
            else
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
			var cartFromDb = _unitOfWork.ShoppingCart.Get(x => x.Id == cartId, tracked: true);

			HttpContext.Session.SetInt32(SD.SessionCart, _unitOfWork.ShoppingCart.GetAll(x => x.UserId == cartFromDb.UserId).Count()-1);
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

			foreach (var cart in ShoppingCartVM.ShoppingCartList)
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
			ApplicationUser applicationUser = _unitOfWork.User.Get(x => x.Id == userId);

			foreach (var cart in ShoppingCartVM.ShoppingCartList)
			{
				cart.Price = GetPriceBasedOnQuantity(cart);
				ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Quantity);

			}

			if (applicationUser.CompanyId.GetValueOrDefault() == 0)
			{
				// it is a regular customer account - need to capture payment
				ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
				ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;

			}
			else
			{
				// company user - defer payment
				ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
				ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
			}
			//create the order header

			_unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
			_unitOfWork.Save();

			// create order details records

			foreach (var cart in ShoppingCartVM.ShoppingCartList)
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

			if (applicationUser.CompanyId.GetValueOrDefault() == 0)
			{
				// regular customer, process stripe payment
				var domain = Request.Scheme + "://" + Request.Host.Value + "/";
				var options = new Stripe.Checkout.SessionCreateOptions
				{
					SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
					CancelUrl = domain + $"customer/cart/index",
					LineItems = new List<Stripe.Checkout.SessionLineItemOptions>(),
					Mode = "payment",
				};

				foreach (var item in ShoppingCartVM.ShoppingCartList)
				{
					var sessionLineItem = new SessionLineItemOptions()
					{
						PriceData = new SessionLineItemPriceDataOptions()
						{
							UnitAmount = (long)(item.Price * 100),
							Currency = "gbp",
							ProductData = new SessionLineItemPriceDataProductDataOptions()
							{
								Name = item.Product.Title,
							}
						},
						Quantity = item.Quantity
					};
					options.LineItems.Add(sessionLineItem);
				}

				var service = new Stripe.Checkout.SessionService();
				Session session = service.Create(options);

				_unitOfWork.OrderHeader.UpdateStripeSessionId(ShoppingCartVM.OrderHeader.Id, session.Id);
				_unitOfWork.Save();

				Response.Headers.Add("Location", session.Url);
				return new StatusCodeResult(303);

			}

			// redirect to order confirmation


			return RedirectToAction(nameof(OrderConfirmation), new { id = ShoppingCartVM.OrderHeader.Id });
		}

		public IActionResult OrderConfirmation(int id)
		{
			OrderHeader orderHeader = _unitOfWork.OrderHeader.Get(x => x.Id == id, includeProperties: "ApplicationUser");

			if(orderHeader.PaymentStatus!=SD.PaymentStatusDelayedPayment)
			{
				//this is an order by customer
				//retrieve the stripe session

				var service = new SessionService();
				Session session = service.Get(orderHeader.SessionId);
				if(session.PaymentStatus.ToLower()=="paid")
				{
					_unitOfWork.OrderHeader.UpdateStripePaymentId(id, session.Id, session.PaymentIntentId);
					_unitOfWork.OrderHeader.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
					_unitOfWork.Save();
				}

				//empty users shopping cart
				List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCart
					.GetAll(x => x.UserId == orderHeader.ApplicationUserId).ToList();
				_unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
				_unitOfWork.Save();

				HttpContext.Session.Clear();

			}

			return View(id);
		}

		private double GetPriceBasedOnQuantity(ShoppingCart shoppingCart)
		{
			if (shoppingCart.Quantity <= 50)
			{
				return shoppingCart.Product.Price;
			}
			else if (shoppingCart.Quantity <= 100)
			{
				return shoppingCart.Product.Price50;
			}
			else
			{
				return shoppingCart.Product.Price100;
			}
		}
	}
}
