#if ANDROID
using Android.Content.Res;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Color = Microsoft.Maui.Graphics.Color;
#endif
namespace OKKT
{
    public static class MauiEntryExtensions
    {
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

        public static void SetEntryUnderlineColor(this Microsoft.Maui.Controls.Entry entry, Color mauiColor)
        {
#if ANDROID
            entry.HandlerChanged += (s, e) =>
            {
                if (entry.Handler?.PlatformView is Android.Widget.EditText androidEditText)
                {
                    // Átváltás Android.Graphics.Color típusra
                    var androidColor = mauiColor.ToPlatform();
                    
                    // Create a ColorStateList with focused and unfocused states
                    var states = new int[][]
                    {
                        new int[] { Android.Resource.Attribute.StateFocused }, // focused
                        new int[] { -Android.Resource.Attribute.StateFocused } // unfocused
                    };
                    var colors = new int[]
                    {
                        androidColor.ToArgb(), // focused color (narancssárga)
                        Android.Graphics.Color.Gray.ToArgb() // unfocused color (szürke)
                    };
                    var colorStateList = new ColorStateList(states, colors);
                    
                    // Beállítjuk az alsó vonal színét
                    androidEditText.BackgroundTintList = colorStateList;
                }
            };
#endif
        }

        public static void SetEditorUnderlineColor(this Microsoft.Maui.Controls.Editor editor, Color mauiColor)
        {
#if ANDROID
            editor.HandlerChanged += (s, e) =>
            {
                if (editor.Handler?.PlatformView is Android.Widget.EditText androidEditText)
                {
                    // Átváltás Android.Graphics.Color típusra
                    var androidColor = mauiColor.ToPlatform();
                    
                    // Create a ColorStateList with focused and unfocused states
                    var states = new int[][]
                    {
                        new int[] { Android.Resource.Attribute.StateFocused }, // focused
                        new int[] { -Android.Resource.Attribute.StateFocused } // unfocused
                    };
                    var colors = new int[]
                    {
                        androidColor.ToArgb(), // focused color (narancssárga)
                        Android.Graphics.Color.Gray.ToArgb() // unfocused color (szürke)
                    };
                    var colorStateList = new ColorStateList(states, colors);
                    
                    // Beállítjuk az alsó vonal színét
                    androidEditText.BackgroundTintList = colorStateList;
                }
            };
#endif
        }
    }
}