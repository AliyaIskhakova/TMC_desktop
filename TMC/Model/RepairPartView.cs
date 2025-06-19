using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TMC.Model
{
    public class RepairPartView: INotifyPropertyChanged
    {
        private readonly RepairParts _part;
        private double _avgSalesPerDay;
        private int count;
        private double cost;
        private int minStock;
        public int IdPart { get; set; }
        public string Name { get; set; }
        public int Count { get { return count; } set { count = value; OnPropertyChanged();

                OnPropertyChanged(nameof(AvgSalesPerDay));
                OnPropertyChanged(nameof(DaysOfStockLeft));
                OnPropertyChanged(nameof(StockToolTip));
                OnPropertyChanged(nameof(StockStatusColor));

            } }
        public double Cost { get { return cost; } set { cost = value; OnPropertyChanged(); } }
        public int MinStock { get { return minStock; } set { minStock = value; OnPropertyChanged(); OnPropertyChanged(nameof(DaysOfStockLeft));
                OnPropertyChanged(nameof(StockToolTip));
                OnPropertyChanged(nameof(StockStatusColor));
            } }


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
                if (Count == 0) return "#FF4444"; // Красный - нет в наличии
                if (Count < MinStock) return "#FF4444"; // Красный - ниже минимального
                if (AvgSalesPerDay <= 0) return "Transparent"; // Нет данных о спросе
                double daysLeft = DaysOfStockLeft;
                if (daysLeft < 3) return "#FFA500"; // Оранжевый - меньше 3 дней
                if (daysLeft < 7) return "#FFE417"; // Желтый - меньше недели
                return "Transparent"; // Достаточный запас
            }
        }

        public string StockToolTip
        {
            get
            {
                return $"Текущий остаток: {Count} шт.\n" +
            $"Минимальный запас: {MinStock} шт.\n" +
            (AvgSalesPerDay > 0
                ? $"Средний расход: {AvgSalesPerDay:0.0} шт./день\n" +
                  $"Остаток на: {DaysOfStockLeft:0.0} дней"
                : "Нет данных о продажах за период");
            }
        }
            
  
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
      
    }
}
