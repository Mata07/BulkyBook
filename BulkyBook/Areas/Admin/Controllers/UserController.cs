using BulkyBook.Data;
using BulkyBook.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BulkyBook.Areas.Admin.Controllers
{
    // Ovdje pokazuje način direktnom pristupanju ApplicationDbContext za User-e
    // Inače bi trebalo u produkcijskoj verziji nastaviti koristiti Repository Pattern
    // i IApplicationUserRepository za sve akcije (_unitOfWork.ApplicationUser)
    [Area("Admin")]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _db;

        public UserController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            return View();
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            // list of users
            var userList = _db.ApplicationUsers.Include(u => u.Company).ToList();
            // get mappings between user and roles
            var userRole = _db.UserRoles.ToList();
            // get roles
            var roles = _db.Roles.ToList();

            // for each user assign their role
            foreach (var user in userList)
            {
                // get RoleId
                var roleId = userRole.FirstOrDefault(u => u.UserId == user.Id).RoleId;

                // Role is a prop in ApplicationUser class
                // get name
                user.Role = roles.FirstOrDefault(u => u.Id == roleId).Name;
                if (user.Company == null)
                {
                    // to escape error Object Reference not found
                    user.Company = new Company()
                    {
                        Name = ""
                    };
                }
            }

            return Json(new { data = userList });
        }

        #endregion
    }
}
