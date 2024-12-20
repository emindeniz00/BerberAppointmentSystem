namespace BerberAppointmentSystem.Models
{
    public class Uzmanlik
    {
        public int Id { get; set; }
        public string ?UzmanlikAd { get; set; }
        public List<Service> Services { get; set; }
        public List<User> Users { get; set; }
    }
}
