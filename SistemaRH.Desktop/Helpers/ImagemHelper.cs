using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SistemaRH.Desktop.Helpers;

public static class ImagemHelper
{
    public static byte[] RedimensionarParaJpeg(string caminhoArquivo, int maxLado = 300, int qualidade = 85)
    {
        var original = new BitmapImage();
        original.BeginInit();
        original.CacheOption = BitmapCacheOption.OnLoad;
        original.UriSource = new Uri(caminhoArquivo);
        original.EndInit();

        var escala = (double)maxLado / Math.Max(original.PixelWidth, original.PixelHeight);
        if (escala > 1) escala = 1;

        var transformada = new TransformedBitmap(original, new ScaleTransform(escala, escala));

        var encoder = new JpegBitmapEncoder { QualityLevel = qualidade };
        encoder.Frames.Add(BitmapFrame.Create(transformada));

        using var stream = new MemoryStream();
        encoder.Save(stream);
        return stream.ToArray();
    }
}
