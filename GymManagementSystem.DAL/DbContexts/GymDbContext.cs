using GymManagementSystem.DAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GymManagementSystem.DAL.DbContexts
{
    public class GymDbContext : DbContext
    {
        public GymDbContext(DbContextOptions<GymDbContext> options) : base(options)
        {

        }

        #region DbSets
        public DbSet<Trainer> Trainers { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<HealthRecord> HealthRecords { get; set; }
        public DbSet<Member> Members { get; set; }
        public DbSet<MemberShip> MemberShips { get; set; }
        public DbSet<Plan> Plans { get; set; }
        public DbSet<Session> Sessions { get; set; }
        #endregion
        override protected void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        }
    }
}
