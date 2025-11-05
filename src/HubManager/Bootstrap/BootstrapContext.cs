public sealed class BootstrapContext
{
    public string ApplicationName { get; }
    public string BasePath { get; }
    public string ConfigDir { get; }
    public string LogsPath { get; }
    public string ConfigFile { get; }
    public string EnvironmentName { get; }

    public BootstrapContext(string applicationName)
    {
        ApplicationName = applicationName;
        BasePath = AppDomain.CurrentDomain.BaseDirectory;
        ConfigDir = Path.Combine(BasePath, "config");
        LogsPath = Path.Combine(BasePath, "logs");
        ConfigFile = Path.Combine(ConfigDir, "appsettings.json");
        EnvironmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";
    }
}
public static class BootstrapBanner
{
    public static void Print(BootstrapContext ctx, bool showBasePath = false)
    {
        Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] Starting {ctx.ApplicationName}...");
        if (showBasePath)
            Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] Base path: {Mask(ctx.BasePath)}");
    }

    private static string Mask(string path)
    {
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (!string.IsNullOrEmpty(userProfile))
            path = path.Replace(userProfile, "<USERPROFILE>", StringComparison.OrdinalIgnoreCase);

        var user = Environment.UserName;
        if (!string.IsNullOrWhiteSpace(user))
            path = path.Replace($@"\Users\{user}\", @"\Users\<user>\", StringComparison.OrdinalIgnoreCase);

        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        if (!string.IsNullOrEmpty(baseDir))
            path = path.Replace(baseDir, "<BASE>\\", StringComparison.OrdinalIgnoreCase);

        return path;
    }
}