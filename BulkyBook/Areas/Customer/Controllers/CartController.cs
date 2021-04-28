using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BulkyBook.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<IdentityUser> _userManager;

        public ShoppingCartVM ShoppingCartVM { get; set; }

        public CartController(IUnitOfWork unitOfWork, IEmailSender emailSender, UserManager<IdentityUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
            _userManager = userManager;
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
    }
}
