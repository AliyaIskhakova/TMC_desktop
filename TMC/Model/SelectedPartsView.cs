using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TMC.Model
{
    public class SelectedPartsView : INotifyPropertyChanged
    {
        private int _idRequest;
        private int _idPart;
        private string _name;
        private int _count;
        private double _cost;

        public int IDRequest
        {
            get { return _idRequest; }
            set
            {
                _idRequest = value;
                OnPropertyChanged();
            }
        }
        public int IDPart
        {
            get { return _idPart; }
            set
            {
                _idPart = value;
                OnPropertyChanged();
            }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public int Count
        {
            get { return _count; }
            set
            {
                _count = value;
                OnPropertyChanged();
            }
        }

        public double Cost
        {
            get { return _cost; }
            set
            {
                _cost = value;
                OnPropertyChanged();
            }
        }
    
    public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
