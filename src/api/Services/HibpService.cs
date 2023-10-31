using System.Security.Cryptography;
using System.Text;

namespace albumica;

public class HibpService
{
    readonly ILogger<HibpService> _logger;
    public HttpClient Client { get; }
    public HibpService(ILogger<HibpService> logger, HttpClient client)
    {
        _logger = logger;

        client.BaseAddress = new Uri("https://api.pwnedpasswords.com/");
        client.DefaultRequestHeaders.Add("User-Agent", nameof(albumica));
        client.Timeout = TimeSpan.FromSeconds(5);
        Client = client;
    }
    public async Task<string> CheckAsync(string password, CancellationToken cancellationToken = default)
    {
        try
        {
            var passwordBytes = Encoding.UTF8.GetBytes(password);
            var hashBytes = SHA1.HashData(passwordBytes);
            var hash = Convert.ToHexString(hashBytes);
            var sha1HashFirstFive = hash.Substring(0, 5);
            var sha1HastReminder = hash.Substring(5);

            var hashes = await Client.GetStringAsync($"/range/{sha1HashFirstFive}", cancellationToken);

            var reader = new StringReader(hashes);
            while (true)
            {
                var line = reader.ReadLine();
                if (string.IsNullOrWhiteSpace(line))
                    break;

                if (line.StartsWith(sha1HastReminder))
                {
                    var parts = line.Split(':');
                    return int.TryParse(parts.LastOrDefault(), out var count) ?
                        $"Password is well known and has appeared in {count:#,##0} breaches" :
                        "Password is well known";
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not check HIBP service");
        }

        return string.Empty;
    }
}