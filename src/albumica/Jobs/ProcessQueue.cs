using System.ComponentModel;
using System.Security.Cryptography;
using albumica.Entities;
using FFMpegCore;
using FFMpegCore.Enums;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;

namespace albumica.Jobs;

public class ProcessQueue
{
    const string VID_CREATED_TAG = "creation_time";
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
            var ext = Path.GetExtension(file);
            if (s_imgFormats.Contains(ext) || s_vidFormats.Contains(ext))
                await Process(file, token);
            else
                _logger.LogError("Unsupported file extension {FilePath}", file);
        }
    }
    async Task Process(string filePath, CancellationToken token)
    {
        using var fs = File.Open(filePath, FileMode.Open);
        var sha256Bytes = await SHA256.HashDataAsync(fs, token);
        var sha256 = Convert.ToHexString(sha256Bytes);
        await fs.DisposeAsync();

        var isDuplicate = await _db.Media.AnyAsync(m => m.SHA256 == sha256, token);
        if (isDuplicate)
        {
            _logger.LogWarning("Discarding duplicate from queue {FilePath}", filePath);
            File.Delete(filePath);
            return;
        }

        var origFileName = Path.GetFileName(filePath);
        var prevFileName = $"{Path.GetFileNameWithoutExtension(filePath)}_preview.webp";
        var origFilePath = C.Paths.MediaDataFor(origFileName);
        var prevFilePath = C.Paths.MediaDataFor(prevFileName);
        var ext = Path.GetExtension(origFilePath);
        File.Move(filePath, origFilePath);

        var media = new Media
        {
            SHA256 = sha256,
            Original = origFileName,
            IsVideo = s_vidFormats.Contains(ext),
        };
        _db.Media.Add(media);
        await _db.SaveChangesAsync(CancellationToken.None);

        if (s_imgFormats.Contains(ext))
        {
            var img = await Image.LoadAsync(origFilePath, token);
            if (TryGetCreateDateFromExif(img.Metadata.ExifProfile, out var exifCreated))
                media.Created = exifCreated;

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

        if (profile.TryGetValue(ExifTag.DateTimeOriginal, out var edtOriginal) && edtOriginal.GetValue() is DateTime dtOriginal)
            created = dtOriginal;
        else if (profile.TryGetValue(ExifTag.DateTimeDigitized, out var edtDig) && edtDig.GetValue() is DateTime dtDig)
            created = dtDig;
        else if (profile.TryGetValue(ExifTag.DateTime, out var edt) && edt.GetValue() is DateTime dt)
            created = dt;

        return created == DateTime.MaxValue;
    }

    static bool TryGetCreateDateFromVideo(IMediaAnalysis analysis, out DateTime created)
    {
        created = DateTime.MaxValue;
        if (analysis.Format.Tags != null &&
            analysis.Format.Tags.TryGetValue(VID_CREATED_TAG, out var tdtFormat) &&
            DateTime.TryParse(tdtFormat, out var dtFormat))
            created = dtFormat;
        else if (analysis.PrimaryVideoStream?.Tags != null &&
            analysis.PrimaryVideoStream.Tags.TryGetValue(VID_CREATED_TAG, out var tdtVideo) &&
            DateTime.TryParse(tdtVideo, out var dtVideo))
            created = dtVideo;
        else if (analysis.PrimaryAudioStream?.Tags != null &&
            analysis.PrimaryAudioStream.Tags.TryGetValue(VID_CREATED_TAG, out var tdtAudio) &&
            DateTime.TryParse(tdtAudio, out var dtAudio))
            created = dtAudio;

        return created == DateTime.MaxValue;
    }
}