using System;
using System.Windows;
using TMC.ViewModel;
namespace TMC.View
{
    /// <summary>
    /// Логика взаимодействия для AddServicesWindow.xaml
    /// </summary>
    public partial class AddServicesWindow : Window
    {
        public AddServicesWindow()
        {
            InitializeComponent();
            DataContext = new ServicesViewModel();
            //DataContext = new RequestViewModel();
        }

        void Accept_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ServicesDG.SelectedItem == null) MessageBox.Show("Выберите необходимые услуги для заявки");
                else DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
