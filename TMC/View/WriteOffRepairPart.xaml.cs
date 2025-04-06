using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TMC.View
{
    /// <summary>
    /// Логика взаимодействия для WriteOffRepairPart.xaml
    /// </summary>
    public partial class WriteOffRepairPart : Window
    {
        int _countPart;
        public WriteOffRepairPart(int countPart)
        {
            InitializeComponent();
            _countPart = countPart;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        void Accept_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (int.TryParse(PartCount.Text, out int count) && count >= 1) {
                    if (_countPart >= count) DialogResult = true;
                    else MessageBox.Show("Вы пытаетесь списать больше ЗИП, чем есть на складе!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else MessageBox.Show("Проверьте корректность данных!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
