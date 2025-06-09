using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using TMC.Model;
using TMC.View;
using Microsoft.Office.Interop.Word;
using Application = Microsoft.Office.Interop.Word.Application;
using Table = Microsoft.Office.Interop.Word.Table;
using Paragraph = Microsoft.Office.Interop.Word.Paragraph;
using System.Net.Mail;
using System.Net;
using MailMessage = System.Net.Mail.MailMessage;

namespace TMC.ViewModel
{
    public class RequestViewModel : INotifyPropertyChanged
    {
        ServiceCenterTMCEntities context = new ServiceCenterTMCEntities();
        private readonly System.Timers.Timer _refreshTimer;
        private System.Timers.Timer _costTimer;
        private string _currentFilterStatus = "Все"; 
        private ObservableCollection<RequestView> _requests;
        ClientsViewModel _clientVM;
        StoreViewModel _storeVM;
        private string _searchText;
        private ObservableCollection<RequestView> _filteredRequests;
        private List<EditSelectedPartView> _editSelectedParts = new List<EditSelectedPartView>();

        public ObservableCollection<RequestView> RequestsList
        {
            get { return _requests; }
            set
            {
                _requests = value;
                OnPropertyChanged();
            }
        }

        public RequestViewModel(ClientsViewModel clientVM, StoreViewModel storeVM)
        {
            try
            {
                LoadRequests();
                _clientVM = clientVM;
                _storeVM = storeVM;
                _refreshTimer = new System.Timers.Timer(60000); 
                _refreshTimer.Elapsed += (sender, e) => RefreshRequests();
                _refreshTimer.AutoReset = true;
                _refreshTimer.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                OnPropertyChanged();
                FilterRequests();
            }
        }

