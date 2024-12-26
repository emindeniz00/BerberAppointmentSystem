using BerberAppointmentSystem.Context;
using BerberAppointmentSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BerberAppointmentSystem.Controllers
{
    public class PersonelMesaiController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<UserRole> _roleManager;

        public PersonelMesaiController(ApplicationDbContext context, UserManager<User> userManager, RoleManager<UserRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var personeller = await _context.Personels
                .Where(p => p.UserId != null)
                .Select(p => new
                {
                    p.PersonelId,
                    p.UserId,
                    AdSoyad = p.User.Ad + " " + p.User.Soyad
                }).ToListAsync();

            return View(personeller);
        }

        public IActionResult Create(int userId)
        {
            ViewBag.UserId = userId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PersonelMesai model)
        {

            bool mesaiVarMi = await _context.PersonelMesais
                .AnyAsync(m => m.UserId == model.UserId && m.DayOfWeek == model.DayOfWeek);

            if (mesaiVarMi)
            {
                ModelState.AddModelError(string.Empty, "Bu personelin belirtilen gün için zaten bir mesai kaydı var.");
                ViewBag.UserId = model.UserId;
                return View(model);
            }

            if (ModelState.IsValid)
            {
                _context.PersonelMesais.Add(model);
                await _context.SaveChangesAsync();

                TempData["Mesaj"] = "Mesai başarıyla oluşturuldu.";

                return RedirectToAction(nameof(ListMesai));
            }

            ViewBag.UserId = model.UserId;
            return View(model);

        }

        public async Task<IActionResult> ListMesai()
        {
            var mesaiList = await _context.PersonelMesais.
                Include(p => p.User).
                ToListAsync();

            return View(mesaiList);
        }


    }
}


