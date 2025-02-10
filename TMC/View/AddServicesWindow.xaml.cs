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
            if (ServicesDG.SelectedItem==null) MessageBox.Show("Выберите необходимые услуги для заявки");
            else DialogResult = true;
        }
    }
}