        private void FilterRequests()
        {
            LoadRequests();
            var filtered = RequestsList;

            if (_currentFilterStatus != "Все")
            {
                filtered = new ObservableCollection<RequestView>(
                    filtered.Where(r => r.StatusName == _currentFilterStatus).ToList());
            }

            if (!string.IsNullOrEmpty(SearchText))
            {
                var searchTextLower = SearchText.ToLowerInvariant().Trim();
                filtered = new ObservableCollection<RequestView>(
                    filtered.Where(r =>
                ($"{r.ClientSurname} {r.ClientName} {r.ClientPatronymic}".ToLowerInvariant().Contains(searchTextLower)) ||
                ($"{r.ClientName} {r.ClientSurname} {r.ClientPatronymic}".ToLowerInvariant().Contains(searchTextLower)) ||
                ($"{r.ClientSurname} {r.ClientPatronymic} {r.ClientName}".ToLowerInvariant().Contains(searchTextLower))||
                ($"{r.EmployeeSurname} {r.EmployeeName} {r.EmployeePatronymic}".ToLowerInvariant().Contains(searchTextLower))||
                ($"{r.EmployeeName} {r.EmployeeSurname} {r.EmployeePatronymic}".ToLowerInvariant().Contains(searchTextLower)) ||
                ($"{r.EmployeeSurname} {r.EmployeePatronymic} {r.EmployeeName}".ToLowerInvariant().Contains(searchTextLower))||
                        (r.ClientSurname != null && r.ClientSurname.ToLowerInvariant().Contains(searchTextLower)) ||
                        (r.ClientName != null && r.ClientName.ToLowerInvariant().Contains(searchTextLower)) ||
                        (r.ClientPatronymic != null && r.ClientPatronymic.ToLowerInvariant().Contains(searchTextLower)) ||
                        (r.EmployeeSurname != null && r.EmployeeSurname.ToLowerInvariant().Contains(searchTextLower)) ||
                        (r.EmployeeName != null && r.EmployeeName.ToLowerInvariant().Contains(searchTextLower)) ||
                        (r.EmployeePatronymic != null && r.EmployeePatronymic.ToLowerInvariant().Contains(searchTextLower)) ||
                        (r.Date != null && r.Date.ToLowerInvariant().Contains(searchTextLower)) ||
                        (r.Reason != null && r.Reason.ToLowerInvariant().Contains(searchTextLower)) ||
                        r.IDRequest.ToString().Contains(searchTextLower) ).ToList());
            }
            RequestsList = filtered;
        }
        private void RefreshRequests()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                LoadRequests();
                FilterRequests();
            });
        }
        private void RefreshCost(RequestWindow requestWindow)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                var request = requestWindow.Requests;
                request.Cost = 0;
                foreach (var service in SelectedServices)
                {
                    request.Cost += service.Cost * service.Count;
                }
                foreach (var part in SelectedParts)
                {
                    request.Cost += part.Cost * part.Count;
                }
                requestWindow.RequestCost.Text = request.Cost.ToString();
            });
        }
        public void LoadRequests()
        {
            try
            {
                var result = (from r in context.Requests
                              join c in context.Clients on r.ClientId equals c.IdClient into clientGroup
                              from c in clientGroup.DefaultIfEmpty() 
                              join e in context.Employees on r.MasterId equals e.IdEmployee into employeeGroup
                              from e in employeeGroup.DefaultIfEmpty() 
                              join s in context.Statuses on r.StatusId equals s.IdStatus 
                              select new RequestView
                              {
                                  IDRequest = r.IdRequest,
                                  EmployeeID = e.IdEmployee, 
                                  EmployeeSurname = e.Surname,
                                  EmployeeName = e.Name,
                                  EmployeePatronymic = e.Patronymic,
                                  EmployeeTelephone = e.Telephone,
                                  StatusID = s.IdStatus,
                                  StatusName = s.Name,
                                  ClientID = c.IdClient, 
                                  ClientSurname = c.Surname,
                                  ClientName = c.Name,
                                  ClientPatronymic = c.Patronymic,
                                  ClientTelephone = c.Telephone,
                                  CompletionDate = r.CompletionDate.ToString(),
                                  Reason = r.Reason,
                                  Date = r.Date.ToString(),
                                  Device = r.Device,
                                  IMEI_SN = r.IMEI_SN
                              }).ToList();
                RequestsList = new ObservableCollection<RequestView>(result.Select(r =>
                {
                    r.StatusColor = ColorStatus(r.StatusName);
                    if (r.CompletionDate != "") r.CompletionDate = (Convert.ToDateTime(r.CompletionDate)).ToShortDateString();
                    if (r.Date != "") r.Date = (Convert.ToDateTime(r.Date)).ToString("dd.MM.yyyy \n HH:mm");
                    return r;
                }).OrderByDescending(n => n.IDRequest));
                string role = App.Current.Properties["Role"] as string;
                int id = (int)App.Current.Properties["UserID"];
                if (role == "Мастер") RequestsList = new ObservableCollection<RequestView>(RequestsList.Where(r => r.EmployeeID == id));

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public RelayCommand SelectRequestByStatus
        {
            get
            {
                return new RelayCommand((status) =>
                  {
                      try
                      {
                          string status_name = status as string;
                          _currentFilterStatus = status_name;
                          RequestsList.Clear();
                          RefreshRequests();                          
                      }
                      catch (Exception ex)
                      {
                          MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                      }

                  });
            }
        }
        public List<MastersListView> MastersList()
        {         
            var result = (from m in context.Employees.Where(e => e.RoleId == 3)
            select new MastersListView
                          {
                              Id = m.IdEmployee,
                              Surname = m.Surname,
                              Name = m.Name,
                              Telephone = m.Telephone,
                              OpenRequests = context.Requests.Where(r=> r.MasterId == m.IdEmployee && r.Statuses.Name!= "Готова" && r.Statuses.Name != "Завершена" && r.Statuses.Name != "Отменена").ToList().Count() 
                          }).ToList();
            return result;          
        }
        public string ColorStatus(string statusName)
        {
            try
            {
                switch (statusName)
                {
                    case "Новая":
                        return "#60B7FF";
                    case "Готова":
                        return "#90EE90";
                    case "В работе":
                        return "#FFD700";
                    case "Завершена":
                        return "#D3D3D3";
                    case "Отменена":
                        return "#D3D3D3";
                    case "Ждет ЗИП":
                        return "#FFA500";
                    case "Диагностика":
                        return "#BDFB82";
                    default:
                        return "#FFFFFF";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }
        public RelayCommand AddRequestCommand
        {
            get
            {
                return new RelayCommand((o) =>
                  {

                      try
                      {
                          SelectedServices.Clear();
                          SelectedParts.Clear();
                          RequestWindow requestWindow = new RequestWindow(new Requests(), this, _clientVM);
                          
                          requestWindow.MastersBox.ItemsSource = MastersList();
                          requestWindow.StatusBox.ItemsSource = context.Statuses.Where(s => s.Name != "Завершен" && s.Name != "Отменен").ToList();
                          requestWindow.EndDocuument.Visibility = Visibility.Collapsed;
                          _costTimer = new System.Timers.Timer(100);
                          _costTimer.Elapsed += (sender, e) => RefreshCost(requestWindow);
                          _costTimer.AutoReset = true;
                          _costTimer.Enabled = true;
                          if (requestWindow.ShowDialog() == true)
                          {
                              Requests newRequest = requestWindow.Requests;
                              Clients client = requestWindow.ClientInfo.DataContext as Clients;

                              if (context.Clients.Any(x => x.IdClient == client.IdClient)) newRequest.ClientId = client.IdClient;
                              else context.Clients.Add(client);
                              
                              var selectedStatus = requestWindow.StatusBox.SelectedItem as Statuses;
                              newRequest.StatusId = selectedStatus.IdStatus;
                              newRequest.Date = DateTime.Now;
                              newRequest.Type = (bool)requestWindow.TypeCheck.IsChecked;
                              newRequest.Cost = (int)requestWindow.Requests.Cost;
                              var selectedMaster = requestWindow.MastersBox.SelectedItem as MastersListView;
                              if (selectedMaster != null) newRequest.MasterId = selectedMaster.Id;
                              if (selectedStatus.Name == "Готова") newRequest.CompletionDate = DateTime.Now;
                              context.Requests.Add(newRequest);
                              context.SaveChanges();
                              foreach (var service in SelectedServices)
                              {
                                  Requests_Services requests_Services = new Requests_Services
                                      {
                                          RequestId = newRequest.IdRequest,
                                          ServiceId = service.IDService,
                                          Count = service.Count 
                                      };
                                      context.Requests_Services.Add(requests_Services);
                                  
                              }
                              foreach (var part in SelectedParts)
                              {
                                  Request_RepairParts request_RepairParts = new Request_RepairParts
                                      {
                                          RequestId = newRequest.IdRequest,
                                          RepairPartId = part.IDPart,
                                          Count = part.Count 
                                      };
                                      context.Request_RepairParts.Add(request_RepairParts);
                                  
                              }
                              _costTimer.Enabled = false;
                              context.SaveChanges();
                              SelectedServices.Clear();
                              SelectedParts.Clear();
                              RequestsList.Clear();
                              RefreshRequests();

                          }
                      }
                      catch (Exception ex)
                      {
                          MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                      }
                });
            }
        }
        public void SendEmail(Clients client, Requests requests, string status)
        {
            try
            {
                MailAddress from = new MailAddress("aliya_iskhakova12@mail.ru", "Сервисный центр ТехноМедиаСоюз");
                MailAddress to = new MailAddress(client.Email);
                MailMessage m = new MailMessage(from, to);
                m.Subject = "Изменение статуса заявки в сервисном центре ТехноМедиаСоюз";

                string htmlBody = $@"
                <div style='font-family: Arial; max-width: 600px; margin: 0 auto; border: 1px solid #DFE4FB; border-radius: 5px; overflow: hidden;'>
                    <div style='background-color: #0A1C6F; padding: 15px; color: white;'>
                        <h2 style='margin: 0;'>Сервисный центр ТехноМедиаСоюз</h2>
                    </div>
            
                    <div style='padding: 20px; background-color: #DFE4FB;'>
                        <h3 style='color: #0E2280;'>Статус заявки №{requests.IdRequest} от {requests.Date.ToString("dd.MM.yyyy")} изменен</h3>
                
                        <div style='background-color: white; padding: 15px; border-radius: 5px; margin: 15px 0; border-left: 4px solid #889DFB;'>
                            <p style='font-weight: bold; color: #162774; margin: 0 0 5px 0;'>Новый статус заявки:</p>
                            <p style='font-size: 18px; color: #0E2280; margin: 0;'>{status}</p>
                        </div>
                
                        <p style='color: #162774;'>Для получения подробной информации о вашей заявке вы можете обратиться в наш сервисный центр.</p>
                
                        <p style='color: #162774;'>Спасибо, что выбрали нас!</p>
                    </div>
            
                    <div style='background-color: #0A1C6F; padding: 10px; color: white; text-align: center; font-size: 12px;'>
                        <p style='margin: 0;'>С уважением, команда ТехноМедиаСоюз</p>
                    </div>
                </div>";

                m.Body = htmlBody;
                m.IsBodyHtml = true;

                SmtpClient smtp = new SmtpClient("smtp.mail.ru", 587);
                smtp.Credentials = new NetworkCredential("aliya_iskhakova12@mail.ru", "HKqzZM2FQTJC3v09cmZd");
                smtp.EnableSsl = true;
                smtp.Send(m);

                MessageBox.Show($"Статус заявки был изменен. Уведомление отправлено клиенту",
                               "Изменение статуса", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch
            {
                MessageBox.Show($"Произошла ошибка при отправке уведомления клиенту. Свяжитесь с клиентом по номеру телефона {client.Telephone}",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
      
        public RelayCommand EditRequestCommand
        {
            get
            {
                return new RelayCommand((selectedItem) =>
                  {
                      try
                      {
                          SelectedServices.Clear();
                          SelectedParts.Clear();
                          string role = App.Current.Properties["Role"] as string;
                          int id = (int)App.Current.Properties["UserID"];
                          List<Statuses> status = new List<Statuses>();

                          RequestView request = selectedItem as RequestView;
                          if (request == null) return;
                          Requests selectedRequest = context.Requests.Find(request.IDRequest);
                          RequestWindow requestWindow = new RequestWindow(selectedRequest, this, _clientVM);
                          _costTimer = new System.Timers.Timer(100); 
                          _costTimer.Elapsed += (sender, e) => RefreshCost(requestWindow);
                          _costTimer.AutoReset = true;
                          _costTimer.Enabled = true;
                          if (role == "Мастер" && request.StatusName != "Завершен" && request.StatusName != "Отменен") status = context.Statuses.Where(s => s.Name != "Завершен" && s.Name != "Отменен").ToList();
                          else status = context.Statuses.ToList();
                          requestWindow.RequestDate.Visibility = Visibility.Visible;
                          requestWindow.ClientInfo.DataContext = context.Clients.Find(request.ClientID);
                          requestWindow.ClientInfo.IsEnabled = false;
                          requestWindow.ClientComboBox.Visibility = Visibility.Collapsed;
                          requestWindow.MastersBox.ItemsSource = MastersList();
                          requestWindow.MastersBox.SelectedItem = (requestWindow.MastersBox.ItemsSource as List<MastersListView>).FirstOrDefault(m=>m.Id==request.EmployeeID);
                          requestWindow.StatusBox.ItemsSource = status;
                          requestWindow.StatusBox.SelectedItem = context.Statuses.Find(request.StatusID);
                          string statusName = request.StatusName;
                          if (selectedRequest.Statuses.Name == "Завершена" || selectedRequest.Statuses.Name == "Отменена")
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
                              requestWindow.PrintBtns.Visibility = Visibility.Collapsed;
                              requestWindow.TypeCheck.IsEnabled = false;
                          }
                          if (role == "Директор")
                          {
                              requestWindow.RequestInfo.IsEnabled = false;
                              requestWindow.MasterInfo.IsEnabled = false;
                              requestWindow.ServiceAndPartsInfo.IsEnabled = false;
                              requestWindow.StatusBox.IsEnabled = false;
                              requestWindow.PrintBtns.Visibility = Visibility.Collapsed;
                              requestWindow.SaveBtn.Visibility = Visibility.Collapsed;
                              requestWindow.TypeCheck.IsEnabled = false;

                          }

                          List<Requests_Services> request_services = context.Requests_Services.Where(r => r.RequestId == selectedRequest.IdRequest).ToList();
                          foreach (var item in request_services)
                          {
                              Services service = context.Services.Find(item.ServiceId);
                              SelectedServicesView serviceView = new SelectedServicesView
                              {
                                  IDService = service.IdService,
                                  Name = service.Name,
                                  Cost = service.Cost,
                                  Count = (int)item.Count
                              };
                              SelectedServices.Add(serviceView);
                          }
                          requestWindow.selectedServices.ItemsSource = SelectedServices;
                          List<Request_RepairParts> request_parts = context.Request_RepairParts.Where(r => r.RequestId == selectedRequest.IdRequest).ToList();
                          
                          foreach (var item in request_parts)
                          {
                              RepairParts part = context.RepairParts.Find(item.RepairPartId);
                              SelectedPartsView partsView = new SelectedPartsView
                              {
                                  IDPart = part.IdPart,
                                  Name = part.Name,
                                  Cost = part.Cost,
                                  Count =item.Count
                              };
                              _editSelectedParts.Add(new EditSelectedPartView { IdPart = part.IdPart, Count = item.Count });
                              SelectedParts.Add(partsView);
                          }
                          requestWindow.selectedServices.ItemsSource = SelectedServices;
                          if (requestWindow.ShowDialog() == true)
                          {
                              var selectedStatus = requestWindow.StatusBox.SelectedItem as Statuses;
                              selectedRequest.StatusId = selectedStatus.IdStatus;

                              //ОТПРАВКА УВЕДОМЛЕНИЯ КЛИЕНТУ
                              if(statusName != selectedStatus.Name && selectedRequest.Clients.Email!=null)
                              {
                                  SendEmail(selectedRequest.Clients, selectedRequest, selectedStatus.Name);
                              }
                              if (selectedStatus.Name == "Готова") selectedRequest.CompletionDate = DateTime.Now;
                              var selectedMaster = requestWindow.MastersBox.SelectedItem as MastersListView;
                              if (selectedMaster != null) selectedRequest.MasterId = selectedMaster.Id;
                              selectedRequest.Type = (bool)requestWindow.TypeCheck.IsChecked;
                              selectedRequest.Cost = (int)requestWindow.Requests.Cost;
                              context.Requests.AddOrUpdate(selectedRequest);
                              context.SaveChanges();
                              foreach (var item in request_services)
                              {
                                  context.Requests_Services.Remove(item);
                              }
                              foreach (var service in SelectedServices)
                              {
                   
                                  Requests_Services requests_Services = new Requests_Services
                                  {
                                      RequestId = selectedRequest.IdRequest,
                                      ServiceId = service.IDService,
                                      Count = service.Count 
                                  };
                                  context.Requests_Services.Add(requests_Services);
                                  
                              }
                              foreach (var item in request_parts)
                              {
                                  context.Request_RepairParts.Remove(item);
                              }
                              foreach (var part in SelectedParts)
                              {
                                  Request_RepairParts request_RepairParts = new Request_RepairParts
                                  {
                                      RequestId = selectedRequest.IdRequest,
                                      RepairPartId = part.IDPart,
                                      Count = part.Count
                                  };
                                  context.Request_RepairParts.Add(request_RepairParts);

                              }
                              _costTimer.Enabled = false;
                              context.SaveChanges();
                              SelectedServices.Clear();
                              SelectedParts.Clear();
                              _editSelectedParts.Clear();
                          }
                          _storeVM.LoadParts();
                          _storeVM.FilterParts();
                          RefreshRequests();
                      }
                      catch (Exception ex)
                      {
                          MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                      }
                  });
            }
        }
    
        private ObservableCollection<SelectedServicesView> _selectedServices = new ObservableCollection<SelectedServicesView>();
        public ObservableCollection<SelectedServicesView> SelectedServices
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
                return new RelayCommand((o) =>
                  {
                      try
                      {
                          AddServicesWindow servicesWindow = new AddServicesWindow();
                          var vm = servicesWindow.DataContext as ServicesViewModel;
                          RequestWindow requestWindow = o as RequestWindow;
                          if (servicesWindow.ShowDialog() == true)
                          {
                              // Добавляем выбранные услуги к заявке
                              foreach (var service in vm.SelectedServices)
                              {
                                  bool exists = SelectedServices.Any(rs => rs.IDService == service.IdService);
                                  if (!exists)
                                  {
                                      SelectedServicesView serviceView = new SelectedServicesView
                                      {
                                          IDService = service.IdService,
                                          Name = service.Name,
                                          Cost = service.Cost,
                                          Count = 1
                                      };
                                      SelectedServices.Add(serviceView);
                                      requestWindow.Requests.Cost = (int)(requestWindow.Requests.Cost + service.Cost);
                                  }
                              }
                          }
                      }
                      catch (Exception ex)
                      {
                          MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
                      try
                      {
                          RequestWindow requestWindow = o as RequestWindow;
                          SelectedServicesView services = requestWindow.selectedServices.SelectedItem as SelectedServicesView;
                          if (services != null)
                          {
                              SelectedServices.Remove(services);
                              requestWindow.Requests.Cost = (int)(requestWindow.Requests.Cost - services.Cost*services.Count);
                          }
                          else MessageBox.Show("Если хотите удалить услугу из заявки, выберите услугу для удаления", "Формирование заявки", MessageBoxButton.OK, MessageBoxImage.Information);

                      }
                      catch (Exception ex)
                      {
                          MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                      }
                  });
            }
        }

        private ObservableCollection<SelectedPartsView> _selectedParts = new ObservableCollection<SelectedPartsView>();
        public ObservableCollection<SelectedPartsView> SelectedParts
        {
            get { return _selectedParts; }
            set
            {
                _selectedParts = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand UpdateListCommand
        {
            get
            {
                return new RelayCommand((o) =>
                {
                    try
                    {
                        RefreshRequests();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                });
            }
        }
        public RelayCommand AddPartsCommand
        {
            get
            {
                return new RelayCommand((o) =>
                  {
                      try
                      {
                          AddPartsWindow partsWindow = new AddPartsWindow(_storeVM);
                          var vm = partsWindow.DataContext as StoreViewModel;
                          RequestWindow requestWindow = o as RequestWindow;

                          if (partsWindow.ShowDialog() == true)
                          {
                              foreach (var part in vm.SelectedParts)
                              {
                                  bool exists = SelectedParts.Any(rs => rs.IDPart == part.IdPart);
                                  if (!exists)
                                  {
                                      SelectedPartsView partsView = new SelectedPartsView
                                      {
                                          IDPart = part.IdPart,
                                          Name = part.Name,
                                          Cost = part.Cost,
                                          Count = 1
                                      };
                                      SelectedParts.Add(partsView);
                                      requestWindow.Requests.Cost = (int)(requestWindow.Requests.Cost + part.Cost);
                                  }

                              }
                          }
                      }
                      catch (Exception ex)
                      {
                          MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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

                    try
                    {
                        RequestWindow requestWindow = o as RequestWindow;
                        SelectedPartsView parts = requestWindow.selectedParts.SelectedItem as SelectedPartsView;
                        if (parts != null)
                        {
                            SelectedParts.Remove(parts);
                            requestWindow.Requests.Cost = (int)(requestWindow.Requests.Cost - parts.Cost * parts.Count); ;
                        }
                        else MessageBox.Show("Если хотите удалить ЗИП из заявки, выберите ЗИП для удаления", "Формирование заявки", MessageBoxButton.OK, MessageBoxImage.Information);

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }               
                });
            }
        }

        public RelayCommand PrintRepairActCommand
        {
            get
            {
                return new RelayCommand(async (o) =>
                {

                    try
                    {
                        RequestWindow requestWindow = o as RequestWindow;
                        if (!(!string.IsNullOrWhiteSpace(requestWindow.RequestReason.Text) && !(requestWindow.ClientInfo.DataContext as Clients).HasValidationErrors()
                        && int.TryParse(requestWindow.RequestCost.Text, out int cost) && cost >= 0))
                        {
                            MessageBox.Show("Чтобы сформировать акт приемки заполните обязательные поля корректными данными!", "Формирование документа", MessageBoxButton.OK, MessageBoxImage.Information);
                            return;
                        }
                        if (string.IsNullOrWhiteSpace(requestWindow.RequestDevice.Text) || string.IsNullOrWhiteSpace(requestWindow.RequestSN.Text))
                        {
                           MessageBox.Show("Чтобы сформировать акт приемки заполните поля устройства", "Формирование документа", MessageBoxButton.OK, MessageBoxImage.Information);
                            return;
                        }
                        var request = requestWindow.Requests;
                        var client = requestWindow.ClientInfo.DataContext as Clients;
                        MessageBox.Show("Ожидайте, документ формируется", "Формирование документа", MessageBoxButton.OK, MessageBoxImage.Information);

                        await System.Threading.Tasks.Task.Run(() =>
                        {
                            Application wordApp = new Application();
                            Document wordDoc = wordApp.Documents.Add();

                            wordDoc.Content.ParagraphFormat.SpaceAfter = 0;
                            wordDoc.Content.ParagraphFormat.SpaceBefore = 0;
                            wordDoc.Content.Font.Name = "Times New Roman";
                            wordDoc.Content.Font.Size = 13;

                            Paragraph name = wordDoc.Content.Paragraphs.Add();
                            name.Range.Text = "Сервисный центр ТехноМедиаСоюз";
                            name.Range.Font.Size = 13;
                            name.Range.Font.Bold = 1;
                            name.Format.LineSpacingRule = WdLineSpacing.wdLineSpaceSingle;
                            name.Format.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
                            name.Range.InsertParagraphAfter();

                            Paragraph descriptionParagraph1 = wordDoc.Content.Paragraphs.Add();
                            descriptionParagraph1.Range.Text = "ИП Сулейманов М.Р., г. Арск Советская площадь 22, тел. 8(443) 248-92-60.";
                            descriptionParagraph1.Range.Font.Bold = 0;
                            descriptionParagraph1.Format.LineSpacingRule = WdLineSpacing.wdLineSpaceSingle;
                            descriptionParagraph1.Format.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
                            descriptionParagraph1.Range.InsertParagraphAfter();

                            Paragraph workHours = wordDoc.Content.Paragraphs.Add();
                            workHours.Range.Text = "Время работы с 9.00 до 18.00 (понедельник-пятница), без перерывов";
                            workHours.Range.Font.Bold = 0;
                            workHours.Format.LineSpacingRule = WdLineSpacing.wdLineSpaceSingle;
                            workHours.Format.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
                            workHours.Range.InsertParagraphAfter();
                            workHours.Range.InsertParagraphAfter();

                            // Таблица с информацией 
                            Paragraph titleParagraph = wordDoc.Content.Paragraphs.Add();
                            titleParagraph.Range.Text = $"Акт о приеме на ремонт №{request.IdRequest}";
                            titleParagraph.Range.Font.Size = 13;
                            titleParagraph.Range.Font.Bold = 1;
                            titleParagraph.Format.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
                            titleParagraph.Range.InsertParagraphAfter();

                            Table table = wordDoc.Tables.Add(wordDoc.Content.Paragraphs.Add().Range, 5, 2);
                            table.Borders.Enable = 1;
                            table.Range.Bold = 0;
                            table.Range.ParagraphFormat.LineSpacingRule = WdLineSpacing.wdLineSpaceSingle;

                            table.Columns[1].PreferredWidth = wordApp.CentimetersToPoints(5);
                            table.Columns[2].PreferredWidth = wordApp.CentimetersToPoints(12);

                            table.Cell(1, 1).Range.Text = "Клиент";
                            table.Cell(2, 1).Range.Text = "Устройство";
                            table.Cell(3, 1).Range.Text = "Серийный номер";
                            table.Cell(4, 1).Range.Text = "Проблема со слов клиента";
                            table.Cell(5, 1).Range.Text = "Примечание";

                            table.Cell(1, 2).Range.Text = $"{client.Surname} {client.Name} {client.Patronymic}";
                            table.Cell(2, 2).Range.Text = $"{request.Device}";
                            table.Cell(3, 2).Range.Text = $"{request.IMEI_SN}";
                            table.Cell(4, 2).Range.Text = $"{request.Reason}";
                            table.Cell(5, 2).Range.Text = $"{request.Notes}";

                            for (int i = 1; i <= 5; i++)
                            {
                                table.Cell(i, 1).Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphRight;
                                table.Cell(i, 1).Range.Font.Bold = 1;
                                table.Cell(i, 2).Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphLeft;
                            }

                            // Описание условий
                            Paragraph descriptionParagraph = wordDoc.Content.Paragraphs.Add();
                            descriptionParagraph.Range.Text = "Клиент согласен, что все неисправности и внутренние повреждения, которые " +
                                "могут быть обнаружены в оборудовании при техническом обслуживании, " +
                                "возникли до приема оборудования по данной квитанции. В случае утери акта " +
                                "о приеме оборудования на ремонт выдача аппарата производится при " +
                                "предъявлении паспорта лица сдававшего аппарат и письменного заявления. " +
                                "Внимание: Срок ремонта аппарата 21 день, максимальный срок при " +
                                "отсутствии запчастей на складе поставщика может быть увеличен до 45 " +
                                "дней. Заказчик согласен на обработку персональных данных, а также несет " +
                                "ответственность за достоверность предоставленной информации. С " +
                                "комплектацией, описанием неисправностей и повреждений, условиями " +
                                "хранения и обслуживания оборудования ознакомлен и согласен.";
                            descriptionParagraph.Range.Font.Size = 9;
                            descriptionParagraph.Format.LineSpacingRule = WdLineSpacing.wdLineSpaceSingle;
                            descriptionParagraph.Range.Font.Bold = 0;
                            descriptionParagraph.Format.Alignment = WdParagraphAlignment.wdAlignParagraphJustify;
                            descriptionParagraph.Range.InsertParagraphAfter();
                            descriptionParagraph.Range.InsertParagraphAfter();

                            // Подписи
                            Table signatureTable = wordDoc.Tables.Add(wordDoc.Content.Paragraphs.Add().Range, 2, 2);
                            signatureTable.Borders.Enable = 0;
                            signatureTable.Range.Font.Size = 13;
                            signatureTable.Cell(1, 1).Range.Text = $"Оборудование в ремонт сдал: {client.Surname} {client.Name[0]}.{client.Patronymic[0]}.";
                            signatureTable.Cell(1, 2).Range.Text = "____________";
                            var receiver = context.Employees.Find((int)App.Current.Properties["UserID"]);
                            signatureTable.Cell(2, 1).Range.Text = $"Оборудование в ремонт принял: инженер приемщик {receiver.Surname} {receiver.Name[0]}.{receiver.Patronymic[0]}.";
                            signatureTable.Cell(2, 2).Range.Text = "____________";

                            signatureTable.Columns[1].PreferredWidth = wordApp.CentimetersToPoints(14);
                            signatureTable.Columns[2].PreferredWidth = wordApp.CentimetersToPoints(5);

                            signatureTable.Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphLeft;

                            wordApp.Visible = true;
                            wordDoc.PrintPreview();
                        });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                });
            }
        }

        public RelayCommand PrintDiagnosticActCommand
        {
            get
            {
                return new RelayCommand(async (o) =>
                {
                    try
                    {
                        RequestWindow requestWindow = o as RequestWindow;
                        var request = requestWindow.Requests;
                        var client = requestWindow.ClientInfo.DataContext as Clients;
                        var status = requestWindow.StatusBox.SelectedItem as Statuses;
                        if (status.Name == "Диагностика")
                        {
                            if (!string.IsNullOrWhiteSpace(requestWindow.RequestDetectedMulfunction.Text))
                            {
                                MessageBox.Show("Ожидайте, документ формируется", "Формирование документа", MessageBoxButton.OK, MessageBoxImage.Information);
                                await System.Threading.Tasks.Task.Run(() =>
                                {
                                    Application wordApp = new Application();
                                    Document wordDoc = wordApp.Documents.Add();

                                    wordDoc.Content.ParagraphFormat.SpaceAfter = 0;
                                    wordDoc.Content.ParagraphFormat.SpaceBefore = 0;
                                    wordDoc.Content.Font.Name = "Times New Roman";
                                    wordDoc.Content.Font.Size = 12;

                                    Paragraph name = wordDoc.Content.Paragraphs.Add();
                                    name.Range.Text = "Сервисный центр ТехноМедиаСоюз";
                                    name.Range.Font.Size = 13;
                                    name.Range.Font.Bold = 1;
                                    name.Format.LineSpacingRule = WdLineSpacing.wdLineSpaceSingle;
                                    name.Format.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
                                    name.Range.InsertParagraphAfter();

                                    Paragraph address = wordDoc.Content.Paragraphs.Add();
                                    address.Range.Text = "ИП Сулейманов М.Р., г. Арск Советская площадь 22, тел. 8(443) 248-92-60.";
                                    address.Range.Font.Bold = 0;
                                    address.Format.LineSpacingRule = WdLineSpacing.wdLineSpaceSingle;
                                    address.Format.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
                                    address.Range.InsertParagraphAfter();

                                    Paragraph workHours = wordDoc.Content.Paragraphs.Add();
                                    workHours.Range.Text = "Время работы с 9.00 до 18.00 (понедельник-пятница), без перерывов";
                                    workHours.Range.Font.Bold = 0;
                                    workHours.Format.LineSpacingRule = WdLineSpacing.wdLineSpaceSingle;
                                    workHours.Format.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
                                    workHours.Range.InsertParagraphAfter(); 
                                    workHours.Range.InsertParagraphAfter();

                                    // Заголовок
                                    Paragraph titleParagraph = wordDoc.Content.Paragraphs.Add();
                                    titleParagraph.Range.Text = $"Акт диагностики №{request.IdRequest} от {DateTime.Now.ToString("dd.MM.yyyy")}";
                                    titleParagraph.Range.Font.Size = 13;
                                    titleParagraph.Range.Font.Bold = 1;
                                    titleParagraph.Format.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
                                    titleParagraph.Range.InsertParagraphAfter();

                                    // Информация об устройстве и выявленных неисправностях
                                    Paragraph detected = wordDoc.Content.Paragraphs.Add();
                                    detected.Range.Text = $"При осмотре устройства: {request.Device} выявлены дефекты: {request.DetectedMulfunction}.";
                                    detected.Range.Font.Bold = 0;
                                    detected.Format.LineSpacingRule = WdLineSpacing.wdLineSpaceSingle;
                                    detected.Format.Alignment = WdParagraphAlignment.wdAlignParagraphLeft;
                                    detected.Range.InsertParagraphAfter();

                                    Paragraph information = wordDoc.Content.Paragraphs.Add();
                                    information.Range.Text = "Для устранения выявленных дефектов необходимы следующие запасные части и работы.";
                                    information.Range.Font.Bold = 0;
                                    information.Format.LineSpacingRule = WdLineSpacing.wdLineSpaceSingle;
                                    information.Format.Alignment = WdParagraphAlignment.wdAlignParagraphLeft;
                                    information.Range.InsertParagraphAfter();
                                    information.Range.InsertParagraphAfter();

                                    // Таблица с необходимыми услугами
                                    Paragraph complitedWork = wordDoc.Content.Paragraphs.Add();
                                    complitedWork.Range.Text = "Необходимые работы";
                                    complitedWork.Range.Font.Size = 13;
                                    complitedWork.Range.Font.Bold = 1;
                                    complitedWork.Format.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
                                    complitedWork.Range.InsertParagraphAfter();

                                    Table servicesTable = wordDoc.Tables.Add(wordDoc.Content.Paragraphs.Add().Range, SelectedServices.Count + 1, 2);
                                    servicesTable.Borders.Enable = 1;
                                    servicesTable.Range.Font.Size = 13;
                                    servicesTable.Range.Font.Bold = 0;
                                    servicesTable.Range.ParagraphFormat.LineSpacingRule = WdLineSpacing.wdLineSpaceSingle;

                                    servicesTable.Columns[1].PreferredWidth = wordApp.CentimetersToPoints(13);
                                    servicesTable.Columns[2].PreferredWidth = wordApp.CentimetersToPoints(3);

                                    servicesTable.Cell(1, 1).Range.Text = "Наименование";
                                    servicesTable.Cell(1, 2).Range.Text = "Кол-во";
                                    servicesTable.Cell(1, 1).Range.Font.Bold = 0;
                                    servicesTable.Cell(1, 2).Range.Font.Bold = 0;

                                    for (int i = 0; i < SelectedServices.Count; i++)
                                    {
                                        servicesTable.Cell(i + 2, 1).Range.Text = SelectedServices[i].Name;
                                        servicesTable.Cell(i + 2, 2).Range.Text = SelectedServices[i].Count.ToString();
                                        servicesTable.Cell(i + 2, 1).Range.Font.Bold = 0;
                                        servicesTable.Cell(i + 2, 2).Range.Font.Bold = 0;
                                    }

                                    servicesTable.Range.InsertParagraphAfter();

                                    // Таблица с необходимыми ЗИП
                                    Paragraph needZIP = wordDoc.Content.Paragraphs.Add();
                                    needZIP.Range.Text = "Необходимые запасные части и принадлежности";
                                    needZIP.Range.Font.Size = 13;
                                    needZIP.Range.Font.Bold = 1;
                                    needZIP.Format.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
                                    needZIP.Range.InsertParagraphAfter();

                                    Table partsTable = wordDoc.Tables.Add(wordDoc.Content.Paragraphs.Add().Range, SelectedParts.Count + 1, 2);
                                    partsTable.Borders.Enable = 1;
                                    partsTable.Range.Font.Size = 13;
                                    partsTable.Range.Font.Bold = 0;
                                    partsTable.Range.ParagraphFormat.LineSpacingRule = WdLineSpacing.wdLineSpaceSingle;

                                    partsTable.Columns[1].PreferredWidth = wordApp.CentimetersToPoints(13);
                                    partsTable.Columns[2].PreferredWidth = wordApp.CentimetersToPoints(3);

                                    partsTable.Cell(1, 1).Range.Text = "Наименование";
                                    partsTable.Cell(1, 2).Range.Text = "Кол-во";
                                    partsTable.Cell(1, 1).Range.Font.Bold = 0;
                                    partsTable.Cell(1, 2).Range.Font.Bold = 0;

                                    for (int i = 0; i < SelectedParts.Count; i++)
                                    {
                                        partsTable.Cell(i + 2, 1).Range.Text = SelectedParts[i].Name;
                                        partsTable.Cell(i + 2, 2).Range.Text = SelectedParts[i].Count.ToString();
                                        partsTable.Cell(i + 2, 1).Range.Font.Bold = 0;
                                        partsTable.Cell(i + 2, 2).Range.Font.Bold = 0;
                                    }

                                    partsTable.Range.InsertParagraphAfter();

                                    // Подпись инженера-приёмщика
                                    Paragraph engineer = wordDoc.Content.Paragraphs.Add();
                                    var receiver = context.Employees.Find((int)App.Current.Properties["UserID"]);
                                    engineer.Range.Text = $"Инженер-приёмщик {receiver.Surname} {receiver.Name[0]}.{receiver.Patronymic[0]}.  ________________";
                                    engineer.Range.Font.Bold = 0;
                                    engineer.Format.LineSpacingRule = WdLineSpacing.wdLineSpaceSingle;
                                    engineer.Format.Alignment = WdParagraphAlignment.wdAlignParagraphLeft;
                                    engineer.Range.InsertParagraphAfter();

                                    wordApp.Visible = true;
                                    wordDoc.PrintPreview();
                                }); 
                            }
                            else MessageBox.Show("Чтобы сформировать акт диагностики заполните поле Выявленные неисправности", "Формирование документа", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else MessageBox.Show("Чтобы сформировать акт диагностики статус заявки должен быть Диагностика", "Формирование документа", MessageBoxButton.OK, MessageBoxImage.Information);

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                });
            }
        }


        public RelayCommand PrintComplitedWorkActCommand
        {
            get
            {
                return  new RelayCommand(async (o) =>
                  {
                      try
                      {
                          RequestWindow requestWindow = o as RequestWindow;
                          var request = requestWindow.Requests;
                          var client = requestWindow.ClientInfo.DataContext as Clients;
                          var status = requestWindow.StatusBox.SelectedItem as Statuses;
                          if (status.Name == "Готова" || status.Name == "Завершена")
                          {
                              MessageBox.Show("Ожидайте, документ формируется", "Формирование документа", MessageBoxButton.OK, MessageBoxImage.Information);
                              await System.Threading.Tasks.Task.Run(() =>
                              {
                                  Application wordApp = new Application();
                                  Document wordDoc = wordApp.Documents.Add();

                                  wordDoc.Content.ParagraphFormat.SpaceAfter = 0;
                                  wordDoc.Content.ParagraphFormat.SpaceBefore = 0;
                                  wordDoc.Content.Font.Name = "Times New Roman";
                                  wordDoc.Content.Font.Size = 12;

                                  Paragraph name = wordDoc.Content.Paragraphs.Add();
                                  name.Range.Text = "Сервисный центр ТехноМедиаСоюз";
                                  name.Range.Font.Size = 13;
                                  name.Range.Font.Bold = 1;
                                  name.Format.LineSpacingRule = WdLineSpacing.wdLineSpaceSingle;
                                  name.Format.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
                                  name.Range.InsertParagraphAfter();

                                  Paragraph address = wordDoc.Content.Paragraphs.Add();
                                  address.Range.Text = "ИП Сулейманов М.Р., г. Арск Советская площадь 22, тел. 8(443) 248-92-60.";
                                  address.Range.Font.Bold = 0;
                                  address.Format.LineSpacingRule = WdLineSpacing.wdLineSpaceSingle;
                                  address.Format.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
                                  address.Range.InsertParagraphAfter();

                                  Paragraph workHours = wordDoc.Content.Paragraphs.Add();
                                  workHours.Range.Text = "Время работы с 9.00 до 18.00 (понедельник-пятница), без перерывов";
                                  workHours.Range.Font.Bold = 0;
                                  workHours.Format.LineSpacingRule = WdLineSpacing.wdLineSpaceSingle;
                                  workHours.Format.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
                                  workHours.Range.InsertParagraphAfter();
                                  workHours.Range.InsertParagraphAfter();

                                  Paragraph titleParagraph = wordDoc.Content.Paragraphs.Add();
                                  titleParagraph.Range.Text = $"Акт о выполненных работ №{request.IdRequest} от {DateTime.Now.ToString("dd.MM.yyyy")}";
                                  titleParagraph.Range.Font.Size = 13;
                                  titleParagraph.Range.Font.Bold = 1;
                                  titleParagraph.Format.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
                                  titleParagraph.Range.InsertParagraphAfter();

                                  // Информация о клиенте и устройстве
                                  Table infoTable = wordDoc.Tables.Add(wordDoc.Content.Paragraphs.Add().Range, 6, 2);
                                  infoTable.Borders.Enable = 1;
                                  infoTable.Range.Font.Size = 12;
                                  infoTable.Range.ParagraphFormat.LineSpacingRule = WdLineSpacing.wdLineSpaceSingle;

                                  infoTable.Columns[1].PreferredWidth = wordApp.CentimetersToPoints(5);
                                  infoTable.Columns[2].PreferredWidth = wordApp.CentimetersToPoints(12);

                                  infoTable.Cell(1, 1).Range.Text = "Номер заказа";
                                  infoTable.Cell(2, 1).Range.Text = "ФИО клиента";
                                  infoTable.Cell(3, 1).Range.Text = "Телефон клиента";
                                  infoTable.Cell(4, 1).Range.Text = "Устройство";
                                  infoTable.Cell(5, 1).Range.Text = "Серийный номер";
                                  infoTable.Cell(6, 1).Range.Text = "Дата выдачи";

                                  infoTable.Cell(1, 2).Range.Text = $"{request.IdRequest}";
                                  infoTable.Cell(2, 2).Range.Text = $"{client.Surname} {client.Name} {client.Patronymic}";
                                  infoTable.Cell(3, 2).Range.Text = $"{client.Telephone}";
                                  infoTable.Cell(4, 2).Range.Text = $"{request.Device}";
                                  infoTable.Cell(5, 2).Range.Text = $"{request.IMEI_SN}";
                                  infoTable.Cell(6, 2).Range.Text = $"{DateTime.Now.ToString("dd.MM.yyyy")}";

                                  for (int i = 1; i <= 6; i++)
                                  {
                                      infoTable.Cell(i, 1).Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphRight;
                                      infoTable.Cell(i, 1).Range.Font.Bold = 1;
                                      infoTable.Cell(i, 2).Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphLeft;
                                      infoTable.Cell(i, 2).Range.Font.Bold = 0;
                                  }

                                  infoTable.Range.InsertParagraphAfter();
                                  infoTable.Range.InsertParagraphAfter();

                                  // Таблица выполненные работы
                                  Paragraph completedWork = wordDoc.Content.Paragraphs.Add();
                                  completedWork.Range.Text = "Выполненные работы";
                                  completedWork.Range.Font.Size = 13;
                                  completedWork.Range.Font.Bold = 1;
                                  completedWork.Format.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
                                  completedWork.Range.InsertParagraphAfter();

                                  Table servicesTable = wordDoc.Tables.Add(wordDoc.Content.Paragraphs.Add().Range, SelectedServices.Count + 1, 3);
                                  servicesTable.Borders.Enable = 1;
                                  servicesTable.Range.Font.Size = 11;
                                  servicesTable.Range.ParagraphFormat.LineSpacingRule = WdLineSpacing.wdLineSpaceSingle;

                                  servicesTable.Columns[1].PreferredWidth = wordApp.CentimetersToPoints(11);
                                  servicesTable.Columns[2].PreferredWidth = wordApp.CentimetersToPoints(3);
                                  servicesTable.Columns[3].PreferredWidth = wordApp.CentimetersToPoints(3);

                                  servicesTable.Cell(1, 1).Range.Text = "Наименование";
                                  servicesTable.Cell(1, 2).Range.Text = "Кол-во";
                                  servicesTable.Cell(1, 3).Range.Text = "Цена, руб.";
                                  servicesTable.Rows[1].Range.Font.Bold = 0;

                                  double totalCost = 0;
                                  for (int i = 0; i < SelectedServices.Count; i++)
                                  {
                                      servicesTable.Cell(i + 2, 1).Range.Text = SelectedServices[i].Name;
                                      servicesTable.Cell(i + 2, 2).Range.Text = SelectedServices[i].Count.ToString();
                                      servicesTable.Cell(i + 2, 3).Range.Text = SelectedServices[i].Cost.ToString();
                                      totalCost += SelectedServices[i].Cost;
                                      servicesTable.Cell(i + 2, 1).Range.Font.Bold = 0;
                                      servicesTable.Cell(i + 2, 2).Range.Font.Bold = 0;
                                      servicesTable.Cell(i + 2, 3).Range.Font.Bold = 0;
                                  }

                                  servicesTable.Range.InsertParagraphAfter();
                                  // Таблица с использованными ЗИП
                                  if (SelectedParts.Count > 0)
                                  {
                                      Paragraph usedParts = wordDoc.Content.Paragraphs.Add();
                                      usedParts.Range.Text = "Использованные запасные части и принадлежности";
                                      usedParts.Range.Font.Size = 13;
                                      usedParts.Range.Font.Bold = 1;
                                      usedParts.Format.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
                                      usedParts.Range.InsertParagraphAfter();

                                      Table partsTable = wordDoc.Tables.Add(wordDoc.Content.Paragraphs.Add().Range, SelectedParts.Count + 1, 3);
                                      partsTable.Borders.Enable = 1;
                                      partsTable.Range.Font.Size = 11;
                                      partsTable.Range.ParagraphFormat.LineSpacingRule = WdLineSpacing.wdLineSpaceSingle;

                                      partsTable.Columns[1].PreferredWidth = wordApp.CentimetersToPoints(11);
                                      partsTable.Columns[2].PreferredWidth = wordApp.CentimetersToPoints(3);
                                      partsTable.Columns[3].PreferredWidth = wordApp.CentimetersToPoints(3);

                                      partsTable.Cell(1, 1).Range.Text = "Наименование";
                                      partsTable.Cell(1, 2).Range.Text = "Кол-во";
                                      partsTable.Cell(1, 3).Range.Text = "Цена, руб.";
                                      partsTable.Rows[1].Range.Font.Bold = 0;

                                      for (int i = 0; i < SelectedParts.Count; i++)
                                      {
                                          partsTable.Cell(i + 2, 1).Range.Text = SelectedParts[i].Name;
                                          partsTable.Cell(i + 2, 2).Range.Text = SelectedParts[i].Count.ToString();
                                          partsTable.Cell(i + 2, 3).Range.Text = SelectedParts[i].Cost.ToString();
                                          totalCost += SelectedParts[i].Cost;
                                          partsTable.Cell(i + 2, 1).Range.Font.Bold = 0;
                                          partsTable.Cell(i + 2, 2).Range.Font.Bold = 0; 
                                          partsTable.Cell(i + 2, 3).Range.Font.Bold = 0;
                                      }

                                      partsTable.Range.InsertParagraphAfter();
                                  }
                                  // Итоговая стоимость
                                  Paragraph totalParagraph = wordDoc.Content.Paragraphs.Add();
                                  totalParagraph.Range.Text = $"ИТОГ: {totalCost} руб";
                                  totalParagraph.Range.Font.Bold = 1;
                                  totalParagraph.Format.Alignment = WdParagraphAlignment.wdAlignParagraphRight;
                                  totalParagraph.Range.InsertParagraphAfter();
                                  totalParagraph.Range.InsertParagraphAfter();

                                  // Подписи
                                  Paragraph customerConfirm = wordDoc.Content.Paragraphs.Add();
                                  customerConfirm.Range.Text = "Заказчик (подтверждаю, что работа была выполнена в полном объеме, претензий не имею)";
                                  customerConfirm.Range.Font.Bold = 0;
                                  customerConfirm.Format.Alignment = WdParagraphAlignment.wdAlignParagraphLeft;
                                  customerConfirm.Range.Borders[WdBorderType.wdBorderTop].LineStyle = WdLineStyle.wdLineStyleSingle;
                                  customerConfirm.Range.Borders[WdBorderType.wdBorderTop].LineWidth = WdLineWidth.wdLineWidth050pt;
                                  customerConfirm.Range.InsertParagraphAfter();
                                  customerConfirm.Range.InsertParagraphAfter();

                                  Paragraph performerSignText = wordDoc.Content.Paragraphs.Add();
                                  performerSignText.Range.Text = "Подпись исполнителя";
                                  performerSignText.Range.Font.Bold = 0;
                                  performerSignText.Format.Alignment = WdParagraphAlignment.wdAlignParagraphLeft;
                                  performerSignText.Range.Borders[WdBorderType.wdBorderTop].LineStyle = WdLineStyle.wdLineStyleSingle;
                                  performerSignText.Range.Borders[WdBorderType.wdBorderTop].LineWidth = WdLineWidth.wdLineWidth075pt;
                                  performerSignText.Range.InsertParagraphAfter();

                                  wordApp.Visible = true;
                                  wordDoc.PrintPreview();
                              });
                          }
                          else MessageBox.Show("Чтобы сформировать акт выполненных работ статус заявки должен быть Готов или Завершен", "Формирование документа", MessageBoxButton.OK, MessageBoxImage.Information);

                      }
                      catch (Exception ex)
                      {
                          MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                      }
                  });
            }
        }

        public bool CheckSelectedParts()
        {
            try
            {
                bool result = true;
                int addCount = 0;
                foreach (var part in SelectedParts)
                {
                    RepairParts selectedPart = context.RepairParts.Find(part.IDPart);
                    addCount = part.Count;
                    var editPart = _editSelectedParts.Where(p=> p.IdPart == part.IDPart).FirstOrDefault();
                    
                    if (editPart != null)
                    {
                        if (part.Count > editPart.Count) addCount = part.Count - editPart.Count;
                        if (part.Count == editPart.Count) addCount = 0;
                        if (part.Count < editPart.Count) result = true;
                    }
                    if (selectedPart.Count < addCount)
                    {
                        MessageBox.Show($"Недостаточно ЗИП \"{part.Name}\" на складе. \n На складе доступно: {selectedPart.Count} шт.", "Склад ЗИП", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return false;
                    }
                    result = true;
                }
                return result;
            }
            catch
            {
                return false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    
}
