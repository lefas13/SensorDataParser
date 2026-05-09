using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SensorDataParser.Forms;
using SensorDataParser.Models;
using SensorDataParser.Parser;
using System.Reflection;
using Point = SensorDataParser.Models.Point;

namespace SensorDataParser
{
    public partial class MainForm : Form
    {
        private IConfigurationRoot _configuration = null!;


        private static readonly Type[] _tablesOrder =
        {
            typeof(UnitRefType),
            typeof(UnitEngType),
            typeof(BiasVoltage),
            typeof(ParamType),
            typeof(AxisType),
            typeof(Folder),
            typeof(Machine),
            typeof(Sensor),
            typeof(Point),
            typeof(ParamSet),
            typeof(Axis),
            typeof(MultiSensor),
            typeof(Note),
            typeof(Schedule),
            typeof(Record)
        };

        public MainForm()
        {
            InitializeComponent();
        }

        private void CultivationFilesButtonClick(object sender, EventArgs e)
        {
            var openFileDialog = new OpenFileDialog()
            {
                Multiselect = true,
                Filter = "XML-файлы (*.xml)|*.xml",
                Title = "Выберите файлы для загрузки"
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                var form = new ProgressBarForm(openFileDialog.FileNames, _tablesOrder.Length, _configuration);
                form.ShowDialog();
            }
        }

        private void InitDB()
        {
            var connectionString = _configuration.GetConnectionString("MsSqlConnection")!;
            var database = _configuration.GetSection("Database").Value!;

            using var connection = new SqlConnection(connectionString);
            connection.Open();
            ExecuteSQL(SqlGenerator.GenerateDatabaseCreationQuery(database), connection);
            connection.ChangeDatabase(database);

            foreach (var type in _tablesOrder)
            {
                ExecuteSQL(SqlGenerator.GenerateTableCreationQuery(type), connection);
                var method = typeof(SqlGenerator).GetMethod("Generate", BindingFlags.Static | BindingFlags.Public)!;
                var genericMethod = method.MakeGenericMethod(type);
                dynamic result = genericMethod.Invoke(null, null)!;
                ExecuteSQL(result.Item1, connection);
                ExecuteSQL(result.Item2, connection);
            }
        }

        private static void ExecuteSQL(string sql, SqlConnection connection)
        {
            string[] batches = sql.Split("GO");
            foreach (var batch in batches)
            {
                using SqlCommand command = new();
                command.Connection = connection;
                command.CommandText = batch;
                command.ExecuteNonQuery();
            }
        }

        private static IConfigurationRoot CreateConfiguration()
        {
            var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            return builder.Build();
        }

        private void MainFormLoad(object sender, EventArgs e)
        {
            try
            {
                _configuration = CreateConfiguration();
                InitDB();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при инициализации приложения: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }
        }

        private void btnOpenAnalysis_Click(object sender, EventArgs e)
        {
            var analysisForm = new AnalysisForm(_configuration);
            analysisForm.Show();
        }
    }
}