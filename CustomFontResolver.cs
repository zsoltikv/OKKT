using PdfSharpCore.Fonts;
using System.IO;

public class CustomFontResolver : IFontResolver
{
    public byte[] GetFont(string faceName)
    {
        // A Resources mappában elhelyezett fontot töltjük be
        var assembly = typeof(CustomFontResolver).Assembly;
        using var stream = assembly.GetManifestResourceStream("YourNamespace.Resources.YourFont.ttf");
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }

    public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
    {
        // Itt adhatod meg a default fontot
        return new FontResolverInfo("Montserrat#");
    }
}
