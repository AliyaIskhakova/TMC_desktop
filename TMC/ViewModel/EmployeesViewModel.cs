using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Net.Mail;
using System.Net;
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
                    try
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
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    try
                    {
                        Employees employee = selectedItem as Employees;
                        if (employee == null) return;
                        context.Employees.Remove(employee);
                        context.SaveChanges();
                        _employees = new ObservableCollection<Employees>(context.Employees.ToList()); ;
                        FilterPersons();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                });
            }
        }

        public RelayCommand NewPasswordCommand
        {
            get
            {
                return new RelayCommand((w) =>
                {
                    try
                    {
                        EmployeeWindow employeeWindow = w as EmployeeWindow;
                        if (employeeWindow == null) return;

                        Employees employee = employeeWindow.Employees;
                        employee.password = GeneratePassword();
                        MailAddress from = new MailAddress("aliya_iskhakova12@mail.ru", "Сервисный центр ТехноМедиаСоюз");
                        MailAddress to = new MailAddress(employeeWindow.EmailTxt.Text);
                        MailMessage m = new MailMessage(from, to);
                        m.Subject = "Тест";
                        m.Body = "<h1>Пароль: " + employee.Password + "</h1>";
                        //user.Password = GetHashString(newPasword);
                                                
                        m.IsBodyHtml = true;
                        SmtpClient smtp = new SmtpClient("smtp.mail.ru", 587);
                        smtp.Credentials = new NetworkCredential("aliya_iskhakova12@mail.ru", "HKqzZM2FQTJC3v09cmZd");
                        smtp.EnableSsl = true;
                        smtp.Send(m);
                        context.Employees.AddOrUpdate(employee);
                        context.SaveChanges();
                        _employees = new ObservableCollection<Employees>(context.Employees.ToList()); ;
                        FilterPersons();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                });
            }
        }
        public static string GeneratePassword()
        {
            Random random = new Random();
            // Символы, которые могут использоваться в пароле
            const string chars = "abcdefghijklmnopqrstuvwxyz" +
                                 "ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
                                 "0123456789" +
                                 "!@#$%^&*_";

            // Генерируем случайную длину от 6 до 12 символов
            int length = random.Next(6, 20);

            // Создаём массив символов для пароля
            char[] password = new char[length];

            // Заполняем массив случайными символами
            for (int i = 0; i < length; i++)
            {
                password[i] = chars[random.Next(chars.Length)];
            }

            return new string(password);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
