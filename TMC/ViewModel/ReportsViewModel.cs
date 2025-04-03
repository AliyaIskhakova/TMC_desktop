using LiveCharts.Defaults;
using LiveCharts.Wpf;
using LiveCharts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;
using TMC.Model;

namespace TMC.ViewModel
{
    public class ReportsViewModel : INotifyPropertyChanged
    {
        private DateTime? _startDate;
        private DateTime? _endDate;
        private SeriesCollection _pieSeries;

        public ReportsViewModel()
        {
            LoadDataCommand = new RelayCommand(LoadData, CanLoadData);
        }

        public DateTime? StartDate
        {
            get => _startDate;
            set
            {
                if (_startDate == value) return;
                _startDate = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public DateTime? EndDate
        {
            get => _endDate;
            set
            {
                if (_endDate == value) return;
                _endDate = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public SeriesCollection PieSeries
        {
            get => _pieSeries;
            private set
            {
                if (_pieSeries == value) return;
                _pieSeries = value;
                OnPropertyChanged();
            }
        }

        public ICommand LoadDataCommand { get; }

        private bool CanLoadData(object parameter)
        {
            return StartDate.HasValue && EndDate.HasValue && StartDate <= EndDate;
        }

        private void LoadData(object parameter)
        {
            if (!CanLoadData(parameter)) return;

            try
            {
                var statistics = GetRequestStatistics(StartDate.Value, EndDate.Value);
                PieSeries = CreatePieSeries(statistics);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке данных: {ex.Message}");
            }
        }

        private SeriesCollection CreatePieSeries(List<RequestStatistics> statistics)
        {
            var series = new SeriesCollection();
            var validStats = statistics.Where(s => s.Count > 0).ToList();

            foreach (var stat in validStats)
            {
                var statusName = GetStatusName(stat.StatusID);
                var color = GetStatusColor(statusName);

                var pieSeries = new PieSeries
                {
                    Title = statusName,
                    Values = new ChartValues<double> { stat.Count },
                    DataLabels = true,
                    LabelPoint = point => $"{point.SeriesView.Title}: {point.Y}",
                    Fill = (SolidColorBrush)new BrushConverter().ConvertFromString(color)
                };

                series.Add(pieSeries);
            }

            return series;
        }

        public List<RequestStatistics> GetRequestStatistics(DateTime startDate, DateTime endDate)
        {
            using (var context = new ServiceCenterTMCEntities())
            {
                return context.Status
                    .Select(s => new RequestStatistics
                    {
                        StatusID = s.IDstatus,
                        Count = s.Requests.Count(r => r.Date >= startDate && r.Date <= endDate)
                    })
                    .Where(s => s.Count > 0)
                    .ToList();
            }
        }

        private string GetStatusName(int statusId)
        {
            using (var context = new ServiceCenterTMCEntities())
            {
                return context.Status.Find(statusId)?.Name ?? "Неизвестный статус";
            }
        }

        private string GetStatusColor(string statusName)
        {
            switch (statusName)
            {
                case "Новый": return "#60B7FF";
                case "Готов": return "#90EE90";
                case "В работе": return "#FFD700";
                case "Завершен": return "#D3D3D3";
                case "Отменен": return "#D3D3D3";
                case "Ждет ЗИП": return "#FFA500";
                default: return "#FFFFFF";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}