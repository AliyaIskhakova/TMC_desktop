using Microsoft.Office.Interop.Word;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
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
using Window = System.Windows.Window;

namespace TMC.View
{
    /// <summary>
    /// Логика взаимодействия для RequestWindow.xaml
    /// </summary>
    public partial class RequestWindow : Window
    {
        public Requests Requests { get; private set; }
        public RequestView RequestView { get; private set; }
        public RequestWindow(Requests request, RequestViewModel vm)
        {
            InitializeComponent();
            ComplitionDate.DisplayDateStart = DateTime.Now;
            ComplitionDate.DisplayDateEnd = DateTime.Now.AddMonths(6);
            Requests = request;
            DataContext = Requests;
            RequestServices.DataContext = vm; 
            SaveBtn.DataContext = vm;
            PrintBtns.DataContext = vm;
            RequestRepairParts.DataContext = vm;
            ClientComboBox.DataContext = new ClientsViewModel();
            ClientInfo.DataContext = new Clients();
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        void Accept_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(RequestReason.Text) && !(ClientInfo.DataContext as Clients).HasValidationErrors() && int.TryParse(RequestCost.Text, out int cost) && cost>=0)
            {
                DialogResult = true;
            }
            else MessageBox.Show("Проверьте введенные данные!");
        }

        private void StatusBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = sender as ComboBox;
            string status = selected.SelectionBoxItemTemplate.ToString();
            if (status == "Готов")
            {
                EndDocuument.IsEnabled = true;
            }
            else EndDocuument.IsEnabled = false;
        }

        private void MastersBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ClientComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ClientComboBox.SelectedItem is Clients selectedClient)
            {
                ClientInfo.DataContext = selectedClient;
                ClientComboBox.Text = "" ;
                Info.IsEnabled = false;
                Info2.IsEnabled = false;
                ClientComboBox.IsEnabled = false;
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            ClientComboBox.SelectedItem = null;
            ClientInfo.DataContext = new Clients();
            Info.IsEnabled = true;
            Info2.IsEnabled = true;
            ClientComboBox.IsEnabled = true;
        }

        private void viezd_Checked(object sender, RoutedEventArgs e)
        {
            Address.Visibility = Visibility.Visible;
        }

        private void viezd_Checked_1(object sender, RoutedEventArgs e)
        {

        }

        private void viezd_Unchecked(object sender, RoutedEventArgs e)
        {
            Address.Visibility = Visibility.Collapsed;
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            CompanyTxt.Visibility = Visibility.Visible;
        }

        private void RadioButton_Unchecked(object sender, RoutedEventArgs e)
        {
            CompanyTxt.Visibility = Visibility.Collapsed;
        }
    }
}
