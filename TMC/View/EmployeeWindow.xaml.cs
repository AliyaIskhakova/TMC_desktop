using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TMC.Model;
using TMC.ViewModel;

namespace TMC.View
{
    /// <summary>
    /// Логика взаимодействия для EmployeeWindow.xaml
    /// </summary>
    public partial class EmployeeWindow : Window
    {
        public Employees Employees { get; private set; }
        public string login;
       
        public EmployeeWindow(Employees employee)
        {
            try
            {
                InitializeComponent();
                Employees = employee;
                DataContext = employee;
                login = employee.Login;
                newPassword.DataContext = new EmployeesViewModel();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        void Accept_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(SurnameTxt.Text) && !string.IsNullOrWhiteSpace(NameTxt.Text) && !string.IsNullOrWhiteSpace(TelephoneTxt.Text)
                        && RoleBox.SelectedItem != null && !string.IsNullOrWhiteSpace(TelephoneTxt.Text) && !string.IsNullOrWhiteSpace(LoginTxt.Text)
                        && Regex.IsMatch(TelephoneTxt.Text, @"\+7\(\d{3}\)\d{3}-\d{2}-\d{2}"))
                {
                    ServiceCenterTMCEntities context = new ServiceCenterTMCEntities();
                    var employee = context.Employees.FirstOrDefault(e => e.Login == LoginTxt.Text);
                    if (employee == null || employee.Login == login) DialogResult = true;
                    else MessageBox.Show("Пользователь с таким логином уже существует", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else MessageBox.Show("Заполните обязательные поля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
