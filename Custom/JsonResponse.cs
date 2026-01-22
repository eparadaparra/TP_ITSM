using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Mail;
using System.Text;
using TP_ITSM.Models;
using TP_ITSM.Models.Execon;

namespace TP_ITSM.Custom
{
    public class JsonResponse
    {
        public static JObject TaskITSM(ResponseTaskTP body)
        {
            var data = body.data;
            NotaITSM nota = new NotaITSM();

            var statusITSM = Utilities.GetStatusMap(data.status, data.statusInfo is null ? "Programada" : data.statusInfo.txt);
            var user = data.scheduled_user_email is null ? "" : new MailAddress(data.scheduled_user_email).User;
            var zona = data.classification_category_name;
            var plazaCobertura = data.classification_subcategory_name;

            var jsonUp = new JObject();

            if (data.status == "1" && data.statusInfo?.txt == "Abierta")
            {
                jsonUp.Add("EX_FirebaseID"   , data.firebase_id);
                jsonUp.Add("Owner"           , user);
                jsonUp.Add("Status"          , statusITSM);
                jsonUp.Add("Details"         , data.scheduled_instructions);
                jsonUp.Add("PlannedStartDate", data.scheduled_programming_dateTimeOffset);
                jsonUp.Add("AssignedDateTime", DateTimeOffset.UtcNow);
            }

            if ((data.status is "2" or "3") && (data.statusInfo?.txt is "Abierta" or "En Revisión"))
            {
                jsonUp.Add("EX_FirebaseID"   , data.firebase_id);
                jsonUp.Add("Owner"           , user);
                jsonUp.Add("Status"          , statusITSM);
                jsonUp.Add("Details"         , data.scheduled_instructions);
                jsonUp.Add("PlannedStartDate", data.scheduled_programming_dateTimeOffset);
                jsonUp.Add("AcknowledgedBy"  , user);

                //TODO: Insertar Attachments con Link de detalle de TP
            }

            if (data.status == "3" && (data.statusInfo?.txt is "Autorizada" or "Cerrada"))
            {
                #region Declara variables y obtiene elementos necesarios para cierre de actividad
                var llegadaSitioElement     = data.elements.FirstOrDefault(e => e.title == "Llegada a sitio");
                var cierreActividadElement  = data.elements.FirstOrDefault(e => e.title == "Cierre de actividad");
                var codigoCierreElement     = data.elements.FirstOrDefault(e => e.title == "Código de cierre");

                var subStatusTask           = String.Empty;
                var comentariosCierre       = String.Empty;
                var codigoCierre            = String.Empty;
                var quienProporcionoCierre  = String.Empty;

                var geoHashSite             = String.Empty;
                var addressSite             = String.Empty;
                double latSite   = 0;
                double lonSite   = 0;
                int durationMint = Utilities.DiferenciaMinutos(data.start_date_utc_dateTimeOffset, data.end_date_utc_dateTimeOffset);

                bool isCC = false;

                DateTimeOffset checkInSite         = DateTimeOffset.Now;
                DateTimeOffset startDateTask       = data.start_date_utc_dateTimeOffset;
                DateTimeOffset programmingDateTask = data.scheduled_programming_dateTimeOffset;
                DateTimeOffset endDateTask         = data.end_date_utc_dateTimeOffset;

                #endregion

                #region Asigna variables Elemento Codigo de Cierre
                if (codigoCierreElement is not null)
                {
                    foreach (var item in codigoCierreElement.items)
                    {
                        string valor = item.value?.ToString() ?? string.Empty;
                        switch (item.title.ToLowerInvariant())
                        {
                            case "resolución de tarea":
                                subStatusTask = valor;
                                break;
                            case "comentarios":
                                comentariosCierre = valor;
                                break;
                            case "código de cierre":
                                isCC = true;
                                codigoCierre = valor;
                                break;
                            case "nombre de quién proporcionó el código de cierre":
                                quienProporcionoCierre = valor;
                                break;
                        }
                    }
                }
                #endregion

                #region Asigna variables Elemento Cierre de Actividad
                if (cierreActividadElement is not null)
                {
                    foreach (var item in cierreActividadElement.items)
                    {
                        string valor = item.value?.ToString() ?? string.Empty;
                        switch (item.title.ToLowerInvariant())
                        {
                            case "resolución de tarea":
                                subStatusTask = valor;
                                break;
                            case "comentarios":
                                comentariosCierre = valor;
                                break;
                        }
                    }
                }
                #endregion

                #region Asigna variables Elemento Llegada a Sitio
                if (llegadaSitioElement is not null)
                {
                    var infoMap = llegadaSitioElement.info;

                    checkInSite = infoMap.check_date_utc_dateTimeOffset;
                    addressSite = infoMap.address ??= "";
                    geoHashSite = infoMap.geolocation.geohash ??= "";
                    latSite     = infoMap.geolocation.geopoint._latitude;
                    lonSite     = infoMap.geolocation.geopoint._longitude;
                }
                #endregion

                jsonUp.Add("EX_FirebaseID"      , data.firebase_id);
                jsonUp.Add("Status"             , statusITSM);
                jsonUp.Add("EX_SubStatusTask"   , subStatusTask);
                jsonUp.Add("ResolvedBy"         , "InternalServices");
                jsonUp.Add("PlannedStartDate"   , programmingDateTask);
                jsonUp.Add("StartDate"          , startDateTask);
                jsonUp.Add("ActualStartDate"    , checkInSite);
                jsonUp.Add("ActualEndDate"      , endDateTask);
                jsonUp.Add("ActualEffort"       , durationMint);       //Tarea en minutos, desde que se Inicia la Actividad en TP hasta que finaliza
                jsonUp.Add("CompletedDateTime"  , endDateTask);
                jsonUp.Add("ResolvedDateTime"   , endDateTask);
                jsonUp.Add("PlannedEndDate"     , endDateTask);

                //TODO: Insertar Attachments con Link de PDF de TP
                //TODO: Insertar Comentarios de Cierre en JOURNAL
            }

            if ((data.status is "0" or "4" or "5" or "6") && (data.statusInfo?.txt is "Cancelada" or "Archivada" or "Vencida" or "Eliminada"))
            {
                #region Declara variables y obtiene elementos necesarios para cierre de actividad

                DateTimeOffset programmingDateTask = data.scheduled_programming_dateTimeOffset;
                var deleteBy = data.status == "6" ? new MailAddress(data.deletedBy).User : "";
                var subject = String.Concat(data.statusInfo?.txt, " - Comentarios de Cancelacion");
                var note    = data.status switch
                {
                    "4" => "Tarea archivada en Trackpoint",
                    "5" => "Tarea cancelada por fecha de vencimiento en Trackpoint",
                    "6" => $"Tarea eliminada en Trackpoint por {deleteBy}",
                    _ => "No especificado"
                };

                nota.Object       = "Task";
                nota.objectNumber = data.preload[0].frmAssignmentId.ToString();
                nota.subject      = subject;
                nota.note         = note;

                string uri = "/Ivanti/api/Obj/AddNote";
                StringContent httpContent = new StringContent(JsonConvert.SerializeObject(nota), Encoding.UTF8, "application/json");

                #endregion

                jsonUp.Add("EX_FirebaseID"      , data.firebase_id);
                jsonUp.Add("Status"             , statusITSM);
                jsonUp.Add("ResolvedBy"         , "InternalServices");
                jsonUp.Add("PlannedStartDate"   , programmingDateTask);

                //TODO: Insertar Comentarios de Cierre en JOURNAL
            }

            return jsonUp;
        }

    }
}
