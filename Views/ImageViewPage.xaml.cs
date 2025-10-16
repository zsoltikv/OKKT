using OKKT25.Models;

namespace OKKT25
{
    public partial class ImageViewPage : ContentPage
    {
        private double currentScale = 1;
        private double startScale = 1;
        private double xOffset = 0;
        private double yOffset = 0;

        private ImageSource currentImage;
        private TripDetailPage parentPage;
        private Func<ImageSource, Task<bool>> deleteCallback;

        public ImageViewPage(ImageSource imageSource, Func<ImageSource, Task<bool>> onDelete)
        {
            InitializeComponent();
            currentImage = imageSource;
            deleteCallback = onDelete;
            FullImage.Source = currentImage;

            var pinchGesture = new PinchGestureRecognizer();
            pinchGesture.PinchUpdated += OnPinchUpdated;
            FullImage.GestureRecognizers.Add(pinchGesture);

            var doubleTap = new TapGestureRecognizer { NumberOfTapsRequired = 2 };
            doubleTap.Tapped += (s, e) => ResetZoom();
            FullImage.GestureRecognizers.Add(doubleTap);
        }

        private void OnPinchUpdated(object sender, PinchGestureUpdatedEventArgs e)
        {
            if (e.Status == GestureStatus.Started)
            {
                startScale = FullImage.Scale;
                FullImage.AnchorX = 0;
                FullImage.AnchorY = 0;
            }

            if (e.Status == GestureStatus.Running)
            {
                currentScale = startScale * e.Scale;
                currentScale = Math.Max(1, Math.Min(currentScale, 4));
                FullImage.Scale = currentScale;

                double renderedX = FullImage.X + xOffset;
                double renderedY = FullImage.Y + yOffset;
                double deltaX = renderedX / Width;
                double deltaY = renderedY / Height;
                double deltaWidth = Width / (FullImage.Width * startScale);
                double deltaHeight = Height / (FullImage.Height * startScale);
                double originX = (e.ScaleOrigin.X - deltaWidth / 2) * deltaX;
                double originY = (e.ScaleOrigin.Y - deltaHeight / 2) * deltaY;

                FullImage.TranslationX = -originX * FullImage.Width * (currentScale - 1);
                FullImage.TranslationY = -originY * FullImage.Height * (currentScale - 1);
            }

            if (e.Status == GestureStatus.Completed)
            {
                xOffset = FullImage.TranslationX;
                yOffset = FullImage.TranslationY;
            }
        }

        private void ResetZoom()
        {
            FullImage.Scale = 1;
            FullImage.TranslationX = 0;
            FullImage.TranslationY = 0;
            currentScale = 1;
            xOffset = 0;
            yOffset = 0;
        }

        private async void OnCloseClicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }

        private async void OnDeleteClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Kép törlése",
                "Biztosan törölni szeretnéd ezt a képet?", "Igen", "Mégse");

            if (!confirm)
                return;

            if (deleteCallback != null)
            {
                bool success = await deleteCallback(currentImage);

                if (success)
                {
                    await DisplayAlert("Siker", "A kép törölve lett.", "OK");
                    await Navigation.PopModalAsync();
                }
                else
                {
                    await DisplayAlert("Hiba", "Nem sikerült törölni a képet.", "OK");
                }
            }
        }
    }
}