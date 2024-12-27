using BerberAppointmentSystem.Context;
using BerberAppointmentSystem.Models;
using BerberAppointmentSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BerberAppointmentSystem.Controllers
{
    [Authorize(Roles = "Customer")]
    public class AppointmentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AppointmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> UzmanlikSec()
        {
            var uzmanliklar = await _context.Uzmanliks.ToListAsync();
            return View(uzmanliklar);
        }

       

        public async Task<IActionResult> RandevuOlustur(int serviceId)
        {
            var service = _context.Services.Find(serviceId);

            var personeller = await _context.ServicePersonelAraTablos
                .Where(sp => sp.ServiceId == serviceId)
                .Include(sp => sp.User)
                .Select(sp => sp.User)
                .ToListAsync();

            var personnelWithDetails = await _context.Personels
               .Where(p => personeller.Select(u => u.Id).Contains(p.UserId))
               .ToListAsync();

            var model = new AppointmentViewModel
            {
                ServiceId = serviceId,
                Services = new List<SelectListItem>
    {
        new SelectListItem
        {
            Text = service.ServisAdı,
            Value = serviceId.ToString()
        }
    },
                Personels = personnelWithDetails.Select(p => new SelectListItem
                {
                    Text = $"{p.User.Ad} {p.User.Soyad}",
                    Value = p.PersonelId.ToString()
                }).ToList()
            };

            return View(model);

        }

        [HttpPost]
        public async Task<IActionResult> RandevuOlustur(AppointmentViewModel model)
        {


            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var serviceEntity = await _context.Services.FirstOrDefaultAsync(s => s.Id == model.ServiceId);
            var personel = await _context.Personels.FirstOrDefaultAsync(p => p.PersonelId == model.PersonelId);
            if (personel == null)
            {
                TempData["ErrorMessage"] = "Lütfen bilgileri doğru bir şekilde giriniz!";
                return RedirectToAction("UzmanlikSec");
            }
            var personelId = personel.UserId;

            var personelMesai = await _context.PersonelMesais
                .FirstOrDefaultAsync(pm => pm.UserId == personelId && pm.DayOfWeek == model.StartTime.DayOfWeek);

            if (personelMesai == null)
            {
                TempData["ErrorMessage"] = "Bu personelin seçilen günde mesaisi yok.";
                return RedirectToAction("UzmanlikSec");
            }

            if (model.StartTime.TimeOfDay < personelMesai.StartTime || model.StartTime.TimeOfDay + model.Duration > personelMesai.EndTime)
            {
                TempData["ErrorMessage"] = "Personelin çalışma saatleri dışında!";
                return RedirectToAction("UzmanlikSec");
            }

            var overlappingAppointments = await _context.Appointments
                .Where(a => a.PersonelId == model.PersonelId &&
                            a.StartTime <= model.StartTime.Add(model.Duration) &&
                            a.EndTime > model.StartTime)
                .ToListAsync();

            if (overlappingAppointments.Any())
            {
                TempData["ErrorMessage"] = "Personelin bu zamanda başka randevusu var!";
                return RedirectToAction("UzmanlikSec");
            }

            var appointment = new Appointment
            {
                ServiceId = model.ServiceId,
                UserId = Convert.ToInt32(userId),
                PersonelId = model.PersonelId,
                StartTime = model.StartTime,
                EndTime = model.StartTime.Add(serviceEntity.Sure),
                Onay = false,
                Ret = false
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Randevularim");
        }


        public IActionResult Randevularim()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var randevular = _context.Appointments
                .Where(r => r.UserId == Convert.ToInt32(userId))
                .Include(r => r.Service)
                .Include(r => r.Personel)
                .ThenInclude(r => r.User)
                .ToList();

            return View(randevular);
        }

        public async Task<IActionResult> GetHizmetler(int uzmanlikId)
        {
            var hizmetler = await _context.Services
                .Where(s => s.UzmanlikId == uzmanlikId)
                .ToListAsync();
            return PartialView("_HizmetlerPartial", hizmetler);
        }

        [HttpPost]
        public IActionResult RandevuSil(int id)
        {
            var randevu = _context.Appointments.Find(id);
            _context.Appointments.Remove(randevu);
            _context.SaveChanges();

            return RedirectToAction("Randevularim");
        }
    }
}
