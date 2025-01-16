using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Forms.VisualStyles;
using EngineeringUnits;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json.Linq;



namespace DiplomaWinForms1
{
    public partial class Form2 : Form
    {
        private string databasePath = @"C:\sqlite\results.db";
        private string _databasePath = @"C:\sqlite\pumps.db";
        public double RequiredPressure { get; set; }
        public double NaporStaic { get; set; }
        public double V { get; set; }
        public int numberOfZoom = 1;



        private Chart chart;

        public Form2(double requiredPressure, double naporStaic, double v)
        {
            InitializeComponent();
            RequiredPressure = requiredPressure;
            NaporStaic = naporStaic;
            V = v;
            InitializeChart();
            //InitializeControls();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            LoadData();
            LoadDataPumps();
        }

        private void LoadData()
        {
            string connectionString = @"Data Source=" + databasePath + "; ";
            string viewQuery = "SELECT * FROM results";

            using (var connection = new SqliteConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (var command = new SqliteCommand(viewQuery, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            DataTable dataTable = new DataTable();
                            dataTable.Load(reader);
                            dataGridView1.DataSource = dataTable;
                        }
                    }
                    connection.Close();
                }
                catch (Exception ex) { MessageBox.Show("Ошибка: " + ex.Message); }
            }
        }

        private void LoadDataPumps()
        {
            string connectionString = @"Data Source=" + _databasePath + "; ";
            string viewQuery = "SELECT Name, Pressure, Capacity, Power, Kpd FROM pumps";

            using (var connection = new SqliteConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (var command = new SqliteCommand(viewQuery, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            DataTable dataTable = new DataTable();
                            dataTable.Load(reader);
                            dataGridView2.DataSource = dataTable;
                        }
                    }
                    connection.Close();
                }
                catch (Exception ex) { MessageBox.Show("Ошибка: " + ex.Message); }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string connectionString = @"Data Source=" + databasePath + "; ";
            string deleteQuery = "DELETE FROM results";


            var result = MessageBox.Show(
                "Вы уверены?",
                "Сообщение",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.DefaultDesktopOnly);

            if (result == DialogResult.Yes)
            {
                using (var connection = new SqliteConnection(connectionString))
                {
                    try
                    {
                        connection.Open();
                        using (var command = new SqliteCommand(deleteQuery, connection))
                        {
                            command.ExecuteNonQuery();
                            LoadData();
                        }
                        MessageBox.Show("Все записи были удалены!");

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка!" + ex.Message);
                    }
                }
            }
            else { }
        }

        private void InitializeChart()
        {
            // Настройка Chart
            chart = new Chart();
            chart.Dock = DockStyle.Bottom;
            chart.Height = 630;
            Controls.Add(chart);



            ChartArea chartArea = new ChartArea();
            chart.ChartAreas.Add(chartArea);


            // Создание серии "Сеть"
            Series series = new Series("Сеть");
            series.ChartType = SeriesChartType.Line;
            chart.Series.Add(series);

            Legend legend = new Legend();
            legend.Docking = Docking.Bottom; // Можно установить на Top, Bottom, Left или Right
            chart.Legends.Add(legend);

            //CreateCurve(chart, RequiredPressure, NaporStaic, V);
            FakeCrateCurve(chart, RequiredPressure, NaporStaic);

            // Подключение к базе данных и чтение данных о насосах
            string databasePath = @"C:\sqlite\pumps.db";
            string connectionString = @"Data Source=" + databasePath + "; ";
            List<Pump> pumps = ReadPumpsFromDatabase(connectionString);

            double minX = double.MaxValue;
            double maxX = double.MinValue;

            foreach (var pump in pumps)
            {
                // Добавление данных и построение аппроксимаций для каждой группы точек
                AddPumpDataAndApproximation(chart, pump, pump.XData1, pump.YData1, 1);
                //AddPumpDataAndApproximation(chart, pump, pump.XData2, pump.YData2, 2);

            }
            chart.MouseWheel += new MouseEventHandler(chart_MouseWheel);
            // Название осей
            chartArea.AxisX.Title = "Q [м3/ч]";
            chartArea.AxisY.Title = "H [м]";
            //chartArea.AxisX.Minimum = 0;
            //chartArea.AxisX.Maximum = 0.02;
            //chartArea.AxisY.Minimum = 0;
            //chartArea.AxisY.Maximum = 30;

            //chartArea.AxisX.Minimum = Double.NaN;
            //chart.ChartAreas[0].AxisX.Maximum = 100;
            //chartArea.AxisX.IntervalAutoMode = IntervalAutoMode.VariableCount;
            //chartArea.AxisX.IsStartedFromZero = false;

            //chart.ChartAreas[0].AxisY.Minimum = 0;
            //chartArea.AxisY.Maximum = Double.NaN;
            //chartArea.AxisY.IntervalAutoMode = IntervalAutoMode.VariableCount;
            //chartArea.AxisY.IsStartedFromZero = false;
        }

