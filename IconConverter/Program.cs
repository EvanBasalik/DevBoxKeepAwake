using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

var sourcePath = @"C:\Users\evanba\Downloads\_345fb02b-ab8a-4c56-8b11-114b9fd00939.jpg";
var outputPath = @"C:\Users\evanba\source\repos\DevBoxKeepAlive\DevBoxKeepAwake\app.ico";

try
{
    if (!File.Exists(sourcePath))
    {
        Console.WriteLine($"Error: Source image not found at {sourcePath}");
        return;
    }

    using (var originalImage = Image.FromFile(sourcePath))
    {
        // Create a 256x256 bitmap with transparent background
        var bitmap = new Bitmap(256, 256, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(bitmap))
        {
            g.Clear(Color.Transparent);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            // Calculate aspect ratio and fit image
            var sourceAspect = (double)originalImage.Width / originalImage.Height;
            int destWidth, destHeight, destX, destY;

            if (sourceAspect > 1)
            {
                destWidth = 256;
                destHeight = (int)(256 / sourceAspect);
                destX = 0;
                destY = (256 - destHeight) / 2;
            }
            else
            {
                destHeight = 256;
                destWidth = (int)(256 * sourceAspect);
                destX = (256 - destWidth) / 2;
                destY = 0;
            }

            g.DrawImage(originalImage, destX, destY, destWidth, destHeight);
        }

        // JPEG does not carry alpha, so key out the background color sampled from corners.
        using var sourceBitmap = new Bitmap(originalImage);
        var keyColor = EstimateBackgroundColor(sourceBitmap);
        RemoveBackground(bitmap, keyColor, tolerance: 28);

        // Save as ICO via an icon handle to avoid GDI+ codec limitations.
        var hIcon = bitmap.GetHicon();
        try
        {
            using var icon = Icon.FromHandle(hIcon);
            using var output = File.Create(outputPath);
            icon.Save(output);
        }
        finally
        {
            NativeMethods.DestroyIcon(hIcon);
        }

        Console.WriteLine($"Icon successfully created: {outputPath}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}

static Color EstimateBackgroundColor(Bitmap image)
{
    var c1 = image.GetPixel(0, 0);
    var c2 = image.GetPixel(image.Width - 1, 0);
    var c3 = image.GetPixel(0, image.Height - 1);
    var c4 = image.GetPixel(image.Width - 1, image.Height - 1);

    var r = (c1.R + c2.R + c3.R + c4.R) / 4;
    var g = (c1.G + c2.G + c3.G + c4.G) / 4;
    var b = (c1.B + c2.B + c3.B + c4.B) / 4;
    return Color.FromArgb(r, g, b);
}

static void RemoveBackground(Bitmap bitmap, Color keyColor, int tolerance)
{
    for (var y = 0; y < bitmap.Height; y++)
    {
        for (var x = 0; x < bitmap.Width; x++)
        {
            var pixel = bitmap.GetPixel(x, y);
            if (pixel.A == 0)
            {
                continue;
            }

            var distance = Math.Abs(pixel.R - keyColor.R)
                + Math.Abs(pixel.G - keyColor.G)
                + Math.Abs(pixel.B - keyColor.B);

            if (distance <= tolerance)
            {
                bitmap.SetPixel(x, y, Color.FromArgb(0, pixel.R, pixel.G, pixel.B));
            }
        }
    }
}

internal static partial class NativeMethods
{
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool DestroyIcon(IntPtr hIcon);
}

