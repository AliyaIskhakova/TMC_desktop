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
    /// Логика взаимодействия для ClientWindow.xaml
    /// </summary>
    public partial class ClientWindow : Window
    {
        public Clients Clients { get; private set; }
        public ClientWindow( Clients client)
        {
            InitializeComponent();
            Clients = client;
            DataContext = Clients;
            if(Clients.type==true) ur.IsChecked = true;
            else fiz.IsChecked = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            var vm = new ClientsViewModel();
            vm.updateCommand.Execute(vm);
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
            if (Clients.HasValidationErrors()) MessageBox.Show("Проверьте ввведенные данные"); 
            else DialogResult = true;
        }
    }
}
