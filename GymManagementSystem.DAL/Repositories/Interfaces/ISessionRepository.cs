using GymManagementSystem.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace GymManagementSystem.DAL.Repositories.Interfaces
{
    public interface ISessionRepository:IGenericRepository<Session>
    {
        Task<IEnumerable<Session>> GetAllSessionsWithTrainerAndCategoryAsync(Expression<Func<Session, bool>>? predicate = null, CancellationToken ct = default);
        Task<Session?>GetSessionWithTrainerAndCategoryAsync(int SessionId, CancellationToken ct = default);
        Task<int>GetCountOfBookedSlotsAsync(int sessionId, CancellationToken ct = default);
    }
}
