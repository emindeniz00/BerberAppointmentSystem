using BerberAppointmentSystem.Models;

namespace BerberAppointmentSystem.ViewModels
{
    public class JobViewModel
    {
        public int AppointmentId { get; set; }
        public User User { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string ServiceName { get; set; }
        public int Price { get; set; }
    }
}
