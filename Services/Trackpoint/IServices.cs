namespace TP_ITSM.Services.Trackpoint
{
    public interface IServices
    {
        Task<object> GetToken();

        Task<object> GetAllCustomer();

        Task<(bool, string)> GetCustomer(object obj);

        Task<(bool, string)> InsUpdDelCustomer(object obj, string request);

        Task<(bool, string)> GetActivityTP(string firebaseId);
    }
    
}
