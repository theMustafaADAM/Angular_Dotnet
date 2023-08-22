using System;
using ModelService;

namespace ActivityService
{
	public interface IActivitySvc
	{
        Task AddUserActivity (ActivityModel model);
        Task<List<ActivityModel>> GetUserActivity(string userID);

    }
}

