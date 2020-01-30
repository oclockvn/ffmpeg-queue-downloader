using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace QffmpegDownloader
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .Build();

            var delay = config.GetValue<int>("Delay", 3);
            var start = config.GetValue<int>("Start", 0);

            Console.WriteLine($"> Configuration: start = {start}, delay = {delay} (minutes)");

            var files = Directory.GetFiles(".", "**.bat"); // looking for .bat files

            if (files == null || files.Length == 0)
            {
                Console.WriteLine("No bat files found. Press enter to exit...");
                Console.ReadLine();
                return;
            }

            var list = files.OrderBy(x => x).ToList();
            var eps = list.Select(l => GetEpNumber(l)).ToList();
            var latestEp = eps.Max();

            if (latestEp < 0)
            {
                Console.WriteLine("Invalid bat name. Press enter to exit...");
                Console.ReadLine();
                return;
            }

            while (start <= latestEp)
            {
                var bat = $"{start}.bat"; // = 2.bat
                var result = $"E{start}.mp4"; // = E2.mp4 

                if (eps.Contains(start) && !File.Exists(result))
                {
                    Console.WriteLine($"> {bat} downloading...");
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = bat,
                        CreateNoWindow = false,
                        WindowStyle = ProcessWindowStyle.Normal,
                        UseShellExecute = true,
                    });
                    await Task.Delay(delay * 1000 * 60);
                }

                start++;
            }

            Console.WriteLine("Done! Press enter to exit...");
            Console.ReadLine();
        }

        /// <summary>
        /// get the number from filename.bat
        /// </summary>
        /// <param name="name">015.bat</param>
        /// <returns>15</returns>
        static int GetEpNumber(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return -1;

            var nameonly = Path.GetFileNameWithoutExtension(name);
            //var names = name.Split('.');
            if (int.TryParse(nameonly, out var ep) && ep > 0)
                return ep;

            return -1;
        }
    }
}
