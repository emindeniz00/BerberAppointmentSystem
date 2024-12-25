using BerberAppointmentSystem.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BerberAppointmentSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonelVerimlilikController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PersonelVerimlilikController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("GunlukKazanc")]
        public IActionResult GetDailyEarnings(DateTime date)
        {
            var today = DateTime.Today;

            var dailyEarnings = _context.Appointments
                .Where(a => a.StartTime.Date == today && a.Onay == true) // Sadece bugünü filtrele
                .GroupBy(a => a.PersonelId) // PersonelId'ye göre gruplandır
                .Select(group => new
                {
                    PersonelId = group.Key,
                    Kazanç = group.Sum(a => a.Service.Fiyat ?? 0), // Fiyatı topla
                    PersonelAdSoyad = _context.Users
                        .Where(u => u.Id == group.FirstOrDefault().Personel.UserId)
                        .Select(u => u.Ad + " " + u.Soyad)
                        .FirstOrDefault()
                })
                .ToList();

            return Ok(dailyEarnings);

        }



        [HttpGet("GunlukDetay")]
        public IActionResult GetDailyDetails(int personelId, DateTime date)
        {
            var today = date.Date;

            var dailyDetails = _context.Appointments
                .Where(a => a.PersonelId == personelId && a.StartTime.Date == today && a.Onay == true)
                .Select(a => new
                {
                    AppointmentId = a.AppointmentId,
                    User = a.User,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime,
                    ServiceName = a.Service.ServisAdı,
                    Price = a.Service.Fiyat ?? 0
                })
                .ToList();

            return Ok(dailyDetails);
        }

    }
}