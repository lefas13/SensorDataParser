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

        public AnalysisForm(IConfigurationRoot configuration)
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized;
            _configuration = configuration;
            _analysisService = new AnalysisService(_configuration);

            tvHierarchy.AfterSelect += TvHierarchy_AfterSelect;
            tvHierarchy.ImageList = CreateImageList();

            SetupChart();

            btnExport = new Button
            {
                Text = "💾 Сохранить график",
                Size = new Size(170, 35),
                Location = new System.Drawing.Point(20, 20), // Отступ от левого верхнего угла
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.SteelBlue,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Visible = false 
            };
            btnExport.Click += BtnExport_Click;
            splitContainer1.Panel2.Controls.Add(btnExport);
            btnExport.BringToFront();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            LoadTreeView();
        }

        private void SetupChart()
        {
            chartTrend.Series.Clear();
            chartTrend.ChartAreas.Clear();
            var area = new ChartArea("MainArea");

            area.AxisX.LabelStyle.Format = "dd.MM.yyyy\nHH:mm";
            area.AxisX.Title = "Дата";
            area.AxisY.Title = "RMS (мм/с)";

            area.AxisX.MajorGrid.LineColor = Color.LightGray;
            area.AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dash;
            area.AxisY.MajorGrid.LineColor = Color.LightGray;
            area.AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash;

            chartTrend.ChartAreas.Add(area);
        }

        private void LoadTreeView()
        {
            tvHierarchy.Nodes.Clear();

            var folders = new GenericRepository<Folder>(_configuration).GetAll();
            var machines = new GenericRepository<Machine>(_configuration).GetAll();
            var points = new GenericRepository<Models.Point>(_configuration).GetAll();

            foreach (var f in folders)
            {
                var fNode = tvHierarchy.Nodes.Add(f.Name);
                fNode.ImageKey = "folder";
                fNode.SelectedImageKey = "folder";

                foreach (var m in machines.Where(x => x.SFolderID == f.Id))
                {
                    var mNode = fNode.Nodes.Add(m.Name);
                    mNode.ImageKey = "machine";
                    mNode.SelectedImageKey = "machine";

                    foreach (var p in points.Where(x => x.SMachineID == m.Id))
                    {
                        var pNode = mNode.Nodes.Add(p.Name);
                        pNode.Tag = p.Id;
                        pNode.ImageKey = "point";
                        pNode.SelectedImageKey = "point";
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

                if (data == null || data.Count == 0)
                {
                    btnExport.Visible = false;
                    return;
                }

                string fullPathName = e.Node.FullPath.Replace("\\", " -> ");
                UpdateChart(fullPathName, data);

                btnExport.Visible = true;
            }
            else
            {
                btnExport.Visible = false;
            }
        }

        private void UpdateChart(string fullPathName, List<TrendPoint> data)
        {
            chartTrend.Series.Clear();
            chartTrend.Titles.Clear();
            chartTrend.Legends.Clear();

            int criticalCount = data.Count(p => p.RMS >= 7.1);
            int warningCount = data.Count(p => p.RMS >= 4.5 && p.RMS < 7.1);
            int normalCount = data.Count(p => p.RMS < 4.5);
            var lastPoint = data.Last();

            var titlePath = new Title($"{fullPathName}", Docking.Top, new Font("Segoe UI", 12, FontStyle.Bold), Color.Black);
            chartTrend.Titles.Add(titlePath);

            string statusText = "Текущее состояние: НОРМА";
            Color statusColor = Color.ForestGreen;
            if (lastPoint.RMS >= 7.1) { statusText = "Текущее состояние: АВАРИЯ ⚠️"; statusColor = Color.Red; }
            else if (lastPoint.RMS >= 4.5) { statusText = "Текущее состояние: ПРЕДУПРЕЖДЕНИЕ ❗"; statusColor = Color.DarkOrange; }

            var titleStatus = new Title($"{statusText} ({lastPoint.RMS:F2} мм/с)", Docking.Top, new Font("Segoe UI", 11, FontStyle.Bold), statusColor);
            chartTrend.Titles.Add(titleStatus);

            var titleStats = new Title($"История:  Критично: {criticalCount} |  Предупр.: {warningCount} |  Норма: {normalCount}",
                                       Docking.Top, new Font("Segoe UI", 9, FontStyle.Italic), Color.DimGray);
            chartTrend.Titles.Add(titleStats);

            var series = new Series("RMS")
            {
                ChartType = SeriesChartType.Area,
                XValueType = ChartValueType.DateTime,
                MarkerStyle = MarkerStyle.Circle,
                MarkerSize = 10,
                MarkerColor = Color.FromArgb(200, Color.Crimson), 
                BorderWidth = 3,
                Color = Color.FromArgb(150, 65, 105, 225),
                BackGradientStyle = GradientStyle.TopBottom,
                BackSecondaryColor = Color.FromArgb(0, 255, 255, 255),
                ToolTip = "Дата: #VALX{dd.MM.yyyy HH:mm}\nRMS: #VALY{N2} мм/с"
            };

            foreach (var p in data.OrderBy(x => x.Date))
            {
                series.Points.AddXY(p.Date, p.RMS);
            }

            chartTrend.Series.Add(series);

            var area = chartTrend.ChartAreas[0];
            area.BackColor = Color.White; 

            area.AxisX.MajorGrid.LineColor = Color.FromArgb(230, 230, 230);
            area.AxisY.MajorGrid.LineColor = Color.FromArgb(230, 230, 230);

            area.AxisY.Minimum = 0;
            double maxVal = data.Max(p => p.RMS);
            area.AxisY.Maximum = maxVal > 8.0 ? Math.Ceiling(maxVal) + 2 : 10.0;

            area.AxisX.LabelStyle.Format = "dd.MM\nHH:mm";
            area.AxisY.StripLines.Clear();
            AddLine(4.5, Color.FromArgb(100, Color.Orange), "ПРЕДУПРЕЖДЕНИЕ", Color.DarkOrange);
            AddLine(7.1, Color.FromArgb(100, Color.Red), "АВАРИЯ", Color.Red);
        }

        private void AddLine(double value, Color lineColor, string text, Color textColor)
        {
            var line = new StripLine
            {
                IntervalOffset = value,
                BorderColor = lineColor,
                BorderDashStyle = ChartDashStyle.Dash,
                BorderWidth = 2,
                Text = "  " + text,
                ForeColor = textColor,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                TextAlignment = StringAlignment.Near
            };
            chartTrend.ChartAreas[0].AxisY.StripLines.Add(line);
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            var sfd = new SaveFileDialog
            {
                Filter = "PNG Изображение|*.png|JPEG Изображение|*.jpg",
                Title = "Сохранить график тренда",
                FileName = "Тренд_Вибрации.png"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                ChartImageFormat format = sfd.FileName.EndsWith(".jpg") ? ChartImageFormat.Jpeg : ChartImageFormat.Png;
                chartTrend.SaveImage(sfd.FileName, format);

                MessageBox.Show("График успешно сохранен!", "Экспорт", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private ImageList CreateImageList()
        {
            ImageList imgList = new ImageList();
            imgList.ImageSize = new Size(16, 16);
            imgList.ColorDepth = ColorDepth.Depth32Bit;

            Bitmap folder = new Bitmap(16, 16);
            using (Graphics g = Graphics.FromImage(folder))
            {
                g.Clear(Color.Transparent);
                using (Brush folderBrush = new SolidBrush(Color.FromArgb(230, 180, 80))) 
                using (Brush tabBrush = new SolidBrush(Color.FromArgb(190, 140, 50)))     
                {
                    g.FillRectangle(folderBrush, 1, 4, 14, 9);
                    g.FillPolygon(tabBrush, new System.Drawing.Point[] {
                new System.Drawing.Point(1, 4),
                new System.Drawing.Point(5, 4),
                new System.Drawing.Point(7, 1),
                new System.Drawing.Point(1, 1)
            });
                }
            }
            imgList.Images.Add("folder", folder);
            Bitmap machine = new Bitmap(16, 16);
            using (Graphics g = Graphics.FromImage(machine))
            {
                g.Clear(Color.Transparent);
                using (Brush grayBrush = new SolidBrush(Color.FromArgb(120, 130, 140)))
                using (Brush darkGrayBrush = new SolidBrush(Color.FromArgb(80, 90, 100)))
                {
                    g.FillRectangle(grayBrush, 2, 5, 12, 8); 
                    g.FillRectangle(darkGrayBrush, 12, 8, 3, 2); 
                    g.FillRectangle(darkGrayBrush, 3, 13, 2, 2);  
                    g.FillRectangle(darkGrayBrush, 11, 13, 2, 2); 
                    g.FillRectangle(darkGrayBrush, 1, 6, 2, 6);
                }
            }
            imgList.Images.Add("machine", machine);

            Bitmap point = new Bitmap(16, 16);
            using (Graphics g = Graphics.FromImage(point))
            {
                g.Clear(Color.Transparent);
                using (Brush blueBrush = new SolidBrush(Color.DodgerBlue))
                using (Pen bluePen = new Pen(Color.RoyalBlue, 2))
                {
                    g.DrawEllipse(bluePen, 1, 1, 13, 13);
                    g.FillEllipse(blueBrush, 5, 5, 6, 6);
                }
            }
            imgList.Images.Add("point", point);

            return imgList;
        }
    }
}