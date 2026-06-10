using GymManagementSystem.DAL.DbContexts;
using GymManagementSystem.DAL.Models;
using GymManagementSystem.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace GymManagementSystem.DAL.Repositories.Classes
{
    public class SessionRepository : GenericRepository<Session>, ISessionRepository
    {
        private readonly GymDbContext _dbContext;

        public SessionRepository(GymDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Session>> GetAllSessionsWithTrainerAndCategoryAsync(
            Expression<Func<Session, bool>>? predicate = null, CancellationToken ct = default)
        {
            IQueryable<Session> query = _dbContext.Sessions
                .Include(s => s.Trainer)
                .Include(s => s.Category)
                .AsNoTracking();

            if (predicate is not null)
                query = query.Where(predicate);

            return await query.ToListAsync(ct);
        }

        public Task<int> GetCountOfBookedSlotsAsync(int sessionId, CancellationToken ct = default)
            => _dbContext.Bookings.AsNoTracking()
                .CountAsync(b => b.SessionId == sessionId, ct);

        public Task<Session?> GetSessionWithTrainerAndCategoryAsync(int sessionId, CancellationToken ct = default)
            => _dbContext.Sessions.AsNoTracking()
                .Include(s => s.Category)
                .Include(s => s.Trainer)
                .FirstOrDefaultAsync(s => s.Id == sessionId, ct);
    }
}