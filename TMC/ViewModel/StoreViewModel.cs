using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using TMC.Model;
using TMC.View;

namespace TMC.ViewModel
{
    public class StoreViewModel: INotifyPropertyChanged
    {
        ServiceCenterTMCEntities context = new ServiceCenterTMCEntities();
        ObservableCollection<RepairParts> _parts;
        ObservableCollection<WriteOff_RepairParts> _writeOffParts;
        string _searchText;
        ObservableCollection<RepairParts> _filteredParts;
        private ObservableCollection<RepairPartViewModel> _partsVm;
        private Dictionary<int, double> _avgSalesData;

        public ObservableCollection<RepairPartViewModel> RepairPartsListVm
        {
            get => _partsVm;
            set
            {
                _partsVm = value;
                OnPropertyChanged();
            }
        }
        public StoreViewModel()
        {
            try
            {
                _avgSalesData = CalculateAvgSales();

                var parts = context.RepairParts.ToList();
                _partsVm = new ObservableCollection<RepairPartViewModel>(
                    parts.Select(p => new RepairPartViewModel(p)
                    {
                        AvgSalesPerDay = _avgSalesData.TryGetValue(p.IdPart, out var avg) ? avg : 0
                    })
                );

                _parts = new ObservableCollection<RepairParts>(parts);
                
                _writeOffParts = new ObservableCollection<WriteOff_RepairParts>(context.WriteOff_RepairParts.ToList());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private Dictionary<int, double> CalculateAvgSales()
        {
            var tenDaysAgo = DateTime.Now.AddDays(-14);
            var today = DateTime.Now;

            int totalDays = Enumerable.Range(0, (today - tenDaysAgo).Days)
                .Select(d => tenDaysAgo.AddDays(d))
                .Count(d => d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday);

            totalDays = Math.Max(totalDays, 1);

            return context.Request_RepairParts
                .Where(r => r.Requests.Date >= tenDaysAgo && r.Requests.Date <= today)
                .GroupBy(r => r.RepairPartId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(r => r.Count) / (double)totalDays 
                );
        }


        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                OnPropertyChanged();
                FilterParts();
            }
        }

