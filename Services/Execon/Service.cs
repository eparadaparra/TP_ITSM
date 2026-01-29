using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Specialized;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;
using TP_ITSM.Custom;
using TP_ITSM.Data;
using TP_ITSM.Models;
using TP_ITSM.Models.Execon;
using TP_ITSM.Models.Trackpoint;
using TP_ITSM.Services.Trackpoint;
using Preload = TP_ITSM.Models.Trackpoint.Preload;

namespace TP_ITSM.Services.Execon
{
    public class Services : IExeconServices
    {
        #region Declaración de Variables
        private string? _url;
        private string? _ambiente;
        private string _responseText;

        private int _expirationDate;
        private int _timeOutValue;

        private List<string> _lstLogEvent = [];

        private TaskInfo _task               = new TaskInfo();
        private ParentInfo _parentTask       = new ParentInfo();
        private AccountInfo _account         = new AccountInfo();
        private LocationInfo _location       = new LocationInfo();
        private EmployeeInfo _employee       = new EmployeeInfo();
        private TaskCatalogInfo _taskCatalog = new TaskCatalogInfo();
        private AttachementITSM _attachModel = new AttachementITSM();
        private AttachInfo _attachInfo       = new AttachInfo();

        private TpAuthResponse _tpAuth  = new TpAuthResponse();
        private ActivityReq _tpRequest  = new ActivityReq();
        private Preload _preloadRequest = new Preload();

        private readonly ConnIVANTIDW _dwContext;
        private readonly Trackpoint.ITrackpointServices _tpServices;
        #endregion

        public Services(ITrackpointServices services,
        ConnIVANTIDW dwContext)
        {
            _tpServices = services;
            _dwContext  = dwContext;
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();
            _url = builder.GetSection("HttpClient:url").Value;
            _ambiente = Boolean.Parse(builder.GetSection("SettingsExecon:EnableDev").Value!)
                    ? builder.GetSection("HttpClient:dev").Value
                    : builder.GetSection("HttpClient:pro").Value;
            _expirationDate = Int32.Parse(builder.GetSection("SettingsExecon:ExpirationDate").Value!);
            _timeOutValue   = Int32.Parse(builder.GetSection("SettingsExecon:TimeOutSeconds").Value!);
        }

        private HttpClient CreateHttpClient()
        {
            // Generamos un identificador único para la idempotencia
            var idempotencyKey = Guid.NewGuid().ToString();
            HttpClient _client = new HttpClient();
            
            _client.BaseAddress = new Uri(_url!);
            _client.Timeout = TimeSpan.FromSeconds(_timeOutValue); //.FromMinutes(10);
            // Agregamos el encabezado de idempotencia
            _client.DefaultRequestHeaders.Remove("Idempotency-Key");
            _client.DefaultRequestHeaders.Add("Idempotency-Key", idempotencyKey);
            _client.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue("application/json") );
            _client.DefaultRequestHeaders.AcceptCharset.ParseAdd("utf-8");
            _client.DefaultRequestHeaders.Clear();

            return _client;
        }

