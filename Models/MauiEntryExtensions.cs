#if ANDROID
using Android.Content.Res;
using Android.Graphics;
using Android.Widget;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using OKKT;
using Color = Microsoft.Maui.Graphics.Color;
#endif
using MauiRadioButton = Microsoft.Maui.Controls.RadioButton;

namespace OKKT25.Models
{
    public static class MauiEntryExtensions
    {
        public static void SetRadioButtonCheckedColor(this MauiRadioButton radioButton, Color mauiColor)
        {

            #if ANDROID
            radioButton.HandlerChanged += (s, e) =>
            {
                if (radioButton.Handler.PlatformView is Android.Widget.RadioButton androidRadioButton)
                {
                    var androidColor = mauiColor.ToPlatform();

                    var states = new int[][]
                    {
                        new int[] { Android.Resource.Attribute.StateChecked },
                        new int[] { -Android.Resource.Attribute.StateChecked }
                    };

                    var colors = new int[]
                    {
                        androidColor.ToArgb(),
                        Android.Graphics.Color.Gray.ToArgb()
                    };

                    var colorStateList = new ColorStateList(states, colors);
                    androidRadioButton.ButtonTintList = colorStateList;
                }
            };
            #endif

        }

    }

}