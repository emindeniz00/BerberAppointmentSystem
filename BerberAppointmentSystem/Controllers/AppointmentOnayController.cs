using BerberAppointmentSystem.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BerberAppointmentSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AppointmentOnayController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AppointmentOnayController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult GelenTalepler()
        {

            var gelenTalepler = _context.Appointments
       .Where(r => !r.Onay && !r.Ret)
       .Include(r => r.User)
              .Include(r => r.Service)

       .Include(r => r.Personel)
           .ThenInclude(p => p.User)
       .ToList();

            return View(gelenTalepler);
        }

        [HttpPost]
        public IActionResult Onayla(int randevuId)
        {
            var appointment = _context.Appointments.FirstOrDefault(r => r.AppointmentId == randevuId);



            appointment.Onay = true;
            _context.SaveChanges();

            return RedirectToAction("GelenTalepler");
        }

        [HttpPost]
        public IActionResult IptalEt(int randevuId)
        {
            var appointment = _context.Appointments.FirstOrDefault(r => r.AppointmentId == randevuId);


            appointment.Ret = true;
            _context.SaveChanges();

            return RedirectToAction("GelenTalepler");
        }

        public IActionResult OnaylanmisRandevular()
        {
            var appointments = _context.Appointments.Where(x => x.Onay == true && x.Ret == false)
                  .Include(r => r.Service)
        .Include(r => r.User)
        .Include(r => r.Personel)
        .ThenInclude(p => p.User)
                .ToList();
            return View(appointments);
        }
    }
}
