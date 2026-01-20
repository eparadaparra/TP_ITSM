using System;

namespace TP_ITSM.Custom
{
    public class Utilities
    {
        public class utcToDatetime
        {
            public static string castUtcDatetime(long seconds = 0, long nanoseconds = 0)
            {
                // Datos de la línea JSON
                //long seconds = 1727137428;
                //int nanoseconds = 557000000;

                // Convertir los segundos en un DateTimeOffset a partir del 1 de enero de 1970 (Unix Epoch)
                DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(seconds);

                // Agregar los nanosegundos como una fracción de segundo
                dateTimeOffset = dateTimeOffset.AddTicks(nanoseconds / 100); // 1 tick = 100 nanosegundos

                // Convertir a string (puedes cambiar el formato según lo que necesites)
                //string formattedDate = dateTimeOffset.ToString("yyyy-MM-dd HH:mm:ss");

                // Formato ISO 8601 con 'Z' indicando UTC
                string formattedDate = dateTimeOffset.ToString("yyyy-MM-ddTHH:mm:ssZ");

                // Imprimir la fecha formateada
                Console.WriteLine(formattedDate);
                return formattedDate;
            }
        }

        #region Asigna Nombre de Estatus basado en su Id de Estatus
        public static string GetStatusMap(string idStatus, string statusName)
        {
            return (idStatus, statusName) switch
            {
                ("1", "Programada") => "Logged",
                ("1", "Abierta")    => "Assigned",
                ("2", "Abierta")    => "Accepted",
                ("2", "Rechazada")  => "Accepted",
                ("3", "En Revisión")=> "Waiting",
                ("3", "Autorizada") => "Completed",
                ("2", "Cerrada")    => "Completed",
                ("4", _) or ("5", _) or ("6", _) => "Cancelled",
                _ => ""
            };
        }
        #endregion
    }
}
