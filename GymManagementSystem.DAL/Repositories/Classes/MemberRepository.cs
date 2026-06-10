using GymManagementSystem.DAL.DbContexts;
using GymManagementSystem.DAL.Models;
using GymManagementSystem.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManagementSystem.DAL.Repositories.Classes
{
    public class MemberRepository:IMemberRepository
    {
        private readonly GymDbContext _dbContext;

        public MemberRepository(GymDbContext dbContext)
        {

            _dbContext = dbContext;
        }
        public async Task<int> AddAsync(Member member)
        {
            _dbContext.Members.Add(member);
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteAsync(Member member)
        {
            _dbContext.Members.Remove(member);
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<Member>> GetALLAsync(bool tracking = false, CancellationToken ct = default)
        {
            IQueryable<Member> query = tracking ? _dbContext.Members : _dbContext.Members.AsNoTracking();

            return await query.ToListAsync(ct);
        }

        public async Task<Member?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _dbContext.Members.FindAsync(id, ct);
        }

        public async Task<int> UpdateAsync(Member member)
        {
            _dbContext.Members.Update(member);
            return await _dbContext.SaveChangesAsync();
        }
    }
}
