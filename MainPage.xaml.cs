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
            public List<string> PhotoPaths { get; set; } = new();
            public string TripDestination { get; set; } = string.Empty;
            public DateTime TripDateStart { get; set; } = DateTime.Now;
            public DateTime TripDateEnd { get; set; } = DateTime.Now;
            public bool Calculated = false;
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
            public string TripDestination { get; set; }
            public DateTime TripDateStart { get; set; }
            public DateTime TripDateEnd { get; set; }
            public DateTime LastSaved { get; set; }
            public int Participants { get; set; }
            public double TotalCost { get; set; }
        }

        private List<Entry> pocketMoneyEntries = new List<Entry>();
        private bool isPerPersonMode = false;
        private TripData currentTripData = new TripData();

        DatePicker datePicker = new DatePicker()
        {
            MinimumDate = DateTime.Now,
            Date = DateTime.Now,
        };

        public MainPage()
        {
            InitializeComponent();
            UpdatePocketMoneyLayout();
        }

        private async void SaveData()
        {
            if(!currentTripData.Calculated)
            {
                await DisplayAlert("Hiba", "Kérlek először számold ki az adatokat!", "OK");
                return;
            }
            currentTripData.IsPerPersonMode = isPerPersonMode;
            currentTripData.LastSaved = DateTime.Now;
            currentTripData.TripName = TripName.Text;
            currentTripData.TripDestination = TripDestination.Text;
            currentTripData.TripDateStart = TripDateStart.Date;
            currentTripData.TripDateEnd = TripDateEnd.Date;
            currentTripData.PocketMoney.Clear();
            try
            {
                var json = JsonSerializer.Serialize(currentTripData, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                string tripNameSafe = string.Join("_", currentTripData.TripName.Split(Path.GetInvalidFileNameChars()));
                string timestamp = currentTripData.LastSaved.ToString("yyyyMMdd_HHmmss");
                string safeFileName = $"{tripNameSafe}_{timestamp}.json";

                string targetFile = System.IO.Path.Combine(FileSystem.Current.AppDataDirectory, safeFileName);
                await System.IO.File.WriteAllTextAsync(targetFile, json, Encoding.UTF8);
                await DisplayAlert("Sikeres mentés", "Az adataid el lettek mentve!", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hiba", $"Nem sikerült menteni: {ex.Message}", "OK");
            }
        }

        private void OnEndDateChanged(object sender, DateChangedEventArgs e)
        {
            if (TripDateEnd.Date < TripDateStart.Date)
            {
                TripDateEnd.Date = TripDateStart.Date;
            }
        }

        private void OnStartDateChanged(object sender, DateChangedEventArgs e)
        {
            if(TripDateStart.Date > TripDateEnd.Date)
            {
                TripDateStart.Date = TripDateEnd.Date;
            }
        }

        private void OnSaveClicked(object sender, EventArgs e)
        {
            SaveData();
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
                BackgroundColor = Color.FromHex("#2D2D2D"),  // Frame háttérszíne
                TextColor = Color.FromHex("#FFFFFF"),         // Entry szövegszíne
                PlaceholderColor = Color.FromHex("#C8C8C8"),  // Placeholder színe
                HorizontalOptions = LayoutOptions.FillAndExpand,
                StyleId = "CostType",
                FontFamily = "Arial",
                FontSize = 13,
                CharacterSpacing = 1
            };

            var costAmountEntry = new Entry
            {
                Placeholder = "Összeg (Ft)",
                Keyboard = Keyboard.Numeric,
                BackgroundColor = Color.FromHex("#2D2D2D"),
                TextColor = Color.FromHex("#FFFFFF"),
                PlaceholderColor = Color.FromHex("#C8C8C8"),
                HorizontalOptions = LayoutOptions.FillAndExpand,
                StyleId = "FullCostAmount",
                FontFamily = "Arial",
                FontSize = 13,
                CharacterSpacing = 1
            };

            var numberOfFullCost = new Entry
            {
                Placeholder = "Fő (db)",
                Keyboard = Keyboard.Numeric,
                BackgroundColor = Color.FromHex("#2D2D2D"),
                TextColor = Color.FromHex("#FFFFFF"),
                PlaceholderColor = Color.FromHex("#C8C8C8"),
                HorizontalOptions = LayoutOptions.FillAndExpand,
                StyleId = "FullCostAmount",
                FontFamily = "Arial",
                FontSize = 13,
                CharacterSpacing = 1
            };

            var discountCostAmountEntry = new Entry
            {
                Placeholder = "Kedvezményes összeg (Ft)",
                Keyboard = Keyboard.Numeric,
                BackgroundColor = Color.FromHex("#2D2D2D"),  // Frame háttérszíne
                TextColor = Color.FromHex("#FFFFFF"),         // Entry szövegszíne
                PlaceholderColor = Color.FromHex("#C8C8C8"),  // Placeholder színe
                HorizontalOptions = LayoutOptions.FillAndExpand,
                StyleId = "DiscountCostAmount",
                FontFamily = "Arial",
                FontSize = 13,
                CharacterSpacing = 1
            };

            var numberOfDiscountCost = new Entry
            {
                Placeholder = "Fő (db)",
                Keyboard = Keyboard.Numeric,
                BackgroundColor = Color.FromHex("#2D2D2D"),  // Frame háttérszíne
                TextColor = Color.FromHex("#FFFFFF"),         // Entry szövegszíne
                PlaceholderColor = Color.FromHex("#C8C8C8"),  // Placeholder színe
                HorizontalOptions = LayoutOptions.FillAndExpand,
                StyleId = "DiscountCostAmount",
                FontFamily = "Arial",
                FontSize = 13,
                CharacterSpacing = 1
            };

            var isDiscountAvailable = new CheckBox
            {
                Color = Color.FromHex("#FF9800")
            };

            var discountLabel = new Label
            {
                Text = "Van kedvezmény?",
                VerticalOptions = LayoutOptions.Center,
                TextColor = Color.FromHex("#FFFFFF")
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
                        FontFamily = "Arial",
                        FontSize = 14,
                        TextColor = Color.FromArgb("#FFD700"),
                        Margin = new Thickness(0, i > 1 ? 10 : 0, 0, 0)
                    };

                    var frame = new Frame
                    {
                        CornerRadius = 8,
                        BackgroundColor = Color.FromArgb("#2D2D2D"),
                        BorderColor = Color.FromArgb("#3C3C3C"),
                        HasShadow = false,
                        Padding = 10
                    };

                    var entry = new Entry
                    {
                        Placeholder = "pl. 3000",
                        Keyboard = Keyboard.Numeric,
                        BackgroundColor = Colors.Transparent,
                        TextColor = Color.FromArgb("#FFFFFF"),
                        PlaceholderColor = Color.FromArgb("#C8C8C8"),
                        FontFamily = "Arial",
                        FontSize = 11
                    };

                    pocketMoneyEntries.Add(entry);
                    frame.Content = entry;

                    LayoutPocketMoney.Add(label);
                    LayoutPocketMoney.Add(frame);
                }
            }
            else
            {
                var label = new Label
                {
                    Text = "Átlagos zsebpénz fejenként (Ft):",
                    FontAttributes = FontAttributes.Bold,
                    FontFamily = "Arial",
                    FontSize = 14,
                    TextColor = Color.FromArgb("#FFD700"),
                    Margin = new Thickness(0, 10, 0, 0)
                };

                // Frame a stílushoz
                var frame = new Frame
                {
                    CornerRadius = 8,
                    BackgroundColor = Color.FromArgb("#2D2D2D"),
                    BorderColor = Color.FromArgb("#3C3C3C"),
                    HasShadow = false,
                    Padding = 10
                };

                var entry = new Entry
                {
                    Placeholder = "pl. 3000",
                    Keyboard = Keyboard.Numeric,
                    BackgroundColor = Colors.Transparent,
                    TextColor = Color.FromArgb("#FFFFFF"),
                    PlaceholderColor = Color.FromArgb("#C8C8C8"),
                    FontFamily = "Arial",
                    FontSize = 13,        // ugyanaz, mint a többi Entry
                    CharacterSpacing = 1  // ugyanolyan karakterköz
                };

                frame.Content = entry;           // az Entry a Frame-be kerül
                LayoutPocketMoney.Add(label);    // hozzáadjuk a labelt
                LayoutPocketMoney.Add(frame);    // hozzáadjuk a keretes Entry-t
                pocketMoneyEntries.Add(entry);   // lista a későbbi feldolgozáshoz
            }

            AnimateView(LayoutPocketMoney);
        }

        private async void OnCalculateClicked(object sender, EventArgs e)
        {
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
                    else
                    {
                        await ShowError("Kérlek adj meg érvényes költség összeget minden tételhez!");
                        return;
                    }

                    if (allEntries.Count > 2 && int.TryParse(allEntries[2].Text, out int numberOfPeople))
                        costItem.NumberOfPeople = numberOfPeople;
                    else
                    {
                        await ShowError("Kérlek adj meg érvényes fő számot minden tételhez!");
                        return;
                    }

                    if (allEntries.Count > 3 && double.TryParse(allEntries[3].Text, out double discountAmount))
                        costItem.DiscountAmount = discountAmount;
                    else
                    {
                        costItem.DiscountAmount = 0;
                    }

                    if (allEntries.Count > 4 && int.TryParse(allEntries[4].Text, out int discountPeople))
                        costItem.DiscountNumberOfPeople = discountPeople;
                    else
                    {
                        costItem.DiscountNumberOfPeople = 0;
                    }

                    costItem.HasDiscount = checkBox?.IsChecked ?? false;

                    currentTripData.Costs.Add(costItem);
                }
            }

            currentTripData.Participants = 0;
            if (!int.TryParse(EntryParticipants.Text, out int participants) ||
                participants <= 0 || participants > 100)
            {
                await ShowError("Kérlek adj meg érvényes résztvevő számot (1-100)!");
                return;
            }
            else
            {
                currentTripData.Participants = participants;
            }

            currentTripData.MonthsLeft = 0;
            if (!int.TryParse(EntryMonthsLeft.Text, out int monthsLeft) ||
                monthsLeft <= 0 || monthsLeft > 24)
            {
                await ShowError("Kérlek adj meg érvényes hónapok számát (1-24)!");
                return;
            }
            else
            {
                currentTripData.MonthsLeft = monthsLeft;
            }

            currentTripData.PocketMoney.Clear();
            if (isPerPersonMode)
            {
                for (int i = 0; i < pocketMoneyEntries.Count; i++)
                {
                    if (!double.TryParse(pocketMoneyEntries[i].Text, out double amount) || amount < 0)
                    {
                        await ShowError($"Kérlek add meg a {i + 1}. diák érvényes zsebpénzét!");
                        return;
                    }
                    currentTripData.PocketMoney.Add(amount);
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
                    currentTripData.PocketMoney.Add(amount);
                }
            }

            currentTripData.AveragePocketMoney = 0;
            currentTripData.AveragePocketMoney = currentTripData.PocketMoney.Average();

            currentTripData.IsPerPersonMode = isPerPersonMode;

            double finalCost = currentTripData.Costs.Sum(c =>
                c.Amount * c.NumberOfPeople + (c.HasDiscount ? c.DiscountAmount * c.DiscountNumberOfPeople : 0)
            );
            currentTripData.Calculated = true;
            DisplayResults(finalCost, participants, monthsLeft, currentTripData.PocketMoney);
        }

        private void DisplayResults(double totalCost, int participants, int monthsLeft, List<double> pocketMoneyList)
        {
            LayoutResults.Clear();

            double costPerPerson = totalCost / participants;
            double monthlyPerPerson = costPerPerson / monthsLeft;

            // --- ÖSSZEFOGLALÓ KÁRTYA ---
            var summaryCard = CreateDarkCard("📊 Összefoglaló", "#FFD700");
            var summaryLabel = new Label
            {
                Text = $@"Teljes költség: {FormatNumber(totalCost)} Ft
Résztvevők: {participants} fő
Hátralévő idő: {monthsLeft} hónap

───────────────────────

💰 Fejenként fizetendő:
Összesen: {FormatNumber(costPerPerson)} Ft
Havonta: {FormatNumber(monthlyPerPerson)} Ft",
                FontSize = 13,
                TextColor = Color.FromArgb("#FFFFFF"),
                FontFamily = "Arial",
                Padding = new Thickness(10)
            };
            ((VerticalStackLayout)summaryCard.Content).Add(summaryLabel);
            LayoutResults.Add(summaryCard);


            // --- EGYÉNI ELEMZÉS ---
            var analysisCard = CreateDarkCard("👥 Egyéni elemzés", "#FFD700");
            var analysisLayout = new VerticalStackLayout { Padding = new Thickness(10), Spacing = 10 };

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
                    FontSize = 12,
                    FontFamily = "Arial"
                };

                analysisLayout.Add(studentLabel);

                if (i < pocketMoneyList.Count - 1)
                {
                    analysisLayout.Add(new BoxView
                    {
                        HeightRequest = 1,
                        BackgroundColor = Color.FromArgb("#3C3C3C"),
                        Margin = new Thickness(0, 5)
                    });
                }
            }

    ((VerticalStackLayout)analysisCard.Content).Add(analysisLayout);
            LayoutResults.Add(analysisCard);


            // --- DIAGRAM KÁRTYA ---
            var chartCard = CreateDarkCard("📈 Fedezettségi diagram", "#FFD700");
            var chartView = new PieChartView(pocketMoneyList, costPerPerson, monthsLeft)
            {
                HeightRequest = 300,
                Margin = new Thickness(10)
            };
            ((VerticalStackLayout)chartCard.Content).Add(chartView);
            LayoutResults.Add(chartCard);


            // --- JAVASLATOK / SIKER ---
            if (!allCanPay)
            {
                var suggestionsCard = CreateDarkCard("💡 Javaslatok", "#FFD700");
                var suggestionsLayout = new VerticalStackLayout { Padding = new Thickness(10), Spacing = 10 };

                double totalShortage = cantPayList.Sum(x => x.shortage);

                suggestionsLayout.Add(new Label
                {
                    Text = $"⚠️ {cantPayList.Count} diák nem tudja fedezni a költséget!",
                    FontSize = 13,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#FF6B6B")
                });

                suggestionsLayout.Add(new BoxView { HeightRequest = 1, BackgroundColor = Color.FromArgb("#3C3C3C") });

                double neededReduction = totalShortage;
                suggestionsLayout.Add(new Label
                {
                    Text = $@"1️⃣ Költségcsökkentés
                    Ha {FormatNumber(neededReduction)} Ft-tal csökkentjük a teljes költséget,
                    mindenki tudja fizetni a kirándulást.
                    Új fejenként fizetendő: {FormatNumber(costPerPerson - (neededReduction / participants))} Ft",
                    FontSize = 12,
                    TextColor = Color.FromArgb("#FFFFFF")
                });

                suggestionsLayout.Add(new BoxView { HeightRequest = 1, BackgroundColor = Color.FromArgb("#3C3C3C") });

                double extraPerPerson = totalShortage / (participants - cantPayList.Count);
                suggestionsLayout.Add(new Label
                {
                    Text = $@"2️⃣ Többi diák fizet többet
                    Ha a {participants - cantPayList.Count} másik diák befizeti a hiányt:
                    Extra fejenként: {FormatNumber(extraPerPerson)} Ft
                    Új összeg számukra: {FormatNumber(costPerPerson + extraPerPerson)} Ft",
                    FontSize = 12,
                    TextColor = Color.FromArgb("#FFFFFF")
                });

                suggestionsLayout.Add(new BoxView { HeightRequest = 1, BackgroundColor = Color.FromArgb("#3C3C3C") });

                int neededMonths = (int)Math.Ceiling(costPerPerson / pocketMoneyList.Min());
                if (neededMonths > monthsLeft)
                {
                    suggestionsLayout.Add(new Label
                    {
                        Text = $@"3️⃣ Több idő szükséges
                        Legalább {neededMonths} hónap kellene, hogy mindenki összegyűjtse a pénzt.
                        (Még {neededMonths - monthsLeft} hónap szükséges)",
                        FontSize = 12,
                        TextColor = Color.FromArgb("#FFFFFF")
                    });
                }

                ((VerticalStackLayout)suggestionsCard.Content).Add(suggestionsLayout);
                LayoutResults.Add(suggestionsCard);
            }
            else
            {
                var successCard = CreateDarkCard("🎉 Szuper hír!", "#4CAF50");
                var successLabel = new Label
                {
                    Text = @"✨ Minden diák tudja fizetni a kirándulást!

                    Az osztálykirándulás megvalósítható a megadott feltételekkel.
                    Kezdjétek el gyűjteni a pénzt! 🎒",
                    FontSize = 13,
                    TextColor = Color.FromArgb("#FFFFFF"),
                    FontFamily = "Arial",
                    Padding = new Thickness(10)
                };
                ((VerticalStackLayout)successCard.Content).Add(successLabel);
                LayoutResults.Add(successCard);
            }

            LayoutResults.IsVisible = true;
            AnimateView(LayoutResults);
        }

        private Frame CreateDarkCard(string title, string accentColor)
        {
            var card = new Frame
            {
                CornerRadius = 10,
                BackgroundColor = Color.FromArgb("#1E1E1E"),
                BorderColor = Color.FromArgb("#3C3C3C"),
                HasShadow = false,
                Padding = 10,
                Margin = new Thickness(0, 10)
            };

            var titleLabel = new Label
            {
                Text = title,
                FontAttributes = FontAttributes.Bold,
                FontFamily = "Arial",
                FontSize = 14,
                TextColor = Color.FromArgb(accentColor),
                Margin = new Thickness(0, 0, 0, 5)
            };

            var content = new VerticalStackLayout { Spacing = 5 };
            content.Add(titleLabel);

            card.Content = content;
            return card;
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
        private readonly List<double> pocketMoneyList;
        private readonly double costPerPerson;
        private readonly int monthsLeft;

        public PieChartDrawable(List<double> pocketMoney, double cost, int months)
        {
            pocketMoneyList = pocketMoney;
            costPerPerson = cost;
            monthsLeft = months;
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            if (pocketMoneyList == null || pocketMoneyList.Count == 0)
                return;

            int canPayCount = pocketMoneyList.Count(pm => pm * monthsLeft >= costPerPerson);
            int cantPayCount = pocketMoneyList.Count - canPayCount;

            float total = (float)pocketMoneyList.Count;
            float canPayAngle = (canPayCount / total) * 360f;
            float cantPayAngle = (cantPayCount / total) * 360f;

            float centerX = dirtyRect.Width / 2;
            float centerY = dirtyRect.Height / 2;
            float radius = Math.Min(centerX, centerY) * 0.7f;

            // Rajzoljunk szeleteket
            DrawPieSlice(canvas, centerX, centerY, radius, -90, canPayAngle, Color.FromArgb("#4CAF50"));
            DrawPieSlice(canvas, centerX, centerY, radius, -90 + canPayAngle, cantPayAngle, Color.FromArgb("#F44336"));

            // Fehér vagy fekete középső kör a témától függően
            bool isDarkMode = Application.Current?.RequestedTheme == AppTheme.Dark;
            canvas.FillColor = isDarkMode ? Colors.Black : Colors.White;
            canvas.FillCircle(centerX, centerY, radius * 0.6f);


            // Szövegek
            canvas.FontColor = Color.FromArgb("#4CAF50");
            canvas.FontSize = radius * 0.16f;
            canvas.DrawString($"{canPayCount} fő", centerX, centerY - radius * 0.08f, HorizontalAlignment.Center);
            canvas.FontSize = radius * 0.12f;
            canvas.DrawString("tudja fizetni", centerX, centerY + radius * 0.02f, HorizontalAlignment.Center);

            if (cantPayCount > 0)
            {
                canvas.FontColor = Color.FromArgb("#F44336");
                canvas.FontSize = radius * 0.16f;
                canvas.DrawString($"{cantPayCount} fő", centerX, centerY + radius * 0.18f, HorizontalAlignment.Center);
                canvas.FontSize = radius * 0.12f;
                canvas.DrawString("nem tudja", centerX, centerY + radius * 0.28f, HorizontalAlignment.Center);
            }
        }

        private void DrawPieSlice(ICanvas canvas, float cx, float cy, float radius, float startAngle, float sweepAngle, Color color)
        {
            var path = new PathF();
            path.MoveTo(cx, cy);

            int steps = 100;
            float angleStep = sweepAngle / steps;

            for (int i = 0; i <= steps; i++)
            {
                float angle = (startAngle + i * angleStep) * (float)(Math.PI / 180);
                float x = cx + radius * (float)Math.Cos(angle);
                float y = cy + radius * (float)Math.Sin(angle);
                path.LineTo(x, y);
            }

            path.Close();

            canvas.FillColor = color;
            canvas.FillPath(path);
        }
    }
}
