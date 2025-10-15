#if ANDROID
using Android.Content.Res;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Color = Microsoft.Maui.Graphics.Color;
#endif

namespace OKKT
{
    public static class MauiEntryExtensions
    {
        public static void RemoveUnderline(this Entry entry)
        {
#if ANDROID
            entry.HandlerChanged += (s, e) =>
            {
                if (entry.Handler.PlatformView is Android.Widget.EditText editText)
                {
                    //editText.BackgroundTintList = ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
                    var orangeColor = Android.Graphics.Color.Argb(255, 152, 0, 0);

                    editText.Background.SetColorFilter(orangeColor, Android.Graphics.PorterDuff.Mode.SrcAtop);
                }
            };
#endif
        }

        public static void SetRadioButtonCheckedColor(this Microsoft.Maui.Controls.RadioButton radioButton, Color mauiColor)
        {
#if ANDROID
            radioButton.HandlerChanged += (s, e) =>
            {
                if (radioButton.Handler.PlatformView is Android.Widget.RadioButton androidRadioButton)
                {
                    // Átváltás Android.Graphics.Color típusra
                    var androidColor = mauiColor.ToPlatform();

                    // Create a ColorStateList with checked and unchecked states
                    var states = new int[][]
                    {
                    new int[] { Android.Resource.Attribute.StateChecked },
                    new int[] { -Android.Resource.Attribute.StateChecked }
                    };

                    var colors = new int[]
                    {
                    androidColor.ToArgb(), // checked color
                    Android.Graphics.Color.Gray.ToArgb() // unchecked color, vagy amit szeretnél
                    };

                    var colorStateList = new ColorStateList(states, colors);

                    androidRadioButton.ButtonTintList = colorStateList;
                }
            };
#endif
        }

    }

}
