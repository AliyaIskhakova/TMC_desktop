using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TMC.Model;

namespace TMC.ViewModel
{
    public class RepairPartViewModel : INotifyPropertyChanged
    {
        private readonly RepairParts _part;
        private double _avgSalesPerDay;

        public RepairPartViewModel(RepairParts part)
        {
            _part = part ?? throw new ArgumentNullException(nameof(part));
        }

        // Основные свойства запчасти
        public int IdPart => _part.IdPart;
        public string Name => _part.Name;
        public double Cost => _part.Cost;
        public int Count => _part.Count;

        public int MinStock { get; set; } = 3;

        // Средние продажи в день (рассчитывается отдельно)
        public double AvgSalesPerDay
        {
            get => _avgSalesPerDay;
            set
            {
                _avgSalesPerDay = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DaysOfStockLeft));
                OnPropertyChanged(nameof(StockToolTip));
                OnPropertyChanged(nameof(StockStatusColor));
            }
        }

        // На сколько дней хватит остатка
        public double DaysOfStockLeft => AvgSalesPerDay > 0 ? Count / AvgSalesPerDay : 0;

        public string StockStatusColor
        {
            get
            {
                if (Count <= 0) return "#FF4444"; // Красный - нет в наличии
                if (Count < MinStock) return "#FF4444"; // Красный - ниже минимального
                if (AvgSalesPerDay <= 0) return "Transparent"; // Нет данных о спросе

                double daysLeft = DaysOfStockLeft;

                if (daysLeft < 3) return "#FFA500"; // Оранжевый - меньше 3 дней
                if (daysLeft < 7) return "#FFE417"; // Желтый - меньше недели
                return "Transparent"; // Достаточный запас
            }
        }

        public string StockToolTip =>
            $"Текущий остаток: {Count} шт.\n"  +
            (AvgSalesPerDay > 0
                ? $"Средний расход: {AvgSalesPerDay:0.0} шт./день\n" +
                  $"Остаток на: {DaysOfStockLeft:0.0} дней"
                : "Нет данных о продажах за период");

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Для обновления данных из модели
        public void UpdateFromModel(RepairParts part)
        {
            _part.Name = part.Name;
            _part.Cost = part.Cost;
            _part.Count = part.Count;
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(Cost));
            OnPropertyChanged(nameof(Count));
            OnPropertyChanged(nameof(StockStatusColor));
            OnPropertyChanged(nameof(StockToolTip));
        }
    }
}
