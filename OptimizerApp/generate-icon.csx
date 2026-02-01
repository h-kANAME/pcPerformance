#!/usr/bin/env dotnet-script
// Script para generar AppIcon.ico desde la definición del icono
// Ejecutar: dotnet script generate-icon.csx

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

Console.WriteLine("Generando icono AppIcon.ico...");

// Ruta de salida
string outputPath = @"OptimizerApp\Assets\AppIcon.ico";
Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

// Crear icono en múltiples tamaños para mejor compatibilidad
int[] sizes = { 16, 32, 48, 64, 128, 256 };
List<Bitmap> bitmaps = new List<Bitmap>();

foreach (int size in sizes)
{
    Bitmap bitmap = CreateAppIcon(size);
    bitmaps.Add(bitmap);
}

// Guardar como ICO con múltiples tamaños
SaveAsIcon(bitmaps, outputPath);

Console.WriteLine($"✓ Icono creado exitosamente en: {Path.GetFullPath(outputPath)}");

// Función para crear el bitmap del icono
Bitmap CreateAppIcon(int size)
{
    var bitmap = new Bitmap(size, size);
    using (var graphics = Graphics.FromImage(bitmap))
    {
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

        // Fondo negro
        graphics.Clear(Color.FromArgb(13, 13, 13)); // #0D0D0D

        // Borde verde Razer
        using (var pen = new Pen(Color.FromArgb(68, 214, 44), Math.Max(1, size / 64))) // #44D62C
        {
            int margin = Math.Max(2, size / 16);
            graphics.DrawRectangle(pen, margin, margin, size - margin * 2, size - margin * 2);
        }

        // Texto "OP"
        if (size >= 16)
        {
            float fontSize = size * 0.48f;
            using (var font = new Font("Arial", fontSize, FontStyle.Bold, GraphicsUnit.Pixel))
            using (var brush = new SolidBrush(Color.FromArgb(68, 214, 44)))
            {
                var format = new StringFormat();
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;
                
                float textY = size * 0.35f;
                graphics.DrawString("OP", font, brush, size / 2f, textY, format);
            }
        }

        // Texto "By KYZ" (solo en tamaños mayores)
        if (size >= 32)
        {
            float fontSize = size * 0.12f;
            using (var font = new Font("Arial", fontSize, FontStyle.Regular, GraphicsUnit.Pixel))
            using (var brush = new SolidBrush(Color.FromArgb(232, 232, 232)))
            {
                var format = new StringFormat();
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;
                
                float textY = size * 0.65f;
                graphics.DrawString("By KYZ", font, brush, size / 2f, textY, format);
            }
        }
    }

    return bitmap;
}

// Función para guardar como ICO
void SaveAsIcon(List<Bitmap> bitmaps, string path)
{
    using (var fs = new FileStream(path, FileMode.Create))
    {
        // Encabezado ICO
        fs.WriteByte(0);
        fs.WriteByte(0);
        fs.WriteByte(1);
        fs.WriteByte(0);
        fs.WriteByte((byte)bitmaps.Count);
        fs.WriteByte(0);

        int offset = 6 + (bitmaps.Count * 16);
        var imageOffsets = new List<int>();

        // Escribir directorio de imágenes
        foreach (var bitmap in bitmaps)
        {
            imageOffsets.Add(offset);
            
            fs.WriteByte((byte)bitmap.Width);
            fs.WriteByte((byte)bitmap.Height);
            fs.WriteByte(0); // Color count (0 = no paleta)
            fs.WriteByte(0); // Reserved
            fs.WriteUShort(1); // Color planes
            fs.WriteUShort(32); // Bits per pixel

            // Calcular tamaño de la imagen BMP
            using (var memStream = new MemoryStream())
            {
                bitmap.Save(memStream, System.Drawing.Imaging.ImageFormat.Bmp);
                byte[] bmpData = memStream.ToArray();
                int dataSize = bmpData.Length;
                
                fs.WriteDWord((uint)dataSize);
                fs.WriteDWord((uint)offset);
                
                offset += dataSize;
            }
        }

        // Escribir datos de imágenes
        foreach (var bitmap in bitmaps)
        {
            bitmap.Save(fs, System.Drawing.Imaging.ImageFormat.Bmp);
        }
    }
}

// Extension methods
internal static class StreamExtensions
{
    public static void WriteUShort(this Stream stream, ushort value)
    {
        stream.WriteByte((byte)(value & 0xFF));
        stream.WriteByte((byte)((value >> 8) & 0xFF));
    }

    public static void WriteDWord(this Stream stream, uint value)
    {
        stream.WriteByte((byte)(value & 0xFF));
        stream.WriteByte((byte)((value >> 8) & 0xFF));
        stream.WriteByte((byte)((value >> 16) & 0xFF));
        stream.WriteByte((byte)((value >> 24) & 0xFF));
    }
}
