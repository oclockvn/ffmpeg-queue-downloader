using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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

            var delay = config.GetValue<int>("Delay", 3); // in minutes
            var input = config.GetValue<string>("Input", "playlist.txt"); // playlist name, default is playlist.txt

            var currDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var inputFile = Path.Combine(currDir, input);

            if (!File.Exists(inputFile))
            {
                Console.WriteLine("Download list not found, please add a playlist.txt or read the instruction. Press any key to exit...");
                Console.ReadKey();
                return;
            }

            var links = File.ReadAllLines(inputFile).Where(l => !string.IsNullOrWhiteSpace(l)).ToArray();
            if (links == null || links.Length == 0)
            {
                return;
            }

            var total = links.Length;
            var output = string.Empty;
            var startEpisode = 1; // start chapter, default is chapter 1
            var startIndex = 0;

            // if first line is a ffmpeg command already, no need to create the output folder
            var firstLine = links[0];
            if (!firstLine.StartsWith("ffmpeg "))
            {
                // if first line is a link, the use default output
                if (firstLine.StartsWith("http"))
                {
                    firstLine = "output 1";
                }
                else
                {
                    startIndex = 1;// first line is the info line, so skip it from total download
                }

                // first line format
                // output-folder ep-number
                var info = firstLine.Split(" ");
                output = info[0];

                if (info.Length == 2)
                {
                    if (int.TryParse(info[1], out var temp))
                    {
                        startEpisode = temp;
                    }
                }

                var outputDir = Path.Combine(currDir, output);

                try
                {
                    if (!Directory.Exists(output))
                    {
                        Directory.CreateDirectory(output);
                    }
                }
                catch
                {
                    return; // cannot create folder for some reason, stop program
                }
            }

            for (var i = startIndex; i < total; i++)
            {
                var actualLink = BuildArgument(links[i], output, startEpisode);
                Debug.WriteLine($"> Downloading {actualLink}");

                Process.Start(new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = actualLink,
                    CreateNoWindow = false,
                    WindowStyle = ProcessWindowStyle.Normal,
                    UseShellExecute = true,
                });

                // delay for next download
                await Task.Delay(delay * 1000 * 60);

                startEpisode++;
            }

            // Console.WriteLine("Done! Press enter to exit...");
            // Console.ReadLine();
        }

        /// <summary>
        /// If link is not a ffmpeg link then format it regardless the correct format, otherwise return as is
        /// </summary>
        /// <param name="link">The link</param>
        /// <returns></returns>
        static string BuildArgument(string link, string output, int ep = 1)
        {
            if (string.IsNullOrWhiteSpace(link))
            {
                return string.Empty;
            }

            if (link.StartsWith("ffmpeg"))
            {
                return link[6..]; // remove ffmpeg as we'll use it as the filename for the process
            }

            return $"-i {link} -map p:2? -bsf:a aac_adtstoasc -vcodec copy -c copy -crf 50 {output}/ep-{ep}.mp4";
        }
    }
}
