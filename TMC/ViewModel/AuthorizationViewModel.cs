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
    public class AuthorizationViewModel: INotifyPropertyChanged
    {
        ServiceCenterTMCEntities context = new ServiceCenterTMCEntities();
        RelayCommand authCommand;
        public ObservableCollection<Employees> Employees { get; set; }
        // команда авторизации
        public RelayCommand AuthorizationCommand
        {
            get
            {
                return authCommand ??= new RelayCommand(( w) =>
                  {
                      try
                      {
                          Authorization wi = w as Authorization;
                          if (!string.IsNullOrWhiteSpace(wi.loginBox.Text)&& !string.IsNullOrWhiteSpace(wi.passwordBox.Password)) {
                              Employees employee = context.Employees.Where(u => u.Login == wi.loginBox.Text && u.Password == wi.passwordBox.Password).FirstOrDefault();
                              if (employee != null)
                              {
                                  Application.Current.Properties["UserID"] = employee.IDEmployee;
                                  Application.Current.Properties["Role"] = employee.Roles.Name;
                                  MainWindow mainWindow = new MainWindow();
                                  switch (employee.Roles.Name)
                                  {
                                      case ("Администратор"):
                                          mainWindow.RequestBtn.Visibility = Visibility.Collapsed;
                                          mainWindow.StoreBtn.Visibility = Visibility.Collapsed;
                                          mainWindow.ClientsBtn.Visibility = Visibility.Collapsed;
                                          mainWindow.PrintPriceList.Visibility = Visibility.Collapsed;
                                          mainWindow.AddService.Visibility = Visibility.Visible;
                                          mainWindow.AddEmployee.Visibility = Visibility.Visible;
                                          mainWindow.RequestsWindow.Visibility = Visibility.Collapsed;
                                          mainWindow.ServicesWindow.Visibility = Visibility.Visible;
                                          mainWindow.DeleteEmployee.Visibility = Visibility.Visible;
                                          mainWindow.DeleteService.Visibility = Visibility.Visible;
                                          break;
                                      case ("Мастер"):
                                          mainWindow.StoreBtn.Visibility = Visibility.Collapsed;
                                          mainWindow.ClientsBtn.Visibility = Visibility.Collapsed;
                                          mainWindow.ServicesBtn.Visibility = Visibility.Collapsed;
                                          mainWindow.EmployeesBtn.Visibility = Visibility.Collapsed;
                                          mainWindow.AddRequest.Visibility = Visibility.Collapsed;
                                          break;
                                      case ("Директор"):
                                          mainWindow.ResultsBtn.Visibility = Visibility.Visible;
                                          mainWindow.AddClient.Visibility = Visibility.Collapsed;
                                          mainWindow.AddRequest.Visibility = Visibility.Collapsed;
                                          mainWindow.AddReraipPart.Visibility = Visibility.Collapsed;
                                          break;
                                  }
                                  mainWindow.Show();
                                  wi.Close();
                              }
                              else MessageBox.Show("Неверный логин или пароль!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                          }
                          else MessageBox.Show("Введите логин и пароль!");

                  }
                      catch (Exception ex)
                      {
                          MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                      }

                  });
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
