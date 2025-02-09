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

namespace TMC.View
{
    /// <summary>
    /// Логика взаимодействия для RequestWindow.xaml
    /// </summary>
    public partial class RequestWindow : Window
    {
        public Requests Requests { get; private set; }
        public RequestView RequestView { get; private set; }
        public RequestWindow(RequestView requestView)
        {
            InitializeComponent();
            RequestView = requestView;
            DataContext = requestView;
            MastersBox.DataContext = new RequestViewModel();
        }
        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void RadioButton_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void RadioButton_Unchecked_1(object sender, RoutedEventArgs e)
        {

        }

        private void RadioButton_Checked_1(object sender, RoutedEventArgs e)
        {

        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        void Accept_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
