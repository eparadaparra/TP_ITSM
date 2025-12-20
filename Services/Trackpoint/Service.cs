using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using TP_ITSM.Models;
using TP_ITSM.Models.Execon;
using TP_ITSM.Models.Trackpoint;

namespace TP_ITSM.Services.Trackpoint
{

    public class Service : ITrackpointServices
    {
        #region Declaración de Variables

        private string? _url;
        private string? _custumerKey;
        private string? _autorization;
        private string? _user;
        private string? _pass;
        private string? _token;
        private List<string> _lstLogEvent = [];
        private TpAuthResponse _tokenResponse;

        private string _responseText;
        #endregion

        public Service()
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();
            _url          = builder.GetSection("HttpClientTP:url").Value;
            _custumerKey    = builder.GetSection("HttpClientTP:customer_key").Value;
            _autorization   = builder.GetSection("HttpClientTP:autorization").Value;
            _user           = builder.GetSection("HttpClientTP:email").Value;
            _pass           = builder.GetSection("HttpClientTP:pass").Value;

        }

        private  HttpClient CreateHttpClient(bool token = true)
        {
            HttpClient _client = new HttpClient();
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.BaseAddress = new Uri(_url!);
            _client.Timeout = TimeSpan.FromMinutes(10);
            _client.DefaultRequestHeaders.Add("api-customer-key", _custumerKey);
            _client.DefaultRequestHeaders.AcceptCharset.ParseAdd("utf-8");
            _client.DefaultRequestHeaders.Accept.Add( 
                new MediaTypeWithQualityHeaderValue("application/json")
            );
            _client.DefaultRequestHeaders.Authorization = token
                ? new AuthenticationHeaderValue("Bearer", _token)
                : new AuthenticationHeaderValue("Bearer", _autorization);

            return _client;
        }

        #region (API TP) Autienticación para generar Token
        private async Task AuthTp()
        {
            TpAuthRequest credentials = new TpAuthRequest() { email = _user, password = _pass };
            var json = JsonConvert.SerializeObject(credentials);
            var httpCont = new StringContent (json, Encoding.UTF8, "application/json");
            
            HttpClient client = CreateHttpClient(false);

            HttpResponseMessage result = await client.PostAsync("/apiScheduledAuthToken", httpCont);
            _responseText = await result.Content.ReadAsStringAsync();

            _tokenResponse = JsonConvert.DeserializeObject<TpAuthResponse>(_responseText);

            if (result.IsSuccessStatusCode)
                _token = _tokenResponse.token;

        }
        #endregion

        #region GetToken
        public async Task<TpAuthResponse> GetToken()
        {
            await AuthTp();     
            return _tokenResponse;
        }
        #endregion

        #region GetAllTPCustomer
        public async Task<object> GetAllCustomer()
        {
            await AuthTp();
            HttpClient client = CreateHttpClient();
            var result = new object();

            try
            {
                HttpResponseMessage response = await client.GetAsync("/ApigetAllCustomers");

                if (response.IsSuccessStatusCode)
                {
                    return result = await response.Content.ReadFromJsonAsync<Dictionary<string, dynamic>>();
                }
                else
                {
                    return result = await response.Content.ReadFromJsonAsync<Dictionary<string, dynamic>>();
                }
            }
            catch (Exception ex)
            {
                return result = new ErrorResponse() { Status = null, Mensaje = ex.Message, data = null };
            }
        }
        #endregion

        #region Get Customer by ID
        public async Task<(bool,string)> GetCustomer(object obj)
        {
            var httpCont = new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");

            try
            {
                bool response = await SendPost("/ApigetCustomerbyid", httpCont);

                if (response)
                {
                    return (response, _responseText);
                }
                else
                {
                    return (false, "No se pudo obtener la información del Customer");
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        #endregion

        #region UPD INS DEL Customer
        public async Task<(bool, string)> InsUpdDelCustomer(object obj, string request)
        {
            StringContent httpCont = new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");
            string metodo = String.Empty;
                        
            switch (request)
            {
                case "INS":
                {
                    metodo = "/ApicreateCustomer";
                    break;
                }
                case "UPD":
                {                        
                    metodo = "/ApiupdateCustomer";
                    break;
                }
                case "DEL":
                {
                    metodo = "/ApidisableCustomer";
                    break;
                }
            } 
            try
            {
                var response = await SendPost(metodo, httpCont);
//return result = await response.Content.ReadFromJsonAsync<Dictionary<string, dynamic>>();
                if (response)
                {
                    return (response, _responseText);
                }
                else
                {
                    return (false, $"Problemas al realizar metodo {metodo}");
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        #endregion

        #region GET Activity Info
        public async Task<(bool, string)> GetActivityTP(string firebaseId)
        {
            try
            {
                bool response = await SendPost($"/getDataEventWeb/{firebaseId}", null, false, true);

                if (response)
                {
                    return (response, _responseText);
                }
                else
                {
                    return (false, "No se pudo obtener la información de la Actividad");
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        #endregion

        #region Set Activity ITSM-TP
        public async Task<(bool, string)> SetActivityTP(object request)
        {
            try
            { 
                await AuthTp();
                HttpClient httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri(_url);
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
                httpClient.DefaultRequestHeaders.Add("api-customer-key", _custumerKey);

                var response = await httpClient.PostAsJsonAsync("/apiScheduledProgrammingAdd", request);
                _responseText = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var jsonObjCreated = JsonConvert.DeserializeObject<ActivityResult>(_responseText);
                    string firebaseId = jsonObjCreated?.data.firebase_id ?? "";

                    return (response.IsSuccessStatusCode, _responseText);
                }
                else
                {
                    return (false, _responseText);
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        #endregion

        #region Upd Activity Preload
        public async Task<(bool, string)> UpdActivityTP(Preload preload, string firebaseId)
        {
            List<Preload> lstPreload = new List<Preload>();
            lstPreload.Add(preload);
            DataPreload dataPreload = new DataPreload() { 
                firebase_id = firebaseId, 
                data = lstPreload 
            };
            PreloadReq req = new PreloadReq() { data = dataPreload };

            var content = new StringContent(JsonConvert.SerializeObject(req), Encoding.UTF8, "application/json");
            try
            {
                await AuthTp();
                HttpClient httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri(_url);
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
                httpClient.DefaultRequestHeaders.Add("api-customer-key", _custumerKey);

                var response = await httpClient.PostAsync("/updateEventWebhook", content);
                _responseText = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return (response.IsSuccessStatusCode, _responseText);
                }
                else
                {
                    return (false, _responseText);
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        #endregion

        private async Task<bool> SendPost(string metodo, StringContent httpCont = null, bool token = true, bool isAuth = false)
        {
            try 
            {
                if (!isAuth) 
                    await AuthTp();

                HttpClient client = CreateHttpClient(token);
                HttpResponseMessage response = await client.PostAsync(metodo, httpCont);
                _responseText = await response.Content.ReadAsStringAsync();

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _responseText = ex.Message;
                return false;
            }
        }

    }
}
