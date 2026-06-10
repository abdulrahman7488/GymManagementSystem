using GymManagementSystem.DAL.Models.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManagementSystem.DAL.Models
{
    public abstract class GymUser:BaseEntity
    {
        [Required,MaxLength(50)]
        public string Name { get; set; }
        [Required,MaxLength(100),EmailAddress]
        public string Email { get; set; } = null!;
        [Required,MaxLength(11)]
        [RegularExpression(@"^(010|011}012|015)\d{8}$",ErrorMessage ="Egyptian Phone Format Only")]
        public string Phone { get; set; }
        public DateOnly DateOFBirth { get; set; }
        public Gender Gender { get; set; }

        public Address Address { get; set; } = default!;


    }

    [Owned]
    public class Address
    {
        public int BuildingNumber { get; set; }
        [Required,MaxLength(30)]
        public string City { get; set; } = null!;
        [Required,MaxLength(30)]
        public string Street { get; set; } = null!;

    }
}