        public async Task<(bool, string)> GetTask(int assignmentId)
        {
            try
            {
                string objeto = "Task";
                string filter = $"AssignmentId eq {assignmentId} AND TaskType eq 'Assignment'";
                string select = "RecId, AssignmentID, Priority, PlannedStartDate, Details, ParentLink_RecID, ParentLink_Category, EX_CodigoCierre, Subject, EX_FirebaseID";

                bool response = await GetFilter(objeto, filter, select);
                if (response)
                {
                    _task = await DeserializeODataResponse<TaskInfo>(_responseText);
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
                    _parentTask = await DeserializeODataResponse<ParentInfo>(_responseText);
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
                    _account = await DeserializeODataResponse<AccountInfo>(_responseText);
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
                string select = "RecId, EX_IdSitio, Name, EX_Zona, EX_PlazaCobertura, EX_Latitud, EX_Longitud, Address, EX_Colonia, City, Zip, State, EX_Geohash";

                bool response = await GetFilter("Location", $"RecId eq '{recId}'", select);

                if (response)
                {
                    _location = await DeserializeODataResponse<LocationInfo>(_responseText);
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
                string select = "RecId, DisplayName, Supervisor, Title, Department, Team, Status, Disabled, PrimaryEmail, LoginId";

                bool response = await GetFilter("Employee", $"LoginID eq '{owner}'", select);

                if (response)
                {
                    _employee = await DeserializeODataResponse<EmployeeInfo>(_responseText);
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
                    _taskCatalog = await DeserializeODataResponse<TaskCatalogInfo>(_responseText);
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
                SetValuesModels(_tpRequest, _task);
                #endregion

                #region Busca Información del ParentTask
                var (successParent, respParent) = await GetParentInfo( _task?.ParentLink_RecID, _task.ParentLink_Category);
                if (!successParent)
                    return (false, respParent);
                SetValuesModels(_tpRequest, _parentTask);
                #endregion

                #region Busca Información de la Cuenta del Cliente
                var (successAccount, respAccount) = await GetAccount(_parentTask?.EX_CustID_Link_RecID);
                if (!successAccount)
                    return (false, respAccount);
                
                #region Valida existencia de customer en Trackpoint
                var (successGetCustUuid, tpCustUuid) = await GetSetTpCustomerInfo(_account);
                #endregion
                if (!successGetCustUuid) 
                    return (false, tpCustUuid);

                _tpRequest.scheduled_client_uuid = tpCustUuid;
                SetValuesModels(_tpRequest, _account);
                #endregion

                #region Busca Información del Location
                var (successLocation, respLocation) = await GetLocation(_parentTask?.EX_LocationID_Link_RecID);
                if (!successLocation)
                    return (false, respLocation);
                SetValuesModels(_tpRequest, _location);
                #endregion

                #region Busca Información del Employee
                var (successEmployee, respEmployee) = await GetEmployee(_parentTask?.Owner);
                if (!successEmployee)
                    return (false, respEmployee);
                SetValuesModels(_tpRequest, _employee);
                #endregion

                #region Busca Información del Catalogo de Tareas
                var (successTaskCat, respTaskCat) = await GetTaskCatalog(_task?.Subject);
                if (!successTaskCat)
                    return (false, respTaskCat);
                SetValuesModels(_tpRequest, _taskCatalog);
                #endregion

                #region Asignaciones puntuales
                SetPreloadValues(_task, _parentTask, _location, _account, _employee);
                _tpRequest.scheduled_name_event = String.Concat( _task.AssignmentID, " | ", _parentTask.ParentNumber, " | P", _task.Priority, " | ", _location.EX_IdSitio, " | ", _location.Name);
                _tpRequest.scheduled_expiration_date = _expirationDate!;
                _tpRequest.id_user = _tpAuth.uuid;
                #endregion

                return (true, JsonConvert.SerializeObject(_tpRequest));
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool, string)> ScheduledTask(int assignmentId)
        {
            try 
            {
                #region Obtiene Información de la Tarea
                var (successGetTask, tpRequest) = await GetTaskReq(assignmentId);
                if (!successGetTask)
                    return (false, tpRequest);
                #endregion

                #region Manda Solicitud de creación de Activity en Trackpoint
                var rootElementRequest = ConvertModelToJsonElement(_tpRequest);
                #region Habilitar para ver el JSON generado si la linea de arriba falla
                //using JsonDocument document = JsonDocument.Parse(tpRequest);
                //JsonElement rootElementRequest = document.RootElement;
                #endregion

                var (successSetActivity, resultActivity) = await _tpServices.SetActivityTP(rootElementRequest);
                var jsonObjCreated = JsonConvert.DeserializeObject<ActivityResult>(resultActivity);
                string firebaseId = jsonObjCreated?.data.firebase_id ?? "";
                if (!successSetActivity || firebaseId == "")
                    return (false, resultActivity);
                #endregion

                #region Actualiza el Task en ITSM con el FirebaseID generado
                var upData = new { EX_FirebaseID = firebaseId };
                    Console.WriteLine("Tipo de var jsonPatch: " + upData.GetType().ToString());
                var (successUpd, responseUpd) = await UpPatchITSM("Task", _preloadRequest.frmRecIdTask!, upData);
                if (!successUpd)
                    Console.WriteLine($"No se pudo actualizar el Task con el FirebaseID: {_preloadRequest.frmRecIdTask}");
                #endregion

                #region Asigna el FirebaseID al modelo de Preload a la Actividad creada
                var (successSetPreload, resultPreload) = await _tpServices.UpdActivityTP(_preloadRequest, firebaseId);
                if (!successSetPreload)
                    return (false, resultPreload);
                #endregion

                return (true, resultActivity);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool, string)> UpTask(ResponseTaskTP body)
        {
            try
            {
                if (body.data.created_api)
                {
                    body = await InfoConvert(body);
                    var (upByStatus, action, isNote, note) = JsonResponse.TaskITSM(body);
                    
                    await UpDataITSM(body, isNote, note);

                    var (successUpd, _responseText) = await UpPatchITSM("Task", body.data.preload[0].frmRecIdTask, upByStatus);
                    if (!successUpd)
                        return (successUpd, _responseText);
                }else
                {
                    body = await InfoConvert(body);
                    var parameters = await JsonResponse.TrasladoRetorno(body);

                    var result = await _dwContext.EjecutarProcedimientoAsync("EXsp_TrackPoint_TrasladoRegreso", parameters.ToArray());

                    return (true, result.ToString());
                }

                    return (true, _responseText);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        private async Task<ResponseTaskTP> InfoConvert(ResponseTaskTP body)
        {
            Models.Execon.Data data = body.data;
            Models.Execon.Preload? preload = data.preload is not null ? data.preload[0] : null;
            
            EmployeeInfo? employeeDel = new EmployeeInfo();
            EmployeeInfo? employeeUsr = new EmployeeInfo();
            string deletedBy    = data.deletedBy is "No disponible" ? "" : new MailAddress(data.deletedBy).User; 
            string user         = data.scheduled_user_email is null ? "" : new MailAddress(data.scheduled_user_email).User;

            if (deletedBy != "")
            {
                var (successEmpl, responseEmpl) = await GetEmployee(deletedBy);
                employeeDel = successEmpl ? await DeserializeODataResponse<EmployeeInfo>(responseEmpl) : null;

                body.data.deletedBy = (employeeDel is not null) ? employeeDel.DisplayName : "";
            }

            if (user != "")
            {
                var (successUsr, responseUsr) = await GetEmployee(user);
                employeeUsr = successUsr ? await DeserializeODataResponse<EmployeeInfo>(responseUsr) : null;

                body.data.scheduled_user_email = (employeeUsr is not null) ? employeeUsr.LoginId : "";
            }

            return body;
        }

        private async Task UpDataITSM(ResponseTaskTP body, bool isNote = false, NotaITSM? note = null)
        {
            Models.Execon.Data data = body.data;
            Models.Execon.Preload? preload = data.preload is not null ? data.preload[0] : null;
            
            if (preload is not null)
            {
                var (successTask, responseTask) = await GetTaskReq(preload.frmAssignmentId);

                if (data.status == "2" && data.statusInfo.txt == "Abierta")
                {
                    var (successNote, responseNote) = isNote
                        ? await SendPostITSM("/api/Obj/AddNote", note)
                        : (false, null);

                    var url = String.Concat("https://trackpoint.webpoint.mx/reportnew/?customerId=execon&orderId=", data.firebase_id);
                    await AddAttachementModel(preload!.frmRecIdTask, url, url);
                }

                if (data.status == "3")
                {
                    var (successNote, responseNote) = isNote
                        ? await SendPostITSM("/api/Obj/AddNote", note)
                        : (false, null);

                    var url = String.Concat("https://trackpoint-d2fa6.uc.r.appspot.com/getPdfNew?q=execon&order_id=", data.firebase_id);
                    await AddAttachementModel(preload!.frmRecIdTask, url, url);

                    var llegadaSitioElement = data.elements.FirstOrDefault(e => e.title == "Llegada a sitio");
                    LocationUp? locationUp = new LocationUp();

                    #region Valida actualizacion de Location
                    if (_tpRequest.scheduled_clasification_name == "SIN ZONA")
                    {
                        locationUp.EX_Zona = (data.classification_category_name != "SIN ZONA") ? data.classification_category_name : _tpRequest.scheduled_clasification_name;
                        locationUp.EX_PlazaCobertura = (data.classification_category_name != "SIN ZONA") ? data.classification_subcategory_name : _tpRequest.scheduled_subclasification_name;
                    }
                    #endregion

                    #region Asigna variables Elemento Llegada a Sitio
                    if (llegadaSitioElement is not null)
                    {
                        var infoMap = llegadaSitioElement.info;

                        var addressSite = infoMap.address ??= "";
                        locationUp.EX_Geohash = infoMap.geolocation.geohash ??= "";
                        locationUp.EX_Latitud = infoMap.geolocation.geopoint._latitude;
                        locationUp.EX_Longitud = infoMap.geolocation.geopoint._longitude;

                        var (successUpd, _responseText) = await UpPatchITSM("Location", _location.RecId, locationUp);
                    }
                    #endregion
                }
            }
        }

        private JsonElement ConvertModelToJsonElement<T>(T model) => JsonDocument.Parse(JsonConvert.SerializeObject(model)).RootElement.Clone();

        private async Task AddAttachementModel(string parentRecId, string attachName, string url)
        {
            _attachInfo.ParentLink = parentRecId;
            _attachInfo.ATTACHNAME = attachName;
            _attachInfo.URL = url;
            _attachModel.Attachment = _attachInfo;
            List<AttachementITSM> lstAtt = [_attachModel];

            await SendPostITSM("/api/Obj/Create", lstAtt);
        }

        private void SetPreloadValues(TaskInfo task, ParentInfo parent, LocationInfo location, AccountInfo account, EmployeeInfo employee)
        {
            _preloadRequest.frmRecIdTask       = task.RecId;
            _preloadRequest.frmAssignmentId    = task.AssignmentID;
            _preloadRequest.frmParentNumber    = parent.ParentNumber;
            _preloadRequest.frmParentCategory  = task.ParentLink_Category;
            _preloadRequest.frmIdSitio         = location.EX_IdSitio;
            _preloadRequest.frmCustId          = account.CustID;
            _preloadRequest.frmCodigoCierre    = task.EX_CodigoCierre;
            _preloadRequest.frmParentOwner     = employee.DisplayName;
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

        public async Task<T> DeserializeODataResponse<T>(string jsonResponse)
        {
            var odataResponse = JsonConvert.DeserializeObject<ODataResponse<T>>(jsonResponse);

            if (odataResponse?.Value == null || !odataResponse.Value.Any())
                throw new InvalidOperationException("No data found in OData response");

            return odataResponse.Value[0];
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

        public async Task<(bool, string)> SendPostITSM<T>(string metodo, T httpContent)
        {
            try
            {
                HttpClient client = CreateHttpClient();

                HttpResponseMessage response = await client.PostAsJsonAsync($"{_ambiente}{metodo}", httpContent);
                _responseText = await response.Content.ReadAsStringAsync();

                return (response.IsSuccessStatusCode, _responseText);
            }
            catch (Exception ex)
            {
                _responseText = ex.Message;
                return (false, _responseText);
            }
        }

        private async Task<(bool, string)> UpPatchITSM<T>(string objeto, string recId, T update)
        {
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(update), Encoding.UTF8, "application/json");
                NameValueCollection queryParams = HttpUtility.ParseQueryString(string.Empty);
                queryParams["objeto"] = objeto;
                queryParams["recId"] = recId;

                HttpClient client = CreateHttpClient();

                HttpResponseMessage response = await client.PatchAsync($"{_ambiente}/api/Obj/Update?{queryParams}", content);
                _responseText = await response.Content.ReadAsStringAsync();

                return (response.IsSuccessStatusCode, _responseText);
            }
            catch (Exception ex)
            {
                _responseText = ex.Message;
                return (false, _responseText);
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
    }
}
