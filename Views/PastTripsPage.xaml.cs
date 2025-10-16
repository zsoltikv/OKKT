using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text;
using System.Windows.Input;
using OKKT25.Models;

namespace OKKT25
{
    public partial class PastTripsPage : ContentPage
    {
        public ICommand TripTappedCommand { get; }

        private ObservableCollection<TripSummary> trips = new();

        public PastTripsPage()
        {

            InitializeComponent();

            TripTappedCommand = new Command<TripSummary>(async (trip) => await OpenTripAsync(trip));

            BindingContext = this;
            this.Appearing += PastTripsPage_Appearing;

        }

        private void PastTripsPage_Appearing(object? sender, EventArgs e) => LoadTrips();

        private async Task OpenTripAsync(TripSummary selectedTrip)
        {

            try
            {
                var json = await File.ReadAllTextAsync(selectedTrip.FileName, Encoding.UTF8);
                var tripData = JsonSerializer.Deserialize<TripData>(json);

                if (tripData != null)
                {
                    await Navigation.PushAsync(new TripDetailPage(tripData, selectedTrip.TripName));
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hiba", $"Nem sikerült megnyitni: {ex.Message}", "OK");
            }

        }

        private async void LoadTrips()
        {

            try
            {
                trips.Clear();
                var appDataDir = FileSystem.Current.AppDataDirectory;
                var jsonFiles = Directory.GetFiles(appDataDir, "*.json");

                foreach (var file in jsonFiles)
                {
                    try
                    {
                        var json = await File.ReadAllTextAsync(file, Encoding.UTF8);
                        var tripData = JsonSerializer.Deserialize<TripData>(json);

                        if (tripData != null)
                        {
                            double totalCost = tripData.Costs.Sum(c =>
                                (c.Amount * c.NumberOfPeople) +
                                (c.DiscountAmount * c.DiscountNumberOfPeople));

                            trips.Add(new TripSummary
                            {
                                FileName = file,
                                TripName = string.IsNullOrWhiteSpace(tripData.TripName)
                                            ? Path.GetFileNameWithoutExtension(file)
                                            : tripData.TripName,
                                LastSaved = tripData.LastSaved,
                                Participants = tripData.Participants,
                                TotalCost = totalCost,
                                TripDestination = tripData.TripDestination,
                                TripDateStart = tripData.TripDateStart,
                                TripDateEnd = tripData.TripDateEnd
                            });
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }

                TripsCollectionView.ItemsSource = trips.OrderByDescending(t => t.LastSaved).ToList();
                EmptyStateLabel.IsVisible = trips.Count == 0;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hiba", $"Nem sikerült betölteni a kirándulásokat: {ex.Message}", "OK");
            }

        }

    }

}