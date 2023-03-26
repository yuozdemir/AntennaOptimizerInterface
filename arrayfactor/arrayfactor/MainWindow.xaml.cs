using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO.Ports;
using System.Threading;
using System.Collections.Concurrent;
using System.Windows.Threading;
using System.IO;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using System.Data;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Microsoft.Win32;

using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Configurations;
using LiveCharts.Wpf;

using Excel = Microsoft.Office.Interop.Excel;
using System.Windows.Controls.Primitives;

using System.Windows.Xps.Packaging;

using System.Net.NetworkInformation;
using System.Configuration;

namespace arrayfactor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DataModel.DataModel dataModel = new DataModel.DataModel();

        CancellationTokenSource _tokenSource = null;
        public MainWindow()
        {
            InitializeComponent();

            string[] lines = File.ReadAllLines("settings.txt");
            foreach (string line in lines)
            {
                if (line.StartsWith("PythonIde"))
                {
                    int f = line.IndexOf("=");
                    PythonIDEFileSelectTextBox.Text = line.Substring(f + 1);
                }

                if (line.StartsWith("GeneticAlgorithm"))
                {
                    int f = line.IndexOf("=");
                    GAFileSelectTextBox.Text = line.Substring(f + 1);
                }

                if (line.StartsWith("ParticleSwarmOptimization"))
                {
                    int f = line.IndexOf("=");
                    PSOFileSelectTextBox.Text = line.Substring(f + 1);
                }

                if (line.StartsWith("Database"))
                {
                    int f = line.IndexOf("=");
                    DatabaseFileSelectTextBox.Text = line.Substring(f + 1);
                }
            }

            if (PythonIDEFileSelectTextBox.Text == "" || GAFileSelectTextBox.Text == "" || PSOFileSelectTextBox.Text == "" || DatabaseFileSelectTextBox.Text == "")
            {
                MessageBox.Show("Please set the file path settings!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            Task.Factory.StartNew(() => arrayfactor_opacity());

            XpsDocument antenna = new XpsDocument(@"Documents\antenna.xps", FileAccess.Read);
            AntennaDocument.Document = antenna.GetFixedDocumentSequence();

            XpsDocument ai = new XpsDocument(@"Documents\ai.xps", FileAccess.Read);
            AiDocument.Document = ai.GetFixedDocumentSequence();
        }

        public void arrayfactor_opacity()
        {
            double x = 1.0;
            while (true)
            {
                for (int i = 0; i < 90; i++)
                {
                    try
                    {
                        Dispatcher.Invoke(() => arrayfactor.Opacity = x);
                        x -= 0.01;
                        Thread.Sleep(20);
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }

                for (int i = 0; i < 90; i++)
                {
                    try
                    {
                        Dispatcher.Invoke(() => arrayfactor.Opacity = x);
                        x += 0.01;
                        Thread.Sleep(20);
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }
        }

        public void HiddenPanels()
        {
            HomePanel.Visibility = Visibility.Collapsed;
            AntennaPanel.Visibility = Visibility.Collapsed;
            AiPanel.Visibility = Visibility.Collapsed;
            OptimizationPanel.Visibility = Visibility.Collapsed;
            GraphicsPanel.Visibility = Visibility.Collapsed;
            SettingsPanel.Visibility = Visibility.Collapsed;
            AboutPanel.Visibility = Visibility.Collapsed;
        }

        public void PythonProcess_GA(ChartValues<ObservablePoint>[] chartValues, CancellationToken cancellationToken)
        {
            dataModel.ProgressBar_GA = true;

            var psi = new ProcessStartInfo();

            // Select Python File and Script
            psi.FileName = Dispatcher.Invoke(() => PythonIDEFileSelectTextBox.Text);
            var script = Dispatcher.Invoke(() => GAFileSelectTextBox.Text);

            // Parameters
            var topology = Dispatcher.Invoke(() => AntennaTopologiesComboBox_GA.Text);
            var n_iter = Dispatcher.Invoke(() => IterationComboBox_GA.Text);
            var n_bits = Dispatcher.Invoke(() => BitsComboBox_GA.Text);
            var n_pop = Dispatcher.Invoke(() => PopulationComboBox_GA.Text);
            var r_cross = Dispatcher.Invoke(() => CrossingOverComboBox_GA.Text);
            var N = Dispatcher.Invoke(() => AntennaComboBox_GA.Text);
            var b_boundsl = Dispatcher.Invoke(() => BBoundsLComboBox_GA.Text);
            var b_boundsh = Dispatcher.Invoke(() => BBoundsHComboBox_GA.Text);
            var d_boundsl = Dispatcher.Invoke(() => DBoundsLComboBox_GA.Text);
            var d_boundsh = Dispatcher.Invoke(() => DBoundsHComboBox_GA.Text);
            var w_boundsl = Dispatcher.Invoke(() => WBoundsLComboBox_GA.Text);
            var w_boundsh = Dispatcher.Invoke(() => WBoundsHComboBox_GA.Text);
            var dataBase = Dispatcher.Invoke(() => DatabaseFileSelectTextBox.Text);

            psi.Arguments = $"\"{script}\" \"{n_iter}\" \"{n_bits}\" \"{n_pop}\" \"{r_cross}\" \"{N}\" \"{b_boundsl}\" \"{b_boundsh}\" \"{d_boundsl}\" \"{d_boundsh}\" \"{w_boundsl}\" \"{w_boundsh}\" \"{topology}\" \"{dataBase}\"";

            // Process Configuration
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;

            // Execute Process and get Output.
            var errors = "";
            var results = "";

            using (var process = Process.Start(psi))
            {
                errors = process.StandardError.ReadToEnd();
                results = process.StandardOutput.ReadToEnd();
            }

            Excel.Application xlApp = new Excel.Application();
            xlApp.Visible = false;

            Excel.Workbook xlWorkbook = xlApp.Workbooks.Open(Dispatcher.Invoke(() => DatabaseFileSelectTextBox.Text) + @"\arrayfactor_ga.xlsx", 0, true, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);

            Excel._Worksheet xlWorksheet = xlWorkbook.Sheets[1];

            Excel.Range xlRange = xlWorksheet.UsedRange;


            /* --------------------------------------------------------------------------------------------------------*/

            Dispatcher.Invoke(() => BBoundsListBox_GA.Items.Clear());
            Dispatcher.Invoke(() => DBoundsListBox_GA.Items.Clear());
            Dispatcher.Invoke(() => WBoundsListBox_GA.Items.Clear());
            Dispatcher.Invoke(() => BandWidthListBox_GA.Items.Clear());
            Dispatcher.Invoke(() => SLLListBox_GA.Items.Clear());

            for (int i = 2; i < Convert.ToInt32(Dispatcher.Invoke(() => AntennaComboBox_GA.Text)) + 2; i++)
            {
                Dispatcher.Invoke(() => BBoundsListBox_GA.Items.Add(xlWorksheet.Cells[i, 4].Value));
                Dispatcher.Invoke(() => DBoundsListBox_GA.Items.Add(xlWorksheet.Cells[i, 5].Value));
                Dispatcher.Invoke(() => WBoundsListBox_GA.Items.Add(xlWorksheet.Cells[i, 6].Value));
            }

            Dispatcher.Invoke(() => BandWidthListBox_GA.Items.Add(xlWorksheet.Cells[2, 7].Value));
            Dispatcher.Invoke(() => SLLListBox_GA.Items.Add(xlWorksheet.Cells[2, 8].Value));


            /* --------------------------------------------------------------------------------------------------------*/

            foreach (var item in chartValues)
            {
                for (double x = 2; x <= 502; x++)
                {
                    var point = new ObservablePoint()
                    {
                        X = Convert.ToDouble(xlWorksheet.Cells[x, 9].Value),
                        Y = Convert.ToDouble(xlWorksheet.Cells[x, 10].Value)
                    };
                    item.Add(point);
                }
            }

            dataModel.DataMapper = new CartesianMapper<ObservablePoint>()
                .X(point => point.X)
                .Y(point => point.Y)
                .Stroke(point => point.Y > 0.3 ? Brushes.Red : Brushes.LightGreen)
                .Fill(point => point.Y > 0.3 ? Brushes.Red : Brushes.LightGreen);

            /* --------------------------------------------------------------------------------------------------------*/


            xlWorkbook.Close(true, null, null);
            xlApp.Quit();

            Marshal.ReleaseComObject(xlWorksheet);
            Marshal.ReleaseComObject(xlWorkbook);
            Marshal.ReleaseComObject(xlApp);

            dataModel.ProgressBar_GA = false;
        }

        public void PythonProcess_PSO(ChartValues<ObservablePoint>[] chartValues, CancellationToken cancellationToken)
        {
            dataModel.ProgressBar_PSO = true;

            var psi = new ProcessStartInfo();

            // Select Python File and Script
            psi.FileName = Dispatcher.Invoke(() => PythonIDEFileSelectTextBox.Text);
            var script = Dispatcher.Invoke(() => PSOFileSelectTextBox.Text);

            // Parameters
            var topology = Dispatcher.Invoke(() => AntennaTopologiesComboBox_PSO.Text);
            var n_iter = Dispatcher.Invoke(() => IterationComboBox_PSO.Text);
            var n_swarm = Dispatcher.Invoke(() => SwarmComboBox_PSO.Text);
            var N = Dispatcher.Invoke(() => AntennaComboBox_PSO.Text);
            var b_boundsl = Dispatcher.Invoke(() => BBoundsLComboBox_PSO.Text);
            var b_boundsh = Dispatcher.Invoke(() => BBoundsHComboBox_PSO.Text);
            var d_boundsl = Dispatcher.Invoke(() => DBoundsLComboBox_PSO.Text);
            var d_boundsh = Dispatcher.Invoke(() => DBoundsHComboBox_PSO.Text);
            var w_boundsl = Dispatcher.Invoke(() => WBoundsLComboBox_PSO.Text);
            var w_boundsh = Dispatcher.Invoke(() => WBoundsHComboBox_PSO.Text);
            var c1 = Dispatcher.Invoke(() => C1ComboBox_PSO.Text);
            var c2 = Dispatcher.Invoke(() => C2ComboBox_PSO.Text);
            var dataBase = Dispatcher.Invoke(() => DatabaseFileSelectTextBox.Text);

            psi.Arguments = $"\"{script}\" \"{n_iter}\" \"{n_swarm}\" \"{N}\" \"{b_boundsl}\" \"{b_boundsh}\" \"{d_boundsl}\" \"{d_boundsh}\" \"{w_boundsl}\" \"{w_boundsh}\" \"{c1}\" \"{c2}\" \"{topology}\" \"{dataBase}\"";

            // Process Configuration
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;

            // Execute Process and get Output.
            var errors = "";
            var results = "";

            using (var process = Process.Start(psi))
            {
                errors = process.StandardError.ReadToEnd();
                results = process.StandardOutput.ReadToEnd();
            }

            Excel.Application xlApp = new Excel.Application();
            xlApp.Visible = false;

            Excel.Workbook xlWorkbook = xlApp.Workbooks.Open(Dispatcher.Invoke(() => DatabaseFileSelectTextBox.Text) + @"\arrayfactor_pso.xlsx", 0, true, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);

            Excel._Worksheet xlWorksheet = xlWorkbook.Sheets[1];

            Excel.Range xlRange = xlWorksheet.UsedRange;


            /* --------------------------------------------------------------------------------------------------------*/

            Dispatcher.Invoke(() => BBoundsListBox_PSO.Items.Clear());
            Dispatcher.Invoke(() => DBoundsListBox_PSO.Items.Clear());
            Dispatcher.Invoke(() => WBoundsListBox_PSO.Items.Clear());
            Dispatcher.Invoke(() => BandWidthListBox_PSO.Items.Clear());
            Dispatcher.Invoke(() => SLLListBox_PSO.Items.Clear());

            for (int i = 2; i < Convert.ToInt32(Dispatcher.Invoke(() => AntennaComboBox_PSO.Text)) + 2; i++)
            {
                Dispatcher.Invoke(() => BBoundsListBox_PSO.Items.Add(xlWorksheet.Cells[i, 4].Value));
                Dispatcher.Invoke(() => DBoundsListBox_PSO.Items.Add(xlWorksheet.Cells[i, 5].Value));
                Dispatcher.Invoke(() => WBoundsListBox_PSO.Items.Add(xlWorksheet.Cells[i, 6].Value));
            }

            Dispatcher.Invoke(() => BandWidthListBox_PSO.Items.Add(xlWorksheet.Cells[2, 7].Value));
            Dispatcher.Invoke(() => SLLListBox_PSO.Items.Add(xlWorksheet.Cells[2, 8].Value));


            /* --------------------------------------------------------------------------------------------------------*/

            foreach (var item in chartValues)
            {
                for (double x = 2; x <= 502; x++)
                {
                    var point = new ObservablePoint()
                    {
                        X = Convert.ToDouble(xlWorksheet.Cells[x, 9].Value),
                        Y = Convert.ToDouble(xlWorksheet.Cells[x, 10].Value)
                    };
                    item.Add(point);
                }
            }

            dataModel.DataMapper = new CartesianMapper<ObservablePoint>()
                .X(point => point.X)
                .Y(point => point.Y)
                .Stroke(point => point.Y > 0.3 ? Brushes.Red : Brushes.LightGreen)
                .Fill(point => point.Y > 0.3 ? Brushes.Red : Brushes.LightGreen);

            /* --------------------------------------------------------------------------------------------------------*/


            xlWorkbook.Close(true, null, null);
            xlApp.Quit();

            Marshal.ReleaseComObject(xlWorksheet);
            Marshal.ReleaseComObject(xlWorkbook);
            Marshal.ReleaseComObject(xlApp);

            dataModel.ProgressBar_PSO = false;
        }

        public void FileSelect(TextBox textBox, string text)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            /*openFileDialog.DefaultExt = ".xlsx";
            openFileDialog.Filter = "Excel Files (*.xlsx)|*.xlsx|Old Excel Files (*.xls)|*.xls";*/

            if (openFileDialog.ShowDialog() == true)
            {
                if (text != "")
                {
                    string str = File.ReadAllText("settings.txt");
                    str = str.Replace(text + textBox.Text, text + openFileDialog.FileName);
                    File.WriteAllText("settings.txt", str);
                }

                textBox.Text = openFileDialog.FileName;
            }
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void ResizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState != WindowState.Maximized)
            {
                this.WindowState = WindowState.Maximized;
            }

            else
            {
                this.WindowState = WindowState.Normal;
            }
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void HomePanel_Click(object sender, MouseButtonEventArgs e)
        {
            HiddenPanels();
            HomePanel.Visibility = Visibility.Visible;
        }

        private void AntennaPanel_Click(object sender, MouseButtonEventArgs e)
        {
            HiddenPanels();
            AntennaPanel.Visibility = Visibility.Visible;
        }

        private void AiPanel_Click(object sender, MouseButtonEventArgs e)
        {
            HiddenPanels();
            AiPanel.Visibility = Visibility.Visible;
        }

        private void OptimizationPanel_Click(object sender, MouseButtonEventArgs e)
        {
            HiddenPanels();
            OptimizationPanel.Visibility = Visibility.Visible;
        }

        private void GraphicsPanel_Click(object sender, MouseButtonEventArgs e)
        {
            HiddenPanels();
            GraphicsPanel.Visibility = Visibility.Visible;
        }

        private void SettingsPanel_Click(object sender, MouseButtonEventArgs e)
        {
            HiddenPanels();
            SettingsPanel.Visibility = Visibility.Visible;
        }

        private void AboutPanel_Click(object sender, MouseButtonEventArgs e)
        {
            HiddenPanels();
            AboutPanel.Visibility = Visibility.Visible;
        }

        private void GAButton_Click(object sender, RoutedEventArgs e)
        {
            AlgorithmsPanel.Visibility = Visibility.Collapsed;
            GeneticAlgorithmPanel.Visibility = Visibility.Visible;
        }

        private void PSOButton_Click(object sender, RoutedEventArgs e)
        {
            AlgorithmsPanel.Visibility = Visibility.Collapsed;
            ParticleSwarmOptimizationPanel.Visibility = Visibility.Visible;
        }

        private void BackButtonGA_Click(object sender, RoutedEventArgs e)
        {
            GeneticAlgorithmPanel.Visibility = Visibility.Collapsed;
            AlgorithmsPanel.Visibility = Visibility.Visible;
        }

        private void BackButtonPSO_Click(object sender, RoutedEventArgs e)
        {
            ParticleSwarmOptimizationPanel.Visibility = Visibility.Collapsed;
            AlgorithmsPanel.Visibility = Visibility.Visible;
        }

        private void StartButton_GA_Click(object sender, RoutedEventArgs e)
        {
            dataModel.ChartValues_GA = new ChartValues<ObservablePoint>();
            ChartValues<ObservablePoint>[] observablePoints = { dataModel.ChartValues_GA };

            this.DataContext = dataModel;

            _tokenSource = new CancellationTokenSource();
            var token = _tokenSource.Token;
            var getPackage = Task.Factory.StartNew(() => PythonProcess_GA(observablePoints, token));
        }

        private void StartButton_PSO_Click(object sender, RoutedEventArgs e)
        {
            dataModel.ChartValues_PSO = new ChartValues<ObservablePoint>();
            ChartValues<ObservablePoint>[] observablePoints = { dataModel.ChartValues_PSO };

            this.DataContext = dataModel;

            _tokenSource = new CancellationTokenSource();
            var token = _tokenSource.Token;
            var getPackage = Task.Factory.StartNew(() => PythonProcess_PSO(observablePoints, token));
        }

        private void Graphic1FileSelectButton_Click(object sender, RoutedEventArgs e)
        {
            FileSelect(Graphic1FileName_TextBox, "");

            if (Graphic1FileName_TextBox.Text != "")
            {
                dataModel.ChartValues_Graphic1 = new ChartValues<ObservablePoint>();
                ChartValues<ObservablePoint>[] observablePoints = { dataModel.ChartValues_Graphic1 };

                this.DataContext = dataModel;

                Excel.Application xlApp = new Excel.Application();
                xlApp.Visible = false;

                Excel.Workbook xlWorkbook = xlApp.Workbooks.Open(Graphic1FileName_TextBox.Text, 0, true, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);

                Excel._Worksheet xlWorksheet = xlWorkbook.Sheets[1];

                Excel.Range xlRange = xlWorksheet.UsedRange;

                /* --------------------------------------------------------------------------------------------------------*/

                foreach (var item in observablePoints)
                {
                    for (double x = 2; x <= 502; x++)
                    {
                        var point = new ObservablePoint()
                        {
                            X = Convert.ToDouble(xlWorksheet.Cells[x, 9].Value),
                            Y = Convert.ToDouble(xlWorksheet.Cells[x, 10].Value)
                        };
                        item.Add(point);
                    }
                }

                dataModel.DataMapper = new CartesianMapper<ObservablePoint>()
                    .X(point => point.X)
                    .Y(point => point.Y)
                    .Stroke(point => point.Y > 0.3 ? Brushes.Red : Brushes.LightGreen)
                    .Fill(point => point.Y > 0.3 ? Brushes.Red : Brushes.LightGreen);

                /* --------------------------------------------------------------------------------------------------------*/


                xlWorkbook.Close(true, null, null);
                xlApp.Quit();

                Marshal.ReleaseComObject(xlWorksheet);
                Marshal.ReleaseComObject(xlWorkbook);
                Marshal.ReleaseComObject(xlApp);
            }
        }

        private void Graphic2FileSelectButton_Click(object sender, RoutedEventArgs e)
        {
            FileSelect(Graphic2FileName_TextBox, "");

            if (Graphic2FileName_TextBox.Text != "")
            {
                dataModel.ChartValues_Graphic2 = new ChartValues<ObservablePoint>();
                ChartValues<ObservablePoint>[] observablePoints = { dataModel.ChartValues_Graphic2 };

                this.DataContext = dataModel;

                Excel.Application xlApp = new Excel.Application();
                xlApp.Visible = false;

                Excel.Workbook xlWorkbook = xlApp.Workbooks.Open(Graphic2FileName_TextBox.Text, 0, true, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);

                Excel._Worksheet xlWorksheet = xlWorkbook.Sheets[1];

                Excel.Range xlRange = xlWorksheet.UsedRange;

                /* --------------------------------------------------------------------------------------------------------*/

                foreach (var item in observablePoints)
                {
                    for (double x = 2; x <= 502; x++)
                    {
                        var point = new ObservablePoint()
                        {
                            X = Convert.ToDouble(xlWorksheet.Cells[x, 9].Value),
                            Y = Convert.ToDouble(xlWorksheet.Cells[x, 10].Value)
                        };
                        item.Add(point);
                    }
                }

                dataModel.DataMapper = new CartesianMapper<ObservablePoint>()
                    .X(point => point.X)
                    .Y(point => point.Y)
                    .Stroke(point => point.Y > 0.3 ? Brushes.Red : Brushes.LightGreen)
                    .Fill(point => point.Y > 0.3 ? Brushes.Red : Brushes.LightGreen);

                /* --------------------------------------------------------------------------------------------------------*/


                xlWorkbook.Close(true, null, null);
                xlApp.Quit();

                Marshal.ReleaseComObject(xlWorksheet);
                Marshal.ReleaseComObject(xlWorkbook);
                Marshal.ReleaseComObject(xlApp);
            }
        }

        private void PythonIDEFileSelectButton_Click(object sender, RoutedEventArgs e)
        {
            FileSelect(PythonIDEFileSelectTextBox, "PythonIde=");
        }

        private void GAFileSelectButton_Click(object sender, RoutedEventArgs e)
        {
            FileSelect(GAFileSelectTextBox, "GeneticAlgorithm=");
        }

        private void PSOFileSelectButton_Click(object sender, RoutedEventArgs e)
        {
            FileSelect(PSOFileSelectTextBox, "ParticleSwarmOptimization=");
        }

        private void DataBaseFileSelectButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.ValidateNames = false;
            openFileDialog.CheckFileExists = false;
            openFileDialog.CheckPathExists = true;
            openFileDialog.FileName = "Folder Selection.";

            if (openFileDialog.ShowDialog() == true)
            {
                string str = File.ReadAllText("settings.txt");
                str = str.Replace("Database=" + DatabaseFileSelectTextBox.Text, "Database=" + System.IO.Path.GetDirectoryName(openFileDialog.FileName));
                File.WriteAllText("settings.txt", str);

                DatabaseFileSelectTextBox.Text = System.IO.Path.GetDirectoryName(openFileDialog.FileName);
            }
        }

        private void GraphicResetButton_Click(object sender, RoutedEventArgs e)
        {
            Graphic1FileName_TextBox.Text = "";
            Graphic2FileName_TextBox.Text = "";

            dataModel.ChartValues_Graphic1 = new ChartValues<ObservablePoint>();
            dataModel.ChartValues_Graphic2 = new ChartValues<ObservablePoint>();

            dataModel.ChartValues_Graphic1 = null;
            dataModel.ChartValues_Graphic2 = null;

            this.DataContext = dataModel;
        }
    }
}
