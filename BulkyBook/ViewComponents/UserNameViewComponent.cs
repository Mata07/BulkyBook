using BulkyBook.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BulkyBook.ViewComponents
{
    // ViewComponents renders a chunk rather than a whole response
    // It is used for Layout or partial pages where we need to get something from the model
    // or need some kind of business logic
    // Naming: Add ViewComponent to name of class OR implements Microsoft.AspNetCore.Mvc.ViewComponent
    // 2. Create  Partial View in Views/Shared/Components/UserName - Default
    // 3. Consume in _LoginPartial - @await Component.InvokeAsync("UserName")
    public class UserNameViewComponent : ViewComponent
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserNameViewComponent(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Get User Name from Db and Display in _Layout View for User Name       
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var userFromDb =  _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claims.Value);

            return View(userFromDb);
        }
    }
}
