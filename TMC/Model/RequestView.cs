namespace TMC.Model
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class RequestView : INotifyPropertyChanged
    {
        private int _idRequest;
        private int? _employeeID;
        private string _employeeSurname;
        private string _employeeName;
        private string _employeePatronymic;
        private string _employeeTelephone;
        private int _statusID;
        private string _statusName;
        private string _statusColor;
        private int? _clientID;
        private string _clientSurname;
        private string _clientName;
        private string _clientPatronymic;
        private string _clientTelephone;
        private string _completionDate;
        private string _reason;
        private string _date;
        private string _device;
        private string _imeiSN;
        private string _detectedMulfunction;

        public int IDRequest
        {
            get { return _idRequest; }
            set
            {
                _idRequest = value;
                OnPropertyChanged();
            }
        }

        public int? EmployeeID
        {
            get { return _employeeID; }
            set
            {
                _employeeID = value;
                OnPropertyChanged();
            }
        }

        public string EmployeeSurname
        {
            get { return _employeeSurname; }
            set
            {
                _employeeSurname = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Employee));
            }
        }

        public string EmployeeName
        {
            get { return _employeeName; }
            set
            {
                _employeeName = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Employee));
            }
        }

        public string EmployeePatronymic
        {
            get { return _employeePatronymic; }
            set
            {
                _employeePatronymic = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Employee));
            }
        }

        public string EmployeeTelephone
        {
            get { return _employeeTelephone; }
            set
            {
                _employeeTelephone = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Employee));
            }
        }

        public string Employee => $"{EmployeeSurname} {EmployeeName} {EmployeePatronymic} \n {EmployeeTelephone}";

        public int StatusID
        {
            get { return _statusID; }
            set
            {
                _statusID = value;
                OnPropertyChanged();
            }
        }

        public string StatusName
        {
            get { return _statusName; }
            set
            {
                _statusName = value;
                OnPropertyChanged();
            }
        }

        public string StatusColor
        {
            get { return _statusColor; }
            set
            {
                _statusColor = value;
                OnPropertyChanged();
            }
        }

        public int? ClientID
        {
            get { return _clientID; }
            set
            {
                _clientID = value;
                OnPropertyChanged();
            }
        }

        public string ClientSurname
        {
            get { return _clientSurname; }
            set
            {
                _clientSurname = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Client));
            }
        }

        public string ClientName
        {
            get { return _clientName; }
            set
            {
                _clientName = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Client));
            }
        }

        public string ClientPatronymic
        {
            get { return _clientPatronymic; }
            set
            {
                _clientPatronymic = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Client));
            }
        }

        public string ClientTelephone
        {
            get { return _clientTelephone; }
            set
            {
                _clientTelephone = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Client));
            }
        }

        public string Client => $"{ClientSurname} {ClientName} {ClientPatronymic} \n {ClientTelephone}";

        public string CompletionDate
        {
            get { return _completionDate ; }
            set
            {
                _completionDate = value;
                OnPropertyChanged();
            }
        }

        public string Reason
        {
            get { return _reason; }
            set
            {
                _reason = value;
                OnPropertyChanged();
            }
        }

        public string Date
        {
            get { return _date; }
            set
            {
                _date = value;
                OnPropertyChanged();
            }
        }

        public string Device
        {
            get { return _device; }
            set
            {
                _device = value;
                OnPropertyChanged();
            }
        }

        public string IMEI_SN
        {
            get { return _imeiSN; }
            set
            {
                _imeiSN = value;
                OnPropertyChanged();
            }
        }

        public string DetectedMulfunction
        {
            get { return _detectedMulfunction; }
            set
            {
                _detectedMulfunction = value;
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
