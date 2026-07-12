using EMIT.Application.DTOs;

namespace EMIT.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardDto> GetDashboardAsync();
}
