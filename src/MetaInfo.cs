using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;

namespace albumica
{
    public class MetaInfo
    {
        static readonly Regex Iso6709 = new(@"(\+|-)\d+\.?\d*", RegexOptions.Compiled);
        static readonly ExifTag[] CoordinateKeys = new ExifTag[] { ExifTag.GPSLatitudeRef, ExifTag.GPSLatitude, ExifTag.GPSLongitude };
        static readonly ExifTag[] AltitudeKeys = new ExifTag[] { ExifTag.GPSAltitude, ExifTag.GPSAltitudeRef };
        public FileInfo FileInfo { get; set; }
        public bool IsVideo { get; set; }
        public DateTime Created { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double? Altitude { get; set; }
        public bool HasGpsData => Latitude.HasValue && Longitude.HasValue;

        public MetaInfo(FileInfo fileInfo)
        {
            FileInfo = fileInfo.Exists ? fileInfo : throw new FileNotFoundException("Not found", fileInfo.FullName);
            Created = fileInfo.CreationTimeUtc;

            Load();
        }
        public bool Load() => LoadImage() || LoadVideo();
        bool LoadImage()
        {
            var imageInfo = Image.Identify(FileInfo.FullName);
            if (imageInfo == null)
                return false;

            var exif = imageInfo.Metadata.ExifProfile.Values.ToDictionary(m => m.Tag);

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
                }
            }

            if (AltitudeKeys.All(k => exif.ContainsKey(k)))
            {
                var alt = (Rational)exif[ExifTag.GPSAltitude].GetValue();
                var altRef = (byte)exif[ExifTag.GPSAltitudeRef].GetValue();

                Altitude = alt.ToDouble();
                if (altRef == 1)
                    Altitude *= -1; // Below sea level
            }

            return true;
        }
        bool LoadVideo()
        {
            var psi = new ProcessStartInfo("ffprobe", $"-v quiet -show_format -print_format json -i {FileInfo.FullName}")
            {
                RedirectStandardOutput = true
            };
            var process = Process.Start(psi);
            process!.WaitForExit();

            var result = process.StandardOutput.ReadToEnd();
            var info = JsonSerializer.Deserialize<FFmpegResult>(result);
            IsVideo = info?.Format != null;

            if (!IsVideo)
                return false;

            if (info!.Format!.Tags.TryGetValue("creation_time", out var dtStr))
                if (DateTime.TryParse(dtStr, out var dt))
                    Created = dt;

            if (info.Format.Tags.TryGetValue("location", out var locStr))
            {
                var matches = Iso6709.Matches(locStr);
                if (matches.Count != 2)
                {
                    //TODO: log locstr
                }

                if (double.TryParse(matches[0].Value, out var latitude))
                    Latitude = latitude;

                if (double.TryParse(matches[1].Value, out var longitude))
                    Longitude = longitude;
            }

            return true;
        }
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

/*
{
    "format": {
        "filename": "/workspaces/albumica/src/VID_20210815_124520.mp4",
        "nb_streams": 2,
        "nb_programs": 0,
        "format_name": "mov,mp4,m4a,3gp,3g2,mj2",
        "format_long_name": "QuickTime / MOV",
        "start_time": "0.000000",
        "duration": "3.200000",
        "size": "8295143",
        "bit_rate": "20737857",
        "probe_score": 100,
        "tags": {
            "major_brand": "mp42",
            "minor_version": "0",
            "compatible_brands": "isommp42",
            "creation_time": "2021-08-15T10:45:25.000000Z",
            "location": "+45.8100+015.9724/",
            "location-eng": "+45.8100+015.9724/",
            "com.android.version": "11",
            "com.android.manufacturer": "Xiaomi",
            "com.android.model": "Mi A2"
        }
    }
}
{

}

*/