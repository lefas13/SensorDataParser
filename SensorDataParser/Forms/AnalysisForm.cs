using Microsoft.Extensions.Configuration;
using SensorDataParser.Models;
using SensorDataParser.Parser;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace SensorDataParser.Forms
{
    public partial class AnalysisForm : Form
    {
        private readonly IConfigurationRoot _configuration;
        private readonly AnalysisService _analysisService;

        public AnalysisForm(IConfigurationRoot configuration)
        {
            InitializeComponent();
            _configuration = configuration;
            _analysisService = new AnalysisService(_configuration);

            // Подписываемся на событие клика по дереву
            tvHierarchy.AfterSelect += TvHierarchy_AfterSelect;

            SetupChart();
        }

        // Это событие вызовется при загрузке формы
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

            area.AxisX.LabelStyle.Format = "dd.MM.yyyy";
            area.AxisX.Title = "Дата";
            area.AxisY.Title = "RMS (мм/с)";

            chartTrend.ChartAreas.Add(area);
        }

        private void LoadTreeView()
        {
            tvHierarchy.Nodes.Clear();

            // Читаем базу (убедись, что добавил метод GetAll() в GenericRepository)
            var folders = new GenericRepository<Folder>(_configuration).GetAll();
            var machines = new GenericRepository<Machine>(_configuration).GetAll();
            var points = new GenericRepository<Models.Point>(_configuration).GetAll();

            foreach (var f in folders)
            {
                var fNode = tvHierarchy.Nodes.Add(f.Name);

                foreach (var m in machines.Where(x => x.SFolderID == f.Id))
                {
                    var mNode = fNode.Nodes.Add(m.Name);

                    foreach (var p in points.Where(x => x.SMachineID == m.Id))
                    {
                        var pNode = mNode.Nodes.Add(p.Name);
                        pNode.Tag = p.Id; // Сохраняем ID точки
                    }
                }
            }

            // Раскрываем все узлы для удобства
            tvHierarchy.ExpandAll();
        }

        private void TvHierarchy_AfterSelect(object sender, TreeViewEventArgs e)
        {
            // Если выбран узел с ID (т.е. точка замера)
            if (e.Node.Tag is int pointId)
            {
                var data = _analysisService.GetTrendData(pointId);
                UpdateChart(e.Node.Text, data);
            }
        }

        private void UpdateChart(string pointName, List<TrendPoint> data)
        {
            chartTrend.Series.Clear();
            chartTrend.Titles.Clear(); // Очищаем старые заголовки

            // 1. ВЫВОДИМ ИНФОРМАЦИЮ (Сколько точек нашли)
            chartTrend.Titles.Add($"Анализ точки: {pointName}");
            chartTrend.Titles.Add($"Найдено замеров в базе: {data.Count}");

            // 2. НАСТРАИВАЕМ ВНЕШНИЙ ВИД ЛИНИИ И ТОЧЕК
            var series = new Series(pointName)
            {
                ChartType = SeriesChartType.Line,
                XValueType = ChartValueType.DateTime,
                MarkerStyle = MarkerStyle.Circle, // Обязательно рисуем кружочки на точках!
                MarkerSize = 15,                  // Делаем их огромными (было 8, стало 15)
                MarkerColor = Color.DarkRed,      // Красим саму точку в красный, чтобы выделялась
                BorderWidth = 3,                  // Толщина линии
                Color = Color.DarkBlue,           // Цвет линии
                IsValueShownAsLabel = true        // Включает цифру вибрации прямо над точкой
            };

            foreach (var p in data)
            {
                series.Points.AddXY(p.Date, p.RMS);
            }

            chartTrend.Series.Add(series);

            // 3. НАСТРАИВАЕМ ОСИ (чтобы точка не прилипла к низу экрана)
            var area = chartTrend.ChartAreas[0];
            area.AxisY.IsStartedFromZero = false; // Разрешаем графику висеть в воздухе
            area.AxisX.IsMarginVisible = true;    // Добавляем отступы по краям графика слева и справа

            // 4. ДОБАВЛЯЕМ ГОСТ УСТАВКИ
            area.AxisY.StripLines.Clear();
            AddLine(4.5, Color.Orange, "Предупреждение (4.5)");
            AddLine(7.1, Color.Red, "Авария (7.1)");
        }

        private void AddLine(double value, Color color, string label)
        {
            var line = new StripLine
            {
                IntervalOffset = value,
                BorderColor = color,
                BorderDashStyle = ChartDashStyle.Dash,
                BorderWidth = 2,
                Text = label,
                TextAlignment = StringAlignment.Near
            };
            chartTrend.ChartAreas[0].AxisY.StripLines.Add(line);
        }

        private void TreeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            // Проверяем, что в Tag лежит ID точки
            if (e.Node.Tag is int pointId)
            {
                // Вытаскиваем данные из базы
                var data = _analysisService.GetTrendData(pointId);

                // ПРОВЕРКА: Если данных нет, выдаем сообщение
                if (data == null || data.Count == 0)
                {
                    MessageBox.Show($"Для точки '{e.Node.Text}' нет замеров RMS в базе данных, либо дата записана в неизвестном формате.",
                                    "Нет данных", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    chartTrend.Series.Clear(); // Очищаем график
                    return;
                }

                // Если данные есть - рисуем
                UpdateChart(e.Node.Text, data);
            }
        }
    }
}