using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.Office.Interop.Word;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using TMC.Model;
using SeriesCollection = LiveCharts.SeriesCollection;

namespace TMC.ViewModel
{
    public class ReportsViewModel : INotifyPropertyChanged
    {
        private DateTime? _startDate = DateTime.Now.AddMonths(-1);
        private DateTime? _endDate = DateTime.Now;
        private SeriesCollection _pieSeries;
        private SeriesCollection _employeeOrdersSeries;
        private SeriesCollection _employeeRevenueSeries;
        private ObservableCollection<EmployeeStat> _employeeStats;
        private int _totalOrders;
        private int _completedOrders;
        private decimal _totalRevenue;
        private List<string> _employeeFullNames;
        public SeriesCollection OrdersByDaySeries { get; private set; }
        public List<string> DayLabels { get; private set; }
        public string DaysCountText { get; private set; }

        public ReportsViewModel()
        {
            LoadDataCommand = new RelayCommand(LoadData, CanLoadData);
            LoadData(null);
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

        public SeriesCollection EmployeeOrdersSeries
        {
            get => _employeeOrdersSeries;
            private set
            {
                if (_employeeOrdersSeries == value) return;
                _employeeOrdersSeries = value;
                OnPropertyChanged();
            }
        }

        public SeriesCollection EmployeeRevenueSeries
        {
            get => _employeeRevenueSeries;
            private set
            {
                if (_employeeRevenueSeries == value) return;
                _employeeRevenueSeries = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<EmployeeStat> EmployeeStats
        {
            get => _employeeStats;
            private set
            {
                if (_employeeStats == value) return;
                _employeeStats = value;
                OnPropertyChanged();
            }
        }

        public List<string> EmployeeFullNames
        {
            get => _employeeFullNames;
            private set
            {
                if (_employeeFullNames == value) return;
                _employeeFullNames = value;
                OnPropertyChanged();
            }
        }

        public int TotalOrders
        {
            get => _totalOrders;
            private set
            {
                if (_totalOrders == value) return;
                _totalOrders = value;
                OnPropertyChanged();
            }
        }

        public int CompletedOrders
        {
            get => _completedOrders;
            private set
            {
                if (_completedOrders == value) return;
                _completedOrders = value;
                OnPropertyChanged();
            }
        }

        public decimal TotalRevenue
        {
            get => _totalRevenue;
            private set
            {
                if (_totalRevenue == value) return;
                _totalRevenue = value;
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
                using (var context = new ServiceCenterTMCEntities())
                {
                    var startDate = StartDate.Value;
                    var endDate = EndDate.Value;

                    // Load status statistics
                    var statusStatistics = context.Statuses
                        .Select(s => new RequestStatistics
                        {
                            StatusID = s.IdStatus,
                            Count = s.Requests.Count(r => r.Date >= startDate && r.Date <= endDate)
                        })
                        .Where(s => s.Count > 0)
                        .ToList();

                    PieSeries = CreatePieSeries(statusStatistics);
                    LoadOrdersByDayData(startDate, endDate);
                    // Load employee statistics
                    var requests = context.Requests
                        .Where(r => r.Date >= startDate && r.Date <= endDate)
                        .ToList();

                    ProcessEmployeeStatistics(requests);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке данных: {ex.Message}");
            }
        }

        private void ProcessEmployeeStatistics(List<Requests> requests)
        {
            TotalOrders = requests.Count;
            CompletedOrders = requests.Count(r => r.StatusId == 5);
            TotalRevenue = (decimal)requests.Sum(r => r.Cost);

            var employeeGroups = requests
                .Where(r => r.MasterId.HasValue)
                .GroupBy(r => r.Employees)
                .Select(g => new EmployeeStat
                {
                    Employee = g.Key,
                    CompletedOrders = g.Count(r => r.StatusId == 5),
                    TotalOrders = g.Count(),
                    Revenue = (decimal)g.Sum(r => r.Cost),
                    Services = g.SelectMany(r => r.Requests_Services)
                        .GroupBy(rs => rs.Services)
                        .Select(sg => new ServiceStat
                        {
                            Name = sg.Key.Name,
                            Count = sg.Sum(x => x.Count),
                            Cost = (decimal)sg.Sum(x => x.Count * sg.Key.Cost)
                        }).ToList()
                })
                .OrderByDescending(e => e.Revenue)
                .ToList();

            EmployeeStats = new ObservableCollection<EmployeeStat>(employeeGroups);
            EmployeeFullNames = employeeGroups.Select(e => e.FullName).ToList();

            // Prepare chart data with full names
            EmployeeOrdersSeries = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "Выполненные заказы",
                    Values = new ChartValues<int>(employeeGroups.Select(e => e.CompletedOrders)),
                    DataLabels = true,
                    LabelPoint = point => $"{point.Y}",
                    Fill = Brushes.DodgerBlue
                }
            };

            EmployeeRevenueSeries = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "Выручка",
                    Values = new ChartValues<decimal>(employeeGroups.Select(e => e.Revenue)),
                    DataLabels = true,
                    LabelPoint = point => $"{point.Y:C}",
                    Fill = Brushes.MediumSeaGreen
                }
            };
        }


