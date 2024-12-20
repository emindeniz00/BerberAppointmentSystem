using Microsoft.AspNetCore.Identity;

namespace BerberAppointmentSystem.Models
{
    public class User : IdentityUser<int>
    {
        public string ?Ad { get; set; }
        public string ?Soyad { get; set; }
        public List<ServicePersonelAraTablo> ServicePersonelAraTablos { get; set; }
        public List<PersonelMesai> PersonelMesais { get; set; }
        public List<Appointment> Appointments { get; set; }
    }
}
