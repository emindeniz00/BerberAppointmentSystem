namespace BerberAppointmentSystem.Models
{
    public class Service
    {
        public int Id { get; set; }
        public string ?ServisAdı { get; set; }
        public int UzmanlikId { get; set; }
        public Uzmanlik Uzmanlik { get; set; }
        public int ?Fiyat { get; set; }

        public TimeSpan Sure { get; set; }

        public List<ServicePersonelAraTablo> ?ServicePersonelAraTablos { get; set; }
        public List<Appointment> ?Appointments { get; set; }

    }
}
