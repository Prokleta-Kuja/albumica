using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;

namespace albumica
{
    public class MetaInfo
    {
        static readonly List<(bool IsVideo, string Signature)> Signatures = new List<(bool IsVideo, string Signature)>
        { // must be ordered, more on https://en.wikipedia.org/wiki/List_of_file_signatures
            (false,"89-50-4E-47-0D-0A-1A-0A"),  // PNG
            (true,"66-74-79-70-69-73-6F-6D"),   // MP4
            (false,"FF-D8"),                    // JPeG
        };
        static readonly ExifTag[] CoordinateKeys = new ExifTag[] { ExifTag.GPSLatitudeRef, ExifTag.GPSLatitude, ExifTag.GPSLongitude };
        static readonly ExifTag[] AltitudeKeys = new ExifTag[] { ExifTag.GPSAltitude, ExifTag.GPSAltitudeRef };
        public FileInfo FileInfo { get; set; }
        public bool IsVideo { get; set; }
        public bool HasGpsData { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Altitude { get; set; }

        public MetaInfo(FileInfo fileInfo)
        {
            FileInfo = fileInfo.Exists ? fileInfo : throw new FileNotFoundException("Not found", fileInfo.FullName);

            LoadMetadata();
        }
        public bool LoadMetadata()
        {
            // Check file signature, load max signature lenght (currently 8)
            byte[] fileHeader = new byte[8];
            using var fileStream = FileInfo.OpenRead();
            fileStream.Read(fileHeader, 0, 8);
            var fileHeaderHex = BitConverter.ToString(fileHeader);

            foreach (var signature in Signatures)
                if (fileHeaderHex.StartsWith(signature.Signature))
                {
                    if (signature.IsVideo)
                        LoadVideoInfo();
                    else
                        LoadImageInfo();

                    return true;
                }

            return false;
        }
        void LoadImageInfo()
        {
            var imageInfo = Image.Identify(FileInfo.FullName);
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

                    HasGpsData = true;
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
                    Altitude *= -1; // Below see level
            }
        }
        void LoadVideoInfo()
        {

        }
    }
}