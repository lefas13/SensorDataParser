using SensorDataParser.Parser;
using System;
using System.ComponentModel;
using System.Globalization;
using Xunit;

namespace SensorDataParser.Tests
{
    public class TypesConverterTests
    {
        private readonly TypesConverter _converter;

        public TypesConverterTests()
        {
            // Инициализация конвертера аналогично тому, как это спроектировано в программе
            _converter = new TypesConverter()
                .AddConvertibleType("integer", v => Convert.ToInt32(v))
                .AddConvertibleType("double", v => {
                    // Важное исправление: заменяем запятую на точку для надежности импорта
                    string cleanValue = v.Replace(',', '.');
                    return Convert.ToDouble(cleanValue, CultureInfo.InvariantCulture);
                })
                .AddConvertibleType("string", v => v.ToString())
                .AddConvertibleType("bool", v => Convert.ToBoolean(v));
        }

        [Fact]
        public void Convert_DoubleWithDot_ReturnsExpectedDouble()
        {
            // Arrange (Подготовка)
            string type = "double";
            string value = "4.15";
            double expected = 4.15;

            // Act (Выполнение)
            var result = _converter.Convert(type, value);

            // Assert (Проверка)
            Assert.IsType<double>(result);
            Assert.Equal(expected, (double)result);
        }

        [Fact]
        public void Convert_DoubleWithComma_ReturnsExpectedDouble()
        {
            // Arrange (Подготовка)
            string type = "double";
            string value = "4,15";
            double expected = 4.15;

            // Act (Выполнение)
            var result = _converter.Convert(type, value);

            // Assert (Проверка)
            Assert.IsType<double>(result);
            Assert.Equal(expected, (double)result);
        }

        [Fact]
        public void Convert_Integer_ReturnsExpectedInt()
        {
            // Arrange (Подготовка)
            string type = "integer";
            string value = "123";
            int expected = 123;

            // Act (Выполнение)
            var result = _converter.Convert(type, value);

            // Assert (Проверка)
            Assert.IsType<int>(result);
            Assert.Equal(expected, (int)result);
        }

        [Fact]
        public void Convert_InvalidDouble_ThrowsFormatException()
        {
            // Arrange (Подготовка)
            string type = "double";
            string value = "abc";

            // Act & Assert (Проверка вызова исключения)
            Assert.Throws<FormatException>(() => _converter.Convert(type, value));
        }

        [Fact]
        public void Convert_UnknownType_ThrowsArgumentException()
        {
            // Arrange (Подготовка)
            string type = "unknown";
            string value = "123";

            // Act & Assert (Проверка вызова исключения при неизвестном типе)
            Assert.Throws<ArgumentException>(() => _converter.Convert(type, value));
        }
    }
}