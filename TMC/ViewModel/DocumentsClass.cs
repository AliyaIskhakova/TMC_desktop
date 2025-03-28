using Microsoft.Office.Interop.Word;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TMC.Model;
using TMC.View;

namespace TMC.ViewModel
{
    public class DocumentsClass
    {
        public async System.Threading.Tasks.Task PrintRepairAct(System.Windows.Window o)
        {
            RequestWindow requestWindow = o as RequestWindow;
            var request = requestWindow.Requests;
            var client = requestWindow.ClientInfo.DataContext as Clients;
            //MessageBox.Show("Ожидайте, документ формируется", "Формирование документа", MessageBoxButton.OK, MessageBoxImage.Information);

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
            table.Range.Font.Size = 12;
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
            //есть случай с нуливым 
            table.Cell(2, 2).Range.Text = $"{request.Model}";
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
            signatureTable.Range.Font.Size = 12;

            // Заполнение таблицы подписей
            signatureTable.Cell(1, 1).Range.Text = $"Оборудование в ремонт сдал: {client.surname} {client.name[0]}. {client.patronymic[0]}.";
            signatureTable.Cell(1, 2).Range.Text = "_____________________";
            signatureTable.Cell(2, 1).Range.Text = $"Оборудование в ремонт принял: инженер приемщик {request.MasterID}";
            signatureTable.Cell(2, 2).Range.Text = "_____________________";

            // Выравнивание текста в таблице подписей
            signatureTable.Rows[1].Cells[1].Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphLeft;
            signatureTable.Rows[1].Cells[2].Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphRight;
            signatureTable.Rows[2].Cells[1].Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphLeft;
            signatureTable.Rows[2].Cells[2].Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphRight;
            // Отображение документа
            wordApp.Visible = true;
            wordDoc.PrintPreview();
        }
    }
}
