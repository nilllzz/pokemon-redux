using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace MinispriteExtractor
{
    class Program
    {
        static void Main(string[] args)
        {
            var execFolder = AppDomain.CurrentDomain.BaseDirectory;
            var outFolder = Path.Combine(execFolder, "out");

            if (!Directory.Exists(outFolder))
            {
                Directory.CreateDirectory(outFolder);
            }

            var bmp = new Bitmap(Path.Combine(execFolder, "minisprites.png"));

            var i = 1;
            for (int y = 0; y < 17; y++)
            {
                for (int x = 0; x < 15; x++)
                {
                    if (i <= 251)
                    {
                        var px = 1 + 17 * x * 2;
                        var py = 1 + 25 * y;
                        var spr1 = bmp.Clone(new Rectangle(px, py, 16, 16), bmp.PixelFormat);
                        var spr2 = bmp.Clone(new Rectangle(px + 17, py, 16, 16), bmp.PixelFormat);
                        var res = new Bitmap(32, 16);
                        var g = Graphics.FromImage(res);
                        g.Clear(Color.Transparent);
                        g.DrawImage(spr1, new Rectangle(0, 0, 16, 16));
                        g.DrawImage(spr2, new Rectangle(16, 0, 16, 16));
                        res.Save(Path.Combine(outFolder, $"{i}.png"), ImageFormat.Png);
                        i++;
                    }
                }
            }
        }
    }
}
