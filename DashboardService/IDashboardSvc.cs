using System;
using ModelService;

namespace DashboardService
{
	public interface IDashboardSvc
	{
        Task<DashboardModel> GetDashboardData();

    }
}

