using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpFluids;
using EngineeringUnits;
using EngineeringUnits.Units;
using Newtonsoft.Json.Linq;
using Microsoft.Data.Sqlite;

namespace DiplomaWinForms1
{
    internal class Model
    {
        public class CalculationData
        {
            public double V { get; set; }// Объем (м³)
            public double D { get; set; }//Диаметр трубы (м)
            public double LengthPipe { get; set; }//Длина трубопровода (м)  
            public int PipeEntrance {  get; set; }//Количество входов
            public int ExitEntrance { get; set; }//Количество выходов
            public int Valve {  get; set; }//Количество вентилей
            public int Turn {  get; set; }//Количество поворотов на 90°
            public int CheckValue { get; set; }//Количество обратных клапанов
            public double Pk { get; set; } //Давление в конечной точки сети 
            public double Po {  get; set; }//Давление на свободной поверхности питающей емкости 
            public double H {  get; set; }//Высота подъема жидкости (м)
            public double FluidDensity { get; set; }//Плотность жидкости (кг/м³)
            public double FluidViscosity { get; set; }//Вязкость жидкости (Па·с)
            
        }

        public class CalcResult
        {
            public double SpeedValue { get; set; }//Скорость жидкости (м/с) 
            public double Reynolds {  get; set; }//Число Рейнольдса
            public double Lambda { get; set; }//Коэффициент гидравлического сопротивления
            public double LostOfPressure {  get; set; }//Потери давления (Па)
            public double LocalLoss { get; set; }//Местные потери давления (Па)
            public double RequiredInstallationPressure { get; set; }//Необходимое давление на входе (Па)
            public string Parameter { get; set; }// Характер потока (например, ламинарный, турбулентный)   
            public double NaporStaic { get; set; }//статический наопр
            public double Required_Pressure { get; set; }
        }

        public interface ICalculationService
        {
            CalcResult PerformCalculation(CalculationData input);
        }

        public class CalucationService : ICalculationService
        {
            public CalcResult PerformCalculation(CalculationData input)
            {
                double kinV = input.FluidViscosity / input.FluidDensity;
                double S = (3.14 * Math.Pow(input.D, 2)) / 4;

                double calcSpeed = CalcSpeed(input.V, S);

                double Re = calcRe(calcSpeed, input.D, kinV);

                string parameter = flowMode(Re);


                double lossOfPressure = firstHalf_lossOfPressure(calcSpeed, input.LengthPipe, input.D, Re) * secondHalf_lossOfPressure(Re, input.D);
                double local_loss = Calc_local_loss(input.Turn, input.ExitEntrance, input.Valve, input.PipeEntrance, input.CheckValue, calcSpeed, input.LengthPipe, input.D);

                double Required_Pressure = Calc_Required_Pressure(input.Po, input.Pk, input.H, input.FluidDensity);
                double cal_R_P = Required_Pressure + local_loss + lossOfPressure;


                double staticNapor = (local_loss + lossOfPressure) / Math.Pow(input.V, 2);

                return new CalcResult
                {
                    SpeedValue = calcSpeed,
                    Reynolds = Re,
                    Parameter = parameter,
                    Lambda = secondHalf_lossOfPressure(Re, input.D),
                    LostOfPressure = lossOfPressure * input.FluidDensity * 9.81f,
                    LocalLoss = local_loss,
                    Required_Pressure = Required_Pressure,
                    RequiredInstallationPressure = cal_R_P,
                    NaporStaic = staticNapor
                };




                static double CalcSpeed(double V, double S)
                {
                    double calc = V / S;
                    return calc;
                }

                static double calcRe(double w, double d, double kinV)
                {
                    double calc = (w * d) / kinV;
                    return calc;
                }

                static string flowMode(double Re)
                {
                    string Parameter = String.Empty;
                    switch (Re)
                    {
                        case double n when n < 2320:
                            return Parameter = "Ламинарный";
                            break;
                        case double n when n >= 2320 && n < 10000:
                            return Parameter = "Переходная область развития турбулентности";
                            break;
                        case double n when n >= 10000:
                            return Parameter = "Турбулентный";
                            break;
                        default:
                            return Parameter = "Некоректный ввод!";
                            break;
                    }
                }

                static double secondHalf_lossOfPressure(double Re, double d)
                {
                    double delta = 0.0002;
                    switch (Re)
                    {
                        case double n when n <= 2320:
                            return 64 / Re;
                        //break; ///еще одно условие нужно
                        case double n when 2320 < n && n < 3000:
                            return 0.029 + 0.775 * (Re - 2320) * Math.Pow(10, -5);
                        //break;
                        case double n when 3000 <= Re && Re < 15 * (d / delta):
                            return 0.3164 / Math.Pow(Re, 0.25);
                        //break;
                        case double n when 15 * (d / delta) < Re && Re < 300 * (d / delta):
                            return 0.11 * (Math.Pow((delta / d) + (68 / Re), 0.25));
                        //break;
                        case double n when n > 300 * (d / delta):
                            return 0.11 * Math.Pow((delta / d), 0.25);
                        //break;
                        default:
                            return 0;
                    }
                }

                static double firstHalf_lossOfPressure(double w, double l, double d, double Re)
                {
                    double g = 9.8;
                    double calc = secondHalf_lossOfPressure(Re, d) * (l / d) * (Math.Pow(w, 2) / (2 * g));
                    return calc;
                }

                static double Calc_local_loss(int turn, int exit, int valve, int pipe_entrance, int check_value, double w, double l, double d)
                {
                    double g = 9.8;
                    double res = pipe_entrance * 0.5 + exit * 1 + valve * 5 + turn * 0.1 + check_value * 7;
                    return res * Math.Pow(w, 2) / (2 * g);
                }
                static double Calc_Required_Pressure(double po, double pk, double h, double Ro)
                {
                    double g = 9.8;
                    double res = h + (pk - po) / (Ro * g);
                    return res;
                }



            }
        }