        //private void InitializeControls()
        //{
        //    // Кнопка добавления насоса
        //    Button addPumpButton = new Button
        //    {
        //        Text = "Add Pump",
        //        Dock = DockStyle.Top
        //    };
        //    addPumpButton.Click += AddPumpButton_Click;
        //    this.Controls.Add(addPumpButton);

        //    // Кнопка удаления насоса
        //    Button deletePumpButton = new Button
        //    {
        //        Text = "Delete Pump",
        //        Dock = DockStyle.Top
        //    };
        //    deletePumpButton.Click += DeletePumpButton_Click;
        //    this.Controls.Add(deletePumpButton);
        //}

        //private void AddPumpButton_Click(object sender, EventArgs e)
        //{
        //    using (AddPumpForm addPumpForm = new AddPumpForm())
        //    {
        //        if (addPumpForm.ShowDialog() == DialogResult.OK)
        //        {
        //            Pump newPump = addPumpForm.NewPump;

        //            InsertPumpIntoDatabase(newPump);

        //            // Обновление графика
        //            InitializeChart();
        //        }
        //    }
        //}

        //private void DeletePumpButton_Click(object sender, EventArgs e)
        //{
        //    using (DeletePumpForm deletePumpForm = new DeletePumpForm())
        //    {
        //        if (deletePumpForm.ShowDialog() == DialogResult.OK)
        //        {
        //            int pumpId = deletePumpForm.PumpId;
        //            DeletePumpFromDatabase(pumpId);

        //            // Обновление графика
        //            InitializeChart();
        //        }
        //    }
        //}