        //private void LoadOrdersByDayData(DateTime startDate, DateTime endDate)
        //{
        //    using (var context = new ServiceCenterTMCEntities())
        //    {
        //        // Рассчитываем реальное количество дней
        //        int totalDays = (endDate - startDate).Days + 1;
        //        DaysCountText = $"{totalDays} дней";

        //        // Получаем данные по дням
        //        var dailyData = context.Requests
        //            .Where(r => r.Date >= startDate && r.Date <= endDate)
        //            .GroupBy(r => EntityFunctions.TruncateTime(r.Date))
        //            .Select(g => new
        //            {
        //                Date = g.Key,
        //                Count = g.Count()
        //            })
        //            .OrderBy(x => x.Date)
        //            .ToList();

        //        // Заполняем пропущенные дни нулями
        //        var allDates = Enumerable.Range(0, totalDays)
        //            .Select(offset => startDate.AddDays(offset).Date)
        //            .ToList();

        //        var completeData = allDates
        //            .GroupJoin(dailyData,
        //                date => date,
        //                data => data.Date,
        //                (date, data) => new
        //                {
        //                    Date = date,
        //                    Count = data.Select(x => x.Count).FirstOrDefault()
        //                })
        //            .OrderBy(x => x.Date)
        //            .ToList();

        //        // Подготавливаем данные для графика
        //        DayLabels = completeData.Select(x => x.Date.ToString("dd.MM.yyyy")).ToList();

        //        OrdersByDaySeries = new SeriesCollection
        //    {
        //        new ColumnSeries
        //        {
        //            Title = "Заказы",
        //            Values = new ChartValues<int>(completeData.Select(x => x.Count)),
        //            Fill = Brushes.DodgerBlue,
        //            DataLabels = true,
        //            LabelPoint = point => point.Y > 0 ? point.Y.ToString() : ""
        //        }
        //    };

        //        OnPropertyChanged(nameof(DaysCountText));
        //        OnPropertyChanged(nameof(DayLabels));
        //        OnPropertyChanged(nameof(OrdersByDaySeries));
        //    }
        //}

        private void LoadOrdersByDayData(DateTime startDate, DateTime endDate)
        {
            try
            {
                using (var context = new ServiceCenterTMCEntities())
                {
                    // Рассчитываем реальное количество дней
                    int totalDays = (endDate - startDate).Days + 1;
                    DaysCountText = $"{totalDays} дней";

                    // Получаем данные по дням
                    var dailyData = context.Requests
                        .Where(r => r.Date >= startDate && r.Date <= endDate)
                        .AsEnumerable() // Переключаемся на клиентскую обработку
                        .GroupBy(r => r.Date)
                        .Select(g => new
                        {
                            Date = g.Key,
                            Count = g.Count()
                        })
                        .OrderBy(x => x.Date)
                        .ToList();

                    // Заполняем пропущенные дни нулями
                    var completeData = Enumerable.Range(0, totalDays)
                        .Select(offset => new
                        {
                            Date = startDate.AddDays(offset).Date,
                            Count = dailyData
                                .Where(d => d.Date == startDate.AddDays(offset).Date)
                                .Select(d => d.Count)
                                .FirstOrDefault()
                        })
                        .ToList();

                    // Подготавливаем данные для графика
                    DayLabels = completeData.Select(x => x.Date.ToString("dd.MM.yyyy")).ToList();


                    // Создаем серии для графика
                    OrdersByDaySeries = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Заказы",
                    Values = new ChartValues<int>(completeData.Select(x => x.Count)),
                    PointGeometry = DefaultGeometries.Circle,
                    PointGeometrySize = 8,
                    Stroke = Brushes.DodgerBlue,
                    StrokeThickness = 2,
                    Fill = Brushes.Transparent
                }
            };

                    OnPropertyChanged(nameof(DaysCountText));
                    OnPropertyChanged(nameof(DayLabels));
                    OnPropertyChanged(nameof(OrdersByDaySeries));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных по дням: {ex.Message}");
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

        private string GetStatusName(int statusId)
        {
            using (var context = new ServiceCenterTMCEntities())
            {
                return context.Statuses.Find(statusId)?.Name ?? "Неизвестный статус";
            }
        }

        private string GetStatusColor(string statusName)
        {
            switch (statusName)
            {
                case "Новая": return "#60B7FF";
                case "Готова": return "#90EE90";
                case "В работе": return "#FFD700";
                case "Завершена": return "#D3D3D3";
                case "Отменена": return "#D3D3D3";
                case "Ждет ЗИП": return "#FFA500";
                case "Диагностика": return "#BDFB82";
                default: return "#FFFFFF";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RequestStatistics
    {
        public int StatusID { get; set; }
        public int Count { get; set; }
    }

    public class EmployeeStat
    {
        public Employees Employee { get; set; }
        public string FullName => $"{Employee.Surname} {Employee.Name[0]}. {Employee.Patronymic[0]}.";
        public int CompletedOrders { get; set; }
        public int TotalOrders { get; set; }
        public decimal Revenue { get; set; }
        public List<ServiceStat> Services { get; set; }
    }

    public class ServiceStat
    {
        public string Name { get; set; }
        public int Count { get; set; }
        public decimal Cost { get; set; }
    }
}