        public interface IFluidService
        {
            double GetDensity(string fluidType);
            double GetViscosity(string fluidType);
        }

        public class FluidService : IFluidService
        {
            public double GetDensity(string fluidType)
            {
                if(fluidType == "Нефть")
                {
                    return GetOilDensity();
                }

                Fluid fluid = GetFluid(fluidType);               
                fluid.UpdatePT(Pressure.FromBars(1), Temperature.FromDegreesCelsius(20)); // Стандартные условия
                return fluid.Density.As(DensityUnit.KilogramPerCubicMeter);
            }

            public double GetViscosity(string fluidType)
            {
                if (fluidType == "Нефть")
                {
                    return GetOilViscosity();
                }

                Fluid fluid = GetFluid(fluidType);               
                fluid.UpdatePT(Pressure.FromBars(1), Temperature.FromDegreesCelsius(20)); // Стандартные условия
                return fluid.DynamicViscosity.PascalSecond;
            }

            private Fluid GetFluid(string fluidType)
            {
                Fluid fluid;

                switch (fluidType)
                {
                    case "Вода":
                        fluid = new Fluid(FluidList.Water);
                        break; 
                    case "Глицерин (50%)":
                        fluid = new Fluid(FluidList.MixGlycerolAQ); 
                        break;
                    case "Метиловый спирт":
                        fluid = new Fluid(FluidList.Methanol); break;
                    case "Этиловый спирт":
                        fluid = new Fluid(FluidList.Ethanol); break;
                    case "Ацетон":
                        fluid = new Fluid(FluidList.Acetone); break;
                    case "Толуол":
                        fluid = new Fluid(FluidList.Toluene); break;
                    case "Нефть":    
                    default:
                        throw new ArgumentException("Неизвестный тип жидкости");
                }

                return fluid;
            }

            private double GetOilDensity()
            {
                return 850;
            }

            private double GetOilViscosity()
            {
                return 0.05;
            }



        }

        public class DataBaseManager
        {
            private string _connectionString;

            public DataBaseManager(string databasePath)
            {
                _connectionString = @"Data Source=" + databasePath+ "; ";
            }

            public void CreateResultsTable()
            {
                using (var connection = new SqliteConnection(_connectionString))
                {
                    connection.Open();

                    string createTableQuery = @"CREATE TABLE IF NOT EXISTS results (
                                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                            Speed REAL,
                                            ReynoldsNumber REAL,
                                            Lambda REAL,
                                            Parameter TEXT,
                                            HeadLossStraightPipe REAL,
                                            LocalLosses REAL,
                                            RequiredHead REAL
                                        );";

                    using (var command = new SqliteCommand(createTableQuery, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }

            public void InsertResult(CalcResult result)
            {

                using (var _connection = new SqliteConnection(_connectionString))
                {
                    _connection.Open();

                    string insertQuery = @"INSERT INTO results (Speed, ReynoldsNumber, Lambda, Parameter, HeadLossStraightPipe, LocalLosses, RequiredHead)
                                    VALUES (@Speed, @ReynoldsNumber, @Lambda, @Parameter, @HeadLossStraightPipe, @LocalLosses, @RequiredHead);";

                    using (var command = new SqliteCommand(insertQuery, _connection))
                    {
                        command.Parameters.AddWithValue("@Speed", result.SpeedValue);
                        command.Parameters.AddWithValue("@ReynoldsNumber", result.Reynolds);
                        command.Parameters.AddWithValue("@Lambda", result.Lambda);
                        command.Parameters.AddWithValue("@Parameter", result.Parameter);
                        command.Parameters.AddWithValue("@HeadLossStraightPipe", result.LostOfPressure);
                        command.Parameters.AddWithValue("@LocalLosses", result.LocalLoss);
                        command.Parameters.AddWithValue("@RequiredHead", result.RequiredInstallationPressure);

                        command.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}
