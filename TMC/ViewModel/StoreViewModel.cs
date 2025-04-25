using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls.WebParts;
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

        public StoreViewModel()
        {
            try
            {
                // Инициализация данных
                _parts = new ObservableCollection<RepairParts>(context.RepairParts.ToList());
                _filteredParts = new ObservableCollection<RepairParts>(_parts);
                _writeOffParts = new ObservableCollection<WriteOff_RepairParts>(context.WriteOff_RepairParts.ToList());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
                    RepairPartsList = new ObservableCollection<RepairParts>(context.RepairParts.ToList());
                    WriteOffList = new ObservableCollection<WriteOff_RepairParts>(context.WriteOff_RepairParts.ToList());
                }
                else
                {
                    _searchText = _searchText.ToLowerInvariant().Trim();
                    var filtered = context.RepairParts.AsEnumerable().Where(e => e.Name.ToLowerInvariant().Contains(_searchText));
                    var filteredWriteOff = context.WriteOff_RepairParts.AsEnumerable().Where(e =>  e.RepairParts.Name.Contains(_searchText) ||
                            e.Date.ToString("dd.MM.yyyy").Contains(_searchText))
                        .ToList(); RepairPartsList = new ObservableCollection<RepairParts>(filtered);
                    WriteOffList = new ObservableCollection<WriteOff_RepairParts>(filteredWriteOff);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private ObservableCollection<RepairParts> _selectedParts = new ObservableCollection<RepairParts>();
        public ObservableCollection<RepairParts> SelectedParts
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
                        var selectedItems = window.RepairPartsDG.SelectedItems.Cast<RepairParts>().ToList();
                        foreach (var item in selectedItems)
                        {
                            SelectedParts.Add(item);
                        }
                     // Закрываем окно после добавления услуг
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
                        MessageBox.Show(_searchText);
                        RepairPartWindow repairPartWindow = new RepairPartWindow(new RepairParts(), this);
                        MessageBox.Show(_searchText);
                        if (repairPartWindow.ShowDialog() == true)
                        {
                            RepairParts parts = repairPartWindow.RepairParts;
                            context.RepairParts.Add(parts);
                            context.SaveChanges();
                            MessageBox.Show(_searchText);
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
                    //try
                    //{

                    MessageBox.Show(_searchText);
                    var dataGrid = selectedItem as DataGrid;
                        RepairParts part = dataGrid.SelectedItem as RepairParts;
                        if (part == null) return;
                        RepairParts vm = new RepairParts
                        {
                            IdPart = part.IdPart,
                            Name = part.Name,
                            Cost = part.Cost,
                            Count = part.Count
                        };

                    MessageBox.Show(_searchText);
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
                    //}
                    //catch (Exception ex)
                    //{
                    //    MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    //}

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
