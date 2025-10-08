using System.Diagnostics;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using StackLayout = Microsoft.Maui.Controls.StackLayout;

namespace OKKT25
{
    public partial class MainPage : ContentPage
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
        }
        public class CostItem
        {
            public string Type { get; set; } = string.Empty;
            public double Amount { get; set; }
            public int NumberOfPeople { get; set; }
            public bool HasDiscount { get; set; }
            public double DiscountAmount { get; set; }
            public int DiscountNumberOfPeople { get; set; }
        }

        public class TripSummary
        {
            public string FileName { get; set; }
            public string TripName { get; set; }
            public DateTime LastSaved { get; set; }
            public int Participants { get; set; }
            public double TotalCost { get; set; }
        }

        private List<Entry> pocketMoneyEntries = new List<Entry>();
        private bool isPerPersonMode = false;
        private TripData currentTripData = new TripData();
        private const string SAVE_FILE_NAME = "trip_data.json";

        public MainPage()
        {
            InitializeComponent();
            UpdatePocketMoneyLayout();
        }

        private async void SaveData()
        {
            try
            {
                if (int.TryParse(EntryParticipants.Text, out int participants))
                    currentTripData.Participants = participants;

                if (int.TryParse(EntryMonthsLeft.Text, out int months))
                    currentTripData.MonthsLeft = months;

                currentTripData.IsPerPersonMode = isPerPersonMode;
                currentTripData.LastSaved = DateTime.Now;
                currentTripData.TripName = TripName.Text;
                currentTripData.PocketMoney.Clear();
                if (isPerPersonMode)
                {
                    foreach (var entry in pocketMoneyEntries)
                    {
                        if (double.TryParse(entry.Text, out double amount))
                            currentTripData.PocketMoney.Add(amount);
                    }
                    currentTripData.AveragePocketMoney = 0;
                }
                else if (pocketMoneyEntries.Count > 0 && double.TryParse(pocketMoneyEntries[0].Text, out double avgAmount))
                {
                    currentTripData.AveragePocketMoney = avgAmount;
                    currentTripData.PocketMoney.Clear();
                }

                currentTripData.Costs.Clear();
                foreach (var layout in DynamicCostsLayout.Children.OfType<StackLayout>())
                {
                    var allEntries = layout.Children
                        .OfType<StackLayout>()
                        .SelectMany(sl => sl.Children.OfType<Entry>())
                        .ToList();

                    var checkBox = layout.Children
                        .OfType<StackLayout>()
                        .SelectMany(sl => sl.Children.OfType<CheckBox>())
                        .FirstOrDefault();

                    if (allEntries.Count >= 3)
                    {
                        var costItem = new CostItem
                        {
                            Type = allEntries[0].Text ?? string.Empty
                        };

                        if (allEntries.Count > 1 && double.TryParse(allEntries[1].Text, out double amount))
                            costItem.Amount = amount;

                        if (allEntries.Count > 2 && int.TryParse(allEntries[2].Text, out int numberOfPeople))
                            costItem.NumberOfPeople = numberOfPeople;

                        if (allEntries.Count > 3 && double.TryParse(allEntries[3].Text, out double discountAmount))
                            costItem.DiscountAmount = discountAmount;

                        if (allEntries.Count > 4 && int.TryParse(allEntries[4].Text, out int discountPeople))
                            costItem.DiscountNumberOfPeople = discountPeople;

                        costItem.HasDiscount = checkBox?.IsChecked ?? false;
                        currentTripData.Costs.Add(costItem);
                    }
                }

                var json = JsonSerializer.Serialize(currentTripData, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                string tripName = string.IsNullOrWhiteSpace(TripName.Text) ? "Unnamed_Trip" : TripName.Text;
                string safeFileName = string.Join("_", tripName.Split(Path.GetInvalidFileNameChars())) + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".json";
                string targetFile = System.IO.Path.Combine(FileSystem.Current.AppDataDirectory, safeFileName);
                await System.IO.File.WriteAllTextAsync(targetFile, json, Encoding.UTF8);
                await DisplayAlert("Sikeres mentés", "Az adataid el lettek mentve!", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hiba", $"Nem sikerült menteni: {ex.Message}", "OK");
            }
        }

        private void RestoreUIFromData()
        {
            EntryParticipants.Text = currentTripData.Participants.ToString();
            EntryMonthsLeft.Text = currentTripData.MonthsLeft.ToString();

            isPerPersonMode = currentTripData.IsPerPersonMode;
            if (isPerPersonMode)
                RadioPerPerson.IsChecked = true;
            else
                RadioGrouped.IsChecked = true;

            UpdatePocketMoneyLayout();
            if (isPerPersonMode && currentTripData.PocketMoney.Count > 0)
            {
                for (int i = 0; i < Math.Min(currentTripData.PocketMoney.Count, pocketMoneyEntries.Count); i++)
                {
                    pocketMoneyEntries[i].Text = currentTripData.PocketMoney[i].ToString();
                }
            }
            else if (!isPerPersonMode && currentTripData.AveragePocketMoney > 0)
            {
                pocketMoneyEntries[0].Text = currentTripData.AveragePocketMoney.ToString();
            }

            DynamicCostsLayout.Children.Clear();
            foreach (var cost in currentTripData.Costs)
            {
                AddCostItemToUI(cost);
            }
        }

        private void AddCostItemToUI(CostItem cost)
        {
            var newCostLayout = new StackLayout
            {
                Orientation = StackOrientation.Vertical,
                Spacing = 10,
                Padding = new Thickness(10)
            };

            var firstRowLayout = new StackLayout { Orientation = StackOrientation.Horizontal, Spacing = 10 };
            var secondRowLayout = new StackLayout { Orientation = StackOrientation.Horizontal, Spacing = 10 };
            var thirdRowLayout = new StackLayout { Orientation = StackOrientation.Horizontal, Spacing = 10, IsVisible = cost.HasDiscount };

            var costTypeEntry = new Entry
            {
                Placeholder = "Költség típusa",
                BackgroundColor = Color.FromHex("#F5F5F5"),
                TextColor = Color.FromHex("#424242"),
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Text = cost.Type
            };

            var costAmountEntry = new Entry
            {
                Placeholder = "Összeg (Ft)",
                Keyboard = Keyboard.Numeric,
                BackgroundColor = Color.FromHex("#F5F5F5"),
                TextColor = Color.FromHex("#424242"),
                HorizontalOptions = LayoutOptions.FillAndExpand,
                StyleId = "FullCostAmount",
                Text = cost.Amount > 0 ? cost.Amount.ToString() : ""
            };

            var numberOfFullCost = new Entry
            {
                Placeholder = "Fő (db)",
                Keyboard = Keyboard.Numeric,
                BackgroundColor = Color.FromHex("#F5F5F5"),
                TextColor = Color.FromHex("#424242"),
                HorizontalOptions = LayoutOptions.FillAndExpand,
                StyleId = "FullCostAmount",
                Text = cost.NumberOfPeople > 0 ? cost.NumberOfPeople.ToString() : ""
            };

            var discountCostAmountEntry = new Entry
            {
                Placeholder = "Kedvezményes összeg (Ft)",
                Keyboard = Keyboard.Numeric,
                BackgroundColor = Color.FromHex("#F5F5F5"),
                TextColor = Color.FromHex("#424242"),
                HorizontalOptions = LayoutOptions.FillAndExpand,
                StyleId = "DiscountCostAmount",
                Text = cost.DiscountAmount > 0 ? cost.DiscountAmount.ToString() : ""
            };

            var numberOfDiscountCost = new Entry
            {
                Placeholder = "Fő (db)",
                Keyboard = Keyboard.Numeric,
                BackgroundColor = Color.FromHex("#F5F5F5"),
                TextColor = Color.FromHex("#424242"),
                HorizontalOptions = LayoutOptions.FillAndExpand,
                StyleId = "DiscountCostAmount",
                Text = cost.DiscountNumberOfPeople > 0 ? cost.DiscountNumberOfPeople.ToString() : ""
            };

            var isDiscountAvailable = new CheckBox
            {
                Color = Color.FromHex("#424242"),
                IsChecked = cost.HasDiscount
            };

            var discountLabel = new Label
            {
                Text = "Van kedvezmény?",
                VerticalOptions = LayoutOptions.Center,
                TextColor = Color.FromHex("#424242")
            };

            var checkBoxLayout = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Spacing = 10,
                Children = { isDiscountAvailable, discountLabel }
            };

            isDiscountAvailable.CheckedChanged += (s, e) =>
            {
                thirdRowLayout.IsVisible = isDiscountAvailable.IsChecked;
            };

            var removeButton = new Button
            {
                Text = "×",
                BackgroundColor = Color.FromHex("#F5F5F5"),
                TextColor = Color.FromHex("#424242"),
                WidthRequest = 40,
                HeightRequest = 40
            };
            removeButton.Clicked += (s, eArgs) =>
            {
                DynamicCostsLayout.Children.Remove(newCostLayout);
            };

            var line = new BoxView
            {
                HeightRequest = 1,
                Color = Color.FromHex("#424242"),
                HorizontalOptions = LayoutOptions.FillAndExpand
            };

            firstRowLayout.Children.Add(costTypeEntry);
            firstRowLayout.Children.Add(removeButton);

            secondRowLayout.Children.Add(costAmountEntry);
            secondRowLayout.Children.Add(numberOfFullCost);

            thirdRowLayout.Children.Add(discountCostAmountEntry);
            thirdRowLayout.Children.Add(numberOfDiscountCost);

            newCostLayout.Children.Add(firstRowLayout);
            newCostLayout.Children.Add(secondRowLayout);
            newCostLayout.Children.Add(checkBoxLayout);
            newCostLayout.Children.Add(thirdRowLayout);
            newCostLayout.Children.Add(line);

            DynamicCostsLayout.Children.Add(newCostLayout);
        }

        private void OnSaveClicked(object sender, EventArgs e)
        {
            SaveData();
        }

        private async void OnClearDataClicked(object sender, EventArgs e)
        {
            bool answer = await DisplayAlert("Adatok törlése",
                "Biztosan törlöd az összes mentett adatot?\nEz a művelet nem visszavonható!",
                "Igen, törlöm", "Mégsem");

            if (answer)
            {
                try
                {
                    string targetFile = System.IO.Path.Combine(FileSystem.Current.AppDataDirectory, SAVE_FILE_NAME);
                    if (System.IO.File.Exists(targetFile))
                    {
                        System.IO.File.Delete(targetFile);
                    }

                    currentTripData = new TripData();
                    EntryParticipants.Text = "";
                    EntryMonthsLeft.Text = "";
                    DynamicCostsLayout.Children.Clear();
                    LayoutPocketMoney.Clear();
                    pocketMoneyEntries.Clear();
                    LayoutResults.Clear();

                    await DisplayAlert("Sikeres törlés", "Az összes adat törölve lett!", "OK");
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Hiba", $"Nem sikerült törölni: {ex.Message}", "OK");
                }
            }
        }

        private void OnAddCostClicked(object sender, EventArgs e)
        {
            var newCostLayout = new StackLayout
            {
                Orientation = StackOrientation.Vertical,
                Spacing = 10,
                Padding = new Thickness(10)
            };

            var firstRowLayout = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Spacing = 10
            };

            var secondRowLayout = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Spacing = 10
            };

            var thirdRowLayout = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Spacing = 10,
                IsVisible = false
            };

            var costTypeEntry = new Entry
            {
                Placeholder = "Költség típusa",
                BackgroundColor = Color.FromHex("#F5F5F5"),
                TextColor = Color.FromHex("#424242"),
                HorizontalOptions = LayoutOptions.FillAndExpand,
                StyleId = "CostType"
            };

            var costAmountEntry = new Entry
            {
                Placeholder = "Összeg (Ft)",
                Keyboard = Keyboard.Numeric,
                BackgroundColor = Color.FromHex("#F5F5F5"),
                TextColor = Color.FromHex("#424242"),
                HorizontalOptions = LayoutOptions.FillAndExpand,
                StyleId = "FullCostAmount"
            };

            var numberOfFullCost = new Entry
            {
                Placeholder = "Fő (db)",
                Keyboard = Keyboard.Numeric,
                BackgroundColor = Color.FromHex("#F5F5F5"),
                TextColor = Color.FromHex("#424242"),
                HorizontalOptions = LayoutOptions.FillAndExpand,
                StyleId = "FullCostAmount"
            };

            var discountCostAmountEntry = new Entry
            {
                Placeholder = "Kedvezményes összeg (Ft)",
                Keyboard = Keyboard.Numeric,
                BackgroundColor = Color.FromHex("#F5F5F5"),
                TextColor = Color.FromHex("#424242"),
                HorizontalOptions = LayoutOptions.FillAndExpand,
                StyleId = "DiscountCostAmount"
            };

            var numberOfDiscountCost = new Entry
            {
                Placeholder = "Fő (db)",
                Keyboard = Keyboard.Numeric,
                BackgroundColor = Color.FromHex("#F5F5F5"),
                TextColor = Color.FromHex("#424242"),
                HorizontalOptions = LayoutOptions.FillAndExpand,
                StyleId = "DiscountCostAmount"
            };

            var isDiscountAvailable = new CheckBox
            {
                Color = Color.FromHex("#424242")
            };

            var discountLabel = new Label
            {
                Text = "Van kedvezmény?",
                VerticalOptions = LayoutOptions.Center,
                TextColor = Color.FromHex("#424242")
            };

            var checkBoxLayout = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Spacing = 10,
                Children = { isDiscountAvailable, discountLabel }
            };

            isDiscountAvailable.CheckedChanged += (s, e) =>
            {
                thirdRowLayout.IsVisible = isDiscountAvailable.IsChecked;
            };

            var removeButton = new Button
            {
                Text = "×",
                BackgroundColor = Color.FromHex("#F5F5F5"),
                TextColor = Color.FromHex("#424242"),
                WidthRequest = 40,
                HeightRequest = 40
            };
            removeButton.Clicked += (s, eArgs) =>
            {
                DynamicCostsLayout.Children.Remove(newCostLayout);
            };

            var line = new BoxView
            {
                HeightRequest = 1,
                Color = Color.FromHex("#424242"),
                HorizontalOptions = LayoutOptions.FillAndExpand
            };

            firstRowLayout.Children.Add(costTypeEntry);
            firstRowLayout.Children.Add(removeButton);

            secondRowLayout.Children.Add(costAmountEntry);
            secondRowLayout.Children.Add(numberOfFullCost);

            thirdRowLayout.Children.Add(discountCostAmountEntry);
            thirdRowLayout.Children.Add(numberOfDiscountCost);

            newCostLayout.Children.Add(firstRowLayout);
            newCostLayout.Children.Add(secondRowLayout);
            newCostLayout.Children.Add(checkBoxLayout);
            newCostLayout.Children.Add(thirdRowLayout);
            newCostLayout.Children.Add(line);

            DynamicCostsLayout.Children.Add(newCostLayout);
        }

        private void OnParticipantsChanged(object sender, TextChangedEventArgs e)
        {
            if (isPerPersonMode)
            {
                UpdatePocketMoneyLayout();
            }
        }

        private void OnPocketMoneyTypeChanged(object sender, CheckedChangedEventArgs e)
        {
            if (sender is RadioButton radioButton && radioButton.IsChecked)
            {
                isPerPersonMode = radioButton == RadioPerPerson;
                UpdatePocketMoneyLayout();
            }
        }

        private void UpdatePocketMoneyLayout()
        {
            LayoutPocketMoney.Clear();
            pocketMoneyEntries.Clear();

            if (isPerPersonMode)
            {
                if (!int.TryParse(EntryParticipants.Text, out int participants) ||
                    participants <= 0 || participants > 100)
                {
                    var warningLabel = new Label
                    {
                        Text = "⚠️ Először add meg a résztvevők számát (1-100)!",
                        TextColor = Colors.Orange,
                        FontAttributes = FontAttributes.Bold
                    };
                    LayoutPocketMoney.Add(warningLabel);
                    return;
                }

                for (int i = 1; i <= participants; i++)
                {
                    var label = new Label
                    {
                        Text = $"{i}. diák zsebpénze (Ft):",
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Color.FromArgb("#424242"),
                        Margin = new Thickness(0, i > 1 ? 10 : 0, 0, 0)
                    };

                    var entry = new Entry
                    {
                        Placeholder = "pl. 3000",
                        Keyboard = Keyboard.Numeric,
                        TextColor = Color.FromArgb("#424242"),
                        BackgroundColor = Color.FromArgb("#F5F5F5")
                    };

                    pocketMoneyEntries.Add(entry);
                    LayoutPocketMoney.Add(label);
                    LayoutPocketMoney.Add(entry);
                }
            }
            else
            {
                var label = new Label
                {
                    Text = "Átlagos zsebpénz fejenként (Ft):",
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#424242")
                };

                var entry = new Entry
                {
                    Placeholder = "pl. 3000",
                    Keyboard = Keyboard.Numeric,
                    TextColor = Color.FromArgb("#424242"),
                    BackgroundColor = Color.FromArgb("#F5F5F5")
                };

                pocketMoneyEntries.Add(entry);
                LayoutPocketMoney.Add(label);
                LayoutPocketMoney.Add(entry);
            }

            AnimateView(LayoutPocketMoney);
        }

        private async void OnCalculateClicked(object sender, EventArgs e)
        {
            var fullCostEntries = DynamicCostsLayout.Children
                .OfType<StackLayout>()
                .SelectMany(nc => nc.Children
                    .OfType<StackLayout>()
                    .SelectMany(sr => sr.Children
                        .OfType<Entry>()
                        .Where(e => e.StyleId == "FullCostAmount")
                    )
                )
                .ToArray();

            if (fullCostEntries.Length != 2)
            {
                await ShowError("Kérlek adj meg 2 teljes költség mezőt!");
                return;
            }

            double fullCostTotal = 0;
            List<double> fullCostValues = new List<double>();

            foreach (var item in fullCostEntries)
            {
                if (item == null || !double.TryParse(item.Text, out double value) || value <= 0)
                {
                    await ShowError("Kérlek adj meg érvényes teljes költséget!");
                    return;
                }
                fullCostValues.Add(value);
            }

            for (int i = 0; i < fullCostValues.Count; i += 2)
            {
                fullCostTotal += fullCostValues[i] * fullCostValues[i + 1];
            }

            var discountCostEntries = DynamicCostsLayout.Children
                .OfType<StackLayout>()
                .SelectMany(nc => nc.Children
                    .OfType<StackLayout>()
                    .SelectMany(sr => sr.Children
                        .OfType<Entry>()
                        .Where(e => e.StyleId == "DiscountCostAmount")
                    )
                )
                .ToArray();

            double discountCostTotal = 0;
            List<double> discountValues = new List<double>();

            foreach (var item in discountCostEntries)
            {
                var parentLayout = item.Parent as StackLayout;
                if (parentLayout?.IsVisible == false) continue;
                if (item == null || !double.TryParse(item.Text, out double value) || value <= 0)
                {
                    await ShowError("Kérlek adj meg érvényes teljes költséget!");
                    return;
                }
                discountValues.Add(value);
            }

            for (int i = 0; i < discountValues.Count; i += 2)
            {
                discountCostTotal += discountValues[i] * discountValues[i + 1];
            }

            double finalCost = fullCostTotal + discountCostTotal;

            if (!int.TryParse(EntryParticipants.Text, out int participants) ||
                participants <= 0 || participants > 100)
            {
                await ShowError("Kérlek adj meg érvényes résztvevő számot (1-100)!");
                return;
            }

            if (!int.TryParse(EntryMonthsLeft.Text, out int monthsLeft) ||
                monthsLeft <= 0 || monthsLeft > 24)
            {
                await ShowError("Kérlek adj meg érvényes hónapok számát (1-24)!");
                return;
            }

            double additionalCosts = 0;
            foreach (var layout in DynamicCostsLayout.Children.OfType<StackLayout>())
            {
                var entries = layout.Children.OfType<Entry>().ToList();
                if (entries.Count >= 2)
                {
                    if (double.TryParse(entries[1].Text, out double cost) && cost >= 0)
                    {
                        additionalCosts += cost;
                    }
                    else
                    {
                        await ShowError($"Kérlek adj meg érvényes összeget a(z) '{entries[0].Text}' költségnél!");
                        return;
                    }
                }
            }

            finalCost += additionalCosts;

            var pocketMoneyList = new List<double>();
            if (isPerPersonMode)
            {
                for (int i = 0; i < pocketMoneyEntries.Count; i++)
                {
                    if (!double.TryParse(pocketMoneyEntries[i].Text, out double amount) || amount < 0)
                    {
                        await ShowError($"Kérlek add meg a {i + 1}. diák érvényes zsebpénzét!");
                        return;
                    }
                    pocketMoneyList.Add(amount);
                }
            }
            else
            {
                if (!double.TryParse(pocketMoneyEntries[0].Text, out double amount) || amount < 0)
                {
                    await ShowError("Kérlek adj meg érvényes átlagos zsebpénzt!");
                    return;
                }
                for (int i = 0; i < participants; i++)
                {
                    pocketMoneyList.Add(amount);
                }
            }

            DisplayResults(finalCost, participants, monthsLeft, pocketMoneyList);
        }

        private void DisplayResults(double totalCost, int participants, int monthsLeft, List<double> pocketMoneyList)
        {
            LayoutResults.Clear();

            double costPerPerson = totalCost / participants;
            double monthlyPerPerson = costPerPerson / monthsLeft;

            var summaryCard = CreateResultCard("📊 Összefoglaló", "#1976D2");
            var summaryLabel = new Label
            {
                Text = $@"Teljes költség: {FormatNumber(totalCost)} Ft
                            Résztvevők: {participants} fő
                            Hátralévő idő: {monthsLeft} hónap

                            ───────────────────────

                            💰 Fejenként fizetendő:
                            Összesen: {FormatNumber(costPerPerson)} Ft
                            Havonta: {FormatNumber(monthlyPerPerson)} Ft",
                FontSize = 16,
                TextColor = Color.FromArgb("#212121"),
                Padding = new Thickness(15)
            };
            ((VerticalStackLayout)summaryCard.Content).Add(summaryLabel);
            LayoutResults.Add(summaryCard);

            var analysisCard = CreateResultCard("👥 Egyéni Elemzés", "#388E3C");
            var analysisLayout = new VerticalStackLayout { Padding = new Thickness(15), Spacing = 10 };

            bool allCanPay = true;
            var cantPayList = new List<(int studentNum, double shortage)>();

            for (int i = 0; i < pocketMoneyList.Count; i++)
            {
                double pocketMoney = pocketMoneyList[i];
                double monthlyTotal = pocketMoney * monthsLeft;
                bool canPay = monthlyTotal >= costPerPerson;

                if (!canPay)
                {
                    allCanPay = false;
                    cantPayList.Add((i + 1, costPerPerson - monthlyTotal));
                }

                string statusIcon = canPay ? "✅" : "❌";
                var statusColor = canPay ? Color.FromArgb("#4CAF50") : Color.FromArgb("#F44336");

                var studentLabel = new Label
                {
                    Text = $@"{statusIcon} {i + 1}. diák
                                Havi zsebpénz: {FormatNumber(pocketMoney)} Ft
                                Összesen {monthsLeft} hónap alatt: {FormatNumber(monthlyTotal)} Ft
                                Fizetendő: {FormatNumber(costPerPerson)} Ft
                                {(canPay ? "Fedezi a költséget! ✓" : $"Hiány: {FormatNumber(costPerPerson - monthlyTotal)} Ft")}",
                    TextColor = statusColor,
                    FontSize = 14
                };

                analysisLayout.Add(studentLabel);

                if (i < pocketMoneyList.Count - 1)
                {
                    analysisLayout.Add(new BoxView
                    {
                        HeightRequest = 1,
                        BackgroundColor = Color.FromArgb("#E0E0E0"),
                        Margin = new Thickness(0, 5)
                    });
                }
            }

            ((VerticalStackLayout)analysisCard.Content).Add(analysisLayout);
            LayoutResults.Add(analysisCard);

            var chartCard = CreateResultCard("📈 Fedezettségi Diagram", "#F57C00");
            var chartView = new PieChartView(pocketMoneyList, costPerPerson, monthsLeft)
            {
                HeightRequest = 300,
                Margin = new Thickness(15)
            };
            ((VerticalStackLayout)chartCard.Content).Add(chartView);
            LayoutResults.Add(chartCard);

            if (!allCanPay)
            {
                var suggestionsCard = CreateResultCard("💡 Javaslatok", "#D32F2F");
                var suggestionsLayout = new VerticalStackLayout { Padding = new Thickness(15), Spacing = 12 };

                double totalShortage = cantPayList.Sum(x => x.shortage);

                suggestionsLayout.Add(new Label
                {
                    Text = $"⚠️ {cantPayList.Count} diák nem tudja fedezni a költséget!",
                    FontSize = 16,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#D32F2F")
                });

                suggestionsLayout.Add(new BoxView { HeightRequest = 1, BackgroundColor = Color.FromArgb("#E0E0E0") });

                double neededReduction = totalShortage;
                suggestionsLayout.Add(new Label
                {
                    Text = $@"1️⃣ Költségcsökkentés
                            Ha {FormatNumber(neededReduction)} Ft-tal csökkentjük a teljes költséget,
                            mindenki tudja fizetni a kirándulást.
                            Új fejenként fizetendő: {FormatNumber(costPerPerson - (neededReduction / participants))} Ft",
                    FontSize = 14
                });

                suggestionsLayout.Add(new BoxView { HeightRequest = 1, BackgroundColor = Color.FromArgb("#E0E0E0") });

                double extraPerPerson = totalShortage / (participants - cantPayList.Count);
                suggestionsLayout.Add(new Label
                {
                    Text = $@"2️⃣ Többi diák fizet többet
                                Ha a {participants - cantPayList.Count} másik diák befizeti a hiányt:
                                Extra fejenként: {FormatNumber(extraPerPerson)} Ft
                                Új összeg számukra: {FormatNumber(costPerPerson + extraPerPerson)} Ft",
                    FontSize = 14
                });

                suggestionsLayout.Add(new BoxView { HeightRequest = 1, BackgroundColor = Color.FromArgb("#E0E0E0") });

                int neededMonths = (int)Math.Ceiling(costPerPerson / pocketMoneyList.Min());
                if (neededMonths > monthsLeft)
                {
                    suggestionsLayout.Add(new Label
                    {
                        Text = $@"3️⃣ Több idő szükséges
                                Legalább {neededMonths} hónap kellene ahhoz, hogy mindenki össze tudja gyűjteni a pénzt.
                                (Még {neededMonths - monthsLeft} hónap)",
                        FontSize = 14
                    });
                }

                ((VerticalStackLayout)suggestionsCard.Content).Add(suggestionsLayout);
                LayoutResults.Add(suggestionsCard);
            }
            else
            {
                var successCard = CreateResultCard("🎉 Szuper Hír!", "#4CAF50");
                var successLabel = new Label
                {
                    Text = @"✨ Minden diák tudja fizetni a kirándulást!

                                Az osztálykirándulás megvalósítható a megadott feltételekkel.
                                Kezdjétek el gyűjteni a pénzt! 🎒",
                    FontSize = 16,
                    TextColor = Color.FromArgb("#1B5E20"),
                    Padding = new Thickness(15)
                };
                ((VerticalStackLayout)successCard.Content).Add(successLabel);
                LayoutResults.Add(successCard);
            }

            LayoutResults.IsVisible = true;
            AnimateView(LayoutResults);
        }

        private Frame CreateResultCard(string title, string colorHex)
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

        private async Task ShowError(string message)
        {
            await DisplayAlert("Hiba", message, "OK");

            await BtnCalculate.TranslateTo(-15, 0, 50);
            await BtnCalculate.TranslateTo(15, 0, 50);
            await BtnCalculate.TranslateTo(-10, 0, 50);
            await BtnCalculate.TranslateTo(10, 0, 50);
            await BtnCalculate.TranslateTo(-5, 0, 50);
            await BtnCalculate.TranslateTo(5, 0, 50);
            await BtnCalculate.TranslateTo(0, 0, 50);
        }

        private async void AnimateView(View view)
        {
            view.Opacity = 0;
            await view.FadeTo(1, 300);
        }

        private string FormatNumber(double number)
        {
            return number.ToString("N0", new CultureInfo("hu-HU"));
        }
    }

    public class PieChartView : GraphicsView
    {
        public PieChartView(List<double> pocketMoney, double cost, int months)
        {
            Drawable = new PieChartDrawable(pocketMoney, cost, months);
        }
    }

    public class PieChartDrawable : IDrawable
    {
        private List<double> pocketMoneyList;
        private double costPerPerson;
        private int monthsLeft;

        public PieChartDrawable(List<double> pocketMoney, double cost, int months)
        {
            pocketMoneyList = pocketMoney;
            costPerPerson = cost;
            monthsLeft = months;
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            int canPayCount = pocketMoneyList.Count(pm => pm * monthsLeft >= costPerPerson);
            int cantPayCount = pocketMoneyList.Count - canPayCount;

            if (canPayCount == 0 && cantPayCount == 0) return;

            float centerX = dirtyRect.Width / 2;
            float centerY = dirtyRect.Height / 2;
            float radius = Math.Min(centerX, centerY) * 0.7f;

            float canPayAngle = (canPayCount / (float)pocketMoneyList.Count) * 360f;
            float cantPayAngle = 360f - canPayAngle;

            canvas.FillColor = Color.FromArgb("#4CAF50");
            canvas.FillArc(centerX - radius, centerY - radius, radius * 2, radius * 2, -90, canPayAngle, true);

            canvas.FillColor = Color.FromArgb("#F44336");
            canvas.FillArc(centerX - radius, centerY - radius, radius * 2, radius * 2, -90 + canPayAngle, cantPayAngle, true);

            canvas.FillColor = Colors.White;
            canvas.FillCircle(centerX, centerY, radius * 0.6f);

            canvas.FontSize = 24;
            canvas.FontColor = Color.FromArgb("#4CAF50");
            canvas.DrawString($"{canPayCount} fő", centerX - 40, centerY - 20, 80, 30, HorizontalAlignment.Center, VerticalAlignment.Top);

            canvas.FontSize = 16;
            canvas.DrawString("tudja fizetni", centerX - 60, centerY + 5, 120, 20, HorizontalAlignment.Center, VerticalAlignment.Top);

            if (cantPayCount > 0)
            {
                canvas.FontSize = 24;
                canvas.FontColor = Color.FromArgb("#F44336");
                canvas.DrawString($"{cantPayCount} fő", centerX - 40, centerY + 35, 80, 30, HorizontalAlignment.Center, VerticalAlignment.Top);

                canvas.FontSize = 16;
                canvas.DrawString("nem tudja", centerX - 60, centerY + 60, 120, 20, HorizontalAlignment.Center, VerticalAlignment.Top);
            }
        }
    }
}