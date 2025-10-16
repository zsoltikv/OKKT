using System.Globalization;
using System.Text;
using System.Text.Json;
using OKKT25.Models;

namespace OKKT25
{
    public partial class MainPage : ContentPage
    {

        private List<Entry> pocketMoneyEntries = new List<Entry>();
        private bool isPerPersonMode = false;
        private TripData currentTripData = new TripData();

        public MainPage()
        {
            InitializeComponent();
            UpdatePocketMoneyLayout();
            TripDateStart.MinimumDate = DateTime.Now;
            TripDateEnd.MinimumDate = TripDateStart.Date;
            TripDateStart.MaximumDate = DateTime.Now.AddYears(6);
            TripDateEnd.MaximumDate = TripDateStart.Date.AddYears(6);
            RadioGrouped.SetRadioButtonCheckedColor(Color.FromHex("#FF9800"));
            RadioPerPerson.SetRadioButtonCheckedColor(Color.FromHex("#FF9800"));
        }

        private async void SaveData()
        {
            if (!currentTripData.Calculated)
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
                string safeFileName = $"{tripNameSafe}.json";
                string targetFile = Path.Combine(FileSystem.Current.AppDataDirectory, safeFileName);
                await File.WriteAllTextAsync(targetFile, json, Encoding.UTF8);
                await DisplayAlert("Sikeres mentés", "Az adatok el lettek mentve!", "OK");
                ClearAllInputs();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hiba", $"Nem sikerült menteni: {ex.Message}", "OK");
            }
        }

        private void ClearAllInputs()
        {
            TripName.Text = string.Empty;
            TripDestination.Text = string.Empty;
            TripDateStart.Date = DateTime.Now;
            TripDateEnd.Date = DateTime.Now;

            foreach (var entry in pocketMoneyEntries)
            {
                entry.Text = string.Empty;
            }

            DynamicCostsLayout.Children.Clear();
            LayoutResults.Clear();
            LayoutResults.IsVisible = false;
            currentTripData = new TripData();
            isPerPersonMode = false;
            UpdatePocketMoneyLayout();
            LayoutResults.IsVisible = false;

            if (DynamicCostsLayout.Parent is Frame costsFrame)
            {
                costsFrame.IsVisible = false;
            }

            if (LayoutResults.Parent is Frame resultsFrame)
            {
                resultsFrame.IsVisible = false;
            }

        }

        private void ButtonResetClicked(object sender, EventArgs e)
        {
            ClearAllInputs();
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
            TripDateEnd.Date = TripDateStart.Date;
        }

        private void OnSaveClicked(object sender, EventArgs e)
        {
            SaveData();
        }

        [Obsolete]
        private void OnAddCostClicked(object sender, EventArgs e)
        {
            if (DynamicCostsLayout.Parent is Frame costsFrame)
            {
                costsFrame.IsVisible = true;
            }

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
                BackgroundColor = Color.FromHex("#2D2D2D"),
                TextColor = Color.FromHex("#FFFFFF"),
                PlaceholderColor = Color.FromHex("#C8C8C8"),
                HorizontalOptions = LayoutOptions.FillAndExpand,
                StyleId = "CostType",
                FontFamily = "Arial",
                FontSize = 13,
                CharacterSpacing = 1
            };

            var costAmountEntry = new Entry
            {
                Placeholder = "Összeg/fő (Ft)",
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
                Placeholder = "Darabszám",
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
                BackgroundColor = Color.FromHex("#2D2D2D"),
                TextColor = Color.FromHex("#FFFFFF"),
                PlaceholderColor = Color.FromHex("#C8C8C8"),
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
                BackgroundColor = Color.FromHex("#2D2D2D"),
                TextColor = Color.FromHex("#FFFFFF"),
                PlaceholderColor = Color.FromHex("#C8C8C8"),
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
                TextColor = Color.FromHex("#C8C8C8")
            };

            var checkBoxLayout = new HorizontalStackLayout
            {
                Spacing = -8,
                VerticalOptions = LayoutOptions.Center,
                Children = { isDiscountAvailable, discountLabel }
            };

            isDiscountAvailable.CheckedChanged += (s, e) =>
            {
                thirdRowLayout.IsVisible = isDiscountAvailable.IsChecked;
            };

