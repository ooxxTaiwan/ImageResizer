using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ImageResizer
{
    class Program
    {

        static async Task Main(string[] args)
        {
            var sourcePath = Path.Combine(Environment.CurrentDirectory, "images");
            var destinationPath = Path.Combine(Environment.CurrentDirectory, "output"); ;

            var imageProcess = new ImageProcess();

            imageProcess.Clean(destinationPath);

            var progress = new Progress<int>((i) =>
            {
                Console.Write('*');
            });

            var tokenSource = new CancellationTokenSource();
            var sw = new Stopwatch();
            sw.Start();
            await imageProcess.ResizeImagesAsync(sourcePath, destinationPath, 2.0, progress, tokenSource.Token);
            sw.Stop();

            Console.WriteLine($"\n\n花費時間: {sw.ElapsedMilliseconds} ms");
        }
    }
}
