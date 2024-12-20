using BerberAppointmentSystem.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BerberAppointmentSystem.Context
{
    public class ApplicationDbContext : IdentityDbContext<User, UserRole, int>
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);



            // Many-to-Many Relationship between Personnel and Services
            modelBuilder.Entity<ServicePersonelAraTablo>()
                .HasKey(sp => new { sp.ServiceId, sp.UserId });

            // İlişkileri tanımlama
            modelBuilder.Entity<ServicePersonelAraTablo>()
                .HasOne(sp => sp.Service)
                .WithMany(s => s.ServicePersonelAraTablos)
                .HasForeignKey(sp => sp.ServiceId);

            modelBuilder.Entity<ServicePersonelAraTablo>()
                .HasOne(sp => sp.User)
                .WithMany(u => u.ServicePersonelAraTablos)
                .HasForeignKey(sp => sp.UserId);
        }

        public DbSet<Uzmanlik> Uzmanliks { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<ServicePersonelAraTablo> ServicePersonelAraTablos { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<PersonelMesai> PersonelMesais { get; set; }
        public DbSet<Personel> Personels { get; set; }
    }
}