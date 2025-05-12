using System;
using System.Windows;
using TMC.Model;
using TMC.ViewModel;

namespace TMC.View
{
    /// <summary>
    /// Логика взаимодействия для ClientWindow.xaml
    /// </summary>
    public partial class ClientWindow : Window
    {
        public Clients Clients { get; private set; }
        public ClientWindow( Clients client)
        {
            try
            {
                InitializeComponent();
                Clients = client;
                DataContext = Clients;
                if (Clients.Type == true) ur.IsChecked = true;
                else fiz.IsChecked = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Close();
                var vm = new ClientsViewModel();
                vm.updateCommand.Execute(vm);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            CompanyTxt.Visibility = Visibility.Visible;
        }

        private void RadioButton_Unchecked(object sender, RoutedEventArgs e)
        {
            CompanyTxt.Visibility = Visibility.Collapsed;
        }

        void Accept_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Clients.HasValidationErrors()) MessageBox.Show("Проверьте корректность данных!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

                else DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
