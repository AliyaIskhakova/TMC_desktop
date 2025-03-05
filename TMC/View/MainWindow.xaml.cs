using Microsoft.Office.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;
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
using Xceed.Wpf.Toolkit.Primitives;

namespace TMC
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        RequestViewModel requestVM = new RequestViewModel();
        public MainWindow()
        {
            InitializeComponent();
            RequestsWindow.DataContext = requestVM;
            ClientsWindow.DataContext = new ClientsViewModel();
            StoreWindow.DataContext = new StoreViewModel();
            ServicesWindow.DataContext = new ServicesViewModel();
            EmployeesWindow.DataContext = new EmployeesViewModel();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            int index = int.Parse(((Button)e.Source).Uid);
            //Button clickedButton = (Button)e.Source;
            //foreach (Button button in MenuPanel.Children)
            //{
            //    button.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#CFD9FF");
            //}
            //clickedButton.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#B4C3FF");
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
                    ClientsWindow.Visibility= Visibility.Visible;   
                    break;
                case 4:
                    ServicesWindow.Visibility= Visibility.Visible; 
                    break;
                case 5:
                    EmployeesWindow.Visibility= Visibility.Visible; 
                    break;
                case 6:
                    ResultsWindow.Visibility = Visibility.Visible;
                    break;
            }
        }


        private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selectedItem = (sender as DataGrid).SelectedItem;
            var viewModel = new ClientsViewModel();
            viewModel.EditClientCommand.Execute(selectedItem);

        }
        private void ClientsDG_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

            var row = ItemsControl.ContainerFromElement((DataGrid)sender, e.OriginalSource as DependencyObject) as DataGridRow;

            if (row != null)
            {
                var selectedItem = (sender as DataGrid).SelectedItem;
                var viewModel = new ClientsViewModel();
                viewModel.EditClientCommand.Execute(selectedItem);
            }
            
        }

        private void ServicesDG_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string role = App.Current.Properties["Role"] as string;
            if (role == "Администратор")
            {
                var row = ItemsControl.ContainerFromElement((DataGrid)sender, e.OriginalSource as DependencyObject) as DataGridRow;

                if (row != null)
                {
                    var selectedItem = (sender as DataGrid).SelectedItem;
                    var viewModel = new ServicesViewModel();
                    viewModel.EditServicesCommand.Execute(selectedItem);
                }
            }
        }

        private void EmployeesDG_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string role = App.Current.Properties["Role"] as string;
            if (role == "Администратор")
            {
                var row = ItemsControl.ContainerFromElement((DataGrid)sender, e.OriginalSource as DependencyObject) as DataGridRow;

                if (row != null)
                {
                    var selectedItem = (sender as DataGrid).SelectedItem;
                    var viewModel = new EmployeesViewModel();
                    viewModel.EditEmployeeCommand.Execute(selectedItem);
                }
            }
        }

        private void StoreDG_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var row = ItemsControl.ContainerFromElement((DataGrid)sender, e.OriginalSource as DependencyObject) as DataGridRow;

            if (row != null)
            {
                var selectedItem = (sender as DataGrid).SelectedItem;
                var viewModel = new StoreViewModel();
                viewModel.EditRepairPartCommand.Execute(selectedItem);
            }
        }

        private void RequestDG_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
             var row = ItemsControl.ContainerFromElement((DataGrid)sender, e.OriginalSource as DependencyObject) as DataGridRow;

                if (row != null)
                {
                    var selectedItem = (sender as DataGrid).SelectedItem;
                    //var viewModel = new RequestViewModel();
                    requestVM.EditRequestCommand.Execute(selectedItem);
                } 
        }

        private void ButtonFilter_Click(object sender, RoutedEventArgs e)
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
            //var viewModel = new RequestViewModel();
            requestVM.SelectRequestByStatus.Execute(clickedButton.Content);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {

        }

       
    }
}
