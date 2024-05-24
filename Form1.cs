using System.Data.SQLite;
using System.DirectoryServices.ActiveDirectory;
using System.Windows.Forms;
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
                    Parameter = result.Parameter
                });

                textBox1.Text = result.SpeedValue.ToString();
                textBox2.Text = result.Reynolds.ToString();
                textBox3.Text = result.Lambda.ToString();
                textBox4.Text = result.LostOfPressure.ToString();
                textBox5.Text = result.LocalLoss.ToString();
                textBox6.Text = result.RequiredInstallationPressure.ToString();
                textBox7.Text = result.Parameter;

                //var matchingEntries = _controller.GetMatchingEntries(result.ResultValue, 0.01);
                //dataGridView.DataSource = matchingEntries.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }
        
    }
}
