using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DiplomaWinForms1.Model;

namespace DiplomaWinForms1
{
    internal class Controller
    {
        private readonly ICalculationService _calculationService;
        //private readonly IDataService _dataService;
        private readonly IFluidService _fluidService;

        public Controller(ICalculationService calculationService, /*IDataService dataService,*/ IFluidService fluidService)
        {
            _calculationService = calculationService;
            //_dataService = dataService;
            _fluidService = fluidService;
        }

        public double GetFluidDensity(string fluidType)
        {
            return _fluidService.GetDensity(fluidType);
        }

        public double GetFluidViscosity(string fluidType)
        {
            return _fluidService.GetViscosity(fluidType);
        }

        public CalcResult Calculate(CalculationData input)
        {
            return _calculationService.PerformCalculation(input);
        }
    }
}