        public ObservableCollection<RepairParts> RepairPartsList
        {
            get { return _filteredParts; }
            set
            {
                _filteredParts = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<WriteOff_RepairParts> WriteOffList
        {
            get { return _writeOffParts; }
            set
            {
                _writeOffParts = value;
                OnPropertyChanged();
            }
        }

        private void FilterParts()
        {
            try
            {
                if (string.IsNullOrEmpty(_searchText))
                {
                    RepairPartsListVm = new ObservableCollection<RepairPartViewModel>(
                        _parts.Select(p => new RepairPartViewModel(p)
                        {
                            AvgSalesPerDay = _avgSalesData.TryGetValue(p.IdPart, out var avg) ? avg : 0
                        })
                    );
                    WriteOffList = new ObservableCollection<WriteOff_RepairParts>(context.WriteOff_RepairParts.ToList());
                }
                else
                {
                    _searchText = _searchText.ToLowerInvariant().Trim();
                    var filtered = context.RepairParts
                        .AsEnumerable()
                        .Where(e => e.Name.ToLowerInvariant().Contains(_searchText))
                        .Select(p => new RepairPartViewModel(p)
                        {
                            AvgSalesPerDay = _avgSalesData.TryGetValue(p.IdPart, out var avg) ? avg : 0
                        })
                        .ToList();

                    RepairPartsListVm = new ObservableCollection<RepairPartViewModel>(filtered);

                    var filteredWriteOff = context.WriteOff_RepairParts
                        .AsEnumerable()
                        .Where(e => e.RepairParts.Name.Contains(_searchText) ||
                               e.Date.ToString("dd.MM.yyyy").Contains(_searchText))
                        .ToList();

                    WriteOffList = new ObservableCollection<WriteOff_RepairParts>(filteredWriteOff);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private ObservableCollection<RepairPartViewModel> _selectedParts = new ObservableCollection<RepairPartViewModel>();
        public ObservableCollection<RepairPartViewModel> SelectedParts
        {
            get { return _selectedParts; }
            set
            {
                _selectedParts = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand AddSelectedPartsCommand
        {
            get
            {
                return new RelayCommand((o) =>
                {
                    try
                    {
                        AddPartsWindow window = o as AddPartsWindow;
                        var selectedItems = window.RepairPartsDG.SelectedItems.Cast<RepairPartViewModel>().ToList();
                        foreach (var item in selectedItems)
                        {
                            if (item.Count < 1) MessageBox.Show($"Недостаточно ЗИП \"{item.Name}\" на складе", "Склад ЗИП", MessageBoxButton.OK, MessageBoxImage.Warning);
                            else SelectedParts.Add(item);
                        }
                     (o as Window).DialogResult = true;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                });
            }
        }

        public RelayCommand AddRepairPartCommand
        {
            get
            {
                return new RelayCommand((o) =>
                {
                    try
                    {
                        RepairPartWindow repairPartWindow = new RepairPartWindow(new RepairParts(), this);
                        repairPartWindow.WriteOffBtn.Visibility = Visibility.Collapsed;
                        if (repairPartWindow.ShowDialog() == true)
                        {
                            RepairParts parts = repairPartWindow.RepairParts;
                            context.RepairParts.Add(parts);
                            context.SaveChanges();
                            _parts = new ObservableCollection<RepairParts>(context.RepairParts.ToList());
                            FilterParts();

                        }

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                });
            }
        }
        public RelayCommand EditRepairPartCommand
        {
            get
            {
                return new RelayCommand((selectedItem) =>
                {
                    try
                    {

                    var dataGrid = selectedItem as DataGrid;
                        var partVM = dataGrid.SelectedItem as RepairPartViewModel;
                        RepairParts part = context.RepairParts.Find(partVM.IdPart);
                        if (part == null) return;
                        RepairParts vm = new RepairParts
                        {
                            IdPart = part.IdPart,
                            Name = part.Name,
                            Cost = part.Cost,
                            Count = part.Count
                        };

                    RepairPartWindow repairPartWindow = new RepairPartWindow(vm, this);
                        if (repairPartWindow.ShowDialog() == true)
                        {
                            part = repairPartWindow.RepairParts;
                            context.RepairParts.AddOrUpdate(part);
                            context.SaveChanges();
                            _parts = new ObservableCollection<RepairParts>(context.RepairParts.ToList());
                            _filteredParts = _parts;
                            FilterParts();
                        }
                }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        });
            }
        }
        public RelayCommand WriteOffCommand
        {
            get
            {
                return new RelayCommand((selectedItem) =>
                {
                    try
                    {
                        var window = selectedItem as Window;
                        RepairParts part = window.DataContext as RepairParts;
                        if (part == null) return;
                        WriteOffRepairPart writeOffWindow = new WriteOffRepairPart(part.Count);
                        if (writeOffWindow.ShowDialog() == true)
                        {
                            WriteOff_RepairParts writeOff = new WriteOff_RepairParts
                            {
                                IdWriteOff = context.WriteOff_RepairParts.Count() + 1,
                                RepaitPartId = part.IdPart,
                                Date = DateTime.Now,
                                Count = Convert.ToInt16(writeOffWindow.PartCount.Text),
                                Reson = writeOffWindow.WriteOffReason.Text
                            };
                            context.WriteOff_RepairParts.AddOrUpdate(writeOff);
                            context.SaveChanges();
                            part.Count = part.Count - writeOff.Count;
                            var selectedPart = context.RepairParts.Find(part.IdPart);
                            selectedPart.Count = selectedPart.Count - writeOff.Count;
                            context.RepairParts.AddOrUpdate(selectedPart);
                            _parts = new ObservableCollection<RepairParts>(context.RepairParts.ToList());
                            _filteredParts = _parts;
                            FilterParts();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                });
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
