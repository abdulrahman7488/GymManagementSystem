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
    public class BookingConfigurations : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            builder.Ignore(X => X.Id);
            builder.Property(x => x.CreatedAt)
                .HasColumnName("BookingDate")
                .HasDefaultValueSql("GETDATE()");

            builder.HasOne(x => x.Session)
                .WithMany(x => x.SessionMembers)
                .HasForeignKey(X => X.SessionId);

            builder.HasOne(x=>x.Member)
                .WithMany(x=>x.MemberSessions)
                .HasForeignKey(x=>x.MemberId);
            builder.HasKey(x=> new { x.MemberId, x.SessionId });
        }
    }
}
