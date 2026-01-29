using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;

namespace TP_ITSM.Data;
public class ConnIVANTIDW : DbContext
{
    public ConnIVANTIDW(DbContextOptions<ConnIVANTIDW> options)
            : base(options)
    {
    }
    public async Task<JObject> EjecutarProcedimientoAsync(string nombreProcedimiento, params SqlParameter[] parametros)
    {
        await using var connection = this.Database.GetDbConnection();
        await using var command = connection.CreateCommand();

        command.CommandText = nombreProcedimiento;
        command.CommandType = CommandType.StoredProcedure;

        if (parametros?.Length > 0)
            command.Parameters.AddRange(parametros);

        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();

        var response = new JObject();

        try
        {
            // 👇 PRIMERO intentamos leer resultados
            await using var reader = await command.ExecuteReaderAsync();

            if (!reader.HasRows)
            {
                response["success"] = true;
                response["rowsAffected"] = reader.RecordsAffected;
                response["data"] = new JArray();
                return response;
            }

            var resultSets = new JArray();

            do
            {
                var table = new DataTable();
                table.Load(reader);

                resultSets.Add(
                    JArray.Parse(JsonConvert.SerializeObject(table))
                );

            } while (await reader.NextResultAsync());

            response["success"] = true;
            response["rowsAffected"] = reader.RecordsAffected;
            response["data"] = resultSets;

            return response;
        }
        catch (InvalidOperationException)
        {
            // 👇 SP sin SELECT → ExecuteNonQuery
            var rows = await command.ExecuteNonQueryAsync();

            response["success"] = true;
            response["rowsAffected"] = rows;
            response["data"] = new JArray();

            return response;
        }



        //using (var command = this.Database.GetDbConnection().CreateCommand())
        //{
        //    command.CommandText = nombreProcedimiento;
        //    command.CommandType = System.Data.CommandType.StoredProcedure;

        //    if (parametros != null && parametros.Length > 0)
        //    {
        //        command.Parameters.AddRange(parametros);
        //    }

        //    await this.Database.OpenConnectionAsync();

        //    var filasAfectadas = await command.ExecuteNonQueryAsync();

        //    return new JObject
        //    {
        //        ["success"] = true,
        //        ["rowsAffected"] = filasAfectadas
        //    };
        //}
    }
}
