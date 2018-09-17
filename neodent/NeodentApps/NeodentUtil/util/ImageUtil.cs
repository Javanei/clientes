using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace NeodentUtil.util
{
    public class ImageUtil
    {
        public static void MergeImageList(string[] files, string outputFile)
        {
            LOG.debug("@@@@ ImageUtil.MergeImageList - 1  - (outputFile=" + outputFile + ")");
            List<Image> images = new List<Image>();
            int maxWidth = 0;
            int maxHeight = 0;
            int totalHeight = 0;
            PixelFormat maxFormat = PixelFormat.Undefined;

            LOG.debug("@@@@ ImageUtil.MergeImageList - 2  - files=" + files.Length);
            foreach (var file in files)
            {
                string s = file.ToLower();
                if (s.EndsWith(".bmp")
                    || s.EndsWith(".emf")
                    || s.EndsWith(".wmf")
                    || s.EndsWith(".gif")
                    || s.EndsWith(".jpg") || s.EndsWith(".jpeg")
                    || s.EndsWith(".png")
                    || s.EndsWith(".tif") || s.EndsWith(".tiff")
                    || s.EndsWith(".exif")
                    || s.EndsWith(".ico")
                    )
                {
                    LOG.debug("@@@@@@ ImageUtil.MergeImageList - 3  - file=" + file);

                    Bitmap img = (Bitmap)Image.FromFile(file);
                    images.Add(img);
                    totalHeight += img.Height;
                    if (img.Height > maxHeight)
                    {
                        maxHeight = img.Height;
                    }
                    if (img.Width > maxWidth)
                    {
                        maxWidth = img.Width;
                    }
                    if (((int)img.PixelFormat) > ((int)maxFormat))
                    {
                        maxFormat = img.PixelFormat;
                    }
                    PixelFormat format = img.PixelFormat;
                    LOG.debug("@@@@@@ ImageUtil.MergeImageList - 4  - file=" + file + " -> " + img.Width + " X " + img.Height);
                    LOG.debug("@@@@@@ ImageUtil.MergeImageList - 5  - PixelFormat: " + format + " -> " + (int)format);
                    LOG.debug("@@@@@@ ImageUtil.MergeImageList - 6  - Type: " + img.GetType());
                }
            }
            LOG.debug("@@@@ ImageUtil.MergeImageList - 7  - Max width...=" + maxWidth);
            LOG.debug("@@@@ ImageUtil.MergeImageList - 8  - Max height..= " + maxHeight);
            LOG.debug("@@@@ ImageUtil.MergeImageList - 9  - Total height= " + totalHeight);
            LOG.debug("@@@@ ImageUtil.MergeImageList - 10 - Max format..= " + maxFormat);


            Bitmap bitmap = new Bitmap(maxWidth, totalHeight, maxFormat);
            Graphics g = Graphics.FromImage(bitmap);
            float startY = 0;
            foreach (var img in images)
            {
                g.DrawImage(img, 0f, startY, img.Width, img.Height);
                startY += img.Height;
                img.Dispose();
            }

            LOG.debug("@@@@ ImageUtil.MergeImageList - 11 - outputFile= " + outputFile);

            if (File.Exists(outputFile))
            {
                LOG.debug("@@@@@@ ImageUtil.MergeImageList - 12 - arquivo '" + outputFile + "' já existia, tentando apagar");
                File.Delete(outputFile);
            }

            if (outputFile.ToLower().EndsWith(".png"))
            {
                LOG.debug("@@@@ ImageUtil.MergeImageList - 13 - Salvando arquivo como PNG");
                bitmap.Save(outputFile, ImageFormat.Png);
            }
            else if (outputFile.ToLower().EndsWith(".gif"))
            {
                LOG.debug("@@@@ ImageUtil.MergeImageList - 14 - Salvando arquivo como GIF");
                bitmap.Save(outputFile, ImageFormat.Gif);
            }
            else if (outputFile.ToLower().EndsWith(".bmp"))
            {
                LOG.debug("@@@@ ImageUtil.MergeImageList - 15 - Salvando arquivo como BMP");
                bitmap.Save(outputFile, ImageFormat.Bmp);
            }
            else
            {
                LOG.debug("@@@@ ImageUtil.MergeImageList - 16 - Salvando arquivo como JPG");
                bitmap.Save(outputFile, ImageFormat.Jpeg);
            }
            bitmap.Dispose();
            LOG.debug("@@@@ ImageUtil.MergeImageList - 17 - FIM");
        }

        public static void MergeImagesOnFolder(string folderPath, string outputFile)
        {
            string[] files = Directory.GetFiles(folderPath);
            MergeImageList(files, outputFile);
        }
    }
}
