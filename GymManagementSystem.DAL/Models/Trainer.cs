using GymManagementSystem.DAL.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManagementSystem.DAL.Models
{
    public class Trainer:GymUser
    {
        public Specialities Specialities { get; set; }
        public string? Photo { get; set; }
        public ICollection<Session> Sessions { get; set; } = default!;
    }
}
