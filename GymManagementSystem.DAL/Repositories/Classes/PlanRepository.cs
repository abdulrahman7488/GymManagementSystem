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
    public class PlanRepository : IplanRepository
    {
        private readonly GymDbContext _dbContext;

        public PlanRepository(GymDbContext dbContext)
        {
             
            _dbContext = dbContext;
        }
        public async Task<int> AddAsync(Plan plan)
        {
            _dbContext.Plans.Add(plan);
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteAsync(Plan plan)
        {
            _dbContext.Plans.Remove(plan);
            return await  _dbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<Plan>> GetALLAsync(bool tracking = false, CancellationToken ct = default)
        {
            IQueryable<Plan> query = tracking ? _dbContext.Plans : _dbContext.Plans.AsNoTracking();

            return await query.ToListAsync(ct);
        }

        public async Task<Plan?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _dbContext.Plans.FindAsync(id, ct);
        }

        public async Task<int> UpdateAsync(Plan plan)
        {
            _dbContext.Plans.Update(plan);
            return await _dbContext.SaveChangesAsync();
        }
    }
}
