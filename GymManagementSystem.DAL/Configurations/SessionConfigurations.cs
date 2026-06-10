using GymManagementSystem.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManagementSystem.DAL.Configurations
{
    internal class SessionConfigurations : IEntityTypeConfiguration<Session>
    {
        public void Configure(EntityTypeBuilder<Session> builder)
        {
            builder.ToTable(T =>

            {
                T.HasCheckConstraint("SessionCapacityConstraint", "Capacity between 1 and 25");
                T.HasCheckConstraint("SessionEndDateConstraint", "EndDate > StartDate");


            });
            builder.HasOne(X => X.Trainer)
                .WithMany(X => X.Sessions)
                .HasForeignKey(X => X.TrainerId);

            builder.HasOne(X => X.Category)
                .WithMany(X => X.Sessions)
                .HasForeignKey(X => X.CategoryId);
        }
    }
}
