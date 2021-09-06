using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;

namespace albumica.Models
{
    public class ImportModel
    {
        static readonly MD5 Hasher = MD5.Create();
        static readonly Regex Iso6709 = new(@"(\+|-)\d+\.?\d*", RegexOptions.Compiled);
        static readonly ExifTag[] CoordinateKeys = new ExifTag[] { ExifTag.GPSLatitudeRef, ExifTag.GPSLatitude, ExifTag.GPSLongitude };
        static readonly ExifTag[] AltitudeKeys = new ExifTag[] { ExifTag.GPSAltitude, ExifTag.GPSAltitudeRef };

        public FileInfo FileInfo { get; set; }
        public string Uri { get; set; }
        public string Hash { get; set; } = string.Empty;
        public ILogger<ImportModel> Logger { get; set; }
        public bool IsVideo { get; set; }
        public DateTime Created { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double? Altitude { get; set; }
        public bool HasGpsData => Latitude.HasValue && Longitude.HasValue;
        public ImportModel(FileInfo fileInfo, ILogger<ImportModel> logger)
        {
            FileInfo = fileInfo.Exists ? fileInfo : throw new FileNotFoundException("Not found", fileInfo.FullName);
            Logger = logger;
            Created = fileInfo.CreationTimeUtc;

            var relativePath = Path.GetRelativePath(C.Settings.ImportRootPath, FileInfo.FullName);
            Uri = C.Routes.ResizerImportFor(relativePath);
        }
        public async Task ComputeHashAsync()
        {
            using var stream = FileInfo.OpenRead();
            var hash = await Hasher.ComputeHashAsync(stream);
            Hash = BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
        public async Task<bool> LoadAsync() => await LoadImageAsync() || await LoadVideoAsync();
        async Task<bool> LoadImageAsync()
        {
            var imageInfo = await Image.IdentifyAsync(FileInfo.FullName);
            var isImage = imageInfo != null;
            if (!isImage)
                return false;

            var exif = imageInfo!.Metadata.ExifProfile.Values.ToDictionary(m => m.Tag);

            if (CoordinateKeys.All(k => exif.ContainsKey(k)))
            {
                var lat = exif[ExifTag.GPSLatitude].GetValue() as Rational[];
                var lon = exif[ExifTag.GPSLongitude].GetValue() as Rational[];
                var latRef = exif[ExifTag.GPSLatitudeRef].GetValue();

                if (lat != null && lon != null && latRef != null)
                {
                    var longitude = lon[0].ToDouble() + (lon[1].ToDouble() / 60) + (lon[2].ToDouble() / 3600);
                    var latitude = lat[0].ToDouble() + (lat[1].ToDouble() / 60) + (lat[2].ToDouble() / 3600);
                    if (latRef.ToString()!.Equals("S", StringComparison.InvariantCultureIgnoreCase))
                        latitude *= -1;

                    Latitude = latitude;
                    Longitude = longitude;
                    Logger.LogDebug("Coordinates found: {Latitude}, {Longitude}", latitude, longitude);
                }
            }
            else
                Logger.LogDebug("No coordinate keys found");

            if (AltitudeKeys.All(k => exif.ContainsKey(k)))
            {
                var alt = (Rational)exif[ExifTag.GPSAltitude].GetValue();
                var altRef = (byte)exif[ExifTag.GPSAltitudeRef].GetValue();

                Altitude = alt.ToDouble();
                if (altRef == 1)
                    Altitude *= -1; // Below sea level
            }
            else
                Logger.LogDebug("No altitude keys found");

            return isImage;
        }
        async Task<bool> LoadVideoAsync()
        {
            var psi = new ProcessStartInfo("ffprobe", $@"-v quiet -show_format -print_format json -i ""{FileInfo.FullName}""")
            {
                RedirectStandardOutput = true
            };
            var process = Process.Start(psi);
            await process!.WaitForExitAsync();

            var result = process.StandardOutput.ReadToEnd();
            if (string.IsNullOrWhiteSpace(result))
                return false;

            var info = JsonSerializer.Deserialize<FFmpegResult>(result);
            IsVideo = info?.Format != null;

            if (!IsVideo)
                return false;

            if (info!.Format!.Tags.TryGetValue("creation_time", out var dtStr))
                if (DateTime.TryParse(dtStr, out var dt))
                    Created = dt.ToUniversalTime();

            if (info.Format.Tags.TryGetValue("location", out var locStr))
            {
                var matches = Iso6709.Matches(locStr);
                if (matches.Count != 2)
                {
                    Logger.LogError("Coordinates format unknown: {location}", locStr);
                }

                if (double.TryParse(matches[0].Value, out var latitude))
                    Latitude = latitude;

                if (double.TryParse(matches[1].Value, out var longitude))
                    Longitude = longitude;


                Logger.LogDebug("Coordinates found: {Latitude}, {Longitude}", latitude, longitude);
            }
            else
                Logger.LogDebug("No coordinate keys found");

            return IsVideo;
        }
        public async Task CreateVideoPreview(string imageDestination)
        {
            var sb = new StringBuilder();
            sb.Append(" -r 16 "); // Framerate
            sb.Append("-ss 0 "); // Start from
            sb.Append($@"-i ""{FileInfo.FullName}"" ");
            sb.Append("-loop 0 "); // Loop webp
            sb.Append("-lavfi "); // Adds nice background to vertical videos
            sb.Append(@"""[0:v]scale=ih*16/9:-1,boxblur=luma_radius=min(h\,w)/20:luma_power=1:chroma_radius=min(cw\,ch)/20:chroma_power=1[bg];[bg][0:v]overlay=(W-w)/2:(H-h)/2,setpts=0.3*PTS,scale=320:-1,crop=h=iw*9/16"" ");
            sb.Append("-vb 800K "); // Variable bitrate
            sb.Append("-t 00:00:05 "); // Length from start
            sb.Append($@"""{imageDestination}"" ");
            sb.Append("-y ");

            var psi = new ProcessStartInfo("ffmpeg", sb.ToString())
            {
                RedirectStandardOutput = true,
            };
            var process = Process.Start(psi);
            await process!.WaitForExitAsync();

            if (process.ExitCode != 0)
                Logger.LogError("Something went wrong when creating video preview {Output}", process.StandardOutput.ReadToEnd());
        }
        public class Format
        {
            [JsonPropertyName("duration")] public string? Duration { get; set; }
            [JsonPropertyName("bit_rate")] public string? BitRate { get; set; }
            [JsonPropertyName("tags")] public Dictionary<string, string> Tags { get; set; } = new();
        }

        public class FFmpegResult
        {
            [JsonPropertyName("format")] public Format? Format { get; set; }
        }
    }
}