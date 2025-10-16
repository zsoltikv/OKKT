namespace OKKT25.Models
{
    public class CostItem
    {
        public string Type { get; set; } = string.Empty;
        public double Amount { get; set; }
        public int NumberOfPeople { get; set; }
        public bool HasDiscount { get; set; }
        public double DiscountAmount { get; set; }
        public int DiscountNumberOfPeople { get; set; }
    }

}