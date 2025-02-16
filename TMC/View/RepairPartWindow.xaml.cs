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
using TMC.Model;

namespace TMC.View
{
    /// <summary>
    /// Логика взаимодействия для RepairPartWindow.xaml
    /// </summary>
    public partial class RepairPartWindow : Window
    {
        public RepairParts RepairParts { get; private set; }

        public RepairPartWindow(RepairParts repairParts)
        {
            InitializeComponent();
            RepairParts = repairParts;
            DataContext = repairParts;
        }
        void Accept_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