            var removeButton = new Button
            {
                Text = "×",
                BackgroundColor = Color.FromHex("#FF9800"),
                TextColor = Color.FromHex("#424242"),
                WidthRequest = 40,
                HeightRequest = 40,
                BorderColor = Colors.Black,  
                BorderWidth = 1.5,                
            };

            removeButton.Clicked += (s, eArgs) =>
            {
                DynamicCostsLayout.Children.Remove(newCostLayout);

                if (DynamicCostsLayout.Children.Count == 0 && DynamicCostsLayout.Parent is Frame costsFrame)
                {
                    costsFrame.IsVisible = false;
                }
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
                int participants = 0;
                var firstCostLayout = DynamicCostsLayout.Children.OfType<StackLayout>().FirstOrDefault();

                if (firstCostLayout != null)
                {
                    var allEntries = firstCostLayout.Children
                        .OfType<StackLayout>()
                        .SelectMany(sl => sl.Children.OfType<Entry>())
                        .ToList();

                    var checkBox = firstCostLayout.Children
                        .OfType<StackLayout>()
                        .SelectMany(sl => sl.Children.OfType<CheckBox>())
                        .FirstOrDefault();

                    if (allEntries.Count >= 3 && int.TryParse(allEntries[2].Text, out int numberOfPeople))
                    {
                        participants = numberOfPeople;
                        if (checkBox?.IsChecked == true && allEntries.Count >= 5 && int.TryParse(allEntries[4].Text, out int discountNumberOfPeople))
                        {
                            participants += discountNumberOfPeople;
                        }
                    }
                }

                if (participants <= 0 || participants > 100)
                {
                    var warningLabel = new Label
                    {
                        Text = "⚠️ Add meg a költségek menüpontban, hányan mennek (a fő mezőkben)!",
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
                    TextColor = Color.FromArgb("#FFD700")
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
                    FontSize = 13,
                    CharacterSpacing = 1
                };

                frame.Content = entry;
                LayoutPocketMoney.Add(label);
                LayoutPocketMoney.Add(frame);
                pocketMoneyEntries.Add(entry);
            }

            AnimateView(LayoutPocketMoney);
        }

