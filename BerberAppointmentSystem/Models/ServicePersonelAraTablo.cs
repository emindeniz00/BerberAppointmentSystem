namespace BerberAppointmentSystem.Models
{
    public class ServicePersonelAraTablo
    {
        public int ServiceId { get; set; }
        public Service Service { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }
    }
}
