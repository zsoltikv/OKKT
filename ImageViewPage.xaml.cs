namespace OKKT25
{
    public partial class ImageViewPage : ContentPage
    {
        public ImageViewPage(ImageSource imageSource)
        {
            InitializeComponent();
            FullImage.Source = imageSource;

            // Pinch gesture hozzáadása
            var pinchGesture = new PinchGestureRecognizer();
            pinchGesture.PinchUpdated += (s, e) =>
            {
                if (e.Status == GestureStatus.Running)
                    FullImage.Scale *= e.Scale;
                else if (e.Status == GestureStatus.Completed)
                    FullImage.Scale = 1; // visszaállítja az alap méretet
            };

            FullImage.GestureRecognizers.Add(pinchGesture);
        }

        private async void OnCloseClicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }
}
