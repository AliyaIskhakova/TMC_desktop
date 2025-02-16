using Microsoft.Office.Interop.Word;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TMC.Model;
using TMC.View;
using Application = Microsoft.Office.Interop.Word.Application;

namespace TMC.ViewModel
{
    public class ServicesViewModel: INotifyPropertyChanged
    {
        ServiceCenterTMCEntities context = new ServiceCenterTMCEntities();
        ObservableCollection<Services> _services;
        string _searchText;
        RelayCommand? addCommand;
        RelayCommand? editCommand;
        RelayCommand? printCommand;
        ObservableCollection<Services> _filteredServices;

        public ServicesViewModel()
        {
            // Инициализация данных
            _services = new ObservableCollection<Services>(context.Services.ToList());
            _filteredServices = new ObservableCollection<Services>(_services);
        }



        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                OnPropertyChanged();
                FilterServices();
            }
        }

        public ObservableCollection<Services> ServicesList
        {
            get { return _filteredServices; }
            set
            {
                _filteredServices = value;
                OnPropertyChanged();
            }
        }

        private void FilterServices()
        {
            if (string.IsNullOrEmpty(_searchText))
            {
                ServicesList = new ObservableCollection<Services>(_services);
            }
            else
            {
                var filtered = _services.Where(e => e.Name.ToLowerInvariant().StartsWith(_searchText.ToLowerInvariant().Trim()));
                ServicesList = new ObservableCollection<Services>(filtered);
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

        public RelayCommand AddSelectedServicesCommand
        {
            get
            {
                return new RelayCommand((o) =>
                {
                    AddServicesWindow window = o as AddServicesWindow;
                    var selectedItems = window.ServicesDG.SelectedItems.Cast<Services>().ToList();
                    foreach (var item in selectedItems)
                    {
                        SelectedServices.Add(item);
                    }
                    // Закрываем окно после добавления услуг
                    (o as System.Windows.Window).DialogResult = true;
                });
            }
        }

        public RelayCommand PrintPriceListCommand
        {
            get
            {
                return printCommand ??
                  (printCommand = new RelayCommand((o) =>
                  {
                      MessageBox.Show("Ожидайте, документ формируется", "Формирование документа", MessageBoxButton.OK, MessageBoxImage.Information);

                      Application wordApp = new Application();
                      Document wordDoc = wordApp.Documents.Add();

                      wordDoc.Content.ParagraphFormat.SpaceAfter = 0;
                      wordDoc.Content.ParagraphFormat.SpaceBefore = 0;
                      wordDoc.Content.Font.Name = "Times New Roman";
                      wordDoc.Content.Font.Size = 12;

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
                      titleParagraph.Range.Text = $"Прайс-лист услуг от {DateTime.Now.Date.ToShortDateString()}";
                      titleParagraph.Range.Font.Size = 13;
                      titleParagraph.Range.Font.Bold = 1;
                      titleParagraph.Format.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
                      titleParagraph.Format.SpaceAfter = 0;
                      titleParagraph.Range.InsertParagraphAfter();
                      titleParagraph.Range.InsertParagraphAfter();

                      // Создание таблицы с услугами
                      Table servicesTable = wordDoc.Tables.Add(wordDoc.Content.Paragraphs.Add().Range, ServicesList.Count + 1, 3);
                      servicesTable.Borders.Enable = 1;
                      servicesTable.Range.Font.Size = 11;
                      servicesTable.Range.Font.Bold = 0;
                      servicesTable.Range.ParagraphFormat.LineSpacingRule = WdLineSpacing.wdLineSpaceSingle;

                      // Настройка ширины столбцов
                      servicesTable.Columns[1].PreferredWidth = wordApp.CentimetersToPoints(2);
                      servicesTable.Columns[2].PreferredWidth = wordApp.CentimetersToPoints(12);
                      servicesTable.Columns[3].PreferredWidth = wordApp.CentimetersToPoints(3);

                      // Заполнение заголовков таблицы
                      servicesTable.Cell(1, 1).Range.Text = "Код";
                      servicesTable.Cell(1, 2).Range.Text = "Наименование";
                      servicesTable.Cell(1, 3).Range.Text = "Цена, руб.";

                      // Заполнение таблицы данными из списка
                      for (int i = 0; i < ServicesList.Count; i++)
                      {
                          servicesTable.Cell(i + 2, 1).Range.Text = ServicesList[i].IDservice.ToString();
                          servicesTable.Cell(i + 2, 2).Range.Text = ServicesList[i].Name;
                          servicesTable.Cell(i + 2, 3).Range.Text = ServicesList[i].Cost.ToString();
                      }
                      // Добавление описания в конце документа
                      Paragraph descriptionParagraph = wordDoc.Content.Paragraphs.Add();
                      descriptionParagraph.Range.Text = "1 Вызов специалиста оплачивается в независимости от результатов работы." +
                                                       "2 В зависимости от года выпуска устройств, их состояния, марки и модели, на стоимость ремонта влияют повышающие или понижающие коэффициенты." +
                                                       "3 Стоимость программного обеспечения не входит в стоимость услуг. Сервисный центр \"ТехноМедиаСоюз\" занимается распространением лицензионного программного обеспечения. По ценам на программное обеспечение просьба консультироваться с инженер - приемщиком." +
                                                       "4 Сохраняются только рабочие данные программ, а не сами программы." +
                                                       "**Сервисный центр оставляет за собой право отказать в ремонте не стоящего на гарантии оборудования не объясняя причину отказа.**" +
                                                       "**Сервисный центр не несет ответственности за потерю любой информации на сданном в ремонт или на диагностику оборудовании! Убедительная просьба делать резервную копию все информации с оборудования!**" +
                                                       "Обращаем ваше внимание на то, что данный прайс-лист носит исключительно информационный характер и ни при каких условиях не является публичной офертой, определяемой положениями Статьи 437(2) Гражданского кодекса Российской Федерации. Для получения подробной информации о стоимости товаров, услуг и их наличии, пожалуйста, обращайтесь к инженер - приемщикам ООО \"ТехноМедиаСоюз\".";
                      descriptionParagraph.Range.Font.Size = 9;
                      descriptionParagraph.Format.LineSpacingRule = WdLineSpacing.wdLineSpaceSingle;
                      descriptionParagraph.Range.Font.Bold = 0;
                      descriptionParagraph.Format.Alignment = WdParagraphAlignment.wdAlignParagraphJustify;
                      descriptionParagraph.Range.InsertParagraphAfter();
                      // Отображение документа
                      wordApp.Visible = true;

                      // Сохранение документа во временный файл в формате XPS
                      //string tempFilePath = Path.GetTempFileName() + ".xps";
                      //wordDoc.SaveAs2(tempFilePath, WdSaveFormat.wdFormatXPS);

                      // Предварительный просмотр документа
                      wordDoc.PrintPreview();

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
