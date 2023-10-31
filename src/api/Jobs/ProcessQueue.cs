using System.ComponentModel;
using System.Globalization;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using albumica.Entities;
using FFMpegCore;
using FFMpegCore.Enums;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;

namespace albumica.Jobs;

public partial class ProcessQueue
{
    const string VID_CREATED_TAG = "creation_time";
    const string EXIF_DATETIME_FORMAT = "yyyy:MM:dd HH:mm:ss";
    static readonly HashSet<string> s_imgFormats = new(StringComparer.InvariantCultureIgnoreCase) { ".JPG", ".JPEG", ".PNG", };
    static readonly HashSet<string> s_vidFormats = new(StringComparer.InvariantCultureIgnoreCase) { ".MOV", ".AVI", ".MP4", };
    readonly ILogger<ProcessQueue> _logger;
    readonly AppDbContext _db;
    public ProcessQueue(ILogger<ProcessQueue> logger, AppDbContext db)
    {
        _logger = logger;
        _db = db;
    }
    [DisplayName("Process queue")]
    [AutomaticRetry(Attempts = 0)]
    [DisableConcurrentExecution(60 * 10)] // 10min
    public async Task Run(CancellationToken token)
    {
        var files = Directory.EnumerateFiles(C.Paths.QueueData, "*", SearchOption.TopDirectoryOnly);
        foreach (var file in files)
        {
            token.ThrowIfCancellationRequested();
            try
            {
                var ext = Path.GetExtension(file);
                if (s_imgFormats.Contains(ext) || s_vidFormats.Contains(ext))
                    await InitialProcess(file, token);
                else
                    _logger.LogError("Unsupported file extension {FilePath}", file);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process {FilePath}", file);
            }
        }
    }

    [DisplayName("Reparse created")]
    [AutomaticRetry(Attempts = 0)]
    public async Task ReparseMissingCreated(CancellationToken token)
    {
        var media = await _db.Media.Where(m => !m.Created.HasValue || m.Created == DateTime.MaxValue).ToListAsync(token);

        foreach (var item in media)
        {
            var file = C.Paths.MediaDataFor(item.Original);
            if (item.IsVideo)
            {
                var mediaInfo = await FFProbe.AnalyseAsync(file, null, token);
                if (TryGetCreateDateFromVideo(mediaInfo, out var created))
                    item.Created = created;
                else if (TryGetCreateDateFromFileName(file, out var fileNameCreated))
                    item.Created = fileNameCreated;
            }
            else
            {
                var img = await Image.LoadAsync(file, token);
                if (TryGetCreateDateFromExif(img.Metadata.ExifProfile, out var exifCreated))
                    item.Created = exifCreated;
                else if (TryGetCreateDateFromFileName(file, out var fileNameCreated))
                    item.Created = fileNameCreated;
            }
        }

        await _db.SaveChangesAsync(token);
    }
    async Task InitialProcess(string filePath, CancellationToken token)
    {
        using var fs = File.Open(filePath, FileMode.Open);
        var sha256Bytes = await SHA256.HashDataAsync(fs, token);
        var sha256 = Convert.ToHexString(sha256Bytes);
        await fs.DisposeAsync();

        var isDuplicate = await _db.Media.AnyAsync(m => m.SHA256 == sha256, token);
        if (isDuplicate)
        {
            _logger.LogWarning("Deleting duplicate {FilePath}", Path.GetFileName(filePath));
            File.Delete(filePath);
            return;
        }

        var ext = Path.GetExtension(filePath); // .jpg
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(filePath); // Image
        var importFileName = Path.GetFileName(filePath); // Image.jpg
        var origFileName = importFileName; // Image.jpg
        var prevFileName = $"{nameWithoutExtension}{C.Paths.PreviewFileNameSuffix}.webp"; // Image_preview.webp
        var origFilePath = C.Paths.MediaDataFor(origFileName); // orig/Image.jpg
        var prevFilePath = C.Paths.PreviewDataFor(prevFileName); // prev/Image_preview.webp

        if (File.Exists(origFilePath)) // orig/Image.jpg
        {
            nameWithoutExtension = $"{nameWithoutExtension}_{DateTime.UtcNow.Ticks}"; // Image_123
            origFileName = $"{nameWithoutExtension}{ext}"; // Image_123.jpg
            prevFileName = $"{nameWithoutExtension}{C.Paths.PreviewFileNameSuffix}.webp";// Image_123_preview.webp
            origFilePath = C.Paths.MediaDataFor(origFileName); // orig/Image_123.jpg
            prevFilePath = C.Paths.PreviewDataFor(prevFileName); // prev/Image_123_preview.webp
        }
        File.Move(filePath, origFilePath);

        var media = new Media
        {
            SHA256 = sha256,
            Import = importFileName,
            Original = origFileName,
            IsVideo = s_vidFormats.Contains(ext),
        };
        _db.Media.Add(media);
        await _db.SaveChangesAsync();

        if (s_imgFormats.Contains(ext))
        {
            var img = await Image.LoadAsync(origFilePath, token);
            if (TryGetCreateDateFromExif(img.Metadata.ExifProfile, out var exifCreated))
                media.Created = exifCreated;
            else if (TryGetCreateDateFromFileName(origFileName, out var fileNameCreated))
                media.Created = fileNameCreated;

            // IMG
            var resize = new ResizeOptions
            {
                Size = new Size(480, 480),
                Mode = ResizeMode.Min,
            };
            img.Mutate(x => x.Resize(resize).AutoOrient());
            await img.SaveAsWebpAsync(prevFilePath, token);

            media.Preview = prevFileName;
        }
        else if (s_vidFormats.Contains(ext))
        {
            // VID
            var mediaInfo = await FFProbe.AnalyseAsync(origFilePath, null, token);
            if (TryGetCreateDateFromVideo(mediaInfo, out var created))
                media.Created = created;
            else if (TryGetCreateDateFromFileName(origFileName, out var fileNameCreated))
                media.Created = fileNameCreated;

            await FFMpegArguments
            .FromFileInput(origFilePath)
            .OutputToFile(prevFilePath, true, opt => opt
                .WithFramerate(5)
                .EndSeek(TimeSpan.FromSeconds(5))
                .Loop(0)
                .WithVideoFilters(fopt => fopt
                    .Scale(VideoSize.Ed))
            )
            .ProcessAsynchronously();

            media.Preview = prevFileName;
        }
        else
            _logger.LogError("Failed to get info or generate preview for {FilePath}", origFilePath);


        await _db.SaveChangesAsync(token);
    }

