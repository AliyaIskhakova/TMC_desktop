using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TMC.Model;

namespace TMC.ViewModel
{
    public class EmployeesViewModel: INotifyPropertyChanged
    {
        ObservableCollection<Employees> _employees;
         string _searchText;
         ObservableCollection<Employees> _filteredEmployees;

        public EmployeesViewModel()
        {
            // Инициализация данных
            ServiceCenterTMCEntities context = new ServiceCenterTMCEntities();
            _employees = new ObservableCollection<Employees>(context.Employees.ToList()); ;
            _filteredEmployees = new ObservableCollection<Employees>(_employees);
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

        public ObservableCollection<Employees> EmployeesList
        {
            get { return _filteredEmployees; }
            set
            {
                _filteredEmployees = value;
                OnPropertyChanged();
            }
        }

        private void FilterPersons()
        {
            if (string.IsNullOrEmpty(_searchText))
            {
                EmployeesList = new ObservableCollection<Employees>(_employees);
            }
            else
            {
                var filtered = _employees.Where(e =>
        e.Surname.ToLowerInvariant().StartsWith(_searchText.ToLowerInvariant().Trim()) || e.Name.ToLowerInvariant().StartsWith(_searchText.ToLowerInvariant().Trim()) || e.Patronymic.ToLowerInvariant().StartsWith(_searchText.ToLowerInvariant().Trim()));
                EmployeesList = new ObservableCollection<Employees>(filtered);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
