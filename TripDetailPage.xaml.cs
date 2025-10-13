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
        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
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
        private async Task ExportPageToPdf()
        {
            try
            {
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
                    var pdfPage = document.AddPage();
                    pdfPage.Size = PdfSharpCore.PageSize.A4;

                    using (var gfx = PdfSharpCore.Drawing.XGraphics.FromPdfPage(pdfPage))
                    {
                        double margin = 50;
                        double yPoint = margin;
                        double pageWidth = pdfPage.Width.Point;
                        double contentWidth = pageWidth - (2 * margin);

                        // 🎨 Fekete–arany színséma
                        var colorBackground = XColor.FromArgb(18, 18, 18);     // háttér
                        var colorCard = XColor.FromArgb(30, 30, 30);           // kártyák
                        var colorPrimary = XColor.FromArgb(255, 215, 0);       // arany
                        var colorAccent = XColor.FromArgb(255, 152, 0);        // narancs
                        var colorText = XColor.FromArgb(255, 255, 255);        // fehér
                        var colorSubText = XColor.FromArgb(200, 200, 200);     // világosszürke
                        var colorBorder = XColor.FromArgb(60, 60, 60);         // finom keret
                        var colorHighlight = XColor.FromArgb(255, 165, 0);     // extra narancs kiemelés

                        // Fontok
                        var fontTitle = new XFont("Arial Black", 28, XFontStyle.Bold);
                        var fontHeader = new XFont("Arial", 16, XFontStyle.Bold);
                        var fontSubHeader = new XFont("Arial", 14, XFontStyle.Bold);
                        var fontNormal = new XFont("Arial", 11, XFontStyle.Regular);
                        var fontBold = new XFont("Arial", 11, XFontStyle.Bold);
                        var fontSmall = new XFont("Arial", 9, XFontStyle.Regular);

                        // 🖤 Háttér
                        gfx.DrawRectangle(new XSolidBrush(colorBackground), 0, 0, pageWidth, pdfPage.Height);

                        // Fejléc
                        gfx.DrawRectangle(new XSolidBrush(colorPrimary), 0, 0, pageWidth, 80);
                        gfx.DrawString(tripData.TripName.ToUpper(), fontTitle, new XSolidBrush(colorBackground),
                            new XPoint(margin, 50));

                        yPoint = 110;

                        // 📍 Helyszín és időpont kártya
                        double infoBoxHeight = 60;
                        gfx.DrawRectangle(new XSolidBrush(colorCard), margin, yPoint, contentWidth, infoBoxHeight);
                        gfx.DrawRectangle(new XPen(colorBorder, 1.5), margin, yPoint, contentWidth, infoBoxHeight);

                        string locationText = !string.IsNullOrEmpty(tripData.TripDestination)
                            ? tripData.TripDestination
                            : "Nincs megadva";
                        gfx.DrawString("Helyszín:", fontBold, new XSolidBrush(colorPrimary),
                            new XPoint(margin + 15, yPoint + 25));
                        gfx.DrawString(locationText, fontNormal, new XSolidBrush(colorText),
                            new XPoint(margin + 100, yPoint + 25));

                        string dateText = tripData.TripDateStart != default
                            ? $"{tripData.TripDateStart:yyyy.MM.dd} - {tripData.TripDateEnd:yyyy.MM.dd}"
                            : "Nincs megadva";
                        gfx.DrawString("Időpont:", fontBold, new XSolidBrush(colorPrimary),
                            new XPoint(margin + 15, yPoint + 45));
                        gfx.DrawString(dateText, fontNormal, new XSolidBrush(colorText),
                            new XPoint(margin + 100, yPoint + 45));

                        yPoint += infoBoxHeight + 25;

                        // 📊 Összesítő kártyák
                        double totalCost = tripData.Costs.Sum(c => (c.Amount * c.NumberOfPeople) + (c.DiscountAmount * c.DiscountNumberOfPeople));
                        double costPerPerson = tripData.Participants > 0 ? totalCost / tripData.Participants : 0;

                        double cardWidth = (contentWidth - 20) / 3;
                        double cardHeight = 80;
                        double cardSpacing = 10;

                        DrawInfoCard(gfx, margin, yPoint, cardWidth, cardHeight, "RÉSZTVEVŐK", tripData.Participants + " fő", colorAccent, fontSubHeader, fontHeader);
                        DrawInfoCard(gfx, margin + cardWidth + cardSpacing, yPoint, cardWidth, cardHeight, "TELJES KÖLTSÉG", $"{totalCost:N0} Ft", colorPrimary, fontSubHeader, fontHeader);
                        DrawInfoCard(gfx, margin + 2 * (cardWidth + cardSpacing), yPoint, cardWidth, cardHeight, "FEJENKÉNT", $"{costPerPerson:N0} Ft", colorHighlight, fontSubHeader, fontHeader);

                        yPoint += cardHeight + 40;

                        // 💰 Zsebpénz infó
                        gfx.DrawString("ZSEBPÉNZ", fontHeader, new XSolidBrush(colorPrimary), new XPoint(margin, yPoint));
                        yPoint += 25;

                        string pocketInfo = tripData.IsPerPersonMode
                            ? $"Személyenkénti beállítás: {tripData.PocketMoney.Count} diák"
                            : $"Átlagos zsebpénz: {tripData.AveragePocketMoney:N0} Ft/fő";

                        gfx.DrawRectangle(new XSolidBrush(colorCard), margin, yPoint, contentWidth, 50);
                        gfx.DrawRectangle(new XPen(colorBorder, 1), margin, yPoint, contentWidth, 50);
                        gfx.DrawString(pocketInfo, fontNormal, new XSolidBrush(colorText),
                            new XPoint(margin + 15, yPoint + 30));

                        yPoint += 80;

                        // 💵 Költségek táblázat
                        gfx.DrawString("KÖLTSÉGEK RÉSZLETESEN", fontHeader, new XSolidBrush(colorPrimary),
                            new XPoint(margin, yPoint));
                        yPoint += 35;

                        double rowHeight = 30;
                        double col1 = margin;
                        double col2 = margin + contentWidth * 0.35;
                        double col3 = margin + contentWidth * 0.55;
                        double col4 = margin + contentWidth * 0.75;

                        // Fejléc
                        gfx.DrawRectangle(new XSolidBrush(colorAccent), margin, yPoint, contentWidth, rowHeight);
                        gfx.DrawString("Típus", fontBold, new XSolidBrush(colorText), new XPoint(col1 + 10, yPoint + 20));
                        gfx.DrawString("Ár/fő", fontBold, new XSolidBrush(colorText), new XPoint(col2 + 10, yPoint + 20));
                        gfx.DrawString("Létszám", fontBold, new XSolidBrush(colorText), new XPoint(col3 + 10, yPoint + 20));
                        gfx.DrawString("Összesen", fontBold, new XSolidBrush(colorText), new XPoint(col4 + 10, yPoint + 20));

                        yPoint += rowHeight;

                        foreach (var cost in tripData.Costs)
                        {
                            gfx.DrawRectangle(new XSolidBrush(colorCard), margin, yPoint, contentWidth, rowHeight);
                            gfx.DrawRectangle(new XPen(colorBorder, 0.5), margin, yPoint, contentWidth, rowHeight);

                            double sum = (cost.Amount * cost.NumberOfPeople);
                            gfx.DrawString(cost.Type, fontNormal, new XSolidBrush(colorText), new XPoint(col1 + 10, yPoint + 20));
                            gfx.DrawString($"{cost.Amount:N0} Ft", fontNormal, new XSolidBrush(colorText), new XPoint(col2 + 10, yPoint + 20));
                            gfx.DrawString($"{cost.NumberOfPeople} fő", fontNormal, new XSolidBrush(colorText), new XPoint(col3 + 10, yPoint + 20));
                            gfx.DrawString($"{sum:N0} Ft", fontBold, new XSolidBrush(colorPrimary), new XPoint(col4 + 10, yPoint + 20));

                            yPoint += rowHeight;
                        }

                        gfx.DrawRectangle(new XSolidBrush(colorPrimary), margin, yPoint, contentWidth, rowHeight + 5);
                        gfx.DrawString("VÉGÖSSZEG", fontHeader, new XSolidBrush(colorBackground), new XPoint(col1 + 10, yPoint + 23));
                        gfx.DrawString($"{totalCost:N0} Ft", fontHeader, new XSolidBrush(colorBackground), new XPoint(col4 + 10, yPoint + 23));

                        yPoint += 50;

                        // 📸 Fotógaléria (ha van)
                        if (tripData.PhotoPaths != null && tripData.PhotoPaths.Count > 0)
                        {
                            var photoPage = document.AddPage();
                            photoPage.Size = PdfSharpCore.PageSize.A4;

                            using (var photoGfx = XGraphics.FromPdfPage(photoPage))
                            {
                                photoGfx.DrawRectangle(new XSolidBrush(colorBackground), 0, 0, pageWidth, photoPage.Height);
                                photoGfx.DrawString("📸 FOTÓGALÉRIA", fontTitle, new XSolidBrush(colorPrimary),
                                    new XPoint(margin, 50));

                                double photoY = 110;
                                double imgMax = 160, spacing = 20;
                                double x = margin;
                                int perRow = 3, index = 0;

                                foreach (var path in tripData.PhotoPaths)
                                {
                                    if (!File.Exists(path)) continue;
                                    using (var img = XImage.FromFile(path))
                                    {
                                        double scale = Math.Min(imgMax / img.PixelWidth, imgMax / img.PixelHeight);
                                        double w = img.PixelWidth * scale;
                                        double h = img.PixelHeight * scale;

                                        if (index % perRow == 0 && index > 0)
                                        {
                                            x = margin;
                                            photoY += imgMax + spacing + 25;
                                        }

                                        if (photoY + h > photoPage.Height.Point - 60)
                                            break;

                                        photoGfx.DrawRectangle(new XSolidBrush(colorCard), x, photoY, w + 6, h + 6);
                                        photoGfx.DrawRectangle(new XPen(colorBorder, 1), x, photoY, w + 6, h + 6);
                                        photoGfx.DrawImage(img, x + 3, photoY + 3, w, h);
                                        photoGfx.DrawString($"#{index + 1}", fontSmall, new XSolidBrush(colorSubText),
                                            new XPoint(x + w / 2, photoY + h + 20));

                                        x += imgMax + spacing;
                                        index++;
                                    }
                                }
                            }
                        }

                        // Lábléc
                        double footerY = pdfPage.Height.Point - 30;
                        gfx.DrawString($"Oldal 1 | Készült: {DateTime.Now:yyyy.MM.dd HH:mm}", fontSmall, new XSolidBrush(colorSubText),
                            new XPoint(margin, footerY));
                        gfx.DrawString("Trip Manager Pro", fontSmall, new XSolidBrush(colorSubText),
                            new XPoint(pageWidth - margin - 100, footerY));
                    }
                    document.Save(filePath);
                }

                await DisplayAlert("✅ Siker", $"PDF elkészült!\n📁 {filePath}", "Rendben");
            }
            catch (Exception ex)
            {
                await DisplayAlert("❌ Hiba", $"Nem sikerült exportálni a PDF-et:\n{ex.Message}", "OK");
            }
        }

        // ═══════════════════════════════════════════════════════
        private void DrawInfoCard(PdfSharpCore.Drawing.XGraphics gfx, double x, double y, double width, double height,
            string title, string value, PdfSharpCore.Drawing.XColor color,
            PdfSharpCore.Drawing.XFont titleFont, PdfSharpCore.Drawing.XFont valueFont)
        {
            // Háttér
            var brush = new PdfSharpCore.Drawing.XSolidBrush(color);
            gfx.DrawRectangle(brush, x, y, width, height);

            // Árnyék effekt
            var shadowBrush = new PdfSharpCore.Drawing.XSolidBrush(PdfSharpCore.Drawing.XColor.FromArgb(30, 0, 0, 0));
            gfx.DrawRectangle(shadowBrush, x + 2, y + 2, width, height);
            gfx.DrawRectangle(brush, x, y, width, height);

            // Szövegek
            gfx.DrawString(title, titleFont, PdfSharpCore.Drawing.XBrushes.White,
                new PdfSharpCore.Drawing.XPoint(x + 15, y + 25));
            gfx.DrawString(value, valueFont, PdfSharpCore.Drawing.XBrushes.White,
                new PdfSharpCore.Drawing.XPoint(x + 15, y + 55));
        }
        private async void OnExportPdfClicked(object sender, EventArgs e)
        {
            await ExportPageToPdf();
        }
    }
}