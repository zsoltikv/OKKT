using System.Collections.ObjectModel;
using System.Globalization;
using PdfSharpCore.Drawing;
using PdfSharpCore.Fonts;
using SkiaSharp;

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
        }restore

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

            double totalCost = tripData.Costs.Sum(c =>
                (c.Amount * c.NumberOfPeople) +
                (c.DiscountAmount * c.DiscountNumberOfPeople));

            // --- Összefoglaló kártya ---
            var summaryCard = CreateCard("📊 Összefoglaló", "#FF9800");
            var summaryLayout = new VerticalStackLayout { Padding = 15, Spacing = 6 };

            summaryLayout.Add(new Label
            {
                FormattedText = new FormattedString
                {
                    Spans =
            {
                new Span { Text = "Teljes költség: ", FontAttributes = FontAttributes.Bold },
                new Span { Text = $"{FormatNumber(totalCost)} Ft" }
            }
                },
                FontSize = 16,
                TextColor = Color.FromArgb("#FFFFFF")
            });

            summaryLayout.Add(new Label
            {
                FormattedText = new FormattedString
                {
                    Spans =
            {
                new Span { Text = "Résztvevők: ", FontAttributes = FontAttributes.Bold },
                new Span { Text = $"{tripData.Participants} fő" }
            }
                },
                FontSize = 16,
                TextColor = Color.FromArgb("#FFFFFF")
            });

            summaryLayout.Add(new Label
            {
                FormattedText = new FormattedString
                {
                    Spans =
            {
                new Span { Text = "Hátralévő idő: ", FontAttributes = FontAttributes.Bold },
                new Span { Text = $"{tripData.MonthsLeft} hónap" }
            }
                },
                FontSize = 16,
                TextColor = Color.FromArgb("#FFFFFF")
            });

            summaryLayout.Add(new Label
            {
                FormattedText = new FormattedString
                {
                    Spans =
            {
                new Span { Text = "Mentve: ", FontAttributes = FontAttributes.Bold },
                new Span { Text = $"{tripData.LastSaved:yyyy.MM.dd HH:mm}" }
            }
                },
                FontSize = 14,
                TextColor = Color.FromArgb("#C8C8C8")
            });

            ((VerticalStackLayout)summaryCard.Content).Add(summaryLayout);
            DetailLayout.Add(summaryCard);

            // --- Költségek kártya ---
            var costsCard = CreateCard("💰 Költségek részletezve", "#FF9800");
            var costsLayout = new VerticalStackLayout { Padding = 15, Spacing = 8 };

            if (tripData.Costs.Count == 0)
            {
                costsLayout.Add(new Label
                {
                    Text = "Nincsenek rögzített költségek.",
                    FontSize = 14,
                    TextColor = Color.FromArgb("#C8C8C8")
                });
            }
            else
            {
                foreach (var cost in tripData.Costs)
                {
                    costsLayout.Add(new Label
                    {
                        FormattedText = new FormattedString
                        {
                            Spans =
                    {
                        new Span { Text = $"{cost.Type} ", FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#FFD700") },
                        new Span { Text = $"(×{cost.NumberOfPeople} fő)", TextColor = Color.FromArgb("#FFD700") }
                    }
                        },
                        FontSize = 15
                    });

                    costsLayout.Add(new Label
                    {
                        Text = $"• Ár/fő: {FormatNumber(cost.Amount)} Ft",
                        FontSize = 14,
                        TextColor = Color.FromArgb("#FFFFFF")
                    });

                    if (cost.HasDiscount)
                    {
                        costsLayout.Add(new Label
                        {
                            Text = $"• Kedvezmény: {FormatNumber(cost.DiscountAmount)} Ft × {cost.DiscountNumberOfPeople} fő",
                            FontSize = 14,
                            TextColor = Color.FromArgb("#FF9800")
                        });
                    }

                    costsLayout.Add(new Label
                    {
                        Text = $"• Összesen: {FormatNumber((cost.Amount * cost.NumberOfPeople) + (cost.DiscountAmount * cost.DiscountNumberOfPeople))} Ft",
                        FontSize = 14,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Color.FromArgb("#FFD700")
                    });

                    costsLayout.Add(new BoxView { HeightRequest = 1, BackgroundColor = Color.FromArgb("#3C3C3C"), Margin = new Thickness(0, 5, 0, 5) });
                }
            }

    ((VerticalStackLayout)costsCard.Content).Add(costsLayout);
            DetailLayout.Add(costsCard);

            // --- Zsebpénz kártya ---
            var pocketMoneyCard = CreateCard("💵 Zsebpénz", "#FF9800");
            var pocketMoneyLayout = new VerticalStackLayout { Padding = 15, Spacing = 6 };

            pocketMoneyLayout.Add(new Label
            {
                FormattedText = new FormattedString
                {
                    Spans =
            {
                tripData.IsPerPersonMode
                    ? new Span { Text = $"Személyenként megadva ({tripData.PocketMoney.Count} diák adatai mentve)", FontAttributes = FontAttributes.Bold }
                    : new Span { Text = $"Átlagos zsebpénz: {FormatNumber(tripData.AveragePocketMoney)} Ft/fő", FontAttributes = FontAttributes.Bold }
            }
                },
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
                // 🎨 Létrehozunk egy szép PDF-szerű nézetet programatikusan
                var pdfView = await CreatePdfViewAsync();

                // Ideiglenesen hozzáadjuk az oldalhoz (láthatatlanul, képernyőn kívül)
                var rootLayout = (this.Content as ScrollView)?.Content as Layout;
                if (rootLayout == null)
                {
                    await DisplayAlert("Hiba", "Nem található a főlayout", "OK");
                    return;
                }

                // Elmentjük az eredeti pozíciót és láthatatlanná tesszük
                pdfView.TranslationY = 10000; // Képernyőn kívülre tesszük
                rootLayout.Add(pdfView);

                // Várunk egy kicsit, hogy renderelődjön
                await Task.Delay(300);

                // 📸 Képernyőkép készítése a nézetről
                var screenshot = await pdfView.CaptureAsync();

                // Eltávolítjuk a view-t
                rootLayout.Remove(pdfView);

                if (screenshot == null)
                {
                    await DisplayAlert("Hiba", "Nem sikerült képernyőképet készíteni", "OK");
                    return;
                }

                // Ideiglenes fájlba mentés
                string tempImagePath = Path.Combine(FileSystem.Current.CacheDirectory, $"temp_pdf_{Guid.NewGuid()}.png");
                using (var stream = await screenshot.OpenReadAsync())
                using (var fileStream = File.Create(tempImagePath))
                {
                    await stream.CopyToAsync(fileStream);
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
                    using (var xImage = XImage.FromFile(tempImagePath))
                    {
                        double pageWidth = page.Width.Point;
                        double pageHeight = page.Height.Point;

                        // Teljes oldal kitöltése
                        gfx.DrawImage(xImage, 0, 0, pageWidth, pageHeight);
                    }

                    // 📸 Fotók hozzáadása két oszlopos elrendezésben, középre igazítva, annyi sorral, amennyi kifér
                    if (tripData.PhotoPaths != null && tripData.PhotoPaths.Count > 0)
                    {
                        const double margin = 40;
                        const double spacing = 20; // Képek közötti vízszintes távolság
                        const double rowSpacing = 20; // Sorok közötti távolság
                        const double border = 4; // Narancssárga keret vastagsága
                        const double pageWidth = 595; // A4 szélesség pontokban
                        const double pageHeight = 842; // A4 magasság pontokban
                        double imgWidth = (pageWidth - 2 * margin - spacing) / 2; // Kép szélessége oszloponként, középre igazításhoz igazítva

                        // Kiszámítjuk a maximum sorok számát a képek magassága alapján
                        double maxImgHeight = 0;
                        foreach (var photoPath in tripData.PhotoPaths.Take(Math.Min(tripData.PhotoPaths.Count, 10))) // Korlátozás 10 képre a ciklusidő miatt
                        {
                            if (File.Exists(photoPath))
                            {
                                using (var img = XImage.FromFile(photoPath))
                                {
                                    double scale = imgWidth / img.PixelWidth;
                                    maxImgHeight = Math.Max(maxImgHeight, img.PixelHeight * scale);
                                }
                            }
                        }

                        // Maximum sorok száma: (teljes magasság - margók - (n-1)*rowSpacing) / n
                        int maxRows = Math.Max(1, (int)Math.Floor((pageHeight - 2 * margin + rowSpacing) / (maxImgHeight + rowSpacing)));

                        for (int i = 0; i < tripData.PhotoPaths.Count; i += 2 * maxRows) // Soronként 2 kép
                        {
                            var photoPage = document.AddPage();
                            photoPage.Size = PdfSharpCore.PageSize.A4;

                            using (var gfx = XGraphics.FromPdfPage(photoPage))
                            {
                                // Sötétszürke háttér (#2A2A2A)
                                gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(42, 42, 42)), 0, 0, photoPage.Width.Point, photoPage.Height.Point);

                                // Sorok és képek hozzáadása
                                for (int row = 0; row < maxRows && i + row * 2 < tripData.PhotoPaths.Count; row++)
                                {
                                    double y = margin + row * (maxImgHeight + rowSpacing); // Sor pozíciója felülről
                                    double leftX = (pageWidth - (2 * imgWidth + spacing)) / 2; // Középre igazítás

                                    // Bal oldali kép
                                    if (i + row * 2 < tripData.PhotoPaths.Count && File.Exists(tripData.PhotoPaths[i + row * 2]))
                                    {
                                        using (var img = XImage.FromFile(tripData.PhotoPaths[i + row * 2]))
                                        {
                                            double scale = Math.Min(imgWidth / img.PixelWidth, maxImgHeight / img.PixelHeight);
                                            double w = img.PixelWidth * scale;
                                            double h = img.PixelHeight * scale;
                                            double x = leftX;

                                            // Narancssárga keret (#FF8C00)
                                            gfx.DrawRectangle(new XPen(XColor.FromArgb(255, 140, 0), border), x - border / 2, y - border / 2, w + border, h + border);
                                            gfx.DrawImage(img, x, y, w, h);
                                        }
                                    }

                                    // Jobb oldali kép
                                    if (i + row * 2 + 1 < tripData.PhotoPaths.Count && File.Exists(tripData.PhotoPaths[i + row * 2 + 1]))
                                    {
                                        using (var img = XImage.FromFile(tripData.PhotoPaths[i + row * 2 + 1]))
                                        {
                                            double scale = Math.Min(imgWidth / img.PixelWidth, maxImgHeight / img.PixelHeight);
                                            double w = img.PixelWidth * scale;
                                            double h = img.PixelHeight * scale;
                                            double x = leftX + imgWidth + spacing;

                                            // Narancssárga keret (#FF8C00)
                                            gfx.DrawRectangle(new XPen(XColor.FromArgb(255, 140, 0), border), x - border / 2, y - border / 2, w + border, h + border);
                                            gfx.DrawImage(img, x, y, w, h);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    document.Save(filePath);
                }

                // Temp file törlése
                if (File.Exists(tempImagePath))
                    File.Delete(tempImagePath);

                await DisplayAlert("✅ Siker", $"PDF elkészült!\n📁 {filePath}", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hiba", $"PDF készítés sikertelen: {ex.Message}", "OK");
            }
        }

        private async Task<View> CreatePdfViewAsync()
        {
            // A4 arány: 595x842 pont (72 DPI-nél)
            // Mobilon 2x-es felbontással: 1190x1684 px
            double width = 1190;
            double height = 1684;

            var mainLayout = new VerticalStackLayout
            {
                WidthRequest = width,
                HeightRequest = height,
                BackgroundColor = Color.FromArgb("#1A1A1A"),
                Spacing = 0,
                Padding = 0
            };

            double totalCost = tripData.Costs.Sum(c => (c.Amount * c.NumberOfPeople) + (c.DiscountAmount * c.DiscountNumberOfPeople));
            double costPerPerson = tripData.Participants > 0 ? totalCost / tripData.Participants : 0;

            // 📌 Fejléc szekció
            var headerLayout = new VerticalStackLayout
            {
                BackgroundColor = Color.FromArgb("#FF8C00"),
                HeightRequest = 110, // Magasabb fejléc a cím és a térköz miatt
                Spacing = 10,
                Padding = new Thickness(40, 20, 40, 20) // Padding a fejlécen belül
            };

            // Cím
            var titleLabel = new Label
            {
                Text = tripData.TripName.ToUpper(),
                FontSize = 48,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.Black,
                HorizontalTextAlignment = TextAlignment.Center
                // Negatív margó eltávolítva, a headerLayout paddingja kezeli a pozíciót
            };
            headerLayout.Add(titleLabel);

            mainLayout.Add(headerLayout);

            // 📍 Helyszín és időpont
            var infoBox = new Frame
            {
                BackgroundColor = Color.FromArgb("#2A2A2A"),
                BorderColor = Color.FromArgb("#FF8C00"),
                HasShadow = false,
                CornerRadius = 12,
                Padding = 25,
                Margin = new Thickness(50, 30, 50, 20), // Nagyobb felső margó (30)
                Content = new VerticalStackLayout
                {
                    Spacing = 15,
                    Children =
            {
                new Label
                {
                    FormattedText = new FormattedString
                    {
                        Spans =
                        {
                            new Span { Text = "📍 Helyszín: ", FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#FF8C00"), FontSize = 20 },
                            new Span { Text = tripData.TripDestination ?? "Nincs megadva", TextColor = Colors.White, FontSize = 20 }
                        }
                    }
                },
                new Label
                {
                    FormattedText = new FormattedString
                    {
                        Spans =
                        {
                            new Span { Text = "📅 Időpont: ", FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#FF8C00"), FontSize = 20 },
                            new Span { Text = tripData.TripDateStart != default ? $"{tripData.TripDateStart:yyyy.MM.dd} - {tripData.TripDateEnd:yyyy.MM.dd}" : "Nincs megadva", TextColor = Colors.White, FontSize = 20 }
                        }
                    }
                }
            }
                }
            };
            mainLayout.Add(infoBox);

            // 📊 Összesítő kártyák
            var cardsLayout = new HorizontalStackLayout
            {
                Spacing = 20,
                Margin = new Thickness(50, 0, 50, 25),
                HorizontalOptions = LayoutOptions.Center,
                Children =
        {
            CreateSummaryCard("RÉSZTVEVŐK", $"{tripData.Participants} fő", "#FF6B00"),
            CreateSummaryCard("TELJES KÖLTSÉG", $"{totalCost:N0} Ft", "#FF8C00"),
            CreateSummaryCard("FEJENKÉNT", $"{costPerPerson:N0} Ft", "#FFA500")
        }
            };
            mainLayout.Add(cardsLayout);

            // 💰 Zsebpénz szekció
            var pocketMoneySection = new VerticalStackLayout
            {
                Spacing = 12,
                Margin = new Thickness(50, 0, 50, 25)
            };

            pocketMoneySection.Add(new Label
            {
                Text = "💰 ZSEBPÉNZ",
                FontSize = 28,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#FF8C00")
            });

            string pocketInfo = tripData.IsPerPersonMode
                ? $"Személyenkénti beállítás: {tripData.PocketMoney.Count} diák"
                : $"Átlagos zsebpénz: {tripData.AveragePocketMoney:N0} Ft/fő";

            pocketMoneySection.Add(new Frame
            {
                BackgroundColor = Color.FromArgb("#2A2A2A"),
                BorderColor = Color.FromArgb("#FF8C00"),
                HasShadow = false,
                CornerRadius = 12,
                Padding = 20,
                Content = new Label
                {
                    Text = pocketInfo,
                    FontSize = 18,
                    TextColor = Colors.White
                }
            });

            mainLayout.Add(pocketMoneySection);

            // 💵 Költségek szekció
            var costsSection = new VerticalStackLayout
            {
                Spacing = 12,
                Margin = new Thickness(50, 0, 50, 25)
            };

            costsSection.Add(new Label
            {
                Text = "💵 KÖLTSÉGEK RÉSZLETESEN",
                FontSize = 28,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#FF8C00")
            });

            var costsStack = new VerticalStackLayout
            {
                Spacing = 12
            };

            foreach (var cost in tripData.Costs)
            {
                double sum = (cost.Amount * cost.NumberOfPeople) + (cost.DiscountAmount * cost.DiscountNumberOfPeople);

                costsStack.Add(new Frame
                {
                    BackgroundColor = Color.FromArgb("#2A2A2A"),
                    BorderColor = Color.FromArgb("#3A3A3A"),
                    HasShadow = false,
                    CornerRadius = 10,
                    Padding = 20,
                    Content = new VerticalStackLayout
                    {
                        Spacing = 8,
                        Children =
                {
                    new Label
                    {
                        Text = $"{cost.Type} (×{cost.NumberOfPeople} fő)",
                        FontSize = 20,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Color.FromArgb("#FF8C00")
                    },
                    new Label
                    {
                        Text = $"Ár/fő: {cost.Amount:N0} Ft",
                        FontSize = 18,
                        TextColor = Color.FromArgb("#CCCCCC")
                    },
                    new Label
                    {
                        Text = $"Összesen: {sum:N0} Ft",
                        FontSize = 18,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Colors.White
                    }
                }
                    }
                });
            }

            costsSection.Add(costsStack);
            mainLayout.Add(costsSection);

            // Végösszeg
            var totalGrid = new Grid
            {
                ColumnDefinitions =
        {
            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            new ColumnDefinition { Width = GridLength.Auto }
        }
            };

            var totalLabel = new Label
            {
                Text = "VÉGÖSSZEG",
                FontSize = 28,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.Black,
                VerticalTextAlignment = TextAlignment.Center
            };
            Grid.SetColumn(totalLabel, 0);
            totalGrid.Add(totalLabel);

            var totalValueLabel = new Label
            {
                Text = $"{totalCost:N0} Ft",
                FontSize = 28,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.Black,
                VerticalTextAlignment = TextAlignment.Center,
                HorizontalTextAlignment = TextAlignment.End
            };
            Grid.SetColumn(totalValueLabel, 1);
            totalGrid.Add(totalValueLabel);

            var totalFrame = new Frame
            {
                BackgroundColor = Color.FromArgb("#FF8C00"),
                BorderColor = Colors.Transparent,
                HasShadow = false,
                CornerRadius = 12,
                Padding = 25,
                Margin = new Thickness(50, 0, 50, 40),
                Content = totalGrid
            };
            mainLayout.Add(totalFrame);

            // Render kikényszerítése
            await Task.Delay(100);

            return mainLayout;
        }

        private Frame CreateSummaryCard(string title, string value, string colorHex)
        {
            return new Frame
            {
                WidthRequest = 340,
                HeightRequest = 130,
                BackgroundColor = Color.FromArgb(colorHex),
                BorderColor = Colors.Transparent,
                HasShadow = false,
                CornerRadius = 12,
                Padding = 20,
                Content = new VerticalStackLayout
                {
                    Spacing = 10,
                    VerticalOptions = LayoutOptions.Center,
                    Children =
            {
                new Label
                {
                    Text = title,
                    FontSize = 16,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.Black,
                    HorizontalTextAlignment = TextAlignment.Center
                },
                new Label
                {
                    Text = value,
                    FontSize = 28,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.Black,
                    HorizontalTextAlignment = TextAlignment.Center
                }
            }
                }
            };
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