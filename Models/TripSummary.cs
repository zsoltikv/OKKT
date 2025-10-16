namespace OKKT25.Models
{
    public class TripSummary
    {
        public string FileName { get; set; }
        public string TripName { get; set; }
        public string TripDestination { get; set; }
        public DateTime TripDateStart { get; set; }
        public DateTime TripDateEnd { get; set; }
        public DateTime LastSaved { get; set; }
        public int Participants { get; set; }
        public double TotalCost { get; set; }
    }

}