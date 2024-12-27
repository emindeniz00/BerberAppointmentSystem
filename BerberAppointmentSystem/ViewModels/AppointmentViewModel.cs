using BerberAppointmentSystem.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace BerberAppointmentSystem.ViewModels
{
    public class AppointmentViewModel
    {
        public int ServiceId { get; set; }
        public Service Service { get; set; }
        public int UserId { get; set; }
        [Required(ErrorMessage = "Personel Alanı zorunludur.")]
        public int PersonelId { get; set; }
        public Personel Personel { get; set; }
        [Required(ErrorMessage = "Başlangıç zamanı zorunludur.")]
        public DateTime StartTime { get; set; }
        public DateTime FinishTime { get; set; }
        public TimeSpan Duration { get; set; }

        [ValidateNever]
        public List<SelectListItem> Services { get; set; }

        [ValidateNever]
        public List<SelectListItem> Personels { get; set; }
    }
}
