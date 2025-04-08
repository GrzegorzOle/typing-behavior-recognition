using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;
using LiveCharts;
using LiveCharts.Wpf;
using System.Collections.Generic;

namespace InvisiTypeAI
{
    using InvisiTypeAI.Model;

    public class ViewModelData : INotifyPropertyChanged
    {
        private bool _isMonitoring;

        private KeystrokeRecorder _recorder;

        private ObservableCollection<LabelScore> _predictions = new();

        public ViewModelData()
        {
            _recorder = new KeystrokeRecorder(OnKeystrokeCaptured);
            ChartSeries = new SeriesCollection();
            AxisX = new AxesCollection { new Axis { Title = "Label", LabelsRotation = 15 } };
            AxisY = new AxesCollection { new Axis { Title = "Score", LabelFormatter = value => value.ToString("P0") } };
        }

        private void OnKeystrokeCaptured(MLModelType.ModelInput input)
        {
            var results = MLModelType.PredictAllLabels(input);

            Predictions = new ObservableCollection<LabelScore>(
                results.Select(kv => new LabelScore { Label = kv.Key, Score = kv.Value }));

            UpdateChart();
        }

        public ObservableCollection<LabelScore> Predictions
        {
            get => _predictions;
            set => SetField(ref _predictions, value);
        }

        public SeriesCollection ChartSeries { get; set; }

        public AxesCollection AxisX { get; set; }

        public AxesCollection AxisY { get; set; }

        public bool IsMonitoring
        {
            get => _isMonitoring;
            set => SetField(ref _isMonitoring, value);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        public void StartOrStopMonitoring()
        {
            if (IsMonitoring)
            {
                _recorder.Stop();
                IsMonitoring = false;
            }
            else
            {
                _recorder.Start();
                IsMonitoring = true;
            }
        }

        private void UpdateChart()
        {
            // Jeżeli seria nie istnieje — zainicjuj ją
            if (ChartSeries.Count == 0)
            {
                var values = new ChartValues<float>(Predictions.Select(p => p.Score));
                ChartSeries.Add(new ColumnSeries { Title = "Prawdopodobieństwo", Values = values, DataLabels = true });
                AxisX[0].Labels = Predictions.Select(p => p.Label).ToArray();
            }
            else
            {
                var columnSeries = (ColumnSeries)ChartSeries[0];
                var newScores = Predictions.Select(p => p.Score).ToList();
                var newLabels = Predictions.Select(p => p.Label).ToArray();

                // aktualizuj tylko wartości
                columnSeries.Values.Clear();
                foreach (var score in newScores)
                    columnSeries.Values.Add(score);

                AxisX[0].Labels = newLabels;
            }
        }

    }
}