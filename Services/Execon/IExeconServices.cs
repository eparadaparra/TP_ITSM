using TP_ITSM.Models;
using TP_ITSM.Models.Execon;

namespace TP_ITSM.Services.Execon
{
    public interface IExeconServices
    {
        Task<(bool, string)> GetTask(int assignmentId);
        Task<(bool, string)> GetAccount(string recId);
        Task<(bool, string)> GetLocation(string recId);
        Task<(bool, string)> GetEmployee(string owner);
        Task<(bool, string)> GetTaskCatalog(string taskSubject);
        Task<(bool, string)> GetParentInfo(string recId, string objName);
        Task<(bool, string)> GetTaskReq(int assignmentId);
        Task<(bool, string)> ScheduledTask(int assignmentId);
        Task<(bool, string)> UpTask(ResponseTaskTP activityTP);
    }
}
