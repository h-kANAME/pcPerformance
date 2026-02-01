using System.Drawing;

// Crear bitmap 256x256
var bitmap = new Bitmap(256, 256);
using (var graphics = Graphics.FromImage(bitmap))
{
    graphics.Clear(Color.FromArgb(13, 13, 13)); // #0D0D0D - Negro Razer
    
    // Borde verde
    using (var pen = new Pen(Color.FromArgb(68, 214, 44), 3)) // #44D62C - Verde Razer
    {
        graphics.DrawRectangle(pen, 8, 8, 240, 240);
    }
    
    // Texto "OP"
    using (var font = new Font("Arial", 96, FontStyle.Bold))
    using (var brush = new SolidBrush(Color.FromArgb(68, 214, 44)))
    {
        var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        graphics.DrawString("OP", font, brush, 128, 110, format);
    }
    
    // Texto "By KYZ"
    using (var font = new Font("Arial", 18, FontStyle.Regular))
    using (var brush = new SolidBrush(Color.FromArgb(232, 232, 232)))
    {
        var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        graphics.DrawString("By KYZ", font, brush, 128, 165, format);
    }
}

// Guardar como PNG
bitmap.Save("Assets/AppIcon.png", System.Drawing.Imaging.ImageFormat.Png);
bitmap.Dispose();

Console.WriteLine("Ícono creado: Assets/AppIcon.png");
