using Microsoft.Extensions.Configuration;
using SensorDataParser.Models;
using SensorDataParser.Parser;
using System.Windows.Forms.DataVisualization.Charting;

namespace SensorDataParser.Forms
{
    public partial class AnalysisForm : Form
    {
        private readonly IConfigurationRoot _configuration;
        private readonly AnalysisService _analysisService;

        private Button btnExport;
        private Button btnToggleSearch;

        // ЭЛЕМЕНТЫ ВЫЕЗЖАЮЩЕЙ ПАНЕЛИ
        private Panel pnlSearch;
        private System.Windows.Forms.Timer tmrSlide;
        private bool isSearchExpanded = false;

        // Элементы управления фильтрами
        private TextBox txtMinRms;
        private TextBox txtMaxRms;
        private DateTimePicker dtpFrom;
        private DateTimePicker dtpTo;
        private TextBox txtMachineSearch;
        private Button btnExecuteSearch;

        // Таблица результатов поиска
        private DataGridView dgvSearchResults;
        private Label lblResultsTitle;

        public AnalysisForm(IConfigurationRoot configuration)
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized;

            _configuration = configuration;
            _analysisService = new AnalysisService(_configuration);

            if (tvHierarchy != null)
            {
                tvHierarchy.ImageList = CreateImageList();
                tvHierarchy.AfterSelect += TvHierarchy_AfterSelect;
            }

            SetupChartAndUI();
            SetupSlidingSearchPanel();
            SetupResultsTable();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            LoadTreeView();
        }

        private void SetupChartAndUI()
        {
            if (splitContainer1 != null) splitContainer1.SplitterDistance = 300;
            Panel pnlToolbar = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.WhiteSmoke };
            splitContainer1.Panel2.Controls.Add(pnlToolbar);

            // Кнопка экспорта
            btnExport = new Button
            {
                Text = "💾 Сохранить график",
                Size = new Size(180, 35),
                Location = new System.Drawing.Point(10, 8),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.SteelBlue,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Visible = false
            };
            btnExport.Click += BtnExport_Click;
            pnlToolbar.Controls.Add(btnExport);

            // Кнопка вызова поиска
            btnToggleSearch = new Button
            {
                Text = "🔍 Параметрический поиск",
                AutoSize = true,
                Location = new System.Drawing.Point(200, 8),
                Height = 35,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(41, 53, 65),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Padding = new Padding(10, 0, 10, 0)
            };
            btnToggleSearch.Click += BtnToggleSearch_Click;
            pnlToolbar.Controls.Add(btnToggleSearch);

            // 2. Создание графика
            chartTrend = new Chart { Dock = DockStyle.Fill };
            var area = new ChartArea("MainArea");
            chartTrend.ChartAreas.Add(area);

            area.AxisX.MajorGrid.LineColor = Color.FromArgb(235, 235, 235);
            area.AxisY.MajorGrid.LineColor = Color.FromArgb(235, 235, 235);
            area.AxisX.LabelStyle.Format = "dd.MM\nHH:mm";

