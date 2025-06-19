using System;
using System.Windows;
using TMC.Model;

namespace TMC.View
{
    public partial class ServiceWindow : Window
    {
        public Services Services { get; private set; }


        public ServiceWindow(Services services)
        {
            try
            {
                InitializeComponent();
                Services = services;
                DataContext = Services;
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
                if (!string.IsNullOrWhiteSpace(ServiceName.Text) && double.TryParse(ServiceCost.Text, out double cost) && cost >= 0 && TypeBox.SelectedItem!=null) DialogResult = true;
                else MessageBox.Show("Проверьте корректность данных!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
