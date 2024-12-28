using BerberAppointmentSystem.Context;
using BerberAppointmentSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BerberAppointmentSystem.Controllers
{
    [Authorize(Roles = "Admin")]
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

            // Servisi ekle
            _context.Services.Add(service);
            await _context.SaveChangesAsync();

            // Uzmanlık ile ilişkili personelleri bul
            var ilgiliPersoneller = await _context.Users
                .Where(u => u.ServicePersonelAraTablos.Any(sp => sp.UserId == u.Id) &&
                            u.ServicePersonelAraTablos.Any(sp => sp.Service.UzmanlikId == service.UzmanlikId))
                .ToListAsync();

            // Her bir personel için ServicePersonelAraTablo kaydı oluştur
            foreach (var personel in ilgiliPersoneller)
            {
                var araTabloKaydi = new ServicePersonelAraTablo
                {
                    ServiceId = service.Id,
                    UserId = personel.Id
                };

                _context.ServicePersonelAraTablos.Add(araTabloKaydi);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));

        }

        public async Task<IActionResult> Services()
        {

            var servisler = await _context.Services
           .Include(s => s.Uzmanlik) // Uzmanlik'ı da dahil et
           .ToListAsync();

            return View(servisler);
        }

        // Servis düzenleme (Edit)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var service = await _context.Services.FindAsync(id);

            if (service == null)
            {
                return NotFound();
            }

            ViewData["UzmanlikId"] = new SelectList(_context.Uzmanliks, "Id", "UzmanlikAd", service.UzmanlikId);
            return View(service);
        }

        // Servis düzenleme (Edit) işlemi POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ServisAdı,UzmanlikId,Fiyat,Sure")] Service service)
        {
            if (id != service.Id)
            {
                return NotFound();
            }

            try
            {
                _context.Update(service);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ServiceExists(service.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Services));


            //ViewData["UzmanlikId"] = new SelectList(_context.Uzmanliks, "Id", "UzmanlikAd", service.UzmanlikId);
        }


        public async Task<IActionResult> Delete(int id)
        {
            var service = await _context.Services
                .Include(s => s.Appointments) // İlgili appointment'ları yükle
                .FirstOrDefaultAsync(s => s.Id == id);

            if (service == null)
            {
                return NotFound();
            }

            // Eğer servisin randevuları varsa, onları silmeden servisi silemeyebiliriz.
            // Burada randevuları silme işlemi yapılabilir, veya kullanıcıya uyarı verilebilir.

            _context.Services.Remove(service);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index)); // Servislerin listelendiği sayfaya yönlendir.
        }

        private bool ServiceExists(int id)
        {
            return _context.Services.Any(e => e.Id == id);
        }
    }
}