        private List<Pump> ReadPumpsFromDatabase(string connectionString)
        {
            List<Pump> pumps = new List<Pump>();

            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                string query = query = "SELECT * FROM pumps";

                using (SqliteCommand command = new SqliteCommand(query, connection))
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Pump pump = new Pump
                        {
                            ID = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Pressure = reader.GetDouble(2),
                            Capacity = reader.GetDouble(3),
                            Power = reader.GetDouble(4),
                            Kpd = reader.GetDouble(5),
                            XData1 = new List<double> { reader.GetDouble(6), reader.GetDouble(7), reader.GetDouble(8), reader.GetDouble(9), reader.GetDouble(10) },
                            YData1 = new List<double> { reader.GetDouble(11), reader.GetDouble(12), reader.GetDouble(13), reader.GetDouble(14), reader.GetDouble(15) },
                            XData2 = new List<double> { reader.GetDouble(16)  , reader.GetDouble(17)  , reader.GetDouble(18)  , reader.GetDouble(19)  , reader.GetDouble(20)   },
                            YData2 = new List<double> { reader.GetDouble(21), reader.GetDouble(22), reader.GetDouble(23), reader.GetDouble(24), reader.GetDouble(25) }
                        };
                        pumps.Add(pump);
                    }
                }
            }

            return pumps;
        }

        private void InsertPumpIntoDatabase(Pump pump)
        {
            string databasePath = @"C:\sqlite\pumps.db";
            string connectionString = @"Data Source=" + databasePath + "; ";
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                string query = @"
                    INSERT INTO pumps 
                    (Name, Pressure, Capacity, Power, Kpd, x1, x2, x3, x4, x5, y1, y2, y3, y4, y5, x11, x12, x13, x14, x15, y11, y12, y13, y14, y15) 
                    VALUES 
                    (@Name, @Pressure, @Capacity, @Power, @Kpd, @x1, @x2, @x3, @x4, @x5, @y1, @y2, @y3, @y4, @y5, @x11, @x12, @x13, @x14, @x15, @y11, @y12, @y13, @y14, @y15)";
                using (SqliteCommand command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Name", pump.Name);
                    command.Parameters.AddWithValue("@Pressure", pump.Pressure);
                    command.Parameters.AddWithValue("@Capacity", pump.Capacity);
                    command.Parameters.AddWithValue("@Power", pump.Power);
                    command.Parameters.AddWithValue("@Kpd", pump.Kpd);
                    command.Parameters.AddWithValue("@x1", pump.XData1[0]);
                    command.Parameters.AddWithValue("@x2", pump.XData1[1]);
                    command.Parameters.AddWithValue("@x3", pump.XData1[2]);
                    command.Parameters.AddWithValue("@x4", pump.XData1[3]);
                    command.Parameters.AddWithValue("@x5", pump.XData1[4]);
                    command.Parameters.AddWithValue("@y1", pump.YData1[0]);
                    command.Parameters.AddWithValue("@y2", pump.YData1[1]);
                    command.Parameters.AddWithValue("@y3", pump.YData1[2]);
                    command.Parameters.AddWithValue("@y4", pump.YData1[3]);
                    command.Parameters.AddWithValue("@y5", pump.YData1[4]);
                    command.Parameters.AddWithValue("@x11", pump.XData2[0]);
                    command.Parameters.AddWithValue("@x12", pump.XData2[1]);
                    command.Parameters.AddWithValue("@x13", pump.XData2[2]);
                    command.Parameters.AddWithValue("@x14", pump.XData2[3]);
                    command.Parameters.AddWithValue("@x15", pump.XData2[4]);
                    command.Parameters.AddWithValue("@y11", pump.YData2[0]);
                    command.Parameters.AddWithValue("@y12", pump.YData2[1]);
                    command.Parameters.AddWithValue("@y13", pump.YData2[2]);
                    command.Parameters.AddWithValue("@y14", pump.YData2[3]);
                    command.Parameters.AddWithValue("@y15", pump.YData2[4]);

                    command.ExecuteNonQuery();
                }
            }
        }

        private void DeletePumpFromDatabase(int pumpId)
        {
            string databasePath = @"C:\sqlite\pumps.db";
            string connectionString = @"Data Source=" + databasePath + "; ";
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                string query = "DELETE FROM Pumps WHERE ID = @ID";

                using (SqliteCommand command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", pumpId);
                    command.ExecuteNonQuery();
                }
            }
        }

        private void AddPumpDataAndApproximation(Chart chart, Pump pump, List<double> xData, List<double> yData, int dataSetIndex)
        {
            string seriesName = $"{pump.Name}_Координаты{dataSetIndex}";
            AddDataSeries(chart, seriesName, xData, yData, SeriesChartType.Point);

            var (a, b) = LinearRegression(xData, yData);
            AddApproximationSeries(chart, $"{pump.Name}_Аппроксимация{dataSetIndex}", xData, a, b);


        }



        private void AddDataSeries(Chart chart, string seriesName, List<double> xData, List<double> yData, SeriesChartType chartType)
        {
            Series dataSeries = new Series
            {
                Name = seriesName,
                ChartType = chartType,
                MarkerSize = 10
            };

            for (int i = 0; i < xData.Count; i++)
            {
                dataSeries.Points.AddXY(xData[i], yData[i]);
            }

            chart.Series.Add(dataSeries);
        }

        private void AddApproximationSeries(Chart chart, string seriesName, List<double> xData, double a, double b)
        {
            Series lineSeries = new Series
            {
                Name = seriesName,
                ChartType = SeriesChartType.Line,
                BorderWidth = 5
            };

            double minX = xData.Min();
            double maxX = xData.Max();
            double step = (maxX - minX) / 100;

            for (double x = minX; x <= maxX; x += step)
            {
                double y = a * x + b;
                lineSeries.Points.AddXY(x, y);
            }

            chart.Series.Add(lineSeries);
        }

        private (double a, double b) LinearRegression(List<double> xData, List<double> yData)
        {
            int n = xData.Count;
            double sumX = xData.Sum();
            double sumY = yData.Sum();
            double sumXY = xData.Zip(yData, (x, y) => x * y).Sum();
            double sumX2 = xData.Sum(x => x * x);

            double a = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
            double b = (sumY - a * sumX) / n;

            return (a, b);
        }

        private void CreateCurve(Chart chart, double requiredPressure, double naporStaic, double V)
        {
            double xMin = 0;
            double xMax = 2;
            double step = 1;


            // Получение серии "Сеть" из Chart
            Series series = chart.Series["Сеть"];
            //series.ChartType = SeriesChartType.Line;
            series.BorderWidth = 3;
            // Проверка, что серия существует
            if (series != null)
            {
                series.Points.Clear();

                for (double x = xMin; x <= xMax; x += step)
                {
                    double y = requiredPressure + (naporStaic * Math.Pow(x, 2));
                    series.Points.AddXY(x, y);
                }
            }
            // Настройка области графика
            ChartArea chartArea = chart.ChartAreas[0];

            // Установка диапазона осей X и Y
            chartArea.AxisX.Minimum = xMin;
            chartArea.AxisX.Maximum = xMax;

            // Установка диапазона оси Y, чтобы данные были видны
            double yMin = series.Points.FindMinByValue("Y1").YValues[0];
            double yMax = series.Points.FindMaxByValue("Y1").YValues[0];
            chartArea.AxisY.Minimum = yMin - (Math.Abs(yMin) * 0.1); // Немного меньше минимального значения
            chartArea.AxisY.Maximum = yMax + (Math.Abs(yMax) * 0.1); // Немного больше максимального значения

            // Опционально: добавить сетку для лучшей видимости
            chartArea.AxisX.MajorGrid.LineColor = System.Drawing.Color.LightGray;
            chartArea.AxisY.MajorGrid.LineColor = System.Drawing.Color.LightGray;

            Console.WriteLine($"Y Min: {yMin}, Y Max: {yMax}");
            foreach (var point in series.Points)
            {
                MessageBox.Show($"X: {point.XValue}, Y: {point.YValues[0]}");
            }
        }

        private void FakeCrateCurve(Chart chart, double requiredPressure, double naporStaic)
        {
            double xMin = 0;
            double xMax = 20;
            double step = 1;

            // Получение серии "Сеть" из Chart
            Series series = chart.Series["Сеть"];

            // Проверка, что серия существует
            if (series != null)
            {
                series.Points.Clear();

                for (double x = xMin; x <= xMax; x += step)
                {
                    double y = 5 + (1 / 3.9) * (x * x);
                    series.Points.AddXY(x, y);
                }
            }
        }
        private const float CZoomScale = 4f;
        private int FZoomLevel = 0;
        private void chart_MouseWheel(object sender, MouseEventArgs e)
        {
            try
            {
                Axis xAxis = chart.ChartAreas[0].AxisX;
                double xMin = xAxis.ScaleView.ViewMinimum;
                double xMax = xAxis.ScaleView.ViewMaximum;
                double xPixelPos = xAxis.PixelPositionToValue(e.Location.X);

                if (e.Delta < 0 && FZoomLevel > 0)
                {
                    // Scrolled down, meaning zoom out
                    if (--FZoomLevel <= 0)
                    {
                        FZoomLevel = 0;
                        xAxis.ScaleView.ZoomReset();
                    }
                    else
                    {
                        double xStartPos = Math.Max(xPixelPos - (xPixelPos - xMin) * CZoomScale, 0);
                        double xEndPos = Math.Min(xStartPos + (xMax - xMin) * CZoomScale, xAxis.Maximum);
                        xAxis.ScaleView.Zoom(xStartPos, xEndPos);
                    }
                }
                else if (e.Delta > 0)
                {
                    // Scrolled up, meaning zoom in
                    double xStartPos = Math.Max(xPixelPos - (xPixelPos - xMin) / CZoomScale, 0);
                    double xEndPos = Math.Min(xStartPos + (xMax - xMin) / CZoomScale, xAxis.Maximum);
                    xAxis.ScaleView.Zoom(xStartPos, xEndPos);
                    FZoomLevel++;
                }
            }
            catch { }
        }
       
    }
    public class Pump
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public double Pressure { get; set; }
        public double Capacity { get; set; }
        public double Power { get; set; }
        public double Kpd { get; set; }
        public List<double> XData1 { get; set; }
        public List<double> YData1 { get; set; }
        public List<double> XData2 { get; set; }
        public List<double> YData2 { get; set; }
    }
}
