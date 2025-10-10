using PdfSharpCore.Fonts;
using System.IO;
using System.Reflection;

public class CustomFontResolver : IFontResolver
{
    public string DefaultFontName => "Montserrat#";

    public byte[] GetFont(string faceName)
    {
        var assembly = typeof(CustomFontResolver).GetTypeInfo().Assembly;
        const string resourceName = "YourNamespace.Fonts.Montserrat-Regular.ttf"; // namespace + mappa + fájlnév

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
            throw new FileNotFoundException($"Nem található a beágyazott font: {resourceName}");

        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }

    public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
    {
        if (string.Equals(familyName, "Montserrat", StringComparison.OrdinalIgnoreCase))
            return new FontResolverInfo("Montserrat#");

        return new FontResolverInfo(DefaultFontName);
    }
}