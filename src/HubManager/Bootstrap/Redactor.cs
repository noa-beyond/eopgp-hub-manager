using Microsoft.Extensions.Configuration;
using System.Runtime.InteropServices;

namespace HubManager.Bootstrap;

public sealed class Redactor
{
    private readonly bool _allowPii;
    private readonly bool _maskUserAndMachine;

    private readonly string? _userProfile;
    private readonly string? _home;

    public Redactor(IConfiguration config)
    {
        _allowPii = config.GetValue("Diagnostics:AllowPII", false);
        _maskUserAndMachine = config.GetValue("Diagnostics:MaskUserAndMachine", true);

        try
        {
            _userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        }
        catch { /* ignore */ }

        _home = Environment.GetEnvironmentVariable(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "USERPROFILE" : "HOME");
    }

    public string Maybe(string value, string label = "pii")
    {
        if (_allowPii) return value;
        return $"<{label}-hidden>";
    }

    public string MaskPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return path;

        string masked = path;
        if (!string.IsNullOrEmpty(_userProfile) && masked.StartsWith(_userProfile, StringComparison.OrdinalIgnoreCase))
            masked = masked.Replace(_userProfile, "<USERPROFILE>", StringComparison.OrdinalIgnoreCase);

        if (!string.IsNullOrEmpty(_home) && masked.StartsWith(_home, StringComparison.OrdinalIgnoreCase))
            masked = masked.Replace(_home, "<HOME>", StringComparison.OrdinalIgnoreCase);

        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        if (!string.IsNullOrEmpty(baseDir) && masked.StartsWith(baseDir, StringComparison.OrdinalIgnoreCase))
            masked = masked.Replace(baseDir, "<BASE>\\", StringComparison.OrdinalIgnoreCase);

        return masked;
    }

    public string MaskUser(string user)
        => _allowPii || !_maskUserAndMachine ? user : "<user>";

    public string MaskMachine(string machine)
        => _allowPii || !_maskUserAndMachine ? machine : "<machine>";
}
