using PdfSharp.Fonts;
using System;
using System.IO;
using System.Reflection;
using System.Diagnostics; // PDF'yi oluşturduktan sonra açmak için (Process.Start)
using System.Linq;         // Toplam ve liste işlemleri
using System.Collections.Generic; // Liste ile çalışırken

public class CustomFontResolver : IFontResolver
{
    private static readonly Lazy<CustomFontResolver> _instance = new Lazy<CustomFontResolver>(() => new CustomFontResolver());
    public static CustomFontResolver Instance => _instance.Value;

    private readonly byte[] arialFont;

    // Sabit font adı – burası önemlidir
    private const string FontName = "CustomArial";  

    public CustomFontResolver()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = "By_ProV2.Fonts.arial.ttf"; // Changed namespace from By_ProV1 to By_ProV2

        using (var stream = assembly.GetManifestResourceStream(resourceName))
        {
            if (stream == null)
                throw new InvalidOperationException("Font dosyası bulunamadı: " + resourceName);

            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                arialFont = ms.ToArray();
            }
        }
    }

    // Font byte dizisini döndür
    public byte[] GetFont(string faceName)
    {
        if (faceName == FontName)
            return arialFont;

        throw new InvalidOperationException("İstenen font bulunamadı: " + faceName);
    }

    // Tüm talepler bu fontla karşılanacak
    public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
    {
        return new FontResolverInfo(FontName);
    }
}