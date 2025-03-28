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
using TMC.Model;
using TMC.View;

namespace TMC.ViewModel
{
    public class StoreViewModel: INotifyPropertyChanged
    {
        ServiceCenterTMCEntities context = new ServiceCenterTMCEntities();
        ObservableCollection<RepairParts> _parts;
        string _searchText;
        ObservableCollection<RepairParts> _filteredParts;

        public StoreViewModel()
        {
            try
            {
                // Инициализация данных
                _parts = new ObservableCollection<RepairParts>(context.RepairParts.ToList());
                _filteredParts = new ObservableCollection<RepairParts>(_parts);
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

        private void FilterParts()
        {
            try
            {
                if (string.IsNullOrEmpty(_searchText))
                {
                    RepairPartsList = new ObservableCollection<RepairParts>(_parts);
                }
                else
                {
                    var filtered = _parts.Where(e => e.Name.ToLowerInvariant().Contains(_searchText.ToLowerInvariant().Trim()));
                    RepairPartsList = new ObservableCollection<RepairParts>(filtered);
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
                        RepairPartWindow repairPartWindow = new RepairPartWindow(new RepairParts());
                        if (repairPartWindow.ShowDialog() == true)
                        {
                            RepairParts parts = repairPartWindow.RepairParts;
                            //client.Telephone = "avae";
                            context.RepairParts.Add(parts);
                            context.SaveChanges();
                            RepairPartsList = new ObservableCollection<RepairParts>(context.RepairParts.ToList());
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
                return new RelayCommand((o) =>
                {
                    try
                    {
                        RepairParts selectedPart = o as RepairParts;
                        RepairPartWindow repairPartWindow = new RepairPartWindow(selectedPart);
                        if (repairPartWindow.ShowDialog() == true)
                        {

                            context.RepairParts.AddOrUpdate(selectedPart);
                            context.SaveChanges();
                            RepairPartsList = new ObservableCollection<RepairParts>(context.RepairParts.ToList());
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
