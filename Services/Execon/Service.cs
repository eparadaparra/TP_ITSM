using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Specialized;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Web;
using TP_ITSM.Models;
using TP_ITSM.Models.Execon;
using TP_ITSM.Models.Trackpoint;
using TP_ITSM.Services.Trackpoint;

namespace TP_ITSM.Services.Execon
{
    public class Service : IExeconServices
    {
        #region Declaración de Variables
        private string? _url;
        private string? _ambiente;
        private List<string> _lstLogEvent = [];
        private string? _expirationDate;
        private string _responseText;

        private ActivityReq _tpRequest = new ActivityReq();
        private Preload _preloadRequest = new Preload();
        //private TpCustomerResponse _tpCustomer = new TpCustomerResponse();
        private TpAuthResponse _tpAuth = new TpAuthResponse();
        private readonly Trackpoint.ITrackpointServices _tpServices;
        #endregion

        public Service(ITrackpointServices services)
        {
            _tpServices = services;
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();
            _url = builder.GetSection("HttpClient:url").Value;
            _ambiente = Boolean.Parse(builder.GetSection("SettingsExecon:EnableDev").Value!)
                    ? builder.GetSection("HttpClient:dev").Value
                    : builder.GetSection("HttpClient:pro").Value;
            _expirationDate = builder.GetSection("SettingsExecon:ExpirationDate").Value;
        }

        private HttpClient CreateHttpClient()
        {
            HttpClient _client = new HttpClient();
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.BaseAddress = new Uri(_url!);
            _client.Timeout = TimeSpan.FromMinutes(10);
            _client.DefaultRequestHeaders.AcceptCharset.ParseAdd("utf-8");
            _client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json")
            );

            _client.DefaultRequestHeaders.Clear();
            return _client;
        }

