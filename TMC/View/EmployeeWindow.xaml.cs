using System;
using System.Linq;
using System.Windows;
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
                        && RoleBox.SelectedItem != null && !string.IsNullOrWhiteSpace(TelephoneTxt.Text) && !string.IsNullOrWhiteSpace(LoginTxt.Text))
                {
                    if (Employees.HasValidationErrors()) MessageBox.Show("Проверьте корректность данных!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    else
                    {
                        ServiceCenterTMCEntities context = new ServiceCenterTMCEntities();
                        var employee = context.Employees.FirstOrDefault(e => e.Login == LoginTxt.Text);
                        if (employee == null || employee.Login == login) DialogResult = true;
                        else MessageBox.Show("Пользователь с таким логином уже существует", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }  
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
