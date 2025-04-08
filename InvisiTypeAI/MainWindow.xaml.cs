using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace InvisiTypeAI
{
    using Microsoft.Win32;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ViewModelData ViewModel { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            ViewModel = new ViewModelData();
            DataContext = ViewModel;
        }

        private void StartMonitoring_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.StartOrStopMonitoring();
        }

        private void StartTraining_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
                                     {
                                         Filter = "CSV Files (*.csv)|*.csv",
                                         Title = "Wybierz plik CSV z danymi treningowymi"
                                     };

            if (openFileDialog.ShowDialog() == true)
            {
                string inputPath = openFileDialog.FileName;
                string modelOutputPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MLModelType.mlnet");

                try
                {
                    double accuracy = MLModelType.TrainAndReportAccuracy(inputPath, modelOutputPath);

                    MessageBox.Show($"Model został wytrenowany.\nDokładność (MicroAccuracy): {accuracy:P2}",
                        "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Błąd podczas trenowania modelu:\n{ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}