        public async Task<(bool, string)> GetTask(int assignmentId)
        {
            try
            {
                string objeto = "Task";
                string filter = $"AssignmentId eq {assignmentId} AND TaskType eq 'Assignment'";
                string select = "RecId, AssignmentID, Priority, PlannedStartDate, Details, ParentLink_RecID, ParentLink_Category, EX_CodigoCierre, Subject";

                bool response = await GetFilter(objeto, filter, select);

                if (response)
                {
                    return (response, _responseText);
                }
                else
                {
                    return (false, $"No se pudo obtener la información de {objeto}");
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            } 
        }

        public async Task<(bool, string)> GetParentInfo(string recId, string objName)
        {
            try
            {
                string number = objName == "ReleaseMilestone" ? "ReleaseNumber" : String.Concat(objName, "Number");
                string select = String.Concat($"RecId,"
                    , number != "ReleaseNumber" ? "Service," : "RecId,"
                    , "Subject,"
                    , $"{number},"
                    , "CreatedDateTime,"
                    , "EX_CustID_Link_RecID,"
                    , "EX_LocationID_Link_RecID,"
                    , "Owner,"
                    , "Owner_Valid"
                );

                bool response = await GetFilter(objName, $"RecId eq '{recId}'", select);

                if (response)
                {
                    return (response, _responseText);
                }
                else
                {
                    return (false, $"No se pudo obtener la información de {objName}");
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool, string)> GetAccount(string recId)
        {
            try
            {
                string select = "RecId, CustID, Name";

                bool response = await GetFilter("Account", $"RecId eq '{recId}'", select);

                if (response)
                {
                    return (response, _responseText);
                }
                else
                {
                    return (false, $"No se pudo obtener la información del Account");
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool, string)> GetLocation(string recId)
        {
            try
            {
                string select = "RecId, EX_IdSitio, Name, EX_Zona, EX_PlazaCobertura, EX_Latitud, EX_Longitud, Address, EX_Colonia, City, Zip, State";

                bool response = await GetFilter("Location", $"RecId eq '{recId}'", select);

                if (response)
                {
                    return (response, _responseText);
                }
                else
                {
                    return (false, $"No se pudo obtener la información del Location");
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool, string)> GetEmployee(string owner)
        {
            try
            {
                string select = "RecId, DisplayName, Supervisor, Title, Department, Team, Status, Disabled";

                bool response = await GetFilter("Employee", $"LoginID eq '{owner}'", select);

                if (response)
                {
                    return (response, _responseText);
                }
                else
                {
                    return (false, $"No se pudo obtener la información del Location");
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool, string)> GetTaskCatalog(string taskSubject)
        {
            try
            {
                string select = "RecId, TaskSubject, EX_IdActividadTP&$top=1";

                bool response = await GetFilter("TaskCatalog__Assignment", $"TaskSubject eq '{taskSubject}'", select);

                if (response)
                {
                    return (response, _responseText);
                }
                else
                {
                    return (false, $"No se pudo obtener la información del Location");
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool, string)> GetTaskReq(int assignmentId)
        {
            try
            {
                _tpAuth = await _tpServices.GetToken();

                #region Busca Información de Task
                var (successTask, respTaskInfo) = await GetTask(assignmentId);
                if (!successTask)
                    return (false, respTaskInfo);
                var task = await DeserializeODataResponse<TaskInfo>(respTaskInfo);
                SetValuesModels(_tpRequest, task);
                SetValuesModels(_preloadRequest, task);
                #endregion

                #region Busca Información del ParentTask
                var (successParent, respParent) = await GetParentInfo( task?.ParentLink_RecID, task.ParentLink_Category);
                if (!successParent)
                    return (false, respParent);
                var parentTask = await DeserializeODataResponse<ParentInfo>(respParent);
                SetValuesModels(_tpRequest, parentTask);
                SetValuesModels(_preloadRequest, parentTask);
                #endregion

                #region Busca Información de la Cuenta del Cliente
                var (successAccount, respAccount) = await GetAccount(parentTask?.EX_CustID_Link_RecID);
                if (!successAccount)
                    return (false, respAccount);
                
                var account = await DeserializeODataResponse<AccountInfo>(respAccount);
                
                #region Valida existencia de customer en Trackpoint
                var (successGetCustUuid, tpCustUuid) = await GetSetTpCustomerInfo(account);
                #endregion
                if (!successGetCustUuid) 
                    return (false, tpCustUuid);

                _tpRequest.scheduled_client_uuid = tpCustUuid;
                SetValuesModels(_tpRequest, account);
                SetValuesModels(_preloadRequest, account);
                #endregion

                #region Busca Información del Location
                var (successLocation, respLocation) = await GetLocation(parentTask?.EX_LocationID_Link_RecID);
                if (!successLocation)
                    return (false, respLocation);
                var location = await DeserializeODataResponse<LocationInfo>(respLocation);
                SetValuesModels(_tpRequest, location);
                SetValuesModels(_preloadRequest, location);
                #endregion

                #region Busca Información del Employee
                var (successEmployee, respEmployee) = await GetEmployee(parentTask?.Owner);
                if (!successEmployee)
                    return (false, respEmployee);
                var employee = await DeserializeODataResponse<EmployeeInfo>(respEmployee);
                SetValuesModels(_tpRequest, employee);
                SetValuesModels(_preloadRequest, employee);
                #endregion

                #region Busca Información del Catalogo de Tareas
                var (successTaskCat, respTaskCat) = await GetTaskCatalog(task?.Subject);
                if (!successTaskCat)
                    return (false, respTaskCat);
                var taskCatalog = await DeserializeODataResponse<TaskCatalogInfo>(respTaskCat);
                SetValuesModels(_tpRequest, taskCatalog);
                SetValuesModels(_preloadRequest, taskCatalog);
                #endregion

                #region Asignaciones puntuales
                _tpRequest.scheduled_name_event = String.Concat( task.AssignmentID, " | ", parentTask.ParentNumber, " | P", task.Priority, " | ", location.EX_IdSitio, " | ", location.Name);
                _tpRequest.scheduled_expiration_date = Int32.Parse(_expirationDate!);
                _tpRequest.id_user = _tpAuth.uuid;
                #endregion

                //Console.WriteLine(JsonConvert.SerializeObject(_tpRequest, Formatting.Indented));
                //Console.WriteLine(JsonConvert.SerializeObject(_preloadRequest, Formatting.Indented));

                return (true, JsonConvert.SerializeObject(_tpRequest));
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
       
        private async Task<(bool, string)> GetSetTpCustomerInfo(AccountInfo customer)
        {
            #region Valida en Trackpoint que exista del cliente
            string etiqueta = customer.CustID.Count() == 4 ? "client_id" : "id";
            object newObj = new Dictionary<string, string> { [etiqueta] = customer.CustID };
            TpCustomerResponse apiResponse = new TpCustomerResponse();
            DataCustomerTP dataCustomerTP = new DataCustomerTP();

            var (successTpAccount, respTPAccount) = await _tpServices.GetCustomer(newObj);
            apiResponse = System.Text.Json.JsonSerializer.Deserialize<TpCustomerResponse>(respTPAccount);
            dataCustomerTP = apiResponse.data;

            if (successTpAccount && dataCustomerTP.modules_notify != null)
            {
                return (successTpAccount, dataCustomerTP.modules_notify.filters_uid!);
            }
            #endregion

            #region Si no existe se crea Customer en Trackpoint y regresa su Id
            TpCustomerResponse insTpCust = new TpCustomerResponse();
            insTpCust.data = new DataCustomerTP();
            insTpCust.data.modules_notify = new ModulesNotify();
            insTpCust.data.modules_notify.notificacion = new Notificacion();
            insTpCust.data.name = customer.Name.Trim();
            insTpCust.data.user_uuid_id = _tpAuth.uuid.Trim();
            insTpCust.data.client_id = customer.CustID.Trim();

            (successTpAccount, respTPAccount) = await _tpServices.InsUpdDelCustomer(insTpCust, "INS");
            insTpCust = System.Text.Json.JsonSerializer.Deserialize<TpCustomerResponse>(respTPAccount);
            
            if (successTpAccount && insTpCust.id != null)
            {
                return (successTpAccount, insTpCust.id);
            }
            
            return (successTpAccount, respTPAccount);            
            #endregion
        }

        private void SetValuesModels(object modelo, object body)
        {
            var jObject = JObject.FromObject(body);
            var modeloType = modelo.GetType();

            foreach (var property in jObject.Properties())
            {
                // Buscar propiedad por JsonPropertyName primero
                var propInfo = modeloType.GetProperties()
                    .FirstOrDefault(p =>
                        p.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name == property.Name ||
                        p.Name == property.Name);

                if (propInfo != null && propInfo.CanWrite)
                {
                    try
                    {
                        var value = property.Value;

                        // Si es nulo
                        if (value.Type == JTokenType.Null)
                        {
                            var propType = propInfo.PropertyType;
                            if (!propType.IsValueType || Nullable.GetUnderlyingType(propType) != null)
                            {
                                propInfo.SetValue(modelo, null);
                            }
                            continue;
                        }

                        // Convertir usando Json.NET
                        object convertedValue;
                        var targetType = propInfo.PropertyType;

                        // Manejar tipos nullable
                        if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
                            targetType = Nullable.GetUnderlyingType(targetType);
                        }

                        convertedValue = value.ToObject(targetType);
                        propInfo.SetValue(modelo, convertedValue);
                    }
                    catch
                    {
                        // Si falla, intentar como string
                        try
                        {
                            propInfo.SetValue(modelo, property.Value.ToString());
                        }
                        catch
                        {
                            // Ignorar error
                        }
                    }
                }
            }
        }

        private async Task<bool> GetFilter(string objeto, string filter, string select)
        {
            try
            {
                NameValueCollection queryParams = HttpUtility.ParseQueryString(string.Empty);
                queryParams["objeto"] = objeto;
                queryParams["filter"] = filter;
                queryParams["select"] = select;

                HttpClient client = CreateHttpClient();
                HttpResponseMessage response = await client.GetAsync($"{_ambiente}/api/Obj/Filter?{queryParams}");
                _responseText = await response.Content.ReadAsStringAsync();

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _responseText = ex.Message;
                return false;
            }
        }

        private async Task<T> DeserializeODataResponse<T>(string jsonResponse)
        {
            var odataResponse = JsonConvert.DeserializeObject<ODataResponse<T>>(jsonResponse);

            if (odataResponse?.Value == null || !odataResponse.Value.Any())
                throw new InvalidOperationException("No data found in OData response");

            return odataResponse.Value[0];
        }

    }
}
