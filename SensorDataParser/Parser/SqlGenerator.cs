using SensorDataParser.Parser.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;

namespace SensorDataParser.Parser
{
    public static class SqlGenerator
    {
        // Сопоставление типов C# -> SQL
        private static readonly Dictionary<Type, string> TypeMap = new()
        {
            { typeof(int), "INT" },
            { typeof(long), "BIGINT" },
            { typeof(short), "SMALLINT" },
            { typeof(byte), "TINYINT" },
            { typeof(bool), "BIT" },
            { typeof(string), "NVARCHAR(MAX)" }, // можно настроить длину
            { typeof(char), "NCHAR(1)" },
            { typeof(decimal), "DECIMAL(18,2)" },
            { typeof(double), "FLOAT" },
            { typeof(float), "REAL" },
            { typeof(DateTime), "DATETIME2" },
            { typeof(DateTime?), "DATETIME2" },
            { typeof(Guid), "UNIQUEIDENTIFIER" },
            { typeof(byte[]), "VARBINARY(MAX)" },
            // Добавьте свои типы при необходимости
        };

        public static (string TypeSql, string ProcedureSql) Generate<T>()
        {
            var type = typeof(T);
            var tableName = type.Name;
            var typeName = tableName + "TableType";
            var procedureName = "Insert" + tableName;
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.Name.ToLower() != "id" && !(p.GetCustomAttribute<IgnoreAttribute>() != null && p.GetCustomAttribute<IgnoreAttribute>()!.Ignore)) // исключаем Id и помеченные [Ignore]
                .ToList();

            // Проверка: хотя бы одно поле
            if (!properties.Any())
                throw new InvalidOperationException("Нет полей для маппинга (возможно, только Id).");

            // Генерация TVP (табличный тип)
            var typeBuilder = new StringBuilder();
            typeBuilder.AppendLine($"-- Табличный тип: {typeName}");
            typeBuilder.AppendLine($"IF TYPE_ID('{typeName}') IS NOT NULL");
            typeBuilder.AppendLine($"BEGIN");
            typeBuilder.AppendLine($"    IF OBJECT_ID('{procedureName}', 'P') IS NOT NULL");
            typeBuilder.AppendLine($"        DROP PROCEDURE {procedureName};");
            typeBuilder.AppendLine($"    DROP TYPE {typeName};");
            typeBuilder.AppendLine($"END");
            typeBuilder.AppendLine($"GO");
            typeBuilder.AppendLine($"CREATE TYPE {typeName} AS TABLE");
            typeBuilder.AppendLine("(");

            var columns = properties.Select(p => $"    {p.Name} {GetSqlType(p)}");

            typeBuilder.AppendLine(string.Join(",\n", columns));
            typeBuilder.AppendLine(");");
            typeBuilder.AppendLine();

            // Генерация хранимой процедуры
            var procBuilder = new StringBuilder();
            procBuilder.AppendLine($"-- Хранимая процедура: {procedureName}");
            procBuilder.AppendLine($"IF OBJECT_ID('{procedureName}', 'P') IS NOT NULL");
            procBuilder.AppendLine($"    DROP PROCEDURE {procedureName};");
            procBuilder.AppendLine($"GO");
            procBuilder.AppendLine($"CREATE PROCEDURE {procedureName}");
            procBuilder.AppendLine($"    @{typeName} {typeName} READONLY");
            procBuilder.AppendLine($"AS");
            procBuilder.AppendLine($"BEGIN");
            procBuilder.AppendLine($"    SET NOCOUNT ON;");
            procBuilder.AppendLine();
            procBuilder.AppendLine($"    -- Таблица для возврата новых ID и данных");
            procBuilder.AppendLine($"    DECLARE @OutputTable TABLE (NewId INT, {string.Join(", ", columns)});");
            procBuilder.AppendLine();
            procBuilder.AppendLine($"    INSERT INTO {tableName} ({string.Join(", ", properties.Select(p => p.Name))})");
            procBuilder.AppendLine($"    OUTPUT INSERTED.Id, {string.Join(", ", properties.Select(p => "INSERTED." + p.Name))}");
            procBuilder.AppendLine($"    INTO @OutputTable");
            procBuilder.AppendLine($"    SELECT {string.Join(", ", properties.Select(p => p.Name))} FROM @{typeName};");
            procBuilder.AppendLine();
            procBuilder.AppendLine($"    -- Возвращаем результат");
            procBuilder.AppendLine($"    SELECT NewId, {string.Join(", ", properties.Select(p => p.Name))} FROM @OutputTable;");
            procBuilder.AppendLine($"END");

            return (typeBuilder.ToString(), procBuilder.ToString());
        }

        public static string GenerateTableCreationQuery(Type entityType)
        {
            var query = new StringBuilder($"IF OBJECT_ID('{entityType.Name}', 'U') IS NULL BEGIN CREATE TABLE {entityType.Name} (");

            foreach (var property in entityType.GetProperties())
            {
                query.Append($"{property.Name} {GetSqlType(property)}");

                var fk = property.GetCustomAttribute<ForeignKeyAttribute>();
                if (fk is not null)
                {
                    query.Append($" FOREIGN KEY REFERENCES {fk.ReferenceTableName}({fk.PrimaryKeyName})");
                }
                else if (property.GetCustomAttribute<KeyAttribute>() is not null)
                {
                    query.Append(" PRIMARY KEY IDENTITY");
                }

                query.Append(',');
            }

            return query.Remove(query.Length - 1, 1).Append(");END;").ToString();
        }

        public static string GenerateDatabaseCreationQuery(string database)
        {
            return $"IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = '{database}') BEGIN CREATE DATABASE [{database}] END;";
        }

        private static string GetSqlType(PropertyInfo property)
        {
            return TypeMap.TryGetValue(Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType, out var t)
                    ? t
                    : "NVARCHAR(255)"; // fallback
        }
    }
}