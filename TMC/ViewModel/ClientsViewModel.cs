using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TMC.Model;
using TMC.View;

namespace TMC.ViewModel
{
    public class ClientsViewModel: INotifyPropertyChanged
    {
        ServiceCenterTMCEntities context = new ServiceCenterTMCEntities();
        ObservableCollection<Clients> _clients;
        string _searchText;
        RelayCommand? addCommand;
        RelayCommand? editCommand;
        ObservableCollection<Clients> _filteredClients;

        public ClientsViewModel()
        {
            // Инициализация данных
            _clients = new ObservableCollection<Clients>(context.Clients.ToList());
            _filteredClients = new ObservableCollection<Clients>(_clients);
        }



        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                OnPropertyChanged();
                FilterPersons();
            }
        }

        public ObservableCollection<Clients> ClientsList
        {
            get { return _filteredClients; }
            set
            {
                _filteredClients = value;
                OnPropertyChanged();
            }
        }

        private void FilterPersons()
        {
            if (string.IsNullOrEmpty(_searchText))
            {
                ClientsList = new ObservableCollection<Clients>(_clients);
            }
            else
            {
                var filtered = _clients.Where(e =>
        e.Surname.ToLowerInvariant().StartsWith(_searchText.ToLowerInvariant().Trim()) || e.Name.ToLowerInvariant().StartsWith(_searchText.ToLowerInvariant().Trim()) || e.Patronymic.ToLowerInvariant().StartsWith(_searchText.ToLowerInvariant().Trim()));
                ClientsList = new ObservableCollection<Clients>(filtered);
            }
        }



        public RelayCommand AddClientCommand
        {
            get
            {
                return addCommand ??
                  (addCommand = new RelayCommand((o) =>
                  {
                      ClientWindow clientWindow = new ClientWindow(new Clients());
                      if (clientWindow.ShowDialog() == true)
                      {
                           Clients client = clientWindow.Clients;
                          MessageBox.Show(client.telephone + client.type);
                          //client.Telephone = "avae";
                           context.Clients.Add(client);
                           context.SaveChanges();
                           _clients = new ObservableCollection<Clients>(context.Clients.ToList());
                           ClientsList = new ObservableCollection<Clients>(_clients);
                          
                      }
                  }));
            }
        }

        public RelayCommand EditClientCommand
        {
            get
            {
                return editCommand ??
                  (editCommand = new RelayCommand((selectedItem) =>
                  {
                      // получаем выделенный объект
                      Clients client = selectedItem as Clients;
                      if (client == null) return;

                      //Clients vm = new Clients
                      //{
                      //    Id = client.Id,
                      //    Name = client.Name,
                      //    Age = client.Age
                      //};
                      ClientWindow userWindow = new ClientWindow(client);


                      if (userWindow.ShowDialog() == true)
                      {
                          client.surname = userWindow.Clients.surname;
                          client.name = userWindow.Clients.name;
                          client.patronymic = userWindow.Clients.patronymic;
                          client.telephone = userWindow.Clients.telephone;
                          client.email = userWindow.Clients.email;
                          client.type = userWindow.Clients.type;
                          client.companyname = userWindow.Clients.companyname;
                          context.Clients.AddOrUpdate(client);
                          //context.Entry(client).State = EntityState.Modified;
                          context.SaveChanges();
                      }
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
