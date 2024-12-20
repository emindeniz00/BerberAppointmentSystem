using System.ComponentModel.DataAnnotations;

namespace BerberAppointmentSystem.Models
{
    public class Appointment
    {
        public int AppointmentId { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }
        public int ServiceId { get; set; }
        public Service Service { get; set; }
        public int PersonelId { get; set; }
        public Personel? Personel { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }
        public bool Onay { get; set; }
        public bool Ret { get; set; }




    }
}
