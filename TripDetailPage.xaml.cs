using System.Collections.ObjectModel;
using System.Globalization;
using PdfSharpCore.Drawing;
using PdfSharpCore.Fonts;

namespace OKKT25
{
    public partial class TripDetailPage : ContentPage
    {
        private MainPage.TripData tripData;
        private ObservableCollection<ImageSource> photoSources = new();

        public TripDetailPage(MainPage.TripData data, string tripName)
        {
            InitializeComponent();

            string tripFileName = $"{tripName}.json";
            string filePath = Path.Combine(FileSystem.Current.AppDataDirectory, tripFileName);

            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                tripData = System.Text.Json.JsonSerializer.Deserialize<MainPage.TripData>(json);
            }
            else
            {
                tripData = data;
            }

            Title = tripName;
            DisplayTripDetails();
            PhotosCollection.ItemsSource = photoSources;
            DisplayPhotos();
            UpdatePhotosLabel();
        }

        private void UpdatePhotosLabel()
        {
            if (tripData.PhotoPaths != null)
                PhotosLabel.Text = $"Fotók: {tripData.PhotoPaths.Count}";
            else
                PhotosLabel.Text = "Fotók: 0";
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
                    string targetPath = Path.Combine(FileSystem.Current.AppDataDirectory, Path.GetFileName(result.FullPath));

                    using (var sourceStream = await result.OpenReadAsync())
                    using (var targetStream = File.Create(targetPath))
                    {
                        await sourceStream.CopyToAsync(targetStream);
                    }

                    photoSources.Add(ImageSource.FromFile(targetPath));

                    // 🔹 Hozzáadjuk a képet az adott kiránduláshoz
                    tripData.PhotoPaths.Add(targetPath);

                    // 🔹 Mentjük a frissített adatokat fájlba
                    UpdatePhotosLabel();
                    await SaveTripDataAsync();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hiba", $"Nem sikerült a művelet: {ex.Message}", "OK");
            }
        }

        private async Task SaveTripDataAsync()
        {
            try
            {
                string json = System.Text.Json.JsonSerializer.Serialize(tripData,
                    new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

                string tripFileName = $"{tripData.TripName}.json";
                string filePath = Path.Combine(FileSystem.Current.AppDataDirectory, tripFileName);

                await File.WriteAllTextAsync(filePath, json);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hiba", $"Nem sikerült menteni az adatokat: {ex.Message}", "OK");
            }
        }

        private void DisplayTripDetails()
        {
            DetailLayout.Clear();

            // Kirándulás címe
            var titleLabel = new Label
            {
                Text = Title,
                FontSize = 26,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#FFD700"),
                Margin = new Thickness(0, 0, 0, 20)
            };
            DetailLayout.Add(titleLabel);

            double totalCost = tripData.Costs.Sum(c =>
                (c.Amount * c.NumberOfPeople) +
                (c.DiscountAmount * c.DiscountNumberOfPeople));

            // Összefoglaló kártya
            var summaryCard = CreateCard("📊 Összefoglaló", "#FF9800");
            var summaryLayout = new VerticalStackLayout { Padding = 15, Spacing = 6 };

            summaryLayout.Add(new Label { Text = $"Teljes költség: {FormatNumber(totalCost)} Ft", FontSize = 16, TextColor = Color.FromArgb("#FFFFFF") });
            summaryLayout.Add(new Label { Text = $"Résztvevők: {tripData.Participants} fő", FontSize = 16, TextColor = Color.FromArgb("#FFFFFF") });
            summaryLayout.Add(new Label { Text = $"Hátralévő idő: {tripData.MonthsLeft} hónap", FontSize = 16, TextColor = Color.FromArgb("#FFFFFF") });
            summaryLayout.Add(new Label { Text = $"Mentve: {tripData.LastSaved:yyyy.MM.dd HH:mm}", FontSize = 14, TextColor = Color.FromArgb("#C8C8C8") });

            ((VerticalStackLayout)summaryCard.Content).Add(summaryLayout);
            DetailLayout.Add(summaryCard);

            // Költségek kártya
            var costsCard = CreateCard("💰 Költségek részletezve", "#FF9800");
            var costsLayout = new VerticalStackLayout { Padding = 15, Spacing = 8 };

            if (tripData.Costs.Count == 0)
            {
                costsLayout.Add(new Label { Text = "Nincsenek rögzített költségek.", FontSize = 14, TextColor = Color.FromArgb("#C8C8C8") });
            }
            else
            {
                foreach (var cost in tripData.Costs)
                {
                    var costHeader = new Label
                    {
                        Text = $"{cost.Type} (×{cost.NumberOfPeople} fő)",
                        FontSize = 15,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Color.FromArgb("#FFD700")
                    };
                    costsLayout.Add(costHeader);

                    var costAmount = new Label
                    {
                        Text = $"• Ár/fő: {FormatNumber(cost.Amount)} Ft",
                        FontSize = 14,
                        TextColor = Color.FromArgb("#FFFFFF")
                    };
                    costsLayout.Add(costAmount);

                    if (cost.HasDiscount)
                    {
                        var discountLabel = new Label
                        {
                            Text = $"• Kedvezmény: {FormatNumber(cost.DiscountAmount)} Ft × {cost.DiscountNumberOfPeople} fő",
                            FontSize = 14,
                            TextColor = Color.FromArgb("#FF9800")
                        };
                        costsLayout.Add(discountLabel);
                    }

                    var totalForCost = new Label
                    {
                        Text = $"• Összesen: {FormatNumber((cost.Amount * cost.NumberOfPeople) + (cost.DiscountAmount * cost.DiscountNumberOfPeople))} Ft",
                        FontSize = 14,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Color.FromArgb("#FFD700")
                    };
                    costsLayout.Add(totalForCost);

                    costsLayout.Add(new BoxView { HeightRequest = 1, BackgroundColor = Color.FromArgb("#3C3C3C"), Margin = new Thickness(0, 5, 0, 5) });
                }
            }

            ((VerticalStackLayout)costsCard.Content).Add(costsLayout);
            DetailLayout.Add(costsCard);

            // Zsebpénz kártya
            var pocketMoneyCard = CreateCard("💵 Zsebpénz", "#FF9800");
            var pocketMoneyLayout = new VerticalStackLayout { Padding = 15, Spacing = 6 };

            pocketMoneyLayout.Add(new Label
            {
                Text = tripData.IsPerPersonMode
                    ? $"Személyenként megadva ({tripData.PocketMoney.Count} diák adatai mentve)"
                    : $"Átlagos zsebpénz: {FormatNumber(tripData.AveragePocketMoney)} Ft/fő",
                FontSize = 14,
                TextColor = Color.FromArgb("#FFFFFF")
            });

            ((VerticalStackLayout)pocketMoneyCard.Content).Add(pocketMoneyLayout);
            DetailLayout.Add(pocketMoneyCard);
        }

        private Frame CreateCard(string title, string colorHex)
        {
            var frame = new Frame
            {
                CornerRadius = 15,
                HasShadow = true,
                BackgroundColor = Color.FromArgb("#1E1E1E"),
                BorderColor = Color.FromArgb("#3C3C3C"),
                Padding = 0,
                Margin = new Thickness(0, 0, 0, 12)
            };

            var container = new VerticalStackLayout();
            var titleLabel = new Label
            {
                Text = title,
                FontSize = 18,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#FFFFFF"),
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

        private async Task<string> CaptureContentAsImage()
        {
            try
            {
                // Képernyőkép készítése a teljes oldalról
                var screenshot = await this.CaptureAsync();

                if (screenshot == null)
                    return null;

                // Elmentjük ideiglenes fájlba
                string tempPath = Path.Combine(FileSystem.Current.CacheDirectory, $"temp_export_{Guid.NewGuid()}.png");

                using (var stream = await screenshot.OpenReadAsync())
                using (var fileStream = File.Create(tempPath))
                {
                    await stream.CopyToAsync(fileStream);
                }

                return tempPath;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hiba", $"Képernyőkép hiba: {ex.Message}", "OK");
                return null;
            }
        }

        private async Task ExportPageToPdf()
        {
            try
            {
                // Először elkészítjük a képet a tartalmunkról
                var screenshotPath = await CaptureContentAsImage();

                if (string.IsNullOrEmpty(screenshotPath))
                {
                    await DisplayAlert("Hiba", "Nem sikerült elkészíteni a képernyőképet", "OK");
                    return;
                }

                string pdfFileName = $"{tripData.TripName}.pdf";
                string filePath = "";

                // ✅ PLATFORMFÜGGŐ ÚTVONAL
#if ANDROID
                var downloadsPath = Android.OS.Environment
                    .GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads)
                    .AbsolutePath;
                filePath = Path.Combine(downloadsPath, pdfFileName);
#elif WINDOWS
                var downloadsPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "Downloads");
                filePath = Path.Combine(downloadsPath, pdfFileName);
#else
                filePath = Path.Combine(FileSystem.Current.AppDataDirectory, pdfFileName);
#endif

                using (var document = new PdfSharpCore.Pdf.PdfDocument())
                {
                    var page = document.AddPage();
                    page.Size = PdfSharpCore.PageSize.A4;

                    using (var gfx = XGraphics.FromPdfPage(page))
                    {
                        // Betöltjük a képet
                        using (var img = XImage.FromFile(screenshotPath))
                        {
                            // Kiszámoljuk a megfelelő méretet (A4-re igazítva)
                            double pageWidth = page.Width.Point;
                            double pageHeight = page.Height.Point;
                            double margin = 40;

                            double maxWidth = pageWidth - (2 * margin);
                            double maxHeight = pageHeight - (2 * margin);

                            double scale = Math.Min(maxWidth / img.PixelWidth, maxHeight / img.PixelHeight);
                            double imgWidth = img.PixelWidth * scale;
                            double imgHeight = img.PixelHeight * scale;

                            // Középre igazítva
                            double x = (pageWidth - imgWidth) / 2;
                            double y = margin;

                            // Rajzoljuk a képet a PDF-be
                            gfx.DrawImage(img, x, y, imgWidth, imgHeight);
                        }
                    }

                    document.Save(filePath);
                }

                // Töröljük az ideiglenes képet
                if (File.Exists(screenshotPath))
                {
                    File.Delete(screenshotPath);
                }

                await DisplayAlert("✅ Siker", $"PDF elkészült!\n📁 {filePath}", "Rendben");
            }
            catch (Exception ex)
            {
                await DisplayAlert("❌ Hiba", $"Nem sikerült exportálni a PDF-et:\n{ex.Message}", "OK");
            }
        }

        private async void OnExportPdfClicked(object sender, EventArgs e)
        {
            await ExportPageToPdf();
        }

        private async void OnDeleteTripClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert(
                "Kirándulás törlése",
                "Biztosan törölni szeretnéd ezt a kirándulást és az összes hozzá tartozó fotót?",
                "Igen",
                "Mégse");

            if (!confirm)
                return;

            try
            {
                // A fájl neve most csak a tripName
                string tripNameSafe = string.Join("_", tripData.TripName.Split(Path.GetInvalidFileNameChars()));
                string filePath = Path.Combine(FileSystem.Current.AppDataDirectory, $"{tripNameSafe}.json");

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                // Fotók törlése
                if (tripData.PhotoPaths != null)
                {
                    foreach (var photoPath in tripData.PhotoPaths)
                    {
                        if (File.Exists(photoPath))
                            File.Delete(photoPath);
                    }
                }

                await DisplayAlert("Siker", "A kirándulás törölve lett.", "OK");

                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hiba", $"Nem sikerült törölni a kirándulást: {ex.Message}", "OK");
            }
        }
    }
}