using AlertAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace AlertAPI.Data
{
    public class AlertAPIDBContext :DbContext
    {
        public AlertAPIDBContext(DbContextOptions<AlertAPIDBContext> options): base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Alert>().Navigation(b => b.AlertsIPAddresses).AutoInclude();
            modelBuilder.Entity<AlertIPAddress>().Navigation(bp => bp.IPAddress).AutoInclude();



            modelBuilder.Entity<AlertIPAddress>().HasKey(bp => new
            {
                bp.AlertID,
                bp.IPString
            });

            modelBuilder.Entity<AlertIPAddress>().HasOne(b => b.Alert).WithMany(bp => bp.AlertsIPAddresses).HasForeignKey(bp => bp.AlertID);
            modelBuilder.Entity<AlertIPAddress>().HasOne(p => p.IPAddress).WithMany(bp => bp.AlertsIPAddresses).HasPrincipalKey(bp => bp.IPString);



            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Alert> Alerts { get; set; }
        public DbSet<IPAddress> IPAddresses { get; set; }
        public DbSet<AlertIPAddress> AlertIPAddresses { get; set; }

    }
}
