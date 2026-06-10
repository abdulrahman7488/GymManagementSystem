using GymManagementSystem.BLL.ViewModels.SessionViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManagementSystem.BLL.Services.Interfaces
{
    public interface IAnalyticsService
    {
        Task<AnalyticsViewModel> GetAnalyticsDataAsync(CancellationToken ct = default);
    }
}
