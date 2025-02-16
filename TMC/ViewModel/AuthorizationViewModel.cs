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
        RelayCommand? authCommand;
        public ObservableCollection<Employees> Employees { get; set; }
        // команда авторизации
        public RelayCommand AuthorizationCommand
        {
            get
            {
                return authCommand ??
                  (authCommand = new RelayCommand(( w) =>
                  {
                      try
                      {
                          Authorization wi = w as Authorization;
                          if (!string.IsNullOrWhiteSpace(wi.loginBox.Text)&& !string.IsNullOrWhiteSpace(wi.passwordBox.Password)) {
                              Employees employee = context.Employees.Where(u => u.Login == wi.loginBox.Text && u.Password == wi.passwordBox.Password).FirstOrDefault();
                              if (employee != null)
                              {
                                  MainWindow mainWindow = new MainWindow();
                                  mainWindow.Show();
                                  wi.Close();
                              }
                              else MessageBox.Show("Неверный логин или пароль!");
                          }
                          else MessageBox.Show("Введите логин и пароль!");

                      }
                      catch (Exception ex) {
                           MessageBox.Show(ex.Message);
                      }

                  }));
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
