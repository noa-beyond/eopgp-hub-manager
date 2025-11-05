using Serilog;
using System.Reflection;

namespace HubManager.Bootstrap;

public static class StartupInfo
{
    public static void LogStartupInformation(BootstrapContext ctx, Redactor redactor)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version?.ToString() ?? "Unknown";
        var buildDate = GetBuildDate(assembly);
        var allowPii = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") == "Development"
               ? true
               : false;


        Log.Information("=== {ApplicationName} Starting ===", ctx.ApplicationName);
        Log.Information("Version: {Version}", version);
        Log.Information("Build Date: {BuildDate}", buildDate);
        Log.Information("Runtime: {Runtime}", Environment.Version);
        Log.Information("OS: {OS}", Environment.OSVersion);

        // Masked fields:
        Log.Information("Machine: {Machine}", redactor.MaskMachine(Environment.MachineName));
        Log.Information("User: {User}", redactor.MaskUser(Environment.UserName));
        Log.Information("Process ID: {ProcessId}", allowPii ? Environment.ProcessId : "<pid-hidden>");

        // Paths (masked):
        Log.Information("Working Directory: {WorkingDirectory}", redactor.MaskPath(Environment.CurrentDirectory));
        Log.Information("Command Line: {CommandLine}", redactor.Maybe(Environment.CommandLine, "cmdline"));

        // Environment name is not PII; keep it:
        Log.Information("Environment: {Environment}", ctx.EnvironmentName);
    }

    private static DateTime GetBuildDate(Assembly assembly)
    {
        try { return File.GetLastWriteTime(assembly.Location); }
        catch { return DateTime.MinValue; }
    }
}
