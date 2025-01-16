using System.Data.SQLite;
using System.DirectoryServices.ActiveDirectory;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Microsoft.Data.Sqlite;
using static DiplomaWinForms1.Model;

namespace DiplomaWinForms1
{
    public partial class Form1 : Form
    {
        private readonly Controller _controller;

        private readonly DataBaseManager _dbManager;
        private string _databasePath = @"C:\sqlite\results.db";


        public Form1()
        {
            InitializeComponent();

            if (!File.Exists(_databasePath))
            {
                // Если файл не существует, создаем новый
                SQLiteConnection.CreateFile(_databasePath);
            }
            _dbManager = new DataBaseManager(_databasePath);
            var calculationService = new CalucationService();
            var fluidService = new FluidService();
            _controller = new Controller(calculationService, fluidService);

            //InitalizeChart();
        }

        ////////////////////////////////////////////////////////////////
        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {

        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {

        }

        //void InitalizeChart()
        //{
        //    chart1.Series.Clear();
        //    chart1.ChartAreas.Clear();
        //    chart1.ChartAreas.Add(new ChartArea("MainArea"));

        //    chart1.Series.Add(new Series("Сеть")
        //    {
        //        ChartType = SeriesChartType.Line
        //    });
        //}

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                var selectedFluid = fluidTypeComboBox.SelectedItem?.ToString();
                if (string.IsNullOrEmpty(selectedFluid))
                {
                    MessageBox.Show("Пожалуйста, выберите тип жидкости.");
                    return;
                }

                var density = _controller.GetFluidDensity(selectedFluid);
                var viscosity = _controller.GetFluidViscosity(selectedFluid);

                _dbManager.CreateResultsTable();
                var input = new CalculationData
                {
                    FluidDensity = density,
                    FluidViscosity = viscosity,
                    V = (double)(numericUpDown1.Value),
                    D = (double)(numericUpDown2.Value),
                    LengthPipe = (double)(numericUpDown3.Value),
                    PipeEntrance = (int)(numericUpDown4.Value),
                    ExitEntrance = (int)(numericUpDown5.Value),
                    Valve = (int)(numericUpDown6.Value),
                    Turn = (int)(numericUpDown7.Value),
                    CheckValue = (int)(numericUpDown8.Value),
                    Pk = (double)(numericUpDown9.Value),
                    Po = (double)(numericUpDown10.Value),
                    H = (double)(numericUpDown11.Value),
                };

                var result = _controller.Calculate(input);
                _dbManager.InsertResult(new CalcResult
                {
                    SpeedValue = result.SpeedValue,
                    Reynolds = result.Reynolds,
                    Lambda = result.Lambda,
                    LostOfPressure = result.LostOfPressure,
                    LocalLoss = result.LocalLoss,
                    RequiredInstallationPressure = result.RequiredInstallationPressure,
                    Parameter = result.Parameter,
                    NaporStaic = result.NaporStaic
                });

                textBox1.Text = result.SpeedValue.ToString();
                textBox2.Text = result.Reynolds.ToString();
                textBox3.Text = result.Lambda.ToString();
                textBox4.Text = result.LostOfPressure.ToString();
                textBox5.Text = result.LocalLoss.ToString();
                textBox6.Text = result.RequiredInstallationPressure.ToString();
                textBox7.Text = result.Parameter;
                textBox8.Text = result.NaporStaic.ToString();

                


                //double xMin = 0;
                //double xMax = input.V * 1.2;
                //double step = 0.0001;
                //double y = 0;

                //var series = chart1.Series["Сеть"];
                //series.Points.Clear();
               
                //for (double x = xMin; x <= xMax; x += step)
                //{
                    
                //    y = result.Required_Pressure + (result.NaporStaic * Math.Pow(x, 2));
                //    series.Points.AddXY(x, y);

                    

                //}

                //// Настраиваем оси
                //chart1.ChartAreas["MainArea"].AxisX.Title = "Q";
                //chart1.ChartAreas["MainArea"].AxisY.Title = "H";
                //chart1.ChartAreas["MainArea"].AxisX.Minimum = double.NaN; // Автоматическое масштабирование
                //chart1.ChartAreas["MainArea"].AxisX.Maximum = double.NaN; // Автоматическое масштабирование
                //chart1.ChartAreas["MainArea"].AxisY.Minimum = double.NaN; // Автоматическое масштабирование
                //chart1.ChartAreas["MainArea"].AxisY.Maximum = double.NaN;

                //AddApproximationLine(xData, yData);



                MessageBox.Show("Расчеты произведены успешно!");
                Form2 form2 = new Form2(result.Required_Pressure, result.NaporStaic, input.V);
                
                
                form2.ShowDialog();

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        //private void AddApproximationLine(List<double> xData, List<double> yData)
        //{
        //    if (chart1.Series.IndexOf("Approximation") != -1)
        //    {
        //        chart1.Series.Remove(chart1.Series["Approximation"]);
        //    }
        //    (double slope, double intercept) = LinearRegression(xData, yData);

        //    // Создаем новую серию для аппроксимированной линии
        //    var approxSeries = new Series("Approximation")
        //    {
        //        ChartType = SeriesChartType.Line,
        //        Color = System.Drawing.Color.Red
        //    };
            

        //    double xMin = xData[0];
        //    double xMax = xData[xData.Count - 1];

        //    approxSeries.Points.AddXY(xMin, slope * xMin + intercept);
        //    approxSeries.Points.AddXY(xMax, slope * xMax + intercept);

        //    chart1.Series.Add(approxSeries);
        //}

        //private (double, double) LinearRegression(List<double> xData, List<double> yData)
        //{
        //    if (xData.Count != yData.Count || xData.Count == 0)
        //        throw new ArgumentException("Списки данных должны быть ненулевой длины и одинакового размера.");

        //    int n = xData.Count;
        //    double sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0;

        //    for (int i = 0; i < n; i++)
        //    {
        //        sumX += xData[i];
        //        sumY += yData[i];
        //        sumXY += xData[i] * yData[i];
        //        sumX2 += xData[i] * xData[i];
        //    }

        //    double slope = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
        //    double intercept = (sumY - slope * sumX) / n;

        //    return (slope, intercept);
        //}
    }
}
