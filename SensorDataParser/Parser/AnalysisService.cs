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

        // История вибрации для конкретной точки замера
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
    }
}