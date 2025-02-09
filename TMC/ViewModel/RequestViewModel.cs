using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TMC.Model;
using TMC.View;

namespace TMC.ViewModel
{
    public class RequestViewModel : INotifyPropertyChanged
    {
        ServiceCenterTMCEntities context = new ServiceCenterTMCEntities();
        RelayCommand? addCommand;
        RelayCommand? relayCommand;
        RelayCommand? editCommand;
        ObservableCollection<Employees> _mastersList;
        Employees SelectedMaster;
        private ObservableCollection<RequestView> _requests;
        public ObservableCollection<RequestView> RequestsList
        {
            get { return _requests; }
            set
            {
                _requests = value;
                OnPropertyChanged();
            }
        }

        public RequestViewModel()
        {
            LoadRequests();
            _mastersList = new ObservableCollection<Employees>(context.Employees.Where(e=>e.RoleID==3).ToList());
        }

        private void LoadRequests()
        {
            var result = (from r in context.Requests
                         join c in context.Clients on r.ClientID equals c.IDClient into clientGroup
                         from c in clientGroup.DefaultIfEmpty() // LEFT JOIN для Clients
                         join e in context.Employees on r.MasterID equals e.IDEmployee into employeeGroup
                         from e in employeeGroup.DefaultIfEmpty() // LEFT JOIN для Employees
                         join s in context.Status on r.StatusID equals s.IDstatus // INNER JOIN для Status
                         join dt in context.DeviseTypes on r.DeviceType equals dt.IDtype // INNER JOIN для DeviseTypes
                         select new RequestView
                         {
                             IDRequest = r.IDrequest,
                             EmployeeID = e.IDEmployee, // Добавьте ID для последующего соединения
                             EmployeeSurname = e.Surname,
                             EmployeeName = e.Name,
                             EmployeePatronymic = e.Patronymic,
                             EmployeeTelephone = e.Telephone,
                             StatusID = s.IDstatus,
                             StatusName = s.Name,
                             ClientID = c.IDClient, // Добавьте ID для последующего соединения
                             ClientSurname = c.Surname,
                             ClientName = c.Name,
                             ClientPatronymic = c.Patronymic,
                             ClientTelephone = c.Telephone,
                             CompletionDate = r.CompletionDate,
                             Reason = r.Reason,
                             Date = (DateTime)r.Date,
                             DeviceID = dt.IDtype,
                             DeviceTypeName = dt.Name,
                             Cost = r.Cost,
                             IMEI_SN = r.IMEI_SN,
                             Model = r.Model,
                         }).ToList();
            RequestsList = new ObservableCollection<RequestView>(result.Select(r =>
            {
                r.StatusColor = ColorStatus(r.StatusName);
                return r;
            }));
        }

        public ObservableCollection<Employees> MastersList
        {
            get { return _mastersList; }
            set
            {
                _mastersList = value;
                OnPropertyChanged();
            }
        }
         public string ColorStatus(string statusName)
        {
            switch (statusName)
            {
                case "Новый":
                    return "#BDFB82";
                case "Готов":
                    return "#BDFB82";
                case "В работе":
                    return "#BDFB82";
                case "Закрыт":
                    return "#BDFB82";
                case "Закрыт неуспешно":
                    return "#BDFB82";
                case "Ожидание ЗИП":
                    return "#BDFB82";
                case "Диагностика":
                    return "#BDFB82";
                default:
                    return "#FFFFF";
            }
        }
        public RelayCommand AddRequestCommand
        {
            get
            {
                return addCommand ??
                  (addCommand = new RelayCommand((o) =>
                  {
                      RequestWindow requestWindow = new RequestWindow(new RequestView());
                      if (requestWindow.ShowDialog() == true)
                      {
                          Requests requests = requestWindow.Requests;
                          context.Requests.Add(requests);
                          context.SaveChanges();
                      }
                  }));
            }
        }
        // команда редактирования
        public RelayCommand EditRequestCommand
        {
            get
            {
                return editCommand ??
                  (editCommand = new RelayCommand((selectedItem) =>
                  {
                      // получаем выделенный объект
                      RequestView request = selectedItem as RequestView;
                      Requests selectedRequest = context.Requests.Find(request.IDRequest);
                      if (request == null) return;
                      //SelectedMaster = _mastersList.First(m => m.IDEmployee == request.EmployeeID);

                      RequestWindow requestWindow = new RequestWindow(request);
                      requestWindow.MastersBox.ItemsSource = MastersList;
                      requestWindow.MastersBox.SelectedItem = context.Employees.Find(request.EmployeeID);
                      requestWindow.StatusBox.ItemsSource = context.Status.ToList();
                      requestWindow.StatusBox.SelectedItem = context.Status.Find(request.StatusID);
                      requestWindow.DeviceTypeBox.ItemsSource = context.DeviseTypes.ToList();
                      requestWindow.DeviceTypeBox.SelectedItem = context.DeviseTypes.Find(request.DeviceID);

                      //requestWindow.MastersBox.SelectedItem = _mastersList.First(m=>m.IDEmployee==request.EmployeeID);
                      if (requestWindow.ShowDialog() == true)
                      {
                          selectedRequest.Reason = request.Reason;
                          selectedRequest.Cost = (int)request.Cost;
                          context.Requests.AddOrUpdate(selectedRequest);
                          context.SaveChanges();
                          //request.Name = userWindow.User.Name;
                          //user.Age = userWindow.User.Age;
                          //db.Entry(user).State = EntityState.Modified;
                          //db.SaveChanges();
                      }
                  }));
            }
        }



        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    
}
