# ffmpeg-queue-downloader
A tool that automatically start window process to download using ffmpeg 😁😎

# appsettings.json

```
{
  "Delay": 3 // delay 3 minutes before next download
  "Input": "playlist.txt" // input file contains list of link to download, default is playlist.txt if not provide
}
```

# playlist.txt

This file contains list of the download link, each link in a separate line. Its content can be in 2 formats:

### 1. ffmpeg format (sample playlist.ffmpeg.txt)

then each line is a ffmpeg format:

```
ffmpeg -i https://path/to/playlist.m3u8 -other -parameters
```

You can generate ffmpeg link with this small tool.

### 2. https link only (sample playlist.https.txt)

In this way, each link is a link of `.m3u8`, the tool will update to correct ffmpeg format. In this format, you can provide output folder and start episode in the first line, if omitted, the tool will use default format

```
my-output-folder 1
https://path/to/playlist1.m3u8
https://path/to/playlist1.m3u8
https://path/to/playlist1.m3u8
```
