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
using System.Windows.Navigation;
using System.Windows.Shapes;
using TMC.Model;
using TMC.ViewModel;

namespace TMC
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class Authorization : Window
    {
        public Authorization()
        {
            InitializeComponent();
            //DataContext = new AuthorizationViewModel();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //ServiceCenterTMCEntities context = new ServiceCenterTMCEntities();
            //Employees employee = context.Employees.Where(u=>u.Login==loginBox.Text && u.Password==passwordBox.Password).FirstOrDefault();
            //if (employee != null)
            //{
            //    MainWindow mainWindow = new MainWindow();
            //    mainWindow.Show();
            //    this.Close();   
            //}
        }
    }
}
