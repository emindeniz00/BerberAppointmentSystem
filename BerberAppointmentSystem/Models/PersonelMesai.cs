using System.ComponentModel.DataAnnotations;

namespace BerberAppointmentSystem.Models
{
    public class PersonelMesai
    {
        [Key]
        public int MesaiId { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public DayOfWeek DayOfWeek { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }
    }
}
