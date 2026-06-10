using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManagementSystem.DAL.Models
{
    public class Member:GymUser
    {
        public string? Photo { get; set; }
        public HealthRecord HealthRecord { get; set; } = default!;
        public ICollection<Booking> MemberSessions { get; set; } = default!;
        public ICollection<MemberShip> MemberPlans { get; set; } = default!;
    }
}
