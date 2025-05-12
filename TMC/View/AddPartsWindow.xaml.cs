using System.Windows;
using TMC.ViewModel;

namespace TMC.View
{
    /// <summary>
    /// Логика взаимодействия для AddPartsWindow.xaml
    /// </summary>
    public partial class AddPartsWindow : Window
    {
        public AddPartsWindow(StoreViewModel storeVM)
        {
            InitializeComponent();
            DataContext = storeVM;
        }
    }
}
