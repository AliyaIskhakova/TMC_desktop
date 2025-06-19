using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TMC.ViewModel;

namespace TMC
{
   
    public partial class MainWindow : Window
    {
        
        
        StoreViewModel storeVM = new StoreViewModel();
        ClientsViewModel clientsVM = new ClientsViewModel();
        ServicesViewModel servicesVM = new ServicesViewModel();
        EmployeesViewModel employeesVM = new EmployeesViewModel();
        ReportsViewModel reportsVM = new ReportsViewModel();
        RequestViewModel requestVM;
        public MainWindow()
        {
            try
            {
                InitializeComponent();
                requestVM = new RequestViewModel(clientsVM, storeVM);
                RequestsWindow.DataContext = requestVM;
                ClientsWindow.DataContext = clientsVM;
                StoreWindow.DataContext = storeVM;
                ServicesWindow.DataContext = servicesVM;
                EmployeesWindow.DataContext = employeesVM;
                ResultsWindow.DataContext = reportsVM;
                EndDate.DisplayDateEnd = DateTime.Now;
                EndDate.DisplayDateStart = DateTime.Parse("02.01.2014");
                StartDate.DisplayDateStart = DateTime.Parse("01.01.2014");
                StartDate.DisplayDateEnd = DateTime.Now.AddDays(-1);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int index = int.Parse(((Button)e.Source).Uid);
                RequestsWindow.Visibility = Visibility.Collapsed;
                StoreWindow.Visibility = Visibility.Collapsed;
                ClientsWindow.Visibility = Visibility.Collapsed;
                ServicesWindow.Visibility = Visibility.Collapsed;
                EmployeesWindow.Visibility = Visibility.Collapsed;
                ResultsWindow.Visibility = Visibility.Collapsed;
                switch (index)
                {
                    case 0:
                        Authorization authorization = new Authorization();
                        authorization.Show();
                        this.Close();
                        break;
                    case 1:
                        RequestsWindow.Visibility = Visibility.Visible;
                        break;
                    case 2:
                        StoreWindow.Visibility = Visibility.Visible;
                        break;
                    case 3:
                        ClientsWindow.Visibility = Visibility.Visible;
                        break;
                    case 4:
                        ServicesWindow.Visibility = Visibility.Visible;
                        break;
                    case 5:
                        EmployeesWindow.Visibility = Visibility.Visible;
                        break;
                    case 6:
                        ResultsWindow.Visibility = Visibility.Visible;
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void ClientsDG_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                string role = App.Current.Properties["Role"] as string;
                if (role == "Инженер-приёмщик")
                {
                    var row = ItemsControl.ContainerFromElement((DataGrid)sender, e.OriginalSource as DependencyObject) as DataGridRow;

                    if (row != null)
                    {
                        var selectedItem = sender as DataGrid;
                        clientsVM.EditClientCommand.Execute(selectedItem);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void ServicesDG_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                string role = App.Current.Properties["Role"] as string;
                if (role == "Администратор")
                {
                    var row = ItemsControl.ContainerFromElement((DataGrid)sender, e.OriginalSource as DependencyObject) as DataGridRow;

                    if (row != null)
                    {
                        var selectedItem = sender as DataGrid;
                        servicesVM.EditServicesCommand.Execute(selectedItem);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EmployeesDG_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                string role = App.Current.Properties["Role"] as string;
                if (role == "Администратор")
                {
                    var row = ItemsControl.ContainerFromElement((DataGrid)sender, e.OriginalSource as DependencyObject) as DataGridRow;

                    if (row != null)
                    {
                        var selectedItem = sender as DataGrid;
                        employeesVM.EditEmployeeCommand.Execute(selectedItem);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StoreDG_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                string role = App.Current.Properties["Role"] as string;
                if (role == "Инженер-приёмщик")
                {
                    var row = ItemsControl.ContainerFromElement((DataGrid)sender, e.OriginalSource as DependencyObject) as DataGridRow;

                    if (row != null)
                    {
                        var selectedItem = sender as DataGrid;
                        storeVM.EditRepairPartCommand.Execute(selectedItem);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RequestDG_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var row = ItemsControl.ContainerFromElement((DataGrid)sender, e.OriginalSource as DependencyObject) as DataGridRow;

                if (row != null)
                {
                    var selectedItem = (sender as DataGrid).SelectedItem;
                    requestVM.EditRequestCommand.Execute(selectedItem);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ButtonFilter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button clickedButton = (Button)sender;
                foreach (Button button in FilterBtnGroup.Children)
                {
                    button.Foreground = Brushes.DarkGray;
                    button.BorderThickness = new Thickness(0);
                    button.FontWeight = FontWeights.Normal;
                }
                var converter = new BrushConverter();
                clickedButton.Foreground = (Brush)converter.ConvertFromString("#2747BB");
                clickedButton.BorderThickness = new Thickness(0, 0, 0, 3);
                clickedButton.BorderBrush = (Brush)converter.ConvertFromString("#2747BB");
                clickedButton.FontWeight = FontWeights.Bold;
                requestVM.SelectRequestByStatus.Execute(clickedButton.CommandParameter);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StoreBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button clickedButton = (Button)sender;
                foreach (Button button in BtnGroup.Children)
                {
                    button.Foreground = Brushes.DarkGray;
                    button.BorderThickness = new Thickness(0);
                    button.FontWeight = FontWeights.Normal;
                }
                var converter = new BrushConverter();
                clickedButton.Foreground = (Brush)converter.ConvertFromString("#2747BB");
                clickedButton.BorderThickness = new Thickness(0, 0, 0, 3);
                clickedButton.BorderBrush = (Brush)converter.ConvertFromString("#2747BB");
                clickedButton.FontWeight = FontWeights.Bold;
                string param = (string)clickedButton.CommandParameter;
                if (param == "0")
                {
                    StoreDG.Visibility = Visibility.Visible;
                    WriteOffDG.Visibility = Visibility.Collapsed;
                }
                else if (param == "1")
                {
                    StoreDG.Visibility = Visibility.Collapsed;
                    WriteOffDG.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
