# LiveStreamingServer is a simple web server which provides an HLS stream via the web.

## Requirements
- [.Net Core 3.1 Runtime](https://dotnet.microsoft.com/download/dotnet-core/3.1)
- [FFmpeg](https://www.ffmpeg.org/)

## Server Options
Usage:
	LiveStreamingServer.exe -ffmpeg <path> -output <path> \[-host <name>\] \[-ffmpegport <port:8889>\] \[-fps <fps>\] \[-keyframe <frames>\] \[-listsize <size>\] \[-hlswrap <count>\] 

Options:
| Option      | Required? | Default Value | Description                                                                                                          |
| ----------- | --------- | ------------- | -------------------------------------------------------------------------------------------------------------------- |
| -ffmpeg     | Yes       |               | Full path to ffmpeg.exe.                                                                                             |
| -output     | Yes       |               | Full path to file output.                                                                                            |
| -host       | No        |               | Hostname for web server.                                                                                             |
| -ffmpegport | No        | 8889          | Port for ffmpeg server.                                                                                              |
| -fps        | No        | 30            | Target FPS output.                                                                                                   |
| -keyframe   | No        | 30            | Frames until key frame. This has a large effect on the stream delay. The formula is roughly 2 to 3 * keyframe / fps. |
| -listsize   | No        | 3             | HLS list size. This is how many video chunks will be listed to the client.                                           |
| -hlswrap    | No        | 2             | HLS file wrap. This is how many video chunks will be created before wrapping around and overwriting old files.       |

## OBS Setup
1. Download OBS from [https://obsproject.com/](https://obsproject.com/)
2. Go to Settings > Stream
3. Change *Service* to `Custom...`
4. Change *Server* to `rtmp://<ip address>:8889/live/app`
5. Clear *Stream key* and uncheck *Use authentication*
6. Start LiveStreamingServer.
7. Click Start Streaming.
