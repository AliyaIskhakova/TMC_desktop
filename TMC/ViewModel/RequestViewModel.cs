using iText.Kernel.Pdf;
using PdfSharpCore.Drawing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity.Migrations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Xps.Packaging;
using TMC.Model;
using TMC.View;
using Microsoft.Office.Interop.Word;
using Application = Microsoft.Office.Interop.Word.Application;
using System.Runtime.Remoting.Messaging;
using Table = Microsoft.Office.Interop.Word.Table;
using Paragraph = Microsoft.Office.Interop.Word.Paragraph;
using System.Web.UI.WebControls.WebParts;
using App = System.Windows.Application;

namespace TMC.ViewModel
{
    public class RequestViewModel : INotifyPropertyChanged
    {
        ServiceCenterTMCEntities context = new ServiceCenterTMCEntities();
        RelayCommand addCommand;
        RelayCommand relayCommand;
        RelayCommand printRepairActCommand;
        RelayCommand editCommand;
        RelayCommand saveCommand;
        RelayCommand selectRequestByStatus;
        RelayCommand addServicesCommand;
        
        RelayCommand addPartsCommand;
        ObservableCollection<Employees> _mastersList;
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
                          /*join dt in context.DeviseTypes on r.DeviceType equals dt.IDtype*/ // INNER JOIN для DeviseTypes
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
                              CompletionDate = r.CompletionDate.ToString(),
                             Reason = r.Reason,
                             Date = r.Date.ToString(),
                         }).ToList();
            RequestsList = new ObservableCollection<RequestView>(result.Select(r =>
            {
                r.StatusColor = ColorStatus(r.StatusName);
                if(r.CompletionDate!= "")  r.CompletionDate = (Convert.ToDateTime(r.CompletionDate)).ToShortDateString();
                if (r.Date != "") r.Date = (Convert.ToDateTime(r.Date)).ToString("dd.MM.yyyy \n HH:mm");
                return r;
            }));
            string role =  App.Current.Properties["Role"] as string;
            int id = (int)App.Current.Properties["UserID"];
            if (role == "Мастер") RequestsList = new ObservableCollection<RequestView>(RequestsList.Where(r=> r.EmployeeID == id));
        }

        public RelayCommand SelectRequestByStatus
        {
            get
            {
                return selectRequestByStatus ??= new RelayCommand((status) =>
                  {
                      RequestsList.Clear();
                      LoadRequests();
                      switch (status)
                      {
                          case "Все":
                              LoadRequests();
                              break;
                          case "Готовые":
                              RequestsList = new ObservableCollection<RequestView>(RequestsList.Where(r => r.StatusName == "Готов").ToList());
                              break;
                          case "Ожидание ЗИП":
                              RequestsList = new ObservableCollection<RequestView>(RequestsList.Where(r => r.StatusName == "Ждет ЗИП").ToList());
                              break;
                          case "В работе":
                              RequestsList = new ObservableCollection<RequestView>(RequestsList.Where(r => r.StatusName == "В работе").ToList());
                              break;
                          case "Новые":
                              RequestsList = new ObservableCollection<RequestView>(RequestsList.Where(r => r.StatusName == "Новый").ToList());
                              break;
                          case "Отменены":
                              RequestsList = new ObservableCollection<RequestView>(RequestsList.Where(r => r.StatusName == "Отменен").ToList());
                              break;
                      }
                                       
                  });
            }
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
                    return "#60B7FF";
                case "Готов":
                    return "#90EE90";
                case "В работе":
                    return "#FFD700";
                case "Завершен":
                    return "#D3D3D3";
                case "Отменен":
                    return "#D3D3D3";
                case "Ждет ЗИП":
                    return "#FFA500";
                //case "Диагностика":
                //    return "#BDFB82";
                default:
                    return "#FFFFFF";
            }
        }
        public RelayCommand AddRequestCommand
        {
            get
            {
                return addCommand ??= new RelayCommand((o) =>
                  {

                      SelectedServices.Clear();
                      SelectedParts.Clear();
                      RequestWindow requestWindow = new RequestWindow(new Requests(), this);
                      requestWindow.MastersBox.ItemsSource = MastersList;
                      requestWindow.StatusBox.ItemsSource = context.Status.Where(s => s.Name != "Завершен" && s.Name != "Отменен").ToList();
                      requestWindow.DeviceTypeBox.ItemsSource = context.DeviseTypes.ToList();
                      requestWindow.EndDocuument.Visibility = Visibility.Collapsed;
                      if (requestWindow.ShowDialog() == true)
                      {
                          Requests newRequest = requestWindow.Requests;
                          Clients client = requestWindow.ClientInfo.DataContext as Clients;

                          if (context.Clients.Any(x => x.IDClient == client.IDClient))
                          {
                              newRequest.ClientID = client.IDClient;
                          }
                          else
                          {
                              context.Clients.Add(client);
                          }
                          var selectedStatus = requestWindow.StatusBox.SelectedItem as Status;
                          newRequest.StatusID = selectedStatus.IDstatus;
                          var selectedDevice = requestWindow.DeviceTypeBox.SelectedItem as DeviseTypes;
                          if (selectedDevice != null) { newRequest.DeviceType = selectedDevice.IDtype; }
                          newRequest.Date = DateTime.Now;
                          newRequest.Cost = (int)requestWindow.Requests.Cost;
                          var selectedMaster = requestWindow.MastersBox.SelectedItem as Employees;
                          if (selectedMaster != null) newRequest.MasterID = selectedMaster.IDEmployee;
                          context.Requests.Add(newRequest);
                          context.SaveChanges();
                          foreach (var service in SelectedServices)
                          {
                              // Проверяем, существует ли уже такая запись
                              bool exists = context.Requests_Services
                                  .Any(rs => rs.RequestID == newRequest.IDrequest && rs.ServiceID == service.IDservice);

                              if (!exists)
                              {
                                  Requests_Services requests_Services = new Requests_Services
                                  {
                                      RequestID = newRequest.IDrequest,
                                      ServiceID = service.IDservice,
                                      Count = 1 // Можно добавить логику для указания количества
                                  };
                                  context.Requests_Services.Add(requests_Services);
                              }
                          }
                          foreach (var part in SelectedParts)
                          {
                              // Проверяем, существует ли уже такая запись
                              bool exists = context.Request_RepairParts
                                  .Any(rs => rs.RequestID == newRequest.IDrequest && rs.RepairPartID == part.IDpart);

                              if (!exists)
                              {
                                  Request_RepairParts request_RepairParts = new Request_RepairParts
                                  {
                                      RequestID = newRequest.IDrequest,
                                      RepairPartID = part.IDpart,
                                      Count = 1 // Можно добавить логику для указания количества
                                  };
                                  context.Request_RepairParts.Add(request_RepairParts);
                              }
                          }
                          context.SaveChanges();
                          SelectedServices.Clear();
                          SelectedParts.Clear();
                          RequestsList.Clear();
                          LoadRequests();

                      }
                  });
            }
        }
        // команда редактирования
        public RelayCommand EditRequestCommand
        {
            get
            {
                return editCommand ??= new RelayCommand((selectedItem) =>
                  {
                      SelectedServices.Clear();
                      SelectedParts.Clear();
                      string role = App.Current.Properties["Role"] as string;
                      int id = (int)App.Current.Properties["UserID"];
                      List<Status> status = new List<Status>();
                      
                      RequestView request = selectedItem as RequestView;
                      if (request == null) return;
                      Requests selectedRequest = context.Requests.Find(request.IDRequest);
                      RequestWindow requestWindow = new RequestWindow(selectedRequest, this);
                      if (role == "Мастер" && request.StatusName != "Завершен" && request.StatusName != "Отменен") status = context.Status.Where(s => s.Name != "Завершен" && s.Name != "Отменен").ToList();
                      else status = context.Status.ToList();
                      requestWindow.RequestDate.Visibility = Visibility.Visible;
                      requestWindow.ClientInfo.DataContext = context.Clients.Find(request.ClientID);
                      requestWindow.ClientInfo.IsEnabled = false;
                      requestWindow.ClientComboBox.Visibility = Visibility.Collapsed;
                      requestWindow.MastersBox.ItemsSource = MastersList;
                      requestWindow.MastersBox.SelectedItem = context.Employees.Find(request.EmployeeID);
                      requestWindow.StatusBox.ItemsSource = status;
                      requestWindow.StatusBox.SelectedItem = context.Status.Find(request.StatusID);
                      requestWindow.DeviceTypeBox.ItemsSource = context.DeviseTypes.ToList();
                      requestWindow.DeviceTypeBox.SelectedItem = context.DeviseTypes.Find(selectedRequest.DeviceType);
                      if(selectedRequest.Status.Name == "Завершен" || selectedRequest.Status.Name == "Отменен")
                      {
                          requestWindow.InfoBlock1.IsEnabled = false;
                          requestWindow.InfoBlock2.IsEnabled = false;
                          requestWindow.ServiceAndPartsInfo.IsEnabled = false;
                          requestWindow.SaveBtn.Visibility = Visibility.Collapsed;
                      }
                      if (role == "Мастер")
                      {
                          requestWindow.RequestInfo.IsEnabled = false;
                          requestWindow.MasterInfo.IsEnabled = false;
                          requestWindow.ServiceAndPartsInfo.IsEnabled = false;
                          requestWindow.PrintBtns.Visibility= Visibility.Collapsed;
                      }
                      //requestWindow.Show();
                      List<Requests_Services> request_services = context.Requests_Services.Where(r => r.RequestID == selectedRequest.IDrequest).ToList();
                      foreach (var item in request_services)
                      {
                          Services service = context.Services.Find(item.ServiceID);
                          SelectedServices.Add(service);
                      }
                      requestWindow.selectedServices.ItemsSource = SelectedServices;
                      List<Request_RepairParts> request_parts = context.Request_RepairParts.Where(r => r.RequestID == selectedRequest.IDrequest).ToList();
                      //requestWindow.Show();
                      foreach (var item in request_parts)
                      {
                          RepairParts part = context.RepairParts.Find(item.RepairPartID);
                          SelectedParts.Add(part);
                      }
                      requestWindow.selectedServices.ItemsSource = SelectedServices;
                      if (requestWindow.ShowDialog() == true)
                      {
                          var selectedStatus = requestWindow.StatusBox.SelectedItem as Status;
                          selectedRequest.StatusID = selectedStatus.IDstatus;
                          var selectedDevice = requestWindow.DeviceTypeBox.SelectedItem as DeviseTypes;
                          if (selectedDevice!=null) selectedRequest.DeviceType = selectedDevice.IDtype;
                          var selectedMaster = requestWindow.MastersBox.SelectedItem as Employees;
                          if (selectedMaster != null) selectedRequest.MasterID = selectedMaster.IDEmployee;
                          selectedRequest.Cost = (int)requestWindow.Requests.Cost;
                          context.Requests.AddOrUpdate(selectedRequest);
                          context.SaveChanges();
                          foreach (var item in request_services)
                          {
                              context.Requests_Services.Remove(item);
                              }
                          foreach (var service in SelectedServices)
                          {
                              // Проверяем, существует ли уже такая запись
                              //bool exists = context.Requests_Services
                              //    .Any(rs => rs.RequestID == selectedRequest.IDrequest && rs.ServiceID == service.IDservice);
                              
                              //if (!exists)
                              //{
                                  Requests_Services requests_Services = new Requests_Services
                                  {
                                      RequestID = selectedRequest.IDrequest,
                                      ServiceID = service.IDservice,
                                      Count = 1 // Можно добавить логику для указания количества
                                  };
                                  context.Requests_Services.Add(requests_Services);
                              //}
                          }
                          foreach (var item in request_parts)
                          {
                              context.Request_RepairParts.Remove(item);
                          }
                          foreach (var part in SelectedParts)
                          {
                              // Проверяем, существует ли уже такая запись
                              //bool exists = context.Request_RepairParts
                              //    .Any(rs => rs.RequestID == selectedRequest.IDrequest && rs.RepairPartID == part.IDpart);

                              //if (!exists)
                              //{
                                  Request_RepairParts request_RepairParts = new Request_RepairParts
                                  {
                                      RequestID = selectedRequest.IDrequest,
                                      RepairPartID = part.IDpart,
                                      Count = 1 // Можно добавить логику для указания количества
                                  };
                                  context.Request_RepairParts.Add(request_RepairParts);
                              //}
                          }
                          context.SaveChanges();
                          SelectedServices.Clear();
                          SelectedParts.Clear();
                      }
                      LoadRequests();
                  });
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
                return addServicesCommand ??= new RelayCommand((o) =>
                  {
                      AddServicesWindow servicesWindow = new AddServicesWindow();
                      var vm = servicesWindow.DataContext as ServicesViewModel;
                      RequestWindow requestWindow = o as RequestWindow;
                      if (servicesWindow.ShowDialog() == true)
                      {
                          // Добавляем выбранные услуги к заявке
                          foreach (var service in vm.SelectedServices)
                          {
                              bool exists = SelectedServices.Any(rs => rs.IDservice == service.IDservice);
                              if (!exists)
                              {
                                  SelectedServices.Add(service);
                                  requestWindow.Requests.Cost = (int)(requestWindow.Requests.Cost + service.Cost);
                              }
                          }
                      }

                  });
            }
        }
        public RelayCommand DeleteServicesCommand
        {
            get
            {
                return new RelayCommand((o) =>
                  {
                      
                      RequestWindow requestWindow = o as RequestWindow;
                      Services services = requestWindow.selectedServices.SelectedItem as Services;
                      if (services != null)
                      {
                          SelectedServices.Remove(services);
                          requestWindow.Requests.Cost = (int)(requestWindow.Requests.Cost - services.Cost);

                      }
                      else MessageBox.Show("Если хотите удалить услугу из заявки, выберите услугу для удаления", "Формирование заявки", MessageBoxButton.OK, MessageBoxImage.Information);
                  });
            }
        }

        private ObservableCollection<RepairParts> _selectedParts = new ObservableCollection<RepairParts>();
        public ObservableCollection<RepairParts> SelectedParts
        {
            get { return _selectedParts; }
            set
            {
                _selectedParts = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand AddPartsCommand
        {
            get
            {
                return addPartsCommand ??= new RelayCommand((o) =>
                  {
                      AddPartsWindow partsWindow = new AddPartsWindow();
                      var vm = partsWindow.DataContext as StoreViewModel;
                      RequestWindow requestWindow = o as RequestWindow;

                      if (partsWindow.ShowDialog() == true)
                      {

                          // Добавляем выбранные услуги к заявке
                          foreach (var part in vm.SelectedParts)
                          {
                              bool exists = SelectedParts.Any(rs => rs.IDpart == part.IDpart);
                              if (!exists)
                              {
                                  SelectedParts.Add(part);
                                  requestWindow.Requests.Cost = (int)(requestWindow.Requests.Cost + part.Cost);
                              }

                          }
                      }
                  });
            }
        }
        public RelayCommand DeletePartsCommand
        {
            get
            {
                return new RelayCommand((o) =>
                {

                    RequestWindow requestWindow = o as RequestWindow;
                    RepairParts parts = requestWindow.selectedParts.SelectedItem as RepairParts;
                    if (parts != null)
                    {
                        SelectedParts.Remove(parts);
                        requestWindow.Requests.Cost = (int)(requestWindow.Requests.Cost - parts.Cost);

                    }
                    else MessageBox.Show("Если хотите удалить ЗИП из заявки, выберите ЗИП для удаления", "Формирование заявки", MessageBoxButton.OK, MessageBoxImage.Information);
                });
            }
        }

        public RelayCommand PrintRepairActCommand
        {
            get
            {
                return new RelayCommand(async (o) =>
                {

                    RequestWindow requestWindow = o as RequestWindow;
                    if (!(!string.IsNullOrWhiteSpace(requestWindow.RequestReason.Text) && !(requestWindow.ClientInfo.DataContext as Clients).HasValidationErrors()
                    && int.TryParse(requestWindow.RequestCost.Text, out int cost) && cost >= 0))
                    {
                        MessageBox.Show("Проверьте введенные данные!");
                        return;
                    }
                    var request = requestWindow.Requests;
                    var device = requestWindow.DeviceTypeBox.SelectedItem as DeviseTypes;
                    if (device != null) device = new DeviseTypes();
                    var client = requestWindow.ClientInfo.DataContext as Clients;
                        // Показываем MessageBox в основном потоке
                    MessageBox.Show("Ожидайте, документ формируется", "Формирование документа", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Запускаем формирование документа в отдельном потоке
                    await System.Threading.Tasks.Task.Run(() =>
                    {
                        Application wordApp = new Microsoft.Office.Interop.Word.Application();
                        Document wordDoc = wordApp.Documents.Add();

                        wordDoc.Content.ParagraphFormat.SpaceAfter = 0;
                        wordDoc.Content.ParagraphFormat.SpaceBefore = 0;
                        wordDoc.Content.Font.Name = "Times New Roman";
                        wordDoc.Content.Font.Size = 12;

                        // Добавление описания
                        Paragraph name = wordDoc.Content.Paragraphs.Add();
                        name.Range.Text = "Сервисный центр ТехноМедиаСоюз";
                        name.Range.Font.Size = 13;
                        name.Range.Font.Bold = 1;
                        name.Format.LineSpacingRule = WdLineSpacing.wdLineSpaceSingle;
                        name.Format.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
                        name.Range.InsertParagraphAfter();

                        Paragraph descriptionParagraph1 = wordDoc.Content.Paragraphs.Add();
                        descriptionParagraph1.Range.Text = "ИП \"Сулейманов М.Р.\", г. Арск ул. Школьная 17, http://www.vk.com/servistmsouz," +
                                                         "тел. 8(443) 248-92-60. Время работы с 9.00 до 18.00 (понедельник-пятница), без перерывов ";
                        descriptionParagraph1.Range.Font.Bold = 0;
                        descriptionParagraph1.Format.LineSpacingRule = WdLineSpacing.wdLineSpaceSingle;
                        descriptionParagraph1.Format.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
                        descriptionParagraph1.Range.InsertParagraphAfter();
                        descriptionParagraph1.Range.InsertParagraphAfter();
                        descriptionParagraph1.Range.InsertParagraphAfter();

                        // Добавление заголовка
                        Paragraph titleParagraph = wordDoc.Content.Paragraphs.Add();
                        titleParagraph.Range.Text = $"Акт о приеме на ремонт №{request.IDrequest}";
                        titleParagraph.Range.Font.Size = 13;
                        titleParagraph.Range.Font.Bold = 1;
                        titleParagraph.Format.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
                        titleParagraph.Range.InsertParagraphAfter();

                        // Создание таблицы с 5 строками и 2 колонками
                        Table table = wordDoc.Tables.Add(wordDoc.Content.Paragraphs.Add().Range, 5, 2);
                        table.Borders.Enable = 1;
                        table.Range.Bold = 0;
                        table.Range.ParagraphFormat.LineSpacingRule = WdLineSpacing.wdLineSpaceSingle;

                        // Настройка ширины столбцов
                        table.Columns[1].PreferredWidth = wordApp.CentimetersToPoints(5);
                        table.Columns[2].PreferredWidth = wordApp.CentimetersToPoints(12);

                        // Заполнение таблицы данными
                        table.Cell(1, 1).Range.Text = "Клиент";
                        table.Cell(2, 1).Range.Text = "Оборудование";
                        table.Cell(3, 1).Range.Text = "Серийный номер";
                        table.Cell(4, 1).Range.Text = "Проблема со слов клиента";
                        table.Cell(5, 1).Range.Text = "Примечание";

                        table.Cell(1, 2).Range.Text = $"{client.surname} {client.name} {client.patronymic}";
                         table.Cell(2, 2).Range.Text = $"{device.Name} {request.Model}";
                        table.Cell(3, 2).Range.Text = $"{request.IMEI_SN}";
                        table.Cell(4, 2).Range.Text = $"{request.Reason}";
                        table.Cell(5, 2).Range.Text = $"{request.Notes}";
                        for (int i = 1; i <= 5; i++)
                        {
                            table.Cell(i, 1).Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphRight;
                            table.Cell(i, 1).Range.Font.Bold = 1; // Жирный шрифт
                        }

                        // Выравнивание текста во втором столбце по левому краю
                        for (int i = 1; i <= 5; i++)
                        {
                            table.Cell(i, 2).Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphLeft;
                        }

                        // Добавление дополнительного описания
                        Paragraph descriptionParagraph = wordDoc.Content.Paragraphs.Add();
                        descriptionParagraph.Range.Text = "Клиент согласен, что все неисправности и внутренние повреждения, которые могут быть обнаружены в оборудовании при техническом обслуживании, " +
                        "возникли до приема оборудования по данной квитанции. В случае утери акта о приеме оборудования на ремонт выдача аппарата производится при предъявлении паспорта лица сдававшего аппарат " +
                        "и письменного заявления. Внимание: Срок ремонта аппарата 21 день, максимальный срок при отсутствии запчастей на складе поставщика может быть увеличен до 45 дней. Заказчик согласен на " +
                        "обработку персональных данных, а также несет ответственность за достоверность предоставленной информации. С комплектацией, описанием неисправностей и повреждений, условиями хранения и " +
                        "обслуживания оборудования ознакомлен и согласен.";
                        descriptionParagraph.Range.Font.Size = 9;
                        descriptionParagraph.Format.LineSpacingRule = WdLineSpacing.wdLineSpaceSingle;
                        descriptionParagraph.Range.Font.Bold = 0;
                        descriptionParagraph.Format.Alignment = WdParagraphAlignment.wdAlignParagraphJustify;

                        descriptionParagraph.Range.InsertParagraphAfter();
                        descriptionParagraph.Range.InsertParagraphAfter();

                        // Создание таблицы для подписей
                        Table signatureTable = wordDoc.Tables.Add(wordDoc.Content.Paragraphs.Add().Range, 2, 2);
                        signatureTable.Borders.Enable = 0;

                        // Заполнение таблицы подписей
                        signatureTable.Cell(1, 1).Range.Text = $"Оборудование в ремонт сдал: {client.surname} {client.name[0]}.{client.patronymic[0]}.";
                        signatureTable.Cell(1, 2).Range.Text = "_____________________";
                        var receiver = context.Employees.Find((int)App.Current.Properties["UserID"]);
                        signatureTable.Cell(2, 1).Range.Text = $"Оборудование в ремонт принял: инженер приемщик {receiver.Surname} {receiver.Name[0]}.{receiver.Patronymic[0]}.";
                        signatureTable.Cell(2, 2).Range.Text = "_____________________";

                        // Выравнивание текста в таблице подписей
                        signatureTable.Rows[1].Cells[1].Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphLeft;
                        signatureTable.Rows[1].Cells[2].Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphRight;
                        signatureTable.Rows[2].Cells[1].Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphLeft;
                        signatureTable.Rows[2].Cells[2].Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphRight;

                        // Отображение документа
                        wordApp.Visible = true;

                        // Предварительный просмотр документа
                        wordDoc.PrintPreview();
                    });

                });
            }
        }
       
        public RelayCommand PrintComplitedWorkActCommand
        {
            get
            {
                return  new RelayCommand((o) =>
                  {
                      RequestWindow requestWindow = o as RequestWindow;
                      var request = requestWindow.Requests;
                      var device = requestWindow.DeviceTypeBox.SelectedItem as DeviseTypes;
                      var client = requestWindow.ClientInfo.DataContext as Clients;
                      var status = requestWindow.StatusBox.SelectedItem as Status;
                      if (status.Name == "Готов" || status.Name == "Завершен")
                      {
                          MessageBox.Show("Ожидайте, документ формируется", "Формирование документа", MessageBoxButton.OK, MessageBoxImage.Information);

                          Microsoft.Office.Interop.Word.Application wordApp = new Microsoft.Office.Interop.Word.Application();
                          Microsoft.Office.Interop.Word.Document wordDoc = wordApp.Documents.Add();

                          wordDoc.Content.ParagraphFormat.SpaceAfter = 0;
                          wordDoc.Content.ParagraphFormat.SpaceBefore = 0;
                          wordDoc.Content.Font.Name = "Times New Roman";
                          wordDoc.Content.Font.Size = 12;
                          // Добавление описания
                          Paragraph name = wordDoc.Content.Paragraphs.Add();
                          name.Range.Text = "Сервисный центр ТехноМедиаСоюз";
                          name.Range.Font.Size = 12;
                          name.Range.Font.Bold = 1;
                          name.Format.LineSpacingRule = WdLineSpacing.wdLineSpaceSingle;
                          name.Format.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
                          name.Format.SpaceAfter = 0;
                          name.Range.InsertParagraphAfter();


                          Paragraph descriptionParagraph1 = wordDoc.Content.Paragraphs.Add();
                          descriptionParagraph1.Range.Text = "ИП \"Сулейманов М.Р.\", г. Арск ул. Школьная 17, http://www.vk.com/servistmsouz," +
                                                           "тел. 8(443) 248-92-60. Время работы с 9.00 до 18.00 (понедельник-пятница), без перерывов ";
                          descriptionParagraph1.Range.Font.Size = 12;
                          descriptionParagraph1.Range.Font.Bold = 0;
                          descriptionParagraph1.Format.LineSpacingRule = WdLineSpacing.wdLineSpaceSingle;
                          descriptionParagraph1.Format.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
                          descriptionParagraph1.Format.SpaceAfter = 0;
                          descriptionParagraph1.Range.InsertParagraphAfter();
                          descriptionParagraph1.Range.InsertParagraphAfter();

                          // Добавление заголовка
                          Paragraph titleParagraph = wordDoc.Content.Paragraphs.Add();
                          titleParagraph.Range.Text = $"Акт о выполненных рвбот №{request.IDrequest} от {DateTime.Now.ToShortDateString()}";
                          titleParagraph.Range.Font.Size = 13;
                          titleParagraph.Range.Font.Bold = 1;
                          titleParagraph.Format.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
                          titleParagraph.Range.InsertParagraphAfter();

                          // Создание таблицы с 5 строками и 2 колонками
                          Table table = wordDoc.Tables.Add(wordDoc.Content.Paragraphs.Add().Range, 8, 2);
                          table.Borders.Enable = 1;
                          table.Range.Bold = 0;
                          table.Range.Font.Size = 12;
                          table.Range.ParagraphFormat.LineSpacingRule = WdLineSpacing.wdLineSpaceSingle;

                          // Настройка ширины столбцов
                          table.Columns[1].PreferredWidth = wordApp.CentimetersToPoints(5);
                          table.Columns[2].PreferredWidth = wordApp.CentimetersToPoints(12);

                          // Заполнение таблицы данными
                          table.Cell(1, 1).Range.Text = "Номер заказа";
                          table.Cell(2, 1).Range.Text = "ФИО клиента";
                          table.Cell(3, 1).Range.Text = "Телефон клиента";
                          table.Cell(4, 1).Range.Text = "Устройство";
                          table.Cell(5, 1).Range.Text = "Серийный номер";
                          table.Cell(6, 1).Range.Text = "Дата приёма";
                          table.Cell(7, 1).Range.Text = "Дата выдачи";
                          table.Cell(8, 1).Range.Text = "Гарантия";

                          table.Cell(1, 2).Range.Text = $"{request.IDrequest}";
                          table.Cell(2, 2).Range.Text = $"{client.surname} {client.name} {client.patronymic}";
                          table.Cell(3, 2).Range.Text = $"{client.telephone}";
                          table.Cell(4, 2).Range.Text = $"{device.Name} {request.Model}";
                          table.Cell(5, 2).Range.Text = $"{request.IMEI_SN}";
                          table.Cell(6, 2).Range.Text = $"{request.Date}";
                          table.Cell(7, 2).Range.Text = $"{DateTime.Now.ToShortDateString()}";
                          table.Cell(8, 2).Range.Text = $"{request.Notes}";

                          for (int i = 1; i <= 8; i++)
                          {
                              table.Cell(i, 1).Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphRight;
                              table.Cell(i, 1).Range.Font.Bold = 1; // Жирный шрифт
                          }

                          // Выравнивание текста во втором столбце по левому краю
                          for (int i = 1; i <= 8; i++)
                          {
                              table.Cell(i, 2).Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphLeft;
                          }
                          Paragraph complitedWork = wordDoc.Content.Paragraphs.Add();
                          complitedWork.Range.Text = $"Выполненнные работы";
                          complitedWork.Range.Font.Size = 13;
                          complitedWork.Range.Font.Bold = 1;
                          complitedWork.Format.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
                          complitedWork.Range.InsertParagraphAfter();
                          List<Requests_Services> request_services = context.Requests_Services.Where(r => r.RequestID == request.IDrequest).ToList();
                          //requestWindow.Show();
                          foreach (var item in request_services)
                          {
                              Services service = context.Services.Find(item.ServiceID);
                              SelectedServices.Add(service);
                          }
                          // Создание таблицы с услугами
                          Table servicesTable = wordDoc.Tables.Add(wordDoc.Content.Paragraphs.Add().Range, request_services.Count + 1, 3);
                          servicesTable.Borders.Enable = 1;
                          servicesTable.Range.Font.Size = 11;
                          servicesTable.Range.Font.Bold = 0;
                          servicesTable.Range.ParagraphFormat.LineSpacingRule = WdLineSpacing.wdLineSpaceSingle;

                          // Настройка ширины столбцов
                          servicesTable.Columns[1].PreferredWidth = wordApp.CentimetersToPoints(10);
                          servicesTable.Columns[2].PreferredWidth = wordApp.CentimetersToPoints(3);
                          servicesTable.Columns[3].PreferredWidth = wordApp.CentimetersToPoints(3);

                          // Заполнение заголовков таблицы
                          servicesTable.Cell(1, 1).Range.Text = "Наименование";
                          servicesTable.Cell(1, 2).Range.Text = "Кол-во";
                          servicesTable.Cell(1, 3).Range.Text = "Цена, руб.";

                          // Заполнение таблицы данными из списка
                          for (int i = 0; i < request_services.Count; i++)
                          {
                              servicesTable.Cell(i + 2, 1).Range.Text = context.Services.Find(request_services[i].ServiceID).Name;
                              servicesTable.Cell(i + 2, 2).Range.Text = request_services[i].Count.ToString();
                              servicesTable.Cell(i + 2, 3).Range.Text = context.Services.Find(request_services[i].ServiceID).Cost.ToString();
                          }
                          Paragraph costParagraph = wordDoc.Content.Paragraphs.Add();
                          costParagraph.Range.Text = $"ИТОГ: {request.Cost} руб";
                          costParagraph.Range.Font.Bold = 1;
                          costParagraph.Format.LineSpacingRule = WdLineSpacing.wdLineSpaceSingle;
                          costParagraph.Format.Alignment = WdParagraphAlignment.wdAlignParagraphRight;
                          costParagraph.Format.SpaceAfter = 0;
                          costParagraph.Range.InsertParagraphAfter();

                          wordApp.Visible = true;

                          wordDoc.PrintPreview();
                      }
                      else MessageBox.Show("Чтобы сформировать акт выполненных работ статус заявки должен быть Готов или Завершен", "Формирование документа", MessageBoxButton.OK, MessageBoxImage.Information);
                  });
            }
        }



        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    
}
