using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using TMC.Model;

namespace TMC.ViewModel
{
    public class AuthorizationViewModel
    {
        ServiceCenterTMCEntities context = new ServiceCenterTMCEntities();
        public ObservableCollection<Employees> Employees { get; set; }
        public RelayCommand AuthorizationCommand
        {
            get
            {
                return new RelayCommand(( w) =>
                  {
                      try
                      {
                          Authorization wi = w as Authorization;
                          if (!string.IsNullOrWhiteSpace(wi.loginBox.Text)&& !string.IsNullOrWhiteSpace(wi.passwordBox.Password)) {
                              string password = GetHashString(wi.passwordBox.Password);
                              Employees employee = context.Employees.Where(u => u.Login == wi.loginBox.Text && u.Password == password).FirstOrDefault();
                              if (employee != null)
                              {
                                  Application.Current.Properties["UserID"] = employee.IdEmployee;
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
        private string GetHashString(string s)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(s);
            MD5CryptoServiceProvider CSP = new MD5CryptoServiceProvider();
            byte[] byteHash = CSP.ComputeHash(bytes);
            string hash = "";
            foreach (byte b in byteHash)
            {
                hash += string.Format("{0:x2}", b);
            }
            return hash;
        }
    }
}
