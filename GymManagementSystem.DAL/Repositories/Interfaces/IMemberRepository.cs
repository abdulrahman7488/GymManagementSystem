using GymManagementSystem.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManagementSystem.DAL.Repositories.Interfaces
{
    public interface IMemberRepository
    {
        Task<IEnumerable<Member>> GetALLAsync(bool tracking = false, CancellationToken ct = default);
        Task<Member?> GetByIdAsync(int id, CancellationToken ct = default);
        public Task<int> AddAsync(Member plan);
        public Task<int> UpdateAsync(Member plan);
        public Task<int> DeleteAsync(Member plan);
    }
}
