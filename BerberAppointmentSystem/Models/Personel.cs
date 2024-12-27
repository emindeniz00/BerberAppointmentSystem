namespace BerberAppointmentSystem.Models

{
    public class Personel
    {
        public int PersonelId { get; set; }
        public int UserId { get; set; }
        public User ?User { get; set; }
        public int ?UzmanlikId { get; set; }
        public Uzmanlik ?Uzmanlik { get; set; }


    }
}
