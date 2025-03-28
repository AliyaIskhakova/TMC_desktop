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
        RelayCommand addCommand;
        RelayCommand editCommand;

        Clients _selectedClient;
        ObservableCollection<Clients> _filteredClients;

        public ClientsViewModel()
        {
            // Инициализация данных
            try
            {
                _clients = new ObservableCollection<Clients>(context.Clients.ToList());
                _filteredClients = new ObservableCollection<Clients>(_clients);
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

        public Clients SelectedClient
        {
            get => _selectedClient;
            set
            {
                if (_selectedClient != value)
                {
                    _selectedClient = value;
                    OnPropertyChanged(nameof(SelectedClient));
                }
            }
        }
        private void FilterPersons()
        {
            try
            {
                if (string.IsNullOrEmpty(_searchText))
                {
                    ClientsList = new ObservableCollection<Clients>(_clients);
                }
                else
                {
                    var filtered = _clients.Where(e =>
            e.Surname.ToLowerInvariant().StartsWith(_searchText.ToLowerInvariant().Trim()) || e.Name.ToLowerInvariant().StartsWith(_searchText.ToLowerInvariant().Trim()));
                    ClientsList = new ObservableCollection<Clients>(filtered);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show($"Произошла ошибка: {e.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public RelayCommand updateCommand
        {
            get
            {
                return new RelayCommand((o) =>
                {
                    try
                    {
                        var context = new ServiceCenterTMCEntities();
                        ClientsList = new ObservableCollection<Clients>(context.Clients.ToList());

                    }
                    catch (Exception e)
                    {
                        MessageBox.Show($"Произошла ошибка: {e.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                });
            }
        }

        public RelayCommand AddClientCommand
        {
            get
            {
                return addCommand ??= new RelayCommand((o) =>
                  {
                      try
                      {
                          ClientWindow clientWindow = new ClientWindow(new Clients());
                          if (clientWindow.ShowDialog() == true)
                          {
                              Clients client = clientWindow.Clients;
                              if (!client.Type) client.companyname = null; 
                              context.Clients.Add(client);
                              context.SaveChanges();
                              ClientsList = new ObservableCollection<Clients>(context.Clients.ToList());

                          }
                      }
                      catch (Exception e)
                      {
                          MessageBox.Show($"Произошла ошибка: {e.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                      }
                  });
            }
        }

        public RelayCommand EditClientCommand
        {
            get
            {
                return editCommand ??= new RelayCommand((selectedItem) =>
                  {
                      // получаем выделенный объект
                      try
                      {
                          Clients client = selectedItem as Clients;
                          if (client == null) return;

                          ClientWindow userWindow = new ClientWindow(client);

                          if (userWindow.ShowDialog() == true)
                          {
                              client = userWindow.Clients;
                              context.Clients.AddOrUpdate(client);
                              //context.Entry(client).State = EntityState.Modified;
                              context.SaveChanges();
                              ClientsList = new ObservableCollection<Clients>(context.Clients.ToList());
                          }

                      }
                      catch (Exception e)
                      {
                          MessageBox.Show($"Произошла ошибка: {e.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
