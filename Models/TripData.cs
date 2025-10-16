using OKKT25.Models;

namespace OKKT25
{
    public class TripData
    {
        public string TripName { get; set; } = string.Empty;
        public int Participants { get; set; }
        public int MonthsLeft { get; set; }
        public bool IsPerPersonMode { get; set; }
        public List<CostItem> Costs { get; set; } = new List<CostItem>();
        public List<double> PocketMoney { get; set; } = new List<double>();
        public double AveragePocketMoney { get; set; }
        public DateTime LastSaved { get; set; } = DateTime.Now;
        public List<string> PhotoPaths { get; set; } = new List<string>();
        public string TripDestination { get; set; } = string.Empty;
        public DateTime TripDateStart { get; set; } = DateTime.Now;
        public DateTime TripDateEnd { get; set; } = DateTime.Now;
        public bool Calculated { get; set; } = false;
    }
}