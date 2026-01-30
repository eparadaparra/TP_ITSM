using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using TP_ITSM.Models;
using TP_ITSM.Models.Trackpoint;
using Preload = TP_ITSM.Models.Trackpoint.Preload;

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
        private TpAuthResponse _tokenResponse;

        private int _timeOutValue;
        private string _responseText;

        private readonly ILogger<Service> _logger;
        private StringBuilder sb = new StringBuilder().Append('*', 60).Append(" ").Append(DateTime.Now.ToString()).Append(" ").Append('*', 60);
        #endregion

        public Service(ILogger<Service> logger)
        {
            _logger = logger;
            _logger.LogInformation(sb.ToString());
            _logger.LogInformation("Inicializando Trackpoint Services");

            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();
            _timeOutValue   = Int32.Parse(builder.GetSection("SettingsExecon:TimeOutSeconds").Value!);
            _url            = builder.GetSection("HttpClientTP:url").Value;
            _custumerKey    = builder.GetSection("HttpClientTP:customer_key").Value;
            _autorization   = builder.GetSection("HttpClientTP:autorization").Value;
            _user           = builder.GetSection("HttpClientTP:email").Value;
            _pass           = builder.GetSection("HttpClientTP:pass").Value;

            _logger.LogInformation($"Configuración cargada | CustomerKey: {_custumerKey} | Timeout: {_timeOutValue}'s");
        }

        private  HttpClient CreateHttpClient(bool token = true)
        {
            // Generamos un identificador único para la idempotencia
            var idempotencyKey = Guid.NewGuid().ToString();

            HttpClient _client = new HttpClient();
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.BaseAddress = new Uri(_url!);
            _client.Timeout = TimeSpan.FromSeconds(_timeOutValue); //.FromMinutes(10);
            _client.DefaultRequestHeaders.Add("api-customer-key", _custumerKey);
            _client.DefaultRequestHeaders.AcceptCharset.ParseAdd("utf-8");
            _client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json")
            );
            _client.DefaultRequestHeaders.Authorization = token
                ? new AuthenticationHeaderValue("Bearer", _token)
                : new AuthenticationHeaderValue("Bearer", _autorization);

            // Agregamos el encabezado de idempotencia
            _client.DefaultRequestHeaders.Remove("Idempotency-Key");
            _client.DefaultRequestHeaders.Add("Idempotency-Key", idempotencyKey);

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

                if (!response)
                {
                    _logger.LogWarning($"      ! *GetCustomerTP sin respuesta válida | Uri: /ApigetCustomerbyid");
                    return (false, "No se pudo obtener la información del Customer");
                }

                TpCustomerResponse custumerTp = JsonConvert.DeserializeObject<TpCustomerResponse>(_responseText);
                _logger.LogInformation($"      ✓ *GetCustomerTP exitoso | CustId: {custumerTp.data.client_id} | IdClientTP: {custumerTp.data.modules_notify.filters_uid} | Cliente: {custumerTp.data.name}");

                return (response, _responseText);                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"      × Error en *GetCustomerTP | Uri: /ApigetCustomerbyid");
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
                if (!response)
                {
                    _logger.LogWarning($"      ! *InsUpdDelCustomerTP sin respuesta válida | Uri: {metodo}");
                    return (false, $"Problemas al realizar metodo {metodo}");
                }

                if (request == "INS")
                {
                    TpCustomerResponse custumerTp = JsonConvert.DeserializeObject<TpCustomerResponse>(_responseText);
                    _logger.LogInformation($"      ✓ *InsUpdDelCustomerTPTP exitoso | CustId: {custumerTp.data.client_id} | IdClientTP: {custumerTp.data.modules_notify.filters_uid} | Cliente: {custumerTp.data.name}");
                }
                return (response, _responseText);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"      × Error en *InsUpdDelCustomerTP | Uri: {metodo}");
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
                //Console.WriteLine(request.GetType());
                await AuthTp();
                HttpClient client = new HttpClient();
                // Generamos un identificador único para la idempotencia
                var idempotencyKey = Guid.NewGuid().ToString();
                // Agregamos el encabezado de idempotencia
                client.DefaultRequestHeaders.Remove("Idempotency-Key");
                client.DefaultRequestHeaders.Add("Idempotency-Key", idempotencyKey);
                client.Timeout = TimeSpan.FromSeconds(_timeOutValue); //.FromMinutes(10);
                client.BaseAddress = new Uri(_url);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
                client.DefaultRequestHeaders.Add("api-customer-key", _custumerKey);

                var response = await client.PostAsJsonAsync("/apiScheduledProgrammingAdd", request);
                _responseText = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"   ! *SetActivityTP sin respuesta válida | Uri: /apiScheduledProgrammingAdd");
                    return (false, _responseText);
                }
                
                var jsonObjCreated = JsonConvert.DeserializeObject<ActivityResult>(_responseText);
                string firebaseId = jsonObjCreated?.data.firebase_id ?? "";

                _logger.LogInformation($"   ✓ *SetActivityTP exitoso | Actividad creada en Tracpoint Exitosamente, FirebaseId: {firebaseId}");

                return (response.IsSuccessStatusCode, _responseText);                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"   × Error crítico en SetActivityTP | Request: {request}");
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
                HttpClient client = new HttpClient();
                // Generamos un identificador único para la idempotencia
                var idempotencyKey = Guid.NewGuid().ToString();
                // Agregamos el encabezado de idempotencia
                client.DefaultRequestHeaders.Remove("Idempotency-Key");
                client.DefaultRequestHeaders.Add("Idempotency-Key", idempotencyKey);
                client.Timeout = TimeSpan.FromSeconds(_timeOutValue); //.FromMinutes(10);
                client.BaseAddress = new Uri(_url);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
                client.DefaultRequestHeaders.Add("api-customer-key", _custumerKey);

                var response = await client.PostAsync("/updateEventWebhook", content);
                _responseText = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"   ! *UpdActivityTP sin respuesta válida | Uri: /updateEventWebhook");
                    return (false, _responseText);
                }

                _logger.LogInformation($"   ✓ *UpdActivityTP exitoso | Actividad actualizada en Tracpoint Exitosamente, FirebaseId: {firebaseId}");
                return (response.IsSuccessStatusCode, _responseText);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"   × Error crítico en *UpdActivityTP | Preload: {preload}");
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
