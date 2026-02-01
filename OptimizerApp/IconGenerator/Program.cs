using System.Drawing;

// Crear bitmap 256x256
var bitmap = new Bitmap(256, 256);
using (var graphics = Graphics.FromImage(bitmap))
{
    // Fondo negro
    graphics.Clear(Color.FromArgb(13, 13, 13)); // #0D0D0D
    
    // Borde verde Razer
    using (var pen = new Pen(Color.FromArgb(68, 214, 44), 3)) // #44D62C
    {
        graphics.DrawRectangle(pen, 8, 8, 240, 240);
    }
    
    // Texto "OP" 
    using (var font = new Font("Arial", 96, FontStyle.Bold))
    using (var brush = new SolidBrush(Color.FromArgb(68, 214, 44)))
    {
        var format = new StringFormat();
        format.Alignment = StringAlignment.Center;
        format.LineAlignment = StringAlignment.Center;
        graphics.DrawString("OP", font, brush, 128, 110, format);
    }
    
    // Texto "By KYZ"
    using (var font = new Font("Arial", 28, FontStyle.Regular))
    using (var brush = new SolidBrush(Color.FromArgb(232, 232, 232)))
    {
        var format = new StringFormat();
        format.Alignment = StringAlignment.Center;
        format.LineAlignment = StringAlignment.Center;
        graphics.DrawString("By KYZ", font, brush, 128, 170, format);
    }
}

// Asegurar que el directorio existe
var outputPath = @"..\OptimizerApp\Assets\AppIcon.png";
Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

// Guardar como PNG
bitmap.Save(outputPath, System.Drawing.Imaging.ImageFormat.Png);
bitmap.Dispose();

Console.WriteLine($"✓ Ícono creado: {Path.GetFullPath(outputPath)}");

