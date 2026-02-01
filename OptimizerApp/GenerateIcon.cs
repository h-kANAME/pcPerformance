using System.Drawing;
using System.Drawing.Imaging;

// Crear ícono desde SVG
string svgPath = @"d:\Desarrollo\pcPerformance\OptimizerApp\OptimizerApp\Assets\AppIcon.svg";
string icoPath = @"d:\Desarrollo\pcPerformance\OptimizerApp\OptimizerApp\Assets\AppIcon.ico";

// Crear bitmap de 256x256
using (Bitmap bitmap = new Bitmap(256, 256))
{
    using (Graphics g = Graphics.FromImage(bitmap))
    {
        g.Clear(Color.FromArgb(13, 13, 13)); // #0D0D0D
        
        // Borde verde
        using (Pen greenPen = new Pen(Color.FromArgb(68, 214, 44), 3))
        {
            g.DrawRectangle(greenPen, 8, 8, 240, 240);
        }
        
        // Texto OP
        using (Font opFont = new Font("Arial", 96, FontStyle.Bold))
        {
            using (SolidBrush greenBrush = new SolidBrush(Color.FromArgb(68, 214, 44)))
            {
                StringFormat format = new StringFormat();
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;
                g.DrawString("OP", opFont, greenBrush, 128, 110, format);
            }
        }
        
        // Texto "By KYZ"
        using (Font kyxFont = new Font("Arial", 18, FontStyle.Regular))
        {
            using (SolidBrush whiteBrush = new SolidBrush(Color.FromArgb(232, 232, 232)))
            {
                StringFormat format = new StringFormat();
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;
                g.DrawString("By KYZ", kyxFont, whiteBrush, 128, 165, format);
            }
        }
    }
    
    // Guardar como ICO
    Icon icon = Icon.FromHandle(bitmap.GetHicon());
    using (FileStream fs = new FileStream(icoPath, FileMode.Create))
    {
        icon.Save(fs);
    }
}

Console.WriteLine($"Ícono creado: {icoPath}");
