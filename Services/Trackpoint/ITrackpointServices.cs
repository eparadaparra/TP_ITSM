using TP_ITSM.Models.Trackpoint;

namespace TP_ITSM.Services.Trackpoint
{
    public interface ITrackpointServices
    {
        Task<TpAuthResponse> GetToken();

        Task<object> GetAllCustomer();

        Task<(bool, string)> GetCustomer(object obj);

        Task<(bool, string)> InsUpdDelCustomer(object obj, string request);

        Task<(bool, string)> GetActivityTP(string firebaseId);

        Task<(bool, string)> SetActivityTP(object resquest);

        Task<(bool, string)> UpdActivityTP(Preload preload, string firebaseId);
    }
    
}
