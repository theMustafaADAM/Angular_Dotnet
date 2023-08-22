using System;
using DataService;
using Microsoft.EntityFrameworkCore;
using ModelService;
using Serilog;

namespace DashboardService
{
	public class DashboardSvc : IDashboardSvc
	{
		private readonly ApplicationDbContext _db;

		public DashboardSvc(ApplicationDbContext db)
		{
			_db = db;
		}

		public async Task<DashboardModel> GetDashboardData()
		{
			DashboardModel dashboard = new DashboardModel();

			try
			{
                await using var dbContextTransaction = _db.Database.BeginTransaction();

				try
				{
					dashboard.TotalUsers = await _db.ApplicationUsers.CountAsync();
					dashboard.TotalPosts = 3516;
					dashboard.PendingRequests = 51;
					dashboard.NewUsers = await _db.ApplicationUsers
						.Where(x => x.AccountCreatedOn == DateTime.Today).CountAsync();
                }
				catch (Exception ex)
				{
                    Log.Error("Error While creating user : {Error} * {StackTrace} * {InnerException} * {Source}",
                        ex.Message, ex.StackTrace, ex.InnerException, ex.Source);

                    dbContextTransaction.Rollback();
                }
            }
			catch (Exception ex)
			{
                Log.Error("Error While creating user : {Error} * {StackTrace} * {InnerException} * {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }

            return dashboard;
        }
	}
}