    static bool TryGetCreateDateFromExif(ExifProfile? profile, out DateTime created)
    {
        created = DateTime.MaxValue;
        if (profile == null)
            return false;

        object? val = null;
        if (profile.TryGetValue(ExifTag.DateTimeOriginal, out var edtOriginal))
            val = edtOriginal.GetValue();
        else if (profile.TryGetValue(ExifTag.DateTimeDigitized, out var edtDig))
            val = edtDig.GetValue();
        else if (profile.TryGetValue(ExifTag.DateTime, out var edt))
            val = edt.GetValue();

        if (val == null)
            return false;

        if (!DateTime.TryParseExact(val.ToString(), EXIF_DATETIME_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var parsed))
            return false;

        created = parsed.ToUniversalTime();
        return true;
    }

    static bool TryGetCreateDateFromVideo(IMediaAnalysis analysis, out DateTime created)
    {
        created = DateTime.MaxValue;
        if (analysis.Format.Tags != null &&
            analysis.Format.Tags.TryGetValue(VID_CREATED_TAG, out var tdtFormat) &&
            DateTime.TryParse(tdtFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var dtFormat))
            created = dtFormat.ToUniversalTime();
        else if (analysis.PrimaryVideoStream?.Tags != null &&
            analysis.PrimaryVideoStream.Tags.TryGetValue(VID_CREATED_TAG, out var tdtVideo) &&
            DateTime.TryParse(tdtVideo, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var dtVideo))
            created = dtVideo.ToUniversalTime();
        else if (analysis.PrimaryAudioStream?.Tags != null &&
            analysis.PrimaryAudioStream.Tags.TryGetValue(VID_CREATED_TAG, out var tdtAudio) &&
            DateTime.TryParse(tdtAudio, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var dtAudio))
            created = dtAudio.ToUniversalTime();

        return created != DateTime.MaxValue;
    }

    [GeneratedRegex("(?<year>[0-9]{4})(?<month>[0-9]{2})(?<day>[0-9]{2})(_|-)(?<hour>[0-9]{2})?(?<minute>[0-9]{2})?(?<second>[0-9]{2})?")]
    private static partial Regex DateTimeRegex();
    internal static bool TryGetCreateDateFromFileName(string fileName, out DateTime created)
    {
        created = DateTime.MaxValue;
        var reg = DateTimeRegex();
        var match = reg.Match(fileName);

        int year = 0, month = 0, day = 0, hour = 0, minute = 0, second = 0;
        bool dateParsed, timeParsed;
        dateParsed = match.Groups.TryGetValue(nameof(year), out var yearGroup) && int.TryParse(yearGroup.ValueSpan, out year);
        dateParsed = dateParsed && match.Groups.TryGetValue(nameof(month), out var monthGroup) && int.TryParse(monthGroup.ValueSpan, out month);
        dateParsed = dateParsed && match.Groups.TryGetValue(nameof(day), out var dayGroup) && int.TryParse(dayGroup.ValueSpan, out day);

        timeParsed = dateParsed && match.Groups.TryGetValue(nameof(hour), out var hourGroup) && int.TryParse(hourGroup.ValueSpan, out hour);
        timeParsed = timeParsed && match.Groups.TryGetValue(nameof(minute), out var minuteGroup) && int.TryParse(minuteGroup.ValueSpan, out minute);
        timeParsed = timeParsed && match.Groups.TryGetValue(nameof(second), out var secondGroup) && int.TryParse(secondGroup.ValueSpan, out second);

        if (timeParsed || dateParsed)
        {
            var dt = new DateTime(year, month, day, hour, minute, second, DateTimeKind.Local);
            created = dt.ToUniversalTime();
            return true;
        }

        return false;
    }

}