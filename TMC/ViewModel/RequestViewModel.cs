using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using TMC.Model;
using TMC.View;
using Word = Microsoft.Office.Interop.Word;

using System.Windows.Documents;
using Paragraph = iText.Layout.Element.Paragraph;
using Table = iText.Layout.Element.Table;
using System.IO;
using System.Diagnostics;
using System.Xml;
using iText.IO.Font.Constants;
using System.Windows.Media;

namespace TMC.ViewModel
{
    public class RequestViewModel : INotifyPropertyChanged
    {
        ServiceCenterTMCEntities context = new ServiceCenterTMCEntities();
        RelayCommand? addCommand;
        RelayCommand? relayCommand;
        RelayCommand printRepairActCommand;
        RelayCommand? editCommand;
        RelayCommand? saveCommand;
        RelayCommand addServicesCommand;
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
                             ClientType = c.Type,
                             ClientCompanyName = c.CompanyName,
                             ClientEmail = c.Email,
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
                      RequestWindow requestWindow = new RequestWindow(new Requests(), this);
                      requestWindow.MastersBox.ItemsSource = MastersList;
                      requestWindow.StatusBox.ItemsSource = context.Status.ToList();
                      requestWindow.DeviceTypeBox.ItemsSource = context.DeviseTypes.ToList();
                      requestWindow.ClientInfo.DataContext = new Clients();
                      
                      if (requestWindow.ShowDialog() == true)
                      {
                          //Clients client = context.Clients.Find(request.ClientID);
                          Clients client = requestWindow.ClientInfo.DataContext as Clients;
                          Requests newRequest = requestWindow.Requests;
                          context.Clients.Add(client);
                          var selectedStatus = requestWindow.StatusBox.SelectedItem as Status;
                          newRequest.StatusID = selectedStatus.IDstatus;
                          newRequest.DeviceType = (requestWindow.DeviceTypeBox.SelectedItem as DeviseTypes).IDtype;
                          newRequest.Date = DateTime.Now;
                          newRequest.Cost = (int)requestWindow.Requests.Cost;
                          context.Requests.Add(newRequest);
                          context.SaveChanges();
                          LoadRequests();
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
                      if (request == null) return;
                      Requests selectedRequest = context.Requests.Find(request.IDRequest);

                      RequestWindow requestWindow = new RequestWindow(selectedRequest, this);
                      requestWindow.ClientInfo.DataContext = context.Clients.Find(request.ClientID);
                      requestWindow.MastersBox.ItemsSource = MastersList;
                      requestWindow.MastersBox.SelectedItem = context.Employees.Find(request.EmployeeID);
                      requestWindow.StatusBox.ItemsSource = context.Status.ToList();
                      requestWindow.StatusBox.SelectedItem = context.Status.Find(request.StatusID);
                      requestWindow.DeviceTypeBox.ItemsSource = context.DeviseTypes.ToList();
                      requestWindow.DeviceTypeBox.SelectedItem = context.DeviseTypes.Find(request.DeviceID);
                      List<Requests_Services> request_services = context.Requests_Services.Where(r=>r.RequestID == selectedRequest.IDrequest).ToList();
                      requestWindow.Show();
                      foreach (var item in request_services)
                      {
                          Services service = context.Services.Find(item.ServiceID);
                          SelectedServices.Add(service);
                          MessageBox.Show(service.Name);
                      }
                      requestWindow.selectedServices.ItemsSource = SelectedServices;
                      
                  }));
            }
        }
        public RelayCommand SaveCommand
        {
            get
            {
                return saveCommand ??
                  (saveCommand = new RelayCommand((o) =>
                  {
                      RequestWindow requestWindow = o as RequestWindow;
                      var selectedRequest = requestWindow.Requests;
                      var selectedStatus = requestWindow.StatusBox.SelectedItem as Status;
                      selectedRequest.StatusID = selectedStatus.IDstatus;
                      selectedRequest.DeviceType = (requestWindow.DeviceTypeBox.SelectedItem as DeviseTypes).IDtype;
                      var selectedMaster = requestWindow.MastersBox.SelectedItem as Employees;
                      if (selectedMaster != null) selectedRequest.MasterID = selectedMaster.IDEmployee;
                      selectedRequest.Cost = (int)requestWindow.Requests.Cost;
                      context.Requests.AddOrUpdate(selectedRequest);
                      context.SaveChanges();
                      foreach (var service in SelectedServices)
                      {
                          // Проверяем, существует ли уже такая запись
                          bool exists = context.Requests_Services
                              .Any(rs => rs.RequestID == selectedRequest.IDrequest && rs.ServiceID == service.IDservice);

                          if (!exists)
                          {
                              Requests_Services requests_Services = new Requests_Services
                              {
                                  RequestID = selectedRequest.IDrequest,
                                  ServiceID = service.IDservice,
                                  Count = 1 // Можно добавить логику для указания количества
                              };
                              context.Requests_Services.Add(requests_Services);
                          }
                      }
                      context.SaveChanges();
                      LoadRequests();
                      requestWindow.Close();
                  }));
            }
        }
        private ObservableCollection<Services> _selectedServices = new ObservableCollection<Services>();
        public ObservableCollection<Services> SelectedServices
        {
            get { return _selectedServices; }
            set
            {
                _selectedServices = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand AddServicesCommand
        {
            get
            {
                return addServicesCommand ??
                  (addServicesCommand = new RelayCommand((o) =>
                  {
                      AddServicesWindow servicesWindow = new AddServicesWindow();
                      var vm = servicesWindow.DataContext as ServicesViewModel;

                      if (servicesWindow.ShowDialog() == true)
                      {
                          // Добавляем выбранные услуги к заявке
                          foreach (var service in vm.SelectedServices)
                          {
                              SelectedServices.Add(service);
                              MessageBox.Show(service.Name);
                          }
                      }
                  }));
            }
        }
        public RelayCommand PrintRepairActCommand
        {
            get
            {
                return printRepairActCommand ??
                  (printRepairActCommand = new RelayCommand((o) =>
                  {
                      RequestWindow requestWindow = o as RequestWindow;
                      var request = requestWindow.Requests;
                      request.DeviceType = (requestWindow.DeviceTypeBox.SelectedItem as DeviseTypes).IDtype;
                      var client = requestWindow.ClientInfo.DataContext as Clients;
                      PdfDocument document = new PdfDocument();

                      // Создаем страницу
                      PdfPage page = document.AddPage();

                      // Создаем объект XGraphics для рисования
                      XGraphics gfx = XGraphics.FromPdfPage(page);

                      // Настраиваем шрифт и цвет
                      XFont font = new XFont("Arial", 12, XFontStyle.Regular);
                      XBrush brush = XBrushes.Black;

                      // Добавляем текст
                      gfx.DrawString("Это текст в PDF-документе.", font, brush, new XPoint(50, 50));
                      gfx.DrawString("Ещё один абзац.", font, brush, new XPoint(50, 100));


                      // Сохраняем PDF-файл
                      string filePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "MyDocument.pdf");
                      document.Save(filePath);

                      // Открываем PDF-файл с помощью стандартного средства просмотра PDF
                      Process.Start(filePath);

                      MessageBox.Show("PDF-файл создан и открыт!");
                  }));
            }
        }
        private static void PrintDocument(string filePath)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = filePath,
                Verb = "print",
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            using (Process process = Process.Start(startInfo))
            {
                process.WaitForInputIdle();  // Дождаться, пока процесс станет неактивным
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    
}
