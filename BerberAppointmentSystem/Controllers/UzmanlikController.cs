using BerberAppointmentSystem.Context;
using BerberAppointmentSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BerberAppointmentSystem.Controllers
{
    public class UzmanlikController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UzmanlikController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var uzmanliklar = await _context.Uzmanliks.ToListAsync();
            return View(uzmanliklar);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Uzmanlik uzmanlik)
        {

            _context.Uzmanliks.Add(uzmanlik);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }

        public async Task<IActionResult> AddService(int uzmanlikId)
        {
            var uzmanlik = await _context.Uzmanliks
                .FirstOrDefaultAsync(u => u.Id == uzmanlikId);

            if (uzmanlik == null)
            {
                return NotFound();
            }

            var model = new Service
            {
                UzmanlikId = uzmanlikId
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddService(Service service)
        {

            _context.Services.Add(service);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }
    }
}
