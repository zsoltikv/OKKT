using Microsoft.Maui.Controls;
using System.Globalization;

namespace OKKT25
{
    public partial class TripDetailPage : ContentPage
    {
        private MainPage.TripData tripData;

        public TripDetailPage(MainPage.TripData data, string tripName)
        {
            InitializeComponent();
            tripData = data;
            Title = tripName;
            DisplayTripDetails();
        }

        private void DisplayTripDetails()
        {
            DetailLayout.Clear();

            var titleLabel = new Label
            {
                Text = Title,
                FontSize = 24,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#1976D2"),
                Margin = new Thickness(0, 0, 0, 20)
            };
            DetailLayout.Add(titleLabel);

            double totalCost = tripData.Costs.Sum(c =>
                (c.Amount * c.NumberOfPeople) +
                (c.DiscountAmount * c.DiscountNumberOfPeople));

            var summaryCard = CreateCard("📊 Összefoglaló", "#1976D2");
            var summaryText = new Label
            {
                Text = $@"Teljes költség: {FormatNumber(totalCost)} Ft
Résztvevők: {tripData.Participants} fő
Hátralévő idő: {tripData.MonthsLeft} hónap
Mentve: {tripData.LastSaved:yyyy.MM.dd HH:mm}",
                FontSize = 16,
                TextColor = Color.FromArgb("#212121"),
                Padding = new Thickness(15)
            };
            ((VerticalStackLayout)summaryCard.Content).Add(summaryText);
            DetailLayout.Add(summaryCard);

            var costsCard = CreateCard("💰 Költségek", "#388E3C");
            var costsLayout = new VerticalStackLayout { Padding = new Thickness(15), Spacing = 8 };

            foreach (var cost in tripData.Costs)
            {
                var costLabel = new Label
                {
                    Text = $"• {cost.Type}: {FormatNumber(cost.Amount)} Ft × {cost.NumberOfPeople} fő" +
                           (cost.HasDiscount ? $"\n  Kedvezmény: {FormatNumber(cost.DiscountAmount)} Ft × {cost.DiscountNumberOfPeople} fő" : ""),
                    FontSize = 14,
                    TextColor = Color.FromArgb("#424242")
                };
                costsLayout.Add(costLabel);
            }
            ((VerticalStackLayout)costsCard.Content).Add(costsLayout);
            DetailLayout.Add(costsCard);

            var pocketMoneyCard = CreateCard("💵 Zsebpénz", "#F57C00");
            var pocketMoneyText = new Label
            {
                Text = tripData.IsPerPersonMode
                    ? $"Személyenként megadva\n{tripData.PocketMoney.Count} diák adatai mentve"
                    : $"Átlagos zsebpénz: {FormatNumber(tripData.AveragePocketMoney)} Ft/fő",
                FontSize = 14,
                TextColor = Color.FromArgb("#424242"),
                Padding = new Thickness(15)
            };
            ((VerticalStackLayout)pocketMoneyCard.Content).Add(pocketMoneyText);
            DetailLayout.Add(pocketMoneyCard);
        }

        private Frame CreateCard(string title, string colorHex)
        {
            var frame = new Frame
            {
                CornerRadius = 15,
                HasShadow = true,
                BackgroundColor = Colors.White,
                Padding = 0,
                Margin = new Thickness(0, 0, 0, 12)
            };

            var container = new VerticalStackLayout();
            var titleLabel = new Label
            {
                Text = title,
                FontSize = 18,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White,
                BackgroundColor = Color.FromArgb(colorHex),
                Padding = new Thickness(15, 12)
            };

            container.Add(titleLabel);
            frame.Content = container;
            return frame;
        }

        private string FormatNumber(double number)
        {
            return number.ToString("N0", new CultureInfo("hu-HU"));
        }
    }
}