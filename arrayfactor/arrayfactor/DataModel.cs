using System;
using System.ComponentModel;
using System.Windows.Media;

using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Defaults;

using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;


using arrayfactor;

namespace DataModel
{
    public class DataModel : INotifyPropertyChanged
    {
        public DataModel()
        {

        }

        private bool progressBar_GA;
        public bool ProgressBar_GA
        {
            get => this.progressBar_GA;
            set
            {
                this.progressBar_GA = value;
                OnPropertyChanged();
            }
        }

        private bool progressBar_PSO;
        public bool ProgressBar_PSO
        {
            get => this.progressBar_PSO;
            set
            {
                this.progressBar_PSO = value;
                OnPropertyChanged();
            }
        }

        private double xMax;
        public double XMax
        {
            get => this.xMax;
            set
            {
                this.xMax = value;
                OnPropertyChanged();
            }
        }

        private double xMin;
        public double XMin
        {
            get => this.xMin;
            set
            {
                this.xMin = value;
                OnPropertyChanged();
            }
        }

        private object dataMapper;
        public object DataMapper
        {
            get => this.dataMapper;
            set
            {
                this.dataMapper = value;
                OnPropertyChanged();
            }
        }

        private ChartValues<ObservablePoint> chartValues_GA;
        public ChartValues<ObservablePoint> ChartValues_GA
        {
            get => this.chartValues_GA;
            set
            {
                this.chartValues_GA = value;
                OnPropertyChanged();
            }
        }

        private ChartValues<ObservablePoint> chartValues_PSO;
        public ChartValues<ObservablePoint> ChartValues_PSO
        {
            get => this.chartValues_PSO;
            set
            {
                this.chartValues_PSO = value;
                OnPropertyChanged();
            }
        }

        private ChartValues<ObservablePoint> chartValues_Graphic1;
        public ChartValues<ObservablePoint> ChartValues_Graphic1
        {
            get => this.chartValues_Graphic1;
            set
            {
                this.chartValues_Graphic1 = value;
                OnPropertyChanged();
            }
        }

        private ChartValues<ObservablePoint> chartValues_Graphic2;
        public ChartValues<ObservablePoint> ChartValues_Graphic2
        {
            get => this.chartValues_Graphic2;
            set
            {
                this.chartValues_Graphic2 = value;
                OnPropertyChanged();
            }
        }

        public Func<double, string> LabelFormatter => value => value.ToString("F");

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
