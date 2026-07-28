using BLL.DTOs;

namespace BLL.Services.Interfaces.ManagerDashboard;

public interface IManagerDashboardService
{
    Task<ManagerDashboardDto> GetAsync(Guid? warehouseId, int? year, int? month, DateTime? date);
}
