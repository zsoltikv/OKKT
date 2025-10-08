using System.Collections.ObjectModel;
using System.Globalization;

namespace OKKT25
{
    public partial class TripDetailPage : ContentPage
    {
        private MainPage.TripData tripData;
        private ObservableCollection<ImageSource> photoSources = new();

        public TripDetailPage(MainPage.TripData data, string tripName)
        {
            InitializeComponent();
            tripData = data;
            Title = tripName;
            DisplayTripDetails();
            PhotosCollection.ItemsSource = photoSources;
            DisplayPhotos();
        }

        private void DisplayPhotos()
        {
            PhotosCollection.ItemsSource = null;

            // 🔹 betöltjük a korábban elmentett képeket
            if (tripData.PhotoPaths != null)
            {
                foreach (var path in tripData.PhotoPaths)
                {
                    if (File.Exists(path))
                    {
                        photoSources.Add(ImageSource.FromFile(path));
                    }
                }
            }

            // 🔹 újra frissítjük az ItemsSource-t
            PhotosCollection.ItemsSource = photoSources;

            // 🔹 minden képre kattintási esemény (nagyban megnyitás)
            PhotosCollection.RemainingItemsThresholdReached += (s, e) =>
            {
                // ez csak akkor kell, ha lapozást használsz
            };

            PhotosCollection.ItemTemplate = new DataTemplate(() =>
            {
                var image = new Image
                {
                    HeightRequest = 120,
                    WidthRequest = 120,
                    Aspect = Aspect.AspectFill
                };

                image.SetBinding(Image.SourceProperty, ".");
                var tapGesture = new TapGestureRecognizer();
                tapGesture.Tapped += async (s, e) =>
                {
                    var imgSource = ((Image)s).Source;
                    await Navigation.PushModalAsync(new ImageViewPage(imgSource));
                };
                image.GestureRecognizers.Add(tapGesture);

                return new Frame
                {
                    Padding = 5,
                    HasShadow = false,
                    BackgroundColor = Colors.Transparent,
                    Content = image
                };
            });
        }

        private async void OnAddPhotoClicked(object sender, EventArgs e)
        {
            string action = await DisplayActionSheet("Fénykép hozzáadása", "Mégse", null, "📷 Kamera", "🖼️ Galéria");

            FileResult result = null;

            try
            {
                if (action == "📷 Kamera")
                {
                    result = await MediaPicker.CapturePhotoAsync();
                }
                else if (action == "🖼️ Galéria")
                {
                    result = await MediaPicker.PickPhotoAsync();
                }

                if (result != null)
                {
                    // 🔹 Mentés az alkalmazás adatkönyvtárába
                    string targetPath = Path.Combine(FileSystem.Current.AppDataDirectory, Path.GetFileName(result.FullPath));

                    using (var sourceStream = await result.OpenReadAsync())
                    using (var targetStream = File.Create(targetPath))
                    {
                        await sourceStream.CopyToAsync(targetStream);
                    }

                    // 🔹 Hozzáadjuk a képet az ObservableCollection-höz fájlelérési útként
                    photoSources.Add(ImageSource.FromFile(targetPath));
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hiba", $"Nem sikerült a művelet: {ex.Message}", "OK");
            }
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
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