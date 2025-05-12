using System;
using System.Windows;
using TMC.Model;
using TMC.ViewModel;

namespace TMC.View
{
    /// <summary>
    /// Логика взаимодействия для RepairPartWindow.xaml
    /// </summary>
    public partial class RepairPartWindow : Window
    {
        public RepairParts RepairParts { get; private set; }

        public RepairPartWindow(RepairParts repairParts, StoreViewModel vm)
        {
            InitializeComponent();
            RepairParts = repairParts;
            DataContext = repairParts;
            WriteOffBtn.DataContext = vm;
        }
        void Accept_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(PartName.Text) && int.TryParse(PartCount.Text, out int count) && double.TryParse(PartCost.Text, out double cost) && cost >= 0 && count >= 1) DialogResult = true;
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

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
