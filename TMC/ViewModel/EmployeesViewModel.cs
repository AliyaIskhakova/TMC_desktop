using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Runtime.CompilerServices;
using System.Windows;
using TMC.Model;
using TMC.View;
using System.Windows.Controls;

namespace TMC.ViewModel
{
    public class EmployeesViewModel: INotifyPropertyChanged
    {
        ObservableCollection<Employees> _employees;
         string _searchText;
        ServiceCenterTMCEntities context = new ServiceCenterTMCEntities();
        
        ObservableCollection<Employees> _filteredEmployees;

        public EmployeesViewModel()
        {
            // Инициализация данных
            _employees = new ObservableCollection<Employees>(context.Employees.ToList()); 
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
                      employeeWindow.newPassword.Visibility = Visibility.Collapsed;
                      employeeWindow.RoleBox.ItemsSource = context.Roles.ToList();
                      if (employeeWindow.ShowDialog() == true)
                      {
                          Employees employee = employeeWindow.Employees;
                          employee.Roles = employeeWindow.RoleBox.SelectedItem as Roles;
                          string password = GeneratePassword();
                          employee.Password = password;
                          SendCredentials(employee, employee.Login, password);
                          context.Employees.AddOrUpdate(employee);
                          context.SaveChanges();
                          _employees = new ObservableCollection<Employees>(context.Employees.ToList());
                          _filteredEmployees = new ObservableCollection<Employees>(_employees);
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
                        var dataGrid = selectedItem as DataGrid;
                        Employees employee = dataGrid.SelectedItem as Employees;
                        if (employee == null) return;
                        Employees vm = new Employees
                        {
                            IdEmployee = employee.IdEmployee,
                            Surname = employee.Surname,
                            Name = employee.Name,
                            Patronymic = employee.Patronymic,
                            RoleId = employee.RoleId,
                            Telephone = employee.Telephone,
                            Login = employee.Login,
                            Password = employee.Password,
                            Email = employee.Email,
                            Roles = employee.Roles
                        };
                        EmployeeWindow employeeWindow = new EmployeeWindow(vm);
                        employeeWindow.RoleBox.ItemsSource = context.Roles.ToList();
                        employeeWindow.RoleBox.SelectedItem = context.Roles.Find(employee.RoleId);
                        if (employeeWindow.ShowDialog() == true)
                        {
                            employee = employeeWindow.Employees;
                            employee.RoleId = (employeeWindow.RoleBox.SelectedItem as Roles).IdRole;
                            context.Employees.AddOrUpdate(employee);
                            context.SaveChanges();
                            _employees = new ObservableCollection<Employees>(context.Employees);
                            _filteredEmployees = _employees;
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
                        if (employee == null) {
                            MessageBox.Show("Выберите сотрудника для удаления", "Удаление сотрудника", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                            }
                        var result = MessageBox.Show($"Вы действительно хотите удалить сотрудника: {employee.Surname} {employee.Name} {employee.Patronymic}?", "Удаление сотрудника", MessageBoxButton.YesNo, MessageBoxImage.Question ) ;
                        if (result == MessageBoxResult.No) return;
                        context.Employees.Remove(employee);
                        context.SaveChanges();
                        _employees = new ObservableCollection<Employees>(context.Employees.ToList());
                        _filteredEmployees = new ObservableCollection<Employees>(_employees);
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
                        string password = GeneratePassword();
                        SendCredentials(employee, employee.Login, password);
                        employee.Password = password;
                        context.Employees.AddOrUpdate(employee);
                        context.SaveChanges();
                        _employees = new ObservableCollection<Employees>(context.Employees.ToList());
                        _filteredEmployees = new ObservableCollection<Employees>(_employees);
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
            const string chars = "abcdefghijklmnopqrstuvwxyz" +
                                 "ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
                                 "0123456789" +
                                 "!#$&_";
            int length = random.Next(6, 16);
            char[] password = new char[length];
            for (int i = 0; i < length; i++)
            {
                password[i] = chars[random.Next(chars.Length)];
            }

            return new string(password);
        }
        public void SendCredentials(Employees employee, string login, string password)
        {
            try
            {
                MailAddress from = new MailAddress("aliya_iskhakova12@mail.ru", "Сервисный центр ТехноМедиаСоюз");
                MailAddress to = new MailAddress(employee.Email);
                MailMessage m = new MailMessage(from, to);
                m.Subject = "Ваши учетные данные для доступа в систему";

                string htmlBody = $@"
        <div style='font-family: Arial; max-width: 600px; margin: 0 auto; border: 1px solid #DFE4FB; border-radius: 5px; overflow: hidden;'>
            <div style='background-color: #0A1C6F; padding: 15px; color: white;'>
                <h2 style='margin: 0;'>Сервисный центр ТехноМедиаСоюз</h2>
            </div>
            
            <div style='padding: 20px; background-color: #DFE4FB;'>
                <h3 style='color: #0E2280;'>Ваши данные для входа в систему</h3>
                
                <div style='background-color: white; padding: 15px; border-radius: 5px; margin: 15px 0; border-left: 4px solid #889DFB;'>
                    <p style='font-weight: bold; color: #162774; margin: 0 0 5px 0;'>Логин:</p>
                    <p style='font-size: 16px; color: #0E2280; margin: 0; padding: 8px; background-color: #f5f5f5; border-radius: 3px;'>{login}</p>
                    
                    <p style='font-weight: bold; color: #162774; margin: 15px 0 5px 0;'>Пароль:</p>
                    <p style='font-size: 16px; color: #0E2280; margin: 0; padding: 8px; background-color: #f5f5f5; border-radius: 3px;'>{password}</p>
                </div>
                
                <p style='color: #162774;'>Используйте эти данные для входа в систему.</p>
                
                <div style='margin-top: 20px; padding: 10px; background-color: #FFEEEE; border-radius: 5px; border: 1px solid #FFCCCC;'>
                    <p style='color: #990000; margin: 0; font-size: 13px;'>
                        <b>Важно:</b> Не передавайте эти данные третьим лицам.
                    </p>
                </div>
            </div>
            
            <div style='background-color: #0A1C6F; padding: 10px; color: white; text-align: center; font-size: 12px;'>
                <p style='margin: 0;'>С уважением, администрация ТехноМедиаСоюз</p>
            </div>
        </div>";

                m.Body = htmlBody;
                m.IsBodyHtml = true;

                SmtpClient smtp = new SmtpClient("smtp.mail.ru", 587);
                smtp.Credentials = new NetworkCredential("aliya_iskhakova12@mail.ru", "HKqzZM2FQTJC3v09cmZd");
                smtp.EnableSsl = true;
                smtp.Send(m);

                MessageBox.Show($"Учетные данные отправлены на почту {employee.Email}",
                    "Отправка учетных данных", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при отправке учетных данных: {ex.Message}",
                               "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
