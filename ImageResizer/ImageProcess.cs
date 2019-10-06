using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ImageResizer
{
    public class ImageProcess
    {
        /// <summary>
        /// 清空目的目錄下的所有檔案與目錄
        /// </summary>
        /// <param name="destPath">目錄路徑</param>
        public void Clean(string destPath)
        {
            if (!Directory.Exists(destPath))
            {
                Directory.CreateDirectory(destPath);
            }
            else
            {
                var allImageFiles = Directory.GetFiles(destPath, "*", SearchOption.AllDirectories);

                foreach (var item in allImageFiles)
                {
                    File.Delete(item);
                }
            }
        }

        /// <summary>
        /// 進行圖片的縮放作業
        /// </summary>
        /// <param name="sourcePath">圖片來源目錄路徑</param>
        /// <param name="destPath">產生圖片目的目錄路徑</param>
        /// <param name="scale">縮放比例</param>
        /// <param name="progress">工作進度回報</param>
        /// <param name="token">取消權杖</param>
        public Task ResizeImagesAsync(string sourcePath, string destPath, double scale, IProgress<int> progress, CancellationToken token = default)
        {
            var tasks = FindImages(sourcePath)
                .AsParallel()
                .Select((filePath, index) => ResizeImageAsync(index, filePath, destPath, scale, progress, token))
                .ToArray();

            return Task.WhenAll(tasks);
        }

        private async Task ResizeImageAsync(int index, string filePath, string destPath, double scale, IProgress<int> progress, CancellationToken token = default)
        {
            var imgPhoto = Image.FromFile(filePath);
            var imgName = Path.GetFileNameWithoutExtension(filePath);

            var sourceWidth = imgPhoto.Width;
            var sourceHeight = imgPhoto.Height;

            var destionatonWidth = (int)(sourceWidth * scale);
            var destionatonHeight = (int)(sourceHeight * scale);

            if (token.IsCancellationRequested)
            {
                return;
            }

            var processedImage =
                await Task.Run(() => processBitmap(
                    (Bitmap)imgPhoto,
                    sourceWidth, sourceHeight,
                    destionatonWidth, destionatonHeight), token);

            var destFile = Path.Combine(destPath, imgName + ".jpg");
            processedImage.Save(destFile, ImageFormat.Jpeg);
            progress.Report(index);
        }

        /// <summary>
        /// 找出指定目錄下的圖片
        /// </summary>
        /// <param name="srcPath">圖片來源目錄路徑</param>
        /// <returns></returns>
        public List<string> FindImages(string srcPath)
        {
            var files = new List<string>();
            files.AddRange(Directory.GetFiles(srcPath, "*.png", SearchOption.AllDirectories));
            files.AddRange(Directory.GetFiles(srcPath, "*.jpg", SearchOption.AllDirectories));
            files.AddRange(Directory.GetFiles(srcPath, "*.jpeg", SearchOption.AllDirectories));
            return files;
        }

        /// <summary>
        /// 針對指定圖片進行縮放作業
        /// </summary>
        /// <param name="img">圖片來源</param>
        /// <param name="srcWidth">原始寬度</param>
        /// <param name="srcHeight">原始高度</param>
        /// <param name="newWidth">新圖片的寬度</param>
        /// <param name="newHeight">新圖片的高度</param>
        /// <returns></returns>
        Bitmap processBitmap(Bitmap img, int srcWidth, int srcHeight, int newWidth, int newHeight)
        {
            var resizedbitmap = new Bitmap(newWidth, newHeight);
            var g = Graphics.FromImage(resizedbitmap);
            g.InterpolationMode = InterpolationMode.High;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.Clear(Color.Transparent);
            g.DrawImage(img,
                new Rectangle(0, 0, newWidth, newHeight),
                new Rectangle(0, 0, srcWidth, srcHeight),
                GraphicsUnit.Pixel);
            return resizedbitmap;
        }
    }
}
