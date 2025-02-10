using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TMC.Model;
using TMC.View;

namespace TMC.ViewModel
{
    public class ServicesViewModel: INotifyPropertyChanged
    {
        ServiceCenterTMCEntities context = new ServiceCenterTMCEntities();
        ObservableCollection<Services> _services;
        string _searchText;
        RelayCommand? addCommand;
        RelayCommand? editCommand;
        ObservableCollection<Services> _filteredServices;

        public ServicesViewModel()
        {
            // Инициализация данных
            _services = new ObservableCollection<Services>(context.Services.ToList());
            _filteredServices = new ObservableCollection<Services>(_services);
        }



        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                OnPropertyChanged();
                FilterServices();
            }
        }

        public ObservableCollection<Services> ServicesList
        {
            get { return _filteredServices; }
            set
            {
                _filteredServices = value;
                OnPropertyChanged();
            }
        }

        private void FilterServices()
        {
            if (string.IsNullOrEmpty(_searchText))
            {
                ServicesList = new ObservableCollection<Services>(_services);
            }
            else
            {
                var filtered = _services.Where(e => e.Name.ToLowerInvariant().StartsWith(_searchText.ToLowerInvariant().Trim()));
                ServicesList = new ObservableCollection<Services>(filtered);
            }
        }

        private ObservableCollection<Services> _selectedServices = new ObservableCollection<Services>();
        public ObservableCollection<Services> SelectedServices
        {
            get { return _selectedServices; }
            set
            {
                _selectedServices = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand AddSelectedServicesCommand
        {
            get
            {
                return new RelayCommand((o) =>
                {
                    AddServicesWindow window = o as AddServicesWindow;
                    var selectedItems = window.ServicesDG.SelectedItems.Cast<Services>().ToList();
                    foreach (var item in selectedItems)
                    {
                        SelectedServices.Add(item);
                    }
                    // Закрываем окно после добавления услуг
                    (o as Window).DialogResult = true;
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
