using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
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
        ObservableCollection<Services> _selectedServices;

        public ServicesViewModel()
        {
            // Инициализация данных
            _services = new ObservableCollection<Services>(context.Services.ToList());
            _filteredServices = new ObservableCollection<Services>(_services);
            SelectedServices = new ObservableCollection<Services>();
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

        public ObservableCollection<Services> SelectedServices
        {
            get {
                return _selectedServices;
            }
            set
            {
                _selectedServices = value;
                OnPropertyChanged(nameof(SelectedServices));
            }
        }


        public RelayCommand AddServiceCommand
        {
            get
            {
                return addCommand ??
                  (addCommand = new RelayCommand((o) =>
                  {
                      //ServiceWindow clientWindow = new ServiceWindow(new Services());
                      //if (clientWindow.ShowDialog() == true)
                      //{
                      //    Services client = clientWindow.Services;
                      //    MessageBox.Show(client.telephone + client.type);
                      //    //client.Telephone = "avae";
                      //    context.Services.Add(client);
                      //    context.SaveChanges();
                      //    _services = new ObservableCollection<Services>(context.Services.ToList());
                      //    ServicesList = new ObservableCollection<Services>(_services);

                      //}
                  }));
            }
        }

        public RelayCommand EditServiceCommand
        {
            get
            {
                return editCommand ??
                  (editCommand = new RelayCommand((selectedItem) =>
                  {
                      //// получаем выделенный объект
                      //Services client = selectedItem as Services;
                      //if (client == null) return;

                      //ServiceWindow userWindow = new ServiceWindow(client);


                      //if (userWindow.ShowDialog() == true)
                      //{
                      //    client.surname = userWindow.Services.surname;
                      //    client.name = userWindow.Services.name;
                      //    client.patronymic = userWindow.Services.patronymic;
                      //    client.telephone = userWindow.Services.telephone;
                      //    client.email = userWindow.Services.email;
                      //    client.type = userWindow.Services.type;
                      //    client.companyname = userWindow.Services.companyname;
                      //    context.Services.AddOrUpdate(client);
                      //    //context.Entry(client).State = EntityState.Modified;
                      //    context.SaveChanges();
                      //}
                  }));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
