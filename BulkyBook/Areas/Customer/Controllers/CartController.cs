using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace BulkyBook.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<IdentityUser> _userManager;

        // Add for Twilio SMS
        private TwilioSettings _twilioOptions { get; set; }

        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }

        public CartController(IUnitOfWork unitOfWork, IEmailSender emailSender,
                UserManager<IdentityUser> userManager, IOptions<TwilioSettings> twilioOptions)
        {
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
            _userManager = userManager;
            _twilioOptions = twilioOptions.Value;
        }

        public IActionResult Index()
        {
            // get UserId of logged-in user
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM = new ShoppingCartVM()
            {
                OrderHeader = new Models.OrderHeader(),
                // get all items for loggedin user
                ListCart = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value, includeProperties: "Product")
            };

            // calculate orderTotal in View
            ShoppingCartVM.OrderHeader.OrderTotal = 0;
            // get User with Company
            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.
                                                            GetFirstOrDefault(u => u.Id == claim.Value,
                                                            includeProperties: "Company");

            foreach (var list in ShoppingCartVM.ListCart)
            {
                // Get Price based on quantity of selected product
                list.Price = SD.GetPriceBasedOnQuantity(list.Count, list.Product.Price,
                                                        list.Product.Price50, list.Product.Price100);
                // Set OrderTotal
                ShoppingCartVM.OrderHeader.OrderTotal += (list.Price * list.Count);
                // Convert Description as RawHTML
                list.Product.Description = SD.ConvertToRawHtml(list.Product.Description);
                // Shorten Description to 100 chars
                if (list.Product.Description.Length > 100)
                {
                    list.Product.Description = list.Product.Description.Substring(0, 99) + "...";
                }
            }

            return View(ShoppingCartVM);
        }

        [HttpPost]
        [ActionName("Index")]
        public async Task<IActionResult> IndexPOST()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var user = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);

            if (user == null)
            {
                // Display Error if user is empty
                ModelState.AddModelError(string.Empty, "Verification email is empty!");
            }

            // send Confirmation email
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId = user.Id, code = code },
                protocol: Request.Scheme);

            await _emailSender.SendEmailAsync(user.Email, "Confirm your email",
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

            ModelState.AddModelError(string.Empty, "Verification email sent. Please check your email.");
            return RedirectToAction("Index");
        }

        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM = new ShoppingCartVM()
            {
                OrderHeader = new Models.OrderHeader(),
                ListCart = _unitOfWork.ShoppingCart.GetAll(c => c.ApplicationUserId == claim.Value,
                                                            includeProperties: "Product")
            };

            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser
                                                            .GetFirstOrDefault(c => c.Id == claim.Value,
                                                            includeProperties: "Company");
            foreach (var list in ShoppingCartVM.ListCart)
            {
                // Get Price based on quantity of selected product
                list.Price = SD.GetPriceBasedOnQuantity(list.Count, list.Product.Price,
                                                        list.Product.Price50, list.Product.Price100);
                // Set OrderTotal
                ShoppingCartVM.OrderHeader.OrderTotal += (list.Price * list.Count);
                // Convert Description as RawHTML
                list.Product.Description = SD.ConvertToRawHtml(list.Product.Description);                
            }

            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
            ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
            ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
            ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
            ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;
            ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;

            return View(ShoppingCartVM);
        }

        [HttpPost]
        [ActionName("Summary")]
        [ValidateAntiForgeryToken]
        public IActionResult SummaryPost(string stripeToken)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser
                                                            .GetFirstOrDefault(c => c.Id == claim.Value,
                                                            includeProperties: "Company");

            ShoppingCartVM.ListCart = _unitOfWork.ShoppingCart.GetAll(c => c.ApplicationUserId == claim.Value,
                                                                includeProperties: "Product");

            ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
            ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
            ShoppingCartVM.OrderHeader.ApplicationUserId = claim.Value;
            ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;

            _unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
            _unitOfWork.Save();

            //List<OrderDetails> orderDetailsList = new List<OrderDetails>();
            foreach (var item in ShoppingCartVM.ListCart)
            {
                // calculate Price
                item.Price = SD.GetPriceBasedOnQuantity(item.Count, item.Product.Price,
                    item.Product.Price50, item.Product.Price100);

                OrderDetails orderDetails = new OrderDetails()
                {
                    ProductId = item.ProductId,
                    OrderId = ShoppingCartVM.OrderHeader.Id,
                    Price = item.Price,
                    Count = item.Count
                };
                ShoppingCartVM.OrderHeader.OrderTotal += orderDetails.Count * orderDetails.Price;
                _unitOfWork.OrderDetails.Add(orderDetails);
                //_unitOfWork.Save();
            }

            // remove from shopping cart and from session (for Home/Index view)
            _unitOfWork.ShoppingCart.RemoveRange(ShoppingCartVM.ListCart);
            _unitOfWork.Save();
            HttpContext.Session.SetInt32(SD.ssShoppingCart, 0);

            if (stripeToken == null)
            {
                // it's null for AuthorizedCompany User - they can place an order without payment(payment on delivery)
                // order will be created for delayed payment for authorized company
                // can be paid in 30 days
                ShoppingCartVM.OrderHeader.PaymentDueDate = DateTime.Now.AddDays(30);
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
            }
            else
            {
                // process the payment using Stripe
                // 
                var options = new ChargeCreateOptions
                {
                    Amount = Convert.ToInt32(ShoppingCartVM.OrderHeader.OrderTotal * 100),
                    Currency = "usd",
                    Description = "Order ID: " + ShoppingCartVM.OrderHeader.Id,
                    Source = stripeToken
                };

                var service = new ChargeService();
                Charge charge = service.Create(options);
                if (charge.Id == null)
                {
                    ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusRejected;
                }
                else
                {
                    ShoppingCartVM.OrderHeader.TransactionId = charge.Id;
                }
                if (charge.Status.ToLower() == "succeded")
                {
                    ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusApproved;
                    ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
                    ShoppingCartVM.OrderHeader.PaymentDate = DateTime.Now;
                }
            }

            _unitOfWork.Save();

            return RedirectToAction("OrderConfirmation", "Cart", new { id = ShoppingCartVM.OrderHeader.Id });
        }

        public IActionResult OrderConfirmation(int id)
        {
            // Setup for Twilio SMS - does not work, Twilio changed settings
            //OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == id);
            //TwilioClient.Init(_twilioOptions.AccountSid, _twilioOptions.AuthToken);
            //try
            //{
            //    var message = MessageResource.Create(
            //        body: "Order Placed on Bulky Book. Your Order ID: " + id,
            //        from: new Twilio.Types.PhoneNumber(_twilioOptions.PhoneNumber),
            //        to: new Twilio.Types.PhoneNumber(orderHeader.PhoneNumber)
            //        );
            //}
            //catch (Exception ex)
            //{
            //    String.Format("Wrong Phone Number. More Details: {0}", ex.Message );
            //}

            // if we want to display everything from order
            // we can get OrderHeader and OrderDetails from id
            return View(id);
        }


        public IActionResult Plus(int cartId)
        {
            // get from db
            var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(c => c.Id == cartId,
                                                    includeProperties: "Product");
            // increment every time
            cart.Count += 1;

            // change Price based on new quantity
            cart.Price = SD.GetPriceBasedOnQuantity(cart.Count, cart.Product.Price,
                                    cart.Product.Price50, cart.Product.Price100);
            _unitOfWork.Save();

            return RedirectToAction(nameof(Index));
        }


        public IActionResult Minus(int cartId)
        {
            // get from db
            var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(c => c.Id == cartId,
                                                    includeProperties: "Product");

            // if it is the last one item remove it from Cart
            if (cart.Count == 1)
            {
                // get total count
                var cnt = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == cart.ApplicationUserId).ToList().Count;
                // remove from cart
                _unitOfWork.ShoppingCart.Remove(cart);
                _unitOfWork.Save();

                // remove from Session
                HttpContext.Session.SetInt32(SD.ssShoppingCart, cnt - 1);
            }
            else
            {
                // decrement every time
                cart.Count -= 1;

                // change Price based on new quantity
                cart.Price = SD.GetPriceBasedOnQuantity(cart.Count, cart.Product.Price,
                                        cart.Product.Price50, cart.Product.Price100);
                _unitOfWork.Save();
            }

            return RedirectToAction(nameof(Index));
        }


        public IActionResult Remove(int cartId)
        {
            // get from db
            var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(c => c.Id == cartId,
                                                    includeProperties: "Product");

            // get total count
            var cnt = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == cart.ApplicationUserId).ToList().Count;
            // remove from cart
            _unitOfWork.ShoppingCart.Remove(cart);
            _unitOfWork.Save();

            // remove from Session
            HttpContext.Session.SetInt32(SD.ssShoppingCart, cnt - 1);

            return RedirectToAction(nameof(Index));
        }

    }
}
