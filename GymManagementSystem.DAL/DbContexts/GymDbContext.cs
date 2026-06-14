using GymManagementSystem.DAL.Models;
using GymManagementSystem.DAL.Models.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace GymManagementSystem.DAL.DbContexts
{
    public class GymDbContext : IdentityDbContext<ApplicationUser>
    {
        public GymDbContext(DbContextOptions<GymDbContext> options) : base(options) { }

        #region DbSets
        public DbSet<Trainer>    Trainers    { get; set; }
        public DbSet<Booking>    Bookings    { get; set; }
        public DbSet<Category>   Categories  { get; set; }
        public DbSet<HealthRecord> HealthRecords { get; set; }
        public DbSet<Member>     Members     { get; set; }
        public DbSet<MemberShip> MemberShips { get; set; }
        public DbSet<Plan>       Plans       { get; set; }
        public DbSet<Session>    Sessions    { get; set; }
        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Required for Identity tables
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
