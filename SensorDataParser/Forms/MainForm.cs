using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SensorDataParser.Forms;
using SensorDataParser.Models;
using SensorDataParser.Parser;
using System.Reflection;

namespace SensorDataParser
{
    public partial class MainForm : Form
    {
        private IConfigurationRoot _configuration = null!;

        private Label lblMachinesCount;
        private Label lblPointsCount;
        private Label lblRecordsCount;

        private static readonly Type[] _tablesOrder =
        {
            typeof(UnitRefType), typeof(UnitEngType), typeof(BiasVoltage), typeof(ParamType),
            typeof(AxisType), typeof(Folder), typeof(Machine), typeof(Sensor),
            typeof(Models.Point), typeof(ParamSet), typeof(Axis), typeof(MultiSensor),
            typeof(Note), typeof(Schedule), typeof(Record)
        };

        public MainForm()
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized;
            SetupModernUI();
        }

        private void SetupModernUI()
        {
            this.Text = "Диагностический комплекс SCOUT 100";
            this.Size = new Size(1000, 600);
            this.MinimumSize = new Size(800, 500);
            this.BackColor = Color.FromArgb(240, 244, 248); // Светло-серый

            Panel pnlSidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 250,
                BackColor = Color.FromArgb(41, 53, 65) // Темно-синий/серый цвет
            };
            this.Controls.Add(pnlSidebar);

            Label lblLogo = new Label
            {
                Text = "VIBRO SYSTEM",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 80,
                TextAlign = ContentAlignment.MiddleCenter
            };
            pnlSidebar.Controls.Add(lblLogo);

            Button btnAnalysis = CreateMenuButton("📈 Анализ трендов");
            btnAnalysis.Click += btnAnalysis_Click;
            pnlSidebar.Controls.Add(btnAnalysis);

            Button btnImport = CreateMenuButton("📥 Импорт XML");
            btnImport.Click += btnImport_Click;
            pnlSidebar.Controls.Add(btnImport);

            Panel pnlMain = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(40) 
            };
            this.Controls.Add(pnlMain);
            pnlMain.BringToFront();

            Label lblTitle = new Label
            {
                Text = "Панель управления",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30),
                AutoSize = true,
                Location = new System.Drawing.Point(40, 40)
            };
            pnlMain.Controls.Add(lblTitle);

            lblMachinesCount = CreateStatLabel(120);
            lblPointsCount = CreateStatLabel(170);
            lblRecordsCount = CreateStatLabel(220);

            pnlMain.Controls.Add(lblMachinesCount);
            pnlMain.Controls.Add(lblPointsCount);
            pnlMain.Controls.Add(lblRecordsCount);
        }

        private Button CreateMenuButton(string text)
        {
            return new Button
            {
                Text = text,
                Dock = DockStyle.Top,
                Height = 60,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(20, 0, 0, 0),
                Cursor = Cursors.Hand
            };
        }

        private Label CreateStatLabel(int yPos)
        {
            return new Label
            {
                Font = new Font("Segoe UI", 14, FontStyle.Regular),
                ForeColor = Color.FromArgb(80, 80, 80),
                AutoSize = true,
                Location = new System.Drawing.Point(40, yPos),
                Text = "Загрузка данных..."
            };
        }

        private void MainFormLoad(object sender, EventArgs e)
        {
            try
            {
                _configuration = CreateConfiguration();
                InitDB();
                UpdateDashboardStatistics();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при инициализации: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }
        }

        private void UpdateDashboardStatistics()
        {
            try
            {
                var machinesRepo = new GenericRepository<Machine>(_configuration);
                var pointsRepo = new GenericRepository<Models.Point>(_configuration);
                var recordsRepo = new GenericRepository<Record>(_configuration);

                int machinesCount = machinesRepo.GetAll().Count;
                int pointsCount = pointsRepo.GetAll().Count;
                int recordsCount = recordsRepo.GetAll().Count;

                lblMachinesCount.Text = $"🏭 Агрегатов в базе: {machinesCount}";
                lblPointsCount.Text = $"📍 Точек контроля: {pointsCount}";
                lblRecordsCount.Text = $"📊 Всего загружено замеров: {recordsCount}";
            }
            catch { }
        }

        private void btnImport_Click(object sender, EventArgs e)
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
                UpdateDashboardStatistics();
            }
        }

        private void btnAnalysis_Click(object sender, EventArgs e)
        {
            var analysisForm = new AnalysisForm(_configuration);
            analysisForm.Show();
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
            string[] batches = sql.Split(new[] { "GO\r\n", "GO\n", "GO" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var batch in batches)
            {
                if (string.IsNullOrWhiteSpace(batch)) continue;

                using SqlCommand command = new SqlCommand();
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
    }
}