using BerberAppointmentSystem.Context;
using BerberAppointmentSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BerberAppointmentSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RoleManagementController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<UserRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public RoleManagementController(UserManager<User> userManager, RoleManager<UserRole> roleManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> RoleChange()
        {
            var kullanicilar = _userManager.Users.ToList();

            var musteriler = new List<User>();

            foreach (var user in kullanicilar)
            {
                if (await _userManager.IsInRoleAsync(user, "Customer"))
                {
                    musteriler.Add(user);
                }
            }

            return View(musteriler);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCustomerToPersonel(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("Kullanıcı bulunamadı.");
            }

            var customerRole = await _roleManager.FindByNameAsync("Customer");
            var personelRole = await _roleManager.FindByNameAsync("Personel");

            if (customerRole == null || personelRole == null)
            {
                return BadRequest("Customer veya Personel rolü bulunamadı.");
            }

            if (await _userManager.IsInRoleAsync(user, "Customer"))
            {
                await _userManager.RemoveFromRoleAsync(user, "Customer");

                await _userManager.AddToRoleAsync(user, "Personel");

                var newPersonel = new Personel
                {
                    UserId = user.Id,
                };

                _context.Personels.Add(newPersonel);
                await _context.SaveChangesAsync();


                return Ok($"User {user.UserName} is now a Personel.");
            }

            return BadRequest("User is not in the Customer role.");
        }
    }
}