        private async void OnCalculateClicked(object sender, EventArgs e)
        {

            LayoutResults.IsVisible = false;

            currentTripData.MonthsLeft = TripDateStart.Date.Month - DateTime.Now.Month +
                                         12 * (TripDateStart.Date.Year - DateTime.Now.Year);
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
                        costItem.DiscountAmount = 0;

                    if (allEntries.Count > 4 && int.TryParse(allEntries[4].Text, out int discountPeople))
                        costItem.DiscountNumberOfPeople = discountPeople;
                    else
                        costItem.DiscountNumberOfPeople = 0;

                    costItem.HasDiscount = checkBox?.IsChecked ?? false;
                    currentTripData.Costs.Add(costItem);
                }
            }

            int participants = currentTripData.Costs.Any()
                ? currentTripData.Costs.Max(c => c.NumberOfPeople + (c.HasDiscount ? c.DiscountNumberOfPeople : 0))
                : 0;

            if (participants <= 0)
            {
                await ShowError("Nem adtál meg érvényes létszámot a költségek mezőknél!");
                return;
            }

            currentTripData.Participants = participants;
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

            currentTripData.AveragePocketMoney = currentTripData.PocketMoney.Average();
            currentTripData.IsPerPersonMode = isPerPersonMode;
            double finalCost = currentTripData.Costs.Sum(c =>
                c.Amount * c.NumberOfPeople + (c.HasDiscount ? c.DiscountAmount * c.DiscountNumberOfPeople : 0));
            currentTripData.Calculated = true;

            if (currentTripData.Calculated)
            {
                DisplayResults(finalCost, participants, currentTripData.MonthsLeft, currentTripData.PocketMoney);
            }

        }

        private void DisplayResults(double totalCost, int participants, int monthsLeft, List<double> pocketMoneyList)
        {

            LayoutResults.Clear();
            double costPerPerson = totalCost / participants;
            double monthlyPerPerson = costPerPerson / monthsLeft;

            var summaryCard = CreateDarkCard("📊 Összefoglaló", "#FFD700");
            var summaryLabel = new Label
            {
                FormattedText = new FormattedString
                {
                    Spans =
                    {
                        new Span { Text = "📊 Összegzés\n\n", FontAttributes = FontAttributes.Bold, FontSize = 15 },
                        new Span { Text = "Teljes költség: ", FontAttributes = FontAttributes.Bold },
                        new Span { Text = $"{FormatNumber(totalCost)} Ft\n" },
                        new Span { Text = "Résztvevők: ", FontAttributes = FontAttributes.Bold },
                        new Span { Text = $"{participants} fő\n" },
                        new Span { Text = "Hátralévő idő: ", FontAttributes = FontAttributes.Bold },
                        new Span { Text = $"{monthsLeft} hónap\n\n" },
                        new Span { Text = "───────────────────────\n\n", FontSize = 12, TextColor = Color.FromArgb("#CCCCCC") },
                        new Span { Text = "💰 Fejenként fizetendő\n\n", FontAttributes = FontAttributes.Bold, FontSize = 14 },
                        new Span { Text = "Összesen: ", FontAttributes = FontAttributes.Bold },
                        new Span { Text = $"{FormatNumber(costPerPerson)} Ft\n" },
                        new Span { Text = "Havonta: ", FontAttributes = FontAttributes.Bold },
                        new Span { Text = $"{FormatNumber(monthlyPerPerson)} Ft" }
                    }
                },
                TextColor = Color.FromArgb("#FFFFFF"),
                FontFamily = "Arial",
                Padding = new Thickness(10),
                LineHeight = 1.3
            };
            ((VerticalStackLayout)summaryCard.Content).Add(summaryLabel);
            LayoutResults.Add(summaryCard);

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
                    FormattedText = new FormattedString
                    {
                        Spans =
                        {
                            new Span { Text = $"{statusIcon} {i + 1}. diák\n", FontAttributes = FontAttributes.Bold, FontSize = 13 },
                            new Span { Text = "💵 Havi zsebpénz: ", FontAttributes = FontAttributes.Bold },
                            new Span { Text = $"{FormatNumber(pocketMoney)} Ft\n" },
                            new Span { Text = "📅 Összesen ", FontAttributes = FontAttributes.Bold },
                            new Span { Text = $"{monthsLeft} hónap alatt: ", FontAttributes = FontAttributes.None },
                            new Span { Text = $"{FormatNumber(monthlyTotal)} Ft\n" },
                            new Span { Text = "💰 Fizetendő: ", FontAttributes = FontAttributes.Bold },
                            new Span { Text = $"{FormatNumber(costPerPerson)} Ft\n\n" },
                            canPay
                                ? new Span { Text = "✅ Fedezi a költséget!", FontAttributes = FontAttributes.Bold }
                                : new Span { Text = $"⚠️ Hiány: {FormatNumber(costPerPerson - monthlyTotal)} Ft", FontAttributes = FontAttributes.Bold }
                        }
                    },
                    TextColor = statusColor,
                    FontSize = 12,
                    FontFamily = "Arial",
                    LineHeight = 1.3,
                    Padding = new Thickness(5, 3)
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

            var chartCard = CreateDarkCard("📈 Fedezettségi diagram", "#FFD700");
            var chartView = new PieChartView(pocketMoneyList, costPerPerson, monthsLeft)
            {
                HeightRequest = 300,
                Margin = new Thickness(10)
            };
            ((VerticalStackLayout)chartCard.Content).Add(chartView);
            LayoutResults.Add(chartCard);

            if (!allCanPay)
            {
                var suggestionsCard = CreateDarkCard("💡 Javaslatok", "#FFD700");
                var suggestionsLayout = new VerticalStackLayout { Padding = new Thickness(10), Spacing = 10 };
                double totalShortage = cantPayList.Sum(x => x.shortage);

                suggestionsLayout.Add(new Label
                {
                    FormattedText = new FormattedString
                    {
                        Spans =
                        {
                            new Span { Text = $"⚠️ {cantPayList.Count} diák nem tudja fedezni a költséget!", FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#FF6B6B"), FontSize = 13 }
                        }
                    }
                });

                suggestionsLayout.Add(new BoxView { HeightRequest = 1, BackgroundColor = Color.FromArgb("#3C3C3C") });

                double neededReduction = totalShortage;
                suggestionsLayout.Add(new Label
                {
                    FormattedText = new FormattedString
                    {
                        Spans =
                        {
                            new Span { Text = "1️⃣ Költségcsökkentés\n", FontAttributes = FontAttributes.Bold, FontSize = 13 },
                            new Span { Text = $"Ha {FormatNumber(neededReduction)} Ft-tal csökkentjük a teljes költséget, mindenki tudja fizetni a kirándulást.\n" },
                            new Span { Text = $"Új fejenként fizetendő: {FormatNumber(costPerPerson - (neededReduction / participants))} Ft", FontAttributes = FontAttributes.Bold }
                        }
                    },
                    FontSize = 12,
                    TextColor = Color.FromArgb("#FFFFFF")
                });

                suggestionsLayout.Add(new BoxView { HeightRequest = 1, BackgroundColor = Color.FromArgb("#3C3C3C") });

                double extraPerPerson = totalShortage / (participants - cantPayList.Count);
                suggestionsLayout.Add(new Label
                {
                    FormattedText = new FormattedString
                    {
                        Spans =
                        {
                            new Span { Text = "2️⃣ Többi diák fizet többet\n", FontAttributes = FontAttributes.Bold, FontSize = 13 },
                            new Span { Text = $"Ha a {participants - cantPayList.Count} másik diák befizeti a hiányt:\n" },
                            new Span { Text = $"Extra fejenként: {FormatNumber(extraPerPerson)} Ft\n", FontAttributes = FontAttributes.Bold },
                            new Span { Text = $"Új összeg számukra: {FormatNumber(costPerPerson + extraPerPerson)} Ft", FontAttributes = FontAttributes.Bold }
                        }
                    },
                    FontSize = 12,
                    TextColor = Color.FromArgb("#FFFFFF")
                });

                suggestionsLayout.Add(new BoxView { HeightRequest = 1, BackgroundColor = Color.FromArgb("#3C3C3C") });

                int neededMonths = (int)Math.Ceiling(costPerPerson / pocketMoneyList.Min());
                if (neededMonths > monthsLeft)
                {
                    suggestionsLayout.Add(new Label
                    {
                        FormattedText = new FormattedString
                        {
                            Spans =
                            {
                                new Span { Text = "3️⃣ Több idő szükséges\n", FontAttributes = FontAttributes.Bold, FontSize = 13 },
                                new Span { Text = $"Legalább {neededMonths} hónap kellene, hogy mindenki összegyűjtse a pénzt.\n" },
                                new Span { Text = $"(Még {neededMonths - monthsLeft} hónap szükséges)", FontAttributes = FontAttributes.Bold }
                            }
                        },
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
                    FormattedText = new FormattedString
                    {
                        Spans =
                        {
                            new Span { Text = "✨ Minden diák tudja fizetni a kirándulást!\n\n", FontAttributes = FontAttributes.Bold, FontSize = 14 },
                            new Span { Text = "Az osztálykirándulás megvalósítható a megadott feltételekkel.\n" },
                            new Span { Text = "Kezdjétek el gyűjteni a pénzt! 🎒", FontAttributes = FontAttributes.Bold }
                        }
                    },
                    FontSize = 13,
                    TextColor = Color.FromArgb("#FFFFFF"),
                    FontFamily = "Arial",
                    Padding = new Thickness(10),
                    LineHeight = 1.3
                };
                ((VerticalStackLayout)successCard.Content).Add(successLabel);
                LayoutResults.Add(successCard);
            }

            LayoutResults.IsVisible = true;

            if (LayoutResults.Parent is Frame resultsFrame)
            {
                resultsFrame.IsVisible = true;
            }

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

            DrawPieSlice(canvas, centerX, centerY, radius, -90, canPayAngle, Color.FromArgb("#4CAF50"));
            DrawPieSlice(canvas, centerX, centerY, radius, -90 + canPayAngle, cantPayAngle, Color.FromArgb("#F44336"));

            bool isDarkMode = Application.Current?.RequestedTheme == AppTheme.Dark;
            Application.Current.Resources.TryGetValue("BlackBg", out var blackBg);
            canvas.FillColor = isDarkMode ? (Color)blackBg : Colors.White;
            canvas.FillCircle(centerX, centerY, radius * 0.6f);

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