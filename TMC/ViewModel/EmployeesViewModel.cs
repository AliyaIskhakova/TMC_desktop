using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TMC.Model;
using TMC.View;
using Xceed.Wpf.Toolkit.Primitives;

namespace TMC.ViewModel
{
    public class EmployeesViewModel: INotifyPropertyChanged
    {
        ObservableCollection<Employees> _employees;
         string _searchText;
        ServiceCenterTMCEntities context = new ServiceCenterTMCEntities();
        RelayCommand addCommand;
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
                var filtered = _employees.Where(e => e.Surname.ToLowerInvariant().Contains(_searchText.ToLowerInvariant().Trim()) || e.Name.ToLowerInvariant().Contains(_searchText.ToLowerInvariant().Trim()) || e.Patronymic.ToLowerInvariant().Contains(_searchText.ToLowerInvariant().Trim()));
                EmployeesList = new ObservableCollection<Employees>(filtered);
            }
        }

        public RelayCommand AddEmployeeCommand
        {
            get
            {
                return new RelayCommand((o) =>
                  {
                      EmployeeWindow employeeWindow = new EmployeeWindow(new Employees());
                      employeeWindow.RoleBox.ItemsSource = context.Roles.ToList();
                      if (employeeWindow.ShowDialog() == true)
                      {
                          Employees employee = employeeWindow.Employees;
                          employee.Roles = employeeWindow.RoleBox.SelectedItem as Roles;
                          context.Employees.AddOrUpdate(employee);
                          context.SaveChanges();
                          _employees = new ObservableCollection<Employees>(context.Employees.ToList()); ;
                          FilterPersons();

                      }
                  });
            }
        }
        public RelayCommand EditEmployeeCommand
        {
            get
            {
                return new RelayCommand((selectedItem) =>
                {
                    Employees employee = selectedItem as Employees;
                    if (employee == null) return;
                    EmployeeWindow employeeWindow = new EmployeeWindow(employee);
                    employeeWindow.RoleBox.ItemsSource = context.Roles.ToList();
                    employeeWindow.RoleBox.SelectedItem = context.Roles.Find(employee.RoleID);
                    if (employeeWindow.ShowDialog() == true)
                    {
                        employee = employeeWindow.Employees;
                        employee.Roles = employeeWindow.RoleBox.SelectedItem as Roles;
                        context.Employees.AddOrUpdate(employee);
                        context.SaveChanges();
                        _employees = new ObservableCollection<Employees>(context.Employees.ToList()); ;
                        FilterPersons();
                    }
                });
            }
        }
        public RelayCommand DeleteEmployeeCommand
        {
            get
            {
                return new RelayCommand((selectedItem) =>
                {
                    Employees employee = selectedItem as Employees;
                    if (employee == null) return;
                    context.Employees.Remove(employee);
                    context.SaveChanges();
                    _employees = new ObservableCollection<Employees>(context.Employees.ToList()); ;
                    FilterPersons();
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
