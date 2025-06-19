using System.Windows;
using TMC.ViewModel;

namespace TMC.View
{
    public partial class AddPartsWindow : Window
    {
        public AddPartsWindow(StoreViewModel storeVM)
        {
            InitializeComponent();
            DataContext = storeVM;
        }
    }
}