            splitContainer1.Panel2.Controls.Add(chartTrend);
            chartTrend.BringToFront();
        }

        //НАСТРОЙКА ВЫЕЗЖАЮЩЕЙ ПАНЕЛИ ФИЛЬТРОВ
        private void SetupSlidingSearchPanel()
        {
            // Таймер для плавной анимации
            tmrSlide = new System.Windows.Forms.Timer { Interval = 15 };
            tmrSlide.Tick += TmrSlide_Tick;

            // Сама панель поиска
            pnlSearch = new Panel
            {
                Dock = DockStyle.Right,
                Width = 0, // Изначально скрыта
                BackColor = Color.FromArgb(245, 247, 250),
                BorderStyle = BorderStyle.FixedSingle,
                Visible = false
            };
            splitContainer1.Panel2.Controls.Add(pnlSearch);
            pnlSearch.BringToFront(); // Поверх графика

            // Наполнение панели элементами (Фильтры)
            Label lblHeader = new Label { Text = "Фильтры поиска", Font = new Font("Segoe UI", 12, FontStyle.Bold), Location = new System.Drawing.Point(15, 15), AutoSize = true };
            pnlSearch.Controls.Add(lblHeader);

            Label lblRms = new Label { Text = "Виброскорость RMS (мм/с):", Font = new Font("Segoe UI", 9, FontStyle.Bold), Location = new System.Drawing.Point(15, 60), AutoSize = true };
            pnlSearch.Controls.Add(lblRms);

            txtMinRms = new TextBox { Location = new System.Drawing.Point(15, 80), Width = 100, PlaceholderText = "От" };
            txtMaxRms = new TextBox { Location = new System.Drawing.Point(125, 80), Width = 100, PlaceholderText = "До" };
            pnlSearch.Controls.Add(txtMinRms); pnlSearch.Controls.Add(txtMaxRms);

            Label lblDates = new Label { Text = "Интервал дат замера:", Font = new Font("Segoe UI", 9, FontStyle.Bold), Location = new System.Drawing.Point(15, 120), AutoSize = true };
            pnlSearch.Controls.Add(lblDates);

            dtpFrom = new DateTimePicker { Location = new System.Drawing.Point(15, 140), Width = 210, Format = DateTimePickerFormat.Short, Value = DateTime.Now.AddMonths(-1) };
            dtpTo = new DateTimePicker { Location = new System.Drawing.Point(15, 170), Width = 210, Format = DateTimePickerFormat.Short, Value = DateTime.Now };
            pnlSearch.Controls.Add(dtpFrom); pnlSearch.Controls.Add(dtpTo);

            Label lblMachine = new Label { Text = "Название агрегата содержит:", Font = new Font("Segoe UI", 9, FontStyle.Bold), Location = new System.Drawing.Point(15, 210), AutoSize = true };
            pnlSearch.Controls.Add(lblMachine);

            txtMachineSearch = new TextBox { Location = new System.Drawing.Point(15, 230), Width = 210, PlaceholderText = "Например: 1ap906" };
            pnlSearch.Controls.Add(txtMachineSearch);

            btnExecuteSearch = new Button
            {
                Text = "Найти прецеденты",
                Location = new System.Drawing.Point(15, 280),
                Width = 210,
                Height = 40,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.SteelBlue,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnExecuteSearch.Click += BtnExecuteSearch_Click;
            pnlSearch.Controls.Add(btnExecuteSearch);
        }

        // ТАБЛИЦА РЕЗУЛЬТАТОВ ПОИСКА (СНИЗУ)
        private void SetupResultsTable()
        {
            lblResultsTitle = new Label
            {
                Text = "📋 Результаты многокритериального анализа:",
                Dock = DockStyle.Bottom,
                Height = 25,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.DimGray,
                Visible = false
            };

            dgvSearchResults = new DataGridView
            {
                Dock = DockStyle.Bottom,
                Height = 180,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                Visible = false
            };

            dgvSearchResults.DoubleClick += DgvSearchResults_DoubleClick;

            splitContainer1.Panel2.Controls.Add(lblResultsTitle);
            splitContainer1.Panel2.Controls.Add(dgvSearchResults);
        }


        // ЛОГИКА АНИМАЦИИ ПАНЕЛИ
        private void TmrSlide_Tick(object sender, EventArgs e)
        {
            const int speed = 30; // Скорость открытия в пикселях за шаг
            if (isSearchExpanded)
            {
                pnlSearch.Width += speed;
                if (pnlSearch.Width >= 250)
                {
                    pnlSearch.Width = 250;
                    tmrSlide.Stop();
                }
            }
            else
            {
                pnlSearch.Width -= speed;
                if (pnlSearch.Width <= 0)
                {
                    pnlSearch.Width = 0;
                    tmrSlide.Stop();
                    pnlSearch.Visible = false;
                }
            }
        }

        private void BtnToggleSearch_Click(object sender, EventArgs e)
        {
            isSearchExpanded = !isSearchExpanded;
            if (isSearchExpanded) pnlSearch.Visible = true;
            tmrSlide.Start();
        }

        // КЛИК "НАЙТИ ПРЕЦЕДЕНТЫ"
        private void BtnExecuteSearch_Click(object sender, EventArgs e)
        {
            double? minRms = double.TryParse(txtMinRms.Text.Replace(',', '.'), out double min) ? min : (double?)null;
            double? maxRms = double.TryParse(txtMaxRms.Text.Replace(',', '.'), out double max) ? max : (double?)null;

            this.Cursor = Cursors.WaitCursor;
            try
            {
                var found = _analysisService.SearchPointsByParameters(minRms, maxRms, dtpFrom.Value, dtpTo.Value, txtMachineSearch.Text);

                if (found.Count > 0)
                {
                    dgvSearchResults.DataSource = found.Select(x => new {
                        x.Дата,
                        x.Оборудование,
                        x.Точка,
                        Vibro = x.Вибрация + " мм/с",
                        Рекомендация = GetExpertDecision(double.Parse(x.Вибрация)),
                        PointId = x.PointId // Прячем ID точки
                    }).ToList();
                    dgvSearchResults.Columns["PointId"].Visible = false;

                    dgvSearchResults.Visible = true;
                    lblResultsTitle.Visible = true;
                    lblResultsTitle.Text = $"📋 Результаты многокритериального анализа (найдено: {found.Count} событий):";
                }
                else
                {
                    MessageBox.Show("Записей по указанным параметрам не найдено.", "Поиск", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    dgvSearchResults.Visible = false;
                    lblResultsTitle.Visible = false;
                }
            }
            catch (Exception ex) { MessageBox.Show("Ошибка поиска: " + ex.Message); }
            finally { this.Cursor = Cursors.Default; }
        }

        private void DgvSearchResults_DoubleClick(object sender, EventArgs e)
        {
            if (dgvSearchResults.CurrentRow != null)
            {
                var row = dgvSearchResults.CurrentRow.DataBoundItem;
                int pointId = (int)row.GetType().GetProperty("PointId").GetValue(row);
                string machine = row.GetType().GetProperty("Оборудование").GetValue(row).ToString();
                string point = row.GetType().GetProperty("Точка").GetValue(row).ToString();

                var data = _analysisService.GetTrendData(pointId);
                if (data != null && data.Count > 0)
                {
                    UpdateChart($"{machine} ➔ {point}", data);
                    btnExport.Visible = true;
                }
            }
        }

        private string GetExpertDecision(double rms)
        {
            if (rms >= 7.1) return "АВАРИЯ: Срочный ремонт!";
            if (rms >= 4.5) return "ВНИМАНИЕ: Проверить зазоры.";
            return "НОРМА: Мониторинг.";
        }

        // СТАНДАРТНАЯ ОТРИСОВКА И ОСТАЛЬНОЙ КОД
        private void LoadTreeView()
        {
            if (tvHierarchy == null || _configuration == null) return;

            tvHierarchy.Nodes.Clear();
            var folders = new GenericRepository<Folder>(_configuration).GetAll();
            var machines = new GenericRepository<Machine>(_configuration).GetAll();
            var points = new GenericRepository<Models.Point>(_configuration).GetAll();

            foreach (var f in folders)
            {
                var fNode = tvHierarchy.Nodes.Add(f.Name);
                fNode.ImageKey = "folder"; fNode.SelectedImageKey = "folder";

                foreach (var m in machines.Where(x => x.SFolderID == f.Id))
                {
                    var mNode = fNode.Nodes.Add(m.Name);
                    mNode.ImageKey = "machine"; mNode.SelectedImageKey = "machine";

                    foreach (var p in points.Where(x => x.SMachineID == m.Id))
                    {
                        var pNode = mNode.Nodes.Add(p.Name);
                        pNode.Tag = p.Id;
                        pNode.ImageKey = "point"; pNode.SelectedImageKey = "point";
                    }
                }
            }
            tvHierarchy.ExpandAll();
        }

        private void TvHierarchy_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag is int pointId)
            {
                var data = _analysisService.GetTrendData(pointId);
                if (data != null && data.Count > 0)
                {
                    UpdateChart(e.Node.FullPath.Replace("\\", " ➔ "), data);
                    btnExport.Visible = true;
                }
            }
            else { btnExport.Visible = false; }
        }

        private void UpdateChart(string path, List<TrendPoint> data)
        {
            chartTrend.Series.Clear();
            chartTrend.Titles.Clear();

            var last = data.OrderBy(x => x.Date).Last();
            int crit = data.Count(x => x.RMS >= 7.1);
            int warn = data.Count(x => x.RMS >= 4.5 && x.RMS < 7.1);

            chartTrend.Titles.Add(new Title(path, Docking.Top, new Font("Segoe UI", 12, FontStyle.Bold), Color.Black));

            string status = last.RMS >= 7.1 ? "АВАРИЯ" : (last.RMS >= 4.5 ? "ВНИМАНИЕ" : "НОРМА");
            Color sCol = last.RMS >= 7.1 ? Color.Red : (last.RMS >= 4.5 ? Color.Orange : Color.Green);

            chartTrend.Titles.Add(new Title($"Статус: {status} ({last.RMS:F2} мм/с) | История выхода за ГОСТ: Критично: {crit} | Предупр: {warn}",
                Docking.Top, new Font("Segoe UI", 10, FontStyle.Bold), sCol));

            var series = new Series("RMS")
            {
                ChartType = SeriesChartType.Area,
                XValueType = ChartValueType.DateTime,
                MarkerStyle = MarkerStyle.Circle,
                MarkerSize = 10,
                BorderWidth = 2,
                Color = Color.FromArgb(180, Color.SteelBlue),
                BackGradientStyle = GradientStyle.TopBottom,
                BackSecondaryColor = Color.Transparent
            };

            foreach (var p in data.OrderBy(x => x.Date)) series.Points.AddXY(p.Date, p.RMS);
            chartTrend.Series.Add(series);

            chartTrend.ChartAreas[0].AxisY.StripLines.Clear();
            AddLine(4.5, Color.Orange, "ПРЕДУПРЕЖДЕНИЕ");
            AddLine(7.1, Color.Red, "АВАРИЯ");

            double max = data.Max(x => x.RMS);
            chartTrend.ChartAreas[0].AxisY.Maximum = max > 8 ? Math.Ceiling(max) + 2 : 10;
        }

        private void AddLine(double val, Color col, string txt)
        {
            chartTrend.ChartAreas[0].AxisY.StripLines.Add(new StripLine
            {
                IntervalOffset = val,
                BorderColor = col,
                BorderDashStyle = ChartDashStyle.Dash,
                Text = "  " + txt,
                ForeColor = col,
                Font = new Font("Segoe UI", 8, FontStyle.Bold)
            });
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            var sfd = new SaveFileDialog { Filter = "PNG Image|*.png|JPEG Image|*.jpg", Title = "Сохранить тренд", FileName = "Trend.png" };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                ChartImageFormat format = sfd.FileName.EndsWith(".jpg") ? ChartImageFormat.Jpeg : ChartImageFormat.Png;
                chartTrend.SaveImage(sfd.FileName, format);
                MessageBox.Show("График сохранен!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private ImageList CreateImageList()
        {
            ImageList il = new ImageList { ImageSize = new Size(16, 16), ColorDepth = ColorDepth.Depth32Bit };
            Bitmap f = new Bitmap(16, 16);
            using (Graphics g = Graphics.FromImage(f))
            {
                g.FillRectangle(new SolidBrush(Color.Gold), 1, 4, 14, 9);
                g.FillRectangle(new SolidBrush(Color.Goldenrod), 1, 2, 6, 3);
            }
            il.Images.Add("folder", f);

            Bitmap m = new Bitmap(16, 16);
            using (Graphics g = Graphics.FromImage(m))
            {
                g.FillRectangle(Brushes.Gray, 2, 6, 10, 8);
                g.FillRectangle(Brushes.Black, 12, 9, 3, 2);
            }
            il.Images.Add("machine", m);

            Bitmap p = new Bitmap(16, 16);
            using (Graphics g = Graphics.FromImage(p))
            {
                g.DrawEllipse(new Pen(Color.Blue, 2), 2, 2, 12, 12);
                g.FillEllipse(Brushes.Red, 6, 6, 4, 4);
            }
            il.Images.Add("point", p);

            return il;
        }
    }
}