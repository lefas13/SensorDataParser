using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SensorDataParser.Models;
using System.Windows.Forms.DataVisualization.Charting;

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

        // Получаем историю вибрации для конкретной точки замера
        public List<TrendPoint> GetTrendData(int pointId)
        {
            var result = new List<TrendPoint>();
            var connectionString = _configuration.GetConnectionString("MsSqlConnection")!;
            var database = _configuration.GetSection("Database").Value!;

            using var connection = new SqlConnection(connectionString);
            connection.Open();
            connection.ChangeDatabase(database);

            // ИСПРАВЛЕННЫЙ ЗАПРОС: Record -> Schedule -> Axis -> Point
            string sql = @"
        SELECT R.Date, R.RMS 
        FROM Record R
        JOIN Schedule S ON R.SScheduleID = S.Id
        JOIN Axis A ON S.SAxisID = A.Id
        WHERE A.SPointID = @PointId
        AND R.RMS IS NOT NULL
        ORDER BY R.Date ASC";

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@PointId", pointId);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                if (DateTime.TryParse(reader["Date"].ToString(), out DateTime dt))
                {
                    result.Add(new TrendPoint
                    {
                        Date = dt,
                        RMS = Convert.ToDouble(reader["RMS"])
                    });
                }
            }
            return result;
        }
    }
}