using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SensorDataParser.Models;
using System.Data;
using System.Reflection;

namespace SensorDataParser.Parser
{
    public class GenericRepository<T> where T : class, IEntity, new()
    {
        private readonly IConfigurationRoot _configuration;
        private readonly string _tableName;

        public GenericRepository(IConfigurationRoot configuration)
        {
            _configuration = configuration;
            _tableName = typeof(T).Name;
        }

        public List<T> InsertOrGetExisting(List<T> entities)
        {
            if (entities.Count == 0)
                return entities;

            var connectionString = _configuration.GetConnectionString("MsSqlConnection")!;
            var database = _configuration.GetSection("Database").Value!;

            using var connection = new SqlConnection(connectionString);
            connection.Open();
            connection.ChangeDatabase(database);

            // Получаем информацию о свойствах модели
            var properties = typeof(T).GetProperties()
                .Where(p => p.Name != "Id" && p.CanWrite)
                .ToList();

            var columnNames = properties.Select(p => p.Name).ToList();

            // Создаем временную таблицу для bulk insert
            GenericRepository<T>.CreateTempTable(connection);

            // Bulk insert во временную таблицу
            GenericRepository<T>.BulkInsertToTemp(connection, entities, properties);

            // MERGE операция для вставки или получения существующих записей
            var result = MergeAndGetIds(connection, entities, columnNames);

            GenericRepository<T>.CleanupTempTable(connection);

            return result;
        }

        private static void CreateTempTable(SqlConnection connection)
        {
            var createTempTableSql = $@"
            CREATE TABLE #TempEntities (
                TempId INT IDENTITY(1,1),
                {string.Join(", ", typeof(T).GetProperties()
                        .Where(p => p.Name != "Id")
                        .Select(p => $"{p.Name} {GenericRepository<T>.GetSqlType(p)}"))},
                SourceIndex INT
            )";

            using var command = new SqlCommand(createTempTableSql, connection);
            command.ExecuteNonQuery();
        }

        private static void BulkInsertToTemp(SqlConnection connection, List<T> entities,
            List<PropertyInfo> properties)
        {
            using var bulkCopy = new SqlBulkCopy(connection)
            {
                BulkCopyTimeout = 0,
                DestinationTableName = "#TempEntities"
            };

            // Добавляем маппинг колонок
            foreach (var property in properties)
            {
                bulkCopy.ColumnMappings.Add(property.Name, property.Name);
            }
            bulkCopy.ColumnMappings.Add("SourceIndex", "SourceIndex");

            // Создаем DataTable для bulk insert
            var dataTable = new DataTable();
            foreach (var property in properties)
            {
                Type colType = property.PropertyType;
                if (property.PropertyType.IsGenericType &&
                    property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    colType = Nullable.GetUnderlyingType(property.PropertyType)!;
                }
                dataTable.Columns.Add(property.Name, colType);
            }
            dataTable.Columns.Add("SourceIndex", typeof(int));

            // Заполняем DataTable
            for (int i = 0; i < entities.Count; i++)
            {
                var row = dataTable.NewRow();
                foreach (var property in properties)
                {
                    row[property.Name] = property.GetValue(entities[i]) ?? DBNull.Value;
                }
                row["SourceIndex"] = i;
                dataTable.Rows.Add(row);
            }

            bulkCopy.WriteToServer(dataTable);
        }

        private List<T> MergeAndGetIds(SqlConnection connection, List<T> entities, List<string> columnNames)
        {
            var conditions = new List<string>();
            foreach (var column in columnNames)
            {
                conditions.Add($@"
            (
                (target.{column} IS NULL AND source.{column} IS NULL) OR
                (target.{column} IS NOT NULL AND source.{column} IS NOT NULL AND target.{column} = source.{column})
            )");
            }

            var mergeSql = $@"
            MERGE {_tableName} WITH (HOLDLOCK) AS target
            USING #TempEntities AS source
            ON {string.Join(" AND ", conditions)}
            WHEN NOT MATCHED BY TARGET THEN
                INSERT ({string.Join(", ", columnNames)})
                VALUES ({string.Join(", ", columnNames.Select(c => $"source.{c}"))})
            WHEN MATCHED THEN
                UPDATE SET target.{columnNames[0]} = target.{columnNames[0]}
            OUTPUT inserted.Id, source.SourceIndex;";


            var result = new Dictionary<int, int>(); // SourceIndex -> Id

            using var command = new SqlCommand(mergeSql, connection)
            {
                CommandTimeout = 0
            };
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var id = reader.GetInt32(0);
                var sourceIndex = reader.GetInt32(1);
                result[sourceIndex] = id;
            }
            reader.Close();

            // Обновляем ID в исходных объектах
            foreach (var kvp in result)
            {
                entities[kvp.Key].Id = kvp.Value;
            }

            return entities;
        }

        private static void CleanupTempTable(SqlConnection connection)
        {
            using var command = new SqlCommand("DROP TABLE #TempEntities", connection);
            command.ExecuteNonQuery();
        }

        private static string GetSqlType(PropertyInfo property)
        {
            var type = property.PropertyType;
            if (property.PropertyType.IsGenericType &&
                property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                type = Nullable.GetUnderlyingType(property.PropertyType)!;
            }

            if (type == typeof(string)) return "NVARCHAR(MAX)";
            if (type == typeof(int)) return "INT";
            if (type == typeof(long)) return "BIGINT";
            if (type == typeof(decimal)) return "DECIMAL(18,2)";
            if (type == typeof(DateTime)) return "DATETIME";
            if (type == typeof(bool)) return "BIT";
            if (type == typeof(double)) return "FLOAT";
            if (type == typeof(float)) return "REAL";
            if (type == typeof(Guid)) return "UNIQUEIDENTIFIER";

            return "NVARCHAR(MAX)";
        }
    }
}