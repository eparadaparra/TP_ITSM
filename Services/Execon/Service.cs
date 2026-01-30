using Microsoft.Extensions.Logging;
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
    public class Service : IExeconServices
    {
        #region Declaración de Variables
        private string? _url;
        private string? _ambiente;
        private string _responseText;

        private int _expirationDate;
        private int _timeOutValue;

        private List<string> _lstLogEvent = [];
        private StringBuilder sb = new StringBuilder().Append('*', 30).Append(" ").Append(DateTime.Now.ToString()).Append(" ").Append('*', 30);

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
        private readonly ITrackpointServices _tpServices;
        private readonly ILogger<Service> _logger;
        #endregion

        public Service(ITrackpointServices services, ConnIVANTIDW dwContext, ILogger<Service> logger)
        {
            _tpServices = services;
            _dwContext  = dwContext;
            _logger     = logger;

            //_logger.LogInformation(sb.ToString());
            _logger.LogInformation("Inicializando Execon Services");

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            _url = builder.GetSection("HttpClient:url").Value;
            _ambiente = Boolean.Parse(builder.GetSection("SettingsExecon:EnableDev").Value!)
                    ? builder.GetSection("HttpClient:dev").Value
                    : builder.GetSection("HttpClient:pro").Value;
            _expirationDate = Int32.Parse(builder.GetSection("SettingsExecon:ExpirationDate").Value!);
            _timeOutValue   = Int32.Parse(builder.GetSection("SettingsExecon:TimeOutSeconds").Value!);

            _logger.LogInformation($"Configuración cargada | Ambiente: {_ambiente} | Timeout: {_timeOutValue}'s");
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

                _logger.LogDebug($"GetTask filtro | Objeto: {objeto} | Buscar: {select}");

                bool response = await GetFilter(objeto, filter, select);

                if (!response)
                {
                    _logger.LogWarning("GetTask sin respuesta válida | AssignmentId: {AssignmentId}", assignmentId);
                    return (false, $"   ! No se pudo obtener la información de {objeto}");
                }

                _task = await DeserializeODataResponse<TaskInfo>(_responseText);

                _logger.LogInformation($"   ✓ GetTask exitoso | RecId: {_task.RecId} | Priority: {_task.Priority}");

                return (true, _responseText);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "   × Error en GetTask | AssignmentId: {AssignmentId}", assignmentId);
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

                _logger.LogDebug($"Get{objName}Info | RecId: {recId} | Buscar: {select}");

                bool response = await GetFilter(objName, $"RecId eq '{recId}'", select);

                if (!response)
                {
                    _logger.LogWarning($"   ! Get{objName}Info sin respuesta válida | RecId: {recId}");
                    return (false, $"No se pudo obtener la información de {objName}");
                }

                _parentTask = await DeserializeODataResponse<ParentInfo>(_responseText);
                
                _logger.LogInformation($"   ✓ Get{objName}Info exitoso | {objName}: {_parentTask.ParentNumber} | RecId: {recId} | Subject: {_parentTask.Subject}");

                return (response, _responseText);
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"   × Error en Get{objName}Info | recId: {recId}");
                return (false, ex.Message);
            }
        }

        public async Task<(bool, string)> GetAccount(string recId)
        {
            try
            {
                string select = "RecId, CustID, Name";

                _logger.LogDebug($"GetAccount | RecId: {recId} | Buscar: {select}");

                bool response = await GetFilter("Account", $"RecId eq '{recId}'", select);

                if (!response)
                {
                    _logger.LogWarning($"   ! GetAccount sin respuesta válida | RecId: {recId}");
                    return (false, $"No se pudo obtener la información del Account");
                }

                _account = await DeserializeODataResponse<AccountInfo>(_responseText);

                _logger.LogInformation($"   ✓ GetAccount exitoso | CustId: {_account.CustID} | Cliente: {_account.Name}");

                return (response, _responseText);                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"   × Error en GetAccount | recId: {recId}");
                return (false, ex.Message);
            }
        }

        public async Task<(bool, string)> GetLocation(string recId)
        {
            try
            {
                string select = "RecId, EX_IdSitio, Name, EX_Zona, EX_PlazaCobertura, EX_Latitud, EX_Longitud, Address, EX_Colonia, City, Zip, State, EX_Geohash";

                _logger.LogDebug($"GetLocation | RecId: {recId} | Buscar: {select}");

                bool response = await GetFilter("Location", $"RecId eq '{recId}'", select);

                if (!response)
                {
                    _logger.LogWarning($"   ! GetLocation sin respuesta válida | RecId: {recId}");
                    return (false, $"No se pudo obtener la información del Location");
                }
                
                _location = await DeserializeODataResponse<LocationInfo>(_responseText);

                _logger.LogInformation($"   ✓ GetLocation exitoso | IdSitio: {_location.EX_IdSitio} | Sitio: {_location.Name}");

                return (response, _responseText);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"   × Error en GetLocation | recId: {recId}");
                return (false, ex.Message);
            }
        }

        public async Task<(bool, string)> GetEmployee(string owner)
        {
            try
            {
                string select = "RecId, DisplayName, Supervisor, Title, Department, Team, Status, Disabled, PrimaryEmail, LoginId";

                _logger.LogDebug($"GetEmployee | Owner: {owner} | Buscar: {select}");

                bool response = await GetFilter("Employee", $"LoginID eq '{owner}'", select);

                if (!response)
                {
                    _logger.LogWarning($"   ! GetEmployee sin respuesta válida | Owner: {owner}");
                    return (false, $"No se pudo obtener la información del Employee");
                }

                _employee = await DeserializeODataResponse<EmployeeInfo>(_responseText);

                _logger.LogInformation($"   ✓ GetEmployee exitoso | Nombre: {_employee.DisplayName} | Puesto: {_employee.Title}");

                return (response, _responseText);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"   × Error en GetEmployee | Owner: {owner}");
                return (false, ex.Message);
            }
        }

        public async Task<(bool, string)> GetTaskCatalog(string taskSubject)
        {
            try
            {
                string select = "RecId, TaskSubject, EX_IdActividadTP&$top=1";

                _logger.LogDebug($"GetTaskCatalog | Tipo de Tarea: {taskSubject} | Buscar: {select}");

                bool response = await GetFilter("TaskCatalog__Assignment", $"TaskSubject eq '{taskSubject}'", select);

                if (!response)
                {
                    _logger.LogWarning($"   ! GetTaskCatalog sin respuesta válida | Tipo de Tarea: {taskSubject}");
                    return (false, $"No se pudo obtener la información del Location");
                }
                
                _taskCatalog = await DeserializeODataResponse<TaskCatalogInfo>(_responseText);

                _logger.LogInformation($"   ✓ GetTaskCatalog exitoso | idTrackpoint: {_taskCatalog.EX_IdActividadTP} | Tipo de Tarea: {_taskCatalog.TaskSubject}");

                return (response, _responseText);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"   × Error en GetTaskCatalog | Tipo de Tarea: {taskSubject}");
                return (false, ex.Message);
            }
        }

        public async Task<(bool, string)> GetTaskReq(int assignmentId)
        {
            _logger.LogInformation($"-> GetTaskReq iniciado | AssignmentId: {assignmentId}");

            try
            {
                _tpAuth = await _tpServices.GetToken();

                #region Busca Información de Task
                var (successTask, _) = await GetTask(assignmentId);
                if (!successTask)
                    return (false, "Error obteniendo la informción de la Tarea");
                SetValuesModels(_tpRequest, _task);
                #endregion

                #region Busca Información del ParentTask
                var (successParent, _) = await GetParentInfo( _task?.ParentLink_RecID, _task.ParentLink_Category);
                if (!successParent)
                    return (false, "Error obteniendo la informción del Parent");
                SetValuesModels(_tpRequest, _parentTask);
                #endregion

                #region Busca Información de la Cuenta del Cliente
                var (successAccount, _) = await GetAccount(_parentTask?.EX_CustID_Link_RecID);
                if (!successAccount)
                    return (false, "Error obteniendo la informción del Cliente");

                #region Valida existencia de customer en Trackpoint
                var (successGetCustUuid, tpCustUuid) = await GetSetTpCustomerInfo(_account);
                if (!successGetCustUuid)
                    return (false, "Error obteniendo la informción del Cliente en Trackpoint");
                #endregion
                _tpRequest.scheduled_client_uuid = tpCustUuid;
                SetValuesModels(_tpRequest, _account);
                #endregion

                #region Busca Información del Location
                var (successLocation, _) = await GetLocation(_parentTask?.EX_LocationID_Link_RecID);
                if (!successLocation)
                    return (false, "Error obteniendo la informción del Sitio");
                SetValuesModels(_tpRequest, _location);
                #endregion

                #region Busca Información del Employee
                var (successEmployee, _) = await GetEmployee(_parentTask?.Owner);
                if (!successEmployee)
                    return (false, "Error obteniendo la informción del Empleado");
                SetValuesModels(_tpRequest, _employee);
                #endregion

                #region Busca Información del Catalogo de Tareas
                var (successTaskCat, _) = await GetTaskCatalog(_task?.Subject);
                if (!successTaskCat)
                    return (false, "Error obteniendo la informción del Catálogo de Tareas");
                SetValuesModels(_tpRequest, _taskCatalog);
                #endregion

                #region Asignaciones puntuales
                SetPreloadValues(_task, _parentTask, _location, _account, _employee);

                _tpRequest.id_user = _tpAuth.uuid;
                _tpRequest.scheduled_expiration_date = _expirationDate!;
                _tpRequest.scheduled_name_event = String.Concat( _task.AssignmentID, " | ", _parentTask.ParentNumber, " | P", _task.Priority, " | ", _location.EX_IdSitio, " | ", _location.Name);

                #endregion

                _logger.LogInformation("✓ GetTaskReq finalizado correctamente");
                _logger.LogDebug("Payload Trackpoint generado {@Request}", _tpRequest);
                return (true, JsonConvert.SerializeObject(_tpRequest));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "× Error crítico en GetTaskReq | AssignmentId: {AssignmentId}", assignmentId);
                return (false, ex.Message);
            }
        }

        public async Task<(bool, string)> ScheduledTask(int assignmentId)
        {
            _logger.LogInformation("ScheduledTask iniciado...");

            try
            {
                #region Obtiene Información de la Tarea
                var (successGetTaskReq, _) = await GetTaskReq(assignmentId);
                if (!successGetTaskReq)
                {
                    _logger.LogInformation("× GetTaskReq finalizado | Error generando al obtener información de la solicitud");
                    return (false, "Error generando al obtener información de la solicitud"); 
                }
                #endregion

                #region Manda Solicitud de creación de Activity en Trackpoint
                var rootElementRequest = ConvertModelToJsonElement(_tpRequest); //Convierte Modelo a JElement

                _logger.LogInformation("-> *SetActivityTP iniciado | Enviando Solicitud a Trackpoint...");
                var (successSetActivity, resultActivity) = await _tpServices.SetActivityTP(rootElementRequest);

                if (!successSetActivity)
                {
                    _logger.LogWarning("   ! Error creando Activity en Trackpoint | Response: {Response}", resultActivity);
                    return (false, resultActivity);
                }
                var jsonObjCreated = JsonConvert.DeserializeObject<ActivityResult>(resultActivity);
                string firebaseId = jsonObjCreated?.data.firebase_id ?? "";
                _logger.LogInformation("✓ *SetActivityTP finalizado exitosamente");
                #endregion

                #region Actualiza el Task en ITSM con el FirebaseID generado
                var upData = new { EX_FirebaseID = firebaseId }; //Console.WriteLine("Tipo de var jsonPatch: " + upData.GetType().ToString());
                _logger.LogInformation($"-> UpPatchITSM iniciado...");
                var (successUpd, _) = await UpPatchITSM("Task", _preloadRequest.frmRecIdTask!, upData);
                if (!successUpd)
                    _logger.LogWarning($"   ! No se pudo actualizar la Task {assignmentId} con el FirebaseID: {_preloadRequest.frmRecIdTask}");

                _logger.LogInformation($"   ✓ UpPatchITSM finalizado correctamente | Actualiza FirebaseId: {_preloadRequest.frmRecIdTask!} en Tarea: {assignmentId}");
                #endregion

                #region Asigna el FirebaseID al modelo de Preload a la Actividad creada
                _logger.LogInformation("-> *UpdActivityTP iniciado | Enviando Preload a Trackpoint...");
                var (successSetPreload, resultPreload) = await _tpServices.UpdActivityTP(_preloadRequest, firebaseId);

                if (!successSetPreload)
                {
                    _logger.LogWarning("   ! Error en *UpdActivityTP, Preload en Trackpoint | Response: {Response}", resultPreload);
                    return (false, resultPreload);
                }
                _logger.LogInformation("✓ *UpdActivityTP finalizado exitosamente");
                #endregion

                _logger.LogInformation("ScheduledTask finalizado exitosamente");
                return (true, resultActivity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "   × Error en ScheduledTask | AssignmentId: {AssignmentId}", assignmentId);
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
                _logger.LogDebug($"GetFilter | Objeto: {objeto} | Filter: {filter}");

                NameValueCollection queryParams = HttpUtility.ParseQueryString(string.Empty);
                queryParams["objeto"] = objeto;
                queryParams["filter"] = filter;
                queryParams["select"] = select;

                HttpClient client = CreateHttpClient();

                HttpResponseMessage response = await client.GetAsync($"{_ambiente}/api/Obj/Filter?{queryParams}");

                if (!response.IsSuccessStatusCode)
                    _logger.LogWarning($"GetFilter {objeto} falló | Status: {response.StatusCode}");

                _responseText = await response.Content.ReadAsStringAsync();

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetFilter | Objeto: {Objeto}", objeto);
                _responseText = ex.Message;
                return false;
            }
        }

        public async Task<(bool, string)> SendPostITSM<T>(string metodo, T httpContent)
        {
            try
            {
                _logger.LogDebug($"SendPostITSM | Objeto: {metodo} | Content: {httpContent}");

                HttpClient client = CreateHttpClient();

                HttpResponseMessage response = await client.PostAsJsonAsync($"{_ambiente}{metodo}", httpContent);
                if (!response.IsSuccessStatusCode)
                    _logger.LogWarning($"SendPostITSM {metodo} falló | Status: {response.StatusCode}");

                _responseText = await response.Content.ReadAsStringAsync();
                return (response.IsSuccessStatusCode, _responseText);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error en SendPostITSM | Método: {metodo} | Content: {httpContent}");
                _responseText = ex.Message;
                return (false, _responseText);
            }
        }

        private async Task<(bool, string)> UpPatchITSM<T>(string objeto, string recId, T update)
        {
            try
            {
                _logger.LogDebug($"UpPatchITSM | Objeto: {objeto} | recId: {recId}");
                HttpClient client = CreateHttpClient();
                
                var content = new StringContent(JsonConvert.SerializeObject(update), Encoding.UTF8, "application/json");
                NameValueCollection queryParams = HttpUtility.ParseQueryString(string.Empty);
                queryParams["objeto"] = objeto;
                queryParams["recId"] = recId;

                HttpResponseMessage response = await client.PatchAsync($"{_ambiente}/api/Obj/Update?{queryParams}", content);

                if (!response.IsSuccessStatusCode)
                    _logger.LogWarning($"UpPatchITSM {objeto} falló | Status: {response.StatusCode}");
                
                _responseText = await response.Content.ReadAsStringAsync();
                return (response.IsSuccessStatusCode, _responseText);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error en UpPatchITSM | Objeto: {objeto} | RecId: {recId} | Update: {update}");
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
