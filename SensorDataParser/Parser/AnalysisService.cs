using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Globalization;

namespace SensorDataParser.Parser
{
    public class TrendPoint
    {
        public DateTime Date { get; set; }
        public double RMS { get; set; }
    }

    public class AnalysisService
    {
        private readonly IConfigurationRoot _configuration;

        public AnalysisService(IConfigurationRoot configuration)
        {
            _configuration = configuration;
        }

        // 1. Получение тренда для конкретной точки
        public List<TrendPoint> GetTrendData(int pointId)
        {
            var result = new List<TrendPoint>();
            var connectionString = _configuration.GetConnectionString("MsSqlConnection")!;
            var database = _configuration.GetSection("Database").Value!;

            using var connection = new SqlConnection(connectionString);
            connection.Open();
            connection.ChangeDatabase(database);

            string sql = @"
                SELECT R.Date, R.RMS 
                FROM Record R
                JOIN Schedule S ON R.SScheduleID = S.Id
                JOIN Axis A ON S.SAxisID = A.Id
                WHERE A.SPointID = @PointId
                ORDER BY R.Date ASC";

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@PointId", pointId);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                string rawDate = reader["Date"].ToString() ?? "";
                string rawRms = reader["RMS"].ToString() ?? "";

                DateTime dt = DateTime.Now;
                if (rawDate.Length >= 19)
                {
                    string cleanDate = rawDate.Substring(0, 19);
                    DateTime.TryParseExact(cleanDate, "yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt);
                }

                double rms = 0;
                rawRms = rawRms.Replace(',', '.');
                double.TryParse(rawRms, NumberStyles.Any, CultureInfo.InvariantCulture, out rms);

                if (rms > 0)
                {
                    result.Add(new TrendPoint { Date = dt, RMS = rms });
                }
            }
            return result;
        }

        // 2. Многокритериальный параметрический поиск по всей базе
        public List<dynamic> SearchPointsByParameters(double? minRms, double? maxRms, DateTime? fromDate, DateTime? toDate, string machineSearch)
        {
            var results = new List<dynamic>();
            var connectionString = _configuration.GetConnectionString("MsSqlConnection")!;
            var database = _configuration.GetSection("Database").Value!;

            using var connection = new SqlConnection(connectionString);
            connection.Open();
            connection.ChangeDatabase(database);

            // Динамический SQL-запрос с иерархией оборудования
            string sql = @"
                SELECT R.Date, M.Name as MachineName, P.Name as PointName, R.RMS, P.Id as PointId
                FROM Record R
                JOIN Schedule S ON R.SScheduleID = S.Id
                JOIN Axis A ON S.SAxisID = A.Id
                JOIN Point P ON A.SPointID = P.Id
                JOIN Machine M ON P.SMachineID = M.Id
                WHERE 1=1";

            if (minRms.HasValue) sql += " AND TRY_CAST(REPLACE(R.RMS, ',', '.') AS FLOAT) >= @minRms";
            if (maxRms.HasValue) sql += " AND TRY_CAST(REPLACE(R.RMS, ',', '.') AS FLOAT) <= @maxRms";
            if (fromDate.HasValue) sql += " AND TRY_CAST(SUBSTRING(R.Date, 1, 10) AS DATETIME) >= @fromDate";
            if (toDate.HasValue) sql += " AND TRY_CAST(SUBSTRING(R.Date, 1, 10) AS DATETIME) <= @toDate";
            if (!string.IsNullOrEmpty(machineSearch)) sql += " AND M.Name LIKE @search";

            sql += " ORDER BY R.Date DESC";

            using var command = new SqlCommand(sql, connection);
            if (minRms.HasValue) command.Parameters.AddWithValue("@minRms", minRms.Value);
            if (maxRms.HasValue) command.Parameters.AddWithValue("@maxRms", maxRms.Value);
            if (fromDate.HasValue) command.Parameters.AddWithValue("@fromDate", fromDate.Value.Date);
            if (toDate.HasValue) command.Parameters.AddWithValue("@toDate", toDate.Value.Date.AddDays(1).AddSeconds(-1));
            if (!string.IsNullOrEmpty(machineSearch)) command.Parameters.AddWithValue("@search", "%" + machineSearch + "%");

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                results.Add(new
                {
                    Дата = reader["Date"].ToString(),
                    Оборудование = reader["MachineName"].ToString(),
                    Точка = reader["PointName"].ToString(),
                    Вибрация = Convert.ToDouble(reader["RMS"]).ToString("F2"),
                    PointId = Convert.ToInt32(reader["PointId"])
                });
            }
            return results;
        }
    }
}