using BerberAppointmentSystem.Context;
using BerberAppointmentSystem.Models;
using BerberAppointmentSystem.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;


namespace BerberAppointmentSystem.Controllers
{
    public class PersonelManagementController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _dbContext;
        private readonly HttpClient _httpClient;


        public PersonelManagementController(UserManager<User> userManager, ApplicationDbContext context, HttpClient httpClient)
        {
            _userManager = userManager;
            _dbContext = context;
            _httpClient = httpClient;
        }

        [HttpGet]
        public async Task<IActionResult> AssignExpertise()
        {
            var allUsers = _userManager.Users.ToList();
            var personelUsers = new List<User>();

            foreach (var user in allUsers)
            {
                if (await _userManager.IsInRoleAsync(user, "Personel"))
                {
                    var isExpertAssigned = _dbContext.Personels.Any(p => p.UserId == user.Id && p.UzmanlikId != null);

                    if (!isExpertAssigned)
                    {
                        personelUsers.Add(user);
                    }
                }
            }

            if (!personelUsers.Any())
            {
                ViewBag.Message = "Tüm personellere uzmanlık atanmış.";
            }
            else
            {
                ViewBag.Message = null;
            }

            ViewBag.ExpertiseList = _dbContext.Uzmanliks.ToList();
            ViewBag.ServiceList = _dbContext.Services.ToList();
            return View(personelUsers);
        }



        // Uzmanlık Belirleme ve Ara Tabloya Ekleme
        [HttpPost]
        public async Task<IActionResult> AssignExpertise(int userId, int expertiseId)
        {
            var relatedServices = _dbContext.Services.Where(s => s.UzmanlikId == expertiseId).ToList();

            if (!relatedServices.Any())
            {
                return BadRequest("Seçilen uzmanlığa ait hizmet bulunamadı.");
            }

            var personnel = _dbContext.Personels.FirstOrDefault(p => p.UserId == userId);
            if (personnel == null)
            {
                personnel = new Personel
                {
                    UserId = userId,
                    UzmanlikId = expertiseId
                };
                _dbContext.Personels.Add(personnel);
            }
            else
            {
                personnel.UzmanlikId = expertiseId;
            }
            await _dbContext.SaveChangesAsync();

            // Ara Tabloya Hizmet ve Personel Ekleme
            foreach (var service in relatedServices)
            {
                var existingRecord = _dbContext.ServicePersonelAraTablos
                    .FirstOrDefault(s => s.UserId == userId && s.ServiceId == service.Id);

                if (existingRecord == null)
                {
                    _dbContext.ServicePersonelAraTablos.Add(new ServicePersonelAraTablo
                    {
                        UserId = userId,
                        ServiceId = service.Id
                    });
                }
            }
            await _dbContext.SaveChangesAsync();

            return RedirectToAction("AssignExpertise");
        }

        // Personelleri listeleme
        [HttpGet]
        public async Task<IActionResult> ListAllPersonnel()
        {
            var allPersonnel = await _dbContext.Personels
                .Include(p => p.User)
                .Include(p => p.Uzmanlik)
                .ToListAsync();

            return View(allPersonnel);
        }

        // Personel Silme
        [HttpPost]
        public async Task<IActionResult> DeletePersonnel(int personelId)
        {
            var personel = await _dbContext.Personels.Include(p => p.User).FirstOrDefaultAsync(p => p.PersonelId == personelId);

            if (personel == null)
            {
                ModelState.AddModelError("", "Personel kaydı bulunamadı.");
                return RedirectToAction("ListAllPersonnel");
            }

            var user = personel.User;
            if (user != null)
            {
                var result = await _userManager.DeleteAsync(user);

                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", "Kullanıcı silinirken bir hata oluştu.");
                    return RedirectToAction("ListAllPersonnel");
                }
            }

            _dbContext.Personels.Remove(personel);
            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                ModelState.AddModelError("", "Kaydı silerken bir hata oluştu. Kayıt başka bir işlem sırasında silinmiş olabilir.");
                return RedirectToAction("ListAllPersonnel");
            }

            return RedirectToAction("ListAllPersonnel");
        }

        [HttpGet]
        public async Task<IActionResult> EditExpertise(int personelId)
        {
            var personel = await _dbContext.Personels.Include(p => p.User).Include(p => p.Uzmanlik)
                                                   .FirstOrDefaultAsync(p => p.PersonelId == personelId);

            if (personel == null)
            {
                return NotFound();
            }

            ViewBag.ExpertiseList = await _dbContext.Uzmanliks.ToListAsync(); // Uzmanlık listesi
            return View(personel);
        }

        [HttpPost]
        public async Task<IActionResult> EditExpertise(int personelId, int newUzmanlikId)
        {
            var personel = await _dbContext.Personels.Include(p => p.User).FirstOrDefaultAsync(p => p.PersonelId == personelId);

            if (personel == null)
            {
                return NotFound("Personel bulunamadı.");
            }

            // Eski Uzmanlık ve yeni Uzmanlık bilgilerini al
            var oldUzmanlikId = personel.UzmanlikId;
            var newUzmanlik = await _dbContext.Uzmanliks.FirstOrDefaultAsync(u => u.Id == newUzmanlikId);

            if (newUzmanlik == null)
            {
                return NotFound("Yeni uzmanlık bulunamadı.");
            }

            // Eski Uzmanlık'a ait ServicePersonelAraTablo kayıtlarını sil
            var oldServicePersonelRecords = _dbContext.ServicePersonelAraTablos
                .Where(s => s.UserId == personel.UserId && s.Service.UzmanlikId == oldUzmanlikId)
                .ToList();

            _dbContext.ServicePersonelAraTablos.RemoveRange(oldServicePersonelRecords);

            // Yeni Uzmanlık'a ait hizmetleri ekle
            var relatedServices = _dbContext.Services.Where(s => s.UzmanlikId == newUzmanlikId).ToList();

            foreach (var service in relatedServices)
            {
                var existingRecord = _dbContext.ServicePersonelAraTablos
                    .FirstOrDefault(s => s.UserId == personel.UserId && s.ServiceId == service.Id);

                if (existingRecord == null)
                {
                    _dbContext.ServicePersonelAraTablos.Add(new ServicePersonelAraTablo
                    {
                        UserId = Convert.ToInt32(personel.UserId),
                        ServiceId = service.Id
                    });
                }
            }

            personel.UzmanlikId = newUzmanlikId;

            await _dbContext.SaveChangesAsync();

            return RedirectToAction("ListAllPersonnel");
        }

        public async Task<IActionResult> GunlukPersonelVerimlilik()
        {
            var apiUrl = "https://localhost:7030/api/PersonelVerimlilik/GunlukKazanc";
            var response = await _httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                var jsonData = await response.Content.ReadAsStringAsync();
                var earnings = JsonConvert.DeserializeObject<List<EarningViewModel>>(jsonData);

                return View(earnings);
            }

            return View(new List<EarningViewModel>());
        }

        public async Task<IActionResult> PersonelDetails(int personelId)
        {
            var apiUrl = $"https://localhost:7030/api/PersonelVerimlilik/GunlukDetay?personelId={personelId}&date={DateTime.Today:yyyy-MM-dd}";
            var response = await _httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                var jsonData = await response.Content.ReadAsStringAsync();
                var details = JsonConvert.DeserializeObject<List<JobViewModel>>(jsonData);

                return View(details);
            }

            return View(new List<JobViewModel>());
        }


    }
}
