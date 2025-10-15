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
        public static void SetEntryCustomUnderline(this Entry entry, Microsoft.Maui.Graphics.Color mauiColor)
        {
#if ANDROID
            entry.HandlerChanged += (s, e) =>
            {
                if (entry.Handler?.PlatformView is Android.Widget.EditText editText)
                {
                    var androidColor = mauiColor.ToPlatform();

                    // Létrehozunk egy átlátszó háttérrel rendelkező alakzatot, ami CSAK alulra rajzol vonalat
                    var shape = new Android.Graphics.Drawables.ShapeDrawable(new Android.Graphics.Drawables.Shapes.RectShape());
                    shape.Paint.Color = Android.Graphics.Color.Transparent; // háttér átlátszó
                    shape.Paint.SetStyle(Android.Graphics.Paint.Style.FillAndStroke);
                    shape.Paint.StrokeWidth = 0;

                    // Alulra jön a vonal – egy második ShapeDrawable, amit LayerDrawable-ba rakunk
                    var underline = new Android.Graphics.Drawables.ShapeDrawable(new Android.Graphics.Drawables.Shapes.RectShape());
                    underline.Paint.Color = androidColor;
                    underline.Paint.SetStyle(Android.Graphics.Paint.Style.Fill);
                    underline.SetPadding(0, 0, 0, 0); // nem kell padding, csak pozicionálás

                    // LayerDrawable létrehozása, két réteggel (háttér + vonal)
                    var layers = new Android.Graphics.Drawables.LayerDrawable(new Android.Graphics.Drawables.Drawable[] { shape, underline });

                    // Beállítjuk, hogy a vonal csak alul legyen (bal, felső, jobb, alsó offset)
                    layers.SetLayerInset(1, 0, editText.Height - 2, 0, 0); // 2px magas vonal az alján

                    // Ezt a drawable-t állítjuk be háttérként
                    editText.SetBackground(layers);

                    // Ha elsőre még nem tudja a magasságot, akkor figyeljük meg később is
                    editText.Post(() =>
                    {
                        layers.SetLayerInset(1, 0, editText.Height - 2, 0, 0);
                    });
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
