using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TMC.Model;

namespace TMC.ViewModel
{
    public class ClientsViewModel: INotifyPropertyChanged
    {
        ObservableCollection<Clients> _clients;
        string _searchText;
        ObservableCollection<Clients> _filteredClients;

        public ClientsViewModel()
        {
            // Инициализация данных
            ServiceCenterTMCEntities context = new ServiceCenterTMCEntities();
            _clients = new ObservableCollection<Clients>(context.Clients.ToList()); ;
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
