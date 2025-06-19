using System;
using System.Windows;
using System.Windows.Controls;
using TMC.Model;
using TMC.ViewModel;
using Window = System.Windows.Window;

namespace TMC.View
{
    public partial class RequestWindow : Window
    {
        public Requests Requests { get; private set; }
        public RequestView RequestView { get; private set; }
        public RequestWindow(Requests request, RequestViewModel vm, ClientsViewModel clientVM)
        {
            try
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
                ClientComboBox.DataContext = clientVM;
                ClientInfo.DataContext = new Clients();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        void Accept_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(RequestReason.Text) && !(ClientInfo.DataContext as Clients).HasValidationErrors() 
                    && int.TryParse(RequestCost.Text, out int cost) && cost >= 0 && !string.IsNullOrWhiteSpace(SurnameTxt.Text) && 
                        !string.IsNullOrWhiteSpace(NameTxt.Text) && !string.IsNullOrWhiteSpace(TelephoneTxt.Text) && !string.IsNullOrWhiteSpace(EmailTxt.Text))
                {
                    if (ComplitionDate.SelectedDate != null)
                    {
                        if (ComplitionDate.SelectedDate < Requests.Date)
                        {
                            MessageBox.Show("Некорректная дата готовности!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        
                    }
                    if ((RequestRepairParts.DataContext as RequestViewModel).CheckSelectedParts() == true)
                    {
                        DialogResult = true;
                    }

                }
                else MessageBox.Show("Заполните обязательные поля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void 
            Box_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var selected = sender as ComboBox;
                string status = selected.SelectionBoxItemTemplate.ToString();
                if (status == "Готова")
                {
                    EndDocuument.IsEnabled = true;
                }
                else EndDocuument.IsEnabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void ClientComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (ClientComboBox.SelectedItem is Clients selectedClient)
                {
                    ClientInfo.DataContext = selectedClient;
                    ClientComboBox.Text = "";
                    Info.IsEnabled = false;
                    Info2.IsEnabled = false;
                    ClientComboBox.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ClientComboBox.SelectedItem = null;
                ClientInfo.DataContext = new Clients();
                Info.IsEnabled = true;
                Info2.IsEnabled = true;
                ClientComboBox.IsEnabled = true;
            }
            catch (Exception)
            {

                throw;
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

        private void TypeCheck_Checked(object sender, RoutedEventArgs e)
        {
            Address.Visibility = Visibility.Visible;
        }

        private void TypeCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            Address.Visibility = Visibility.Collapsed;
        }
    }
}
