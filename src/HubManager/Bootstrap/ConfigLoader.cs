using Microsoft.Extensions.Configuration;

namespace HubManager.Bootstrap;

public static class ConfigLoader
{
    public static IConfiguration BuildConfiguration(BootstrapContext ctx, string[] args)
    {
        try
        {
            var cb = new ConfigurationBuilder()
                .SetBasePath(ctx.BasePath)
                .AddJsonFile(ctx.ConfigFile, optional: false, reloadOnChange: true)
                .AddJsonFile(Path.Combine(ctx.ConfigDir, $"appsettings.{ctx.EnvironmentName}.json"), optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .AddCommandLine(args);

            var configuration = cb.Build();
            Validate(configuration);
            Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] Configuration loaded from: {ctx.ConfigFile}");
            return configuration;
        }
        catch (FileNotFoundException)
        {
            throw new FileNotFoundException($"Configuration file not found: {ctx.ConfigFile}. Please ensure appsettings.json exists in the config directory.");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load configuration from {ctx.ConfigFile}", ex);
        }
    }

    private static void Validate(IConfiguration configuration)
    {
        var required = new[] { "Serilog", "KafkaConfiguration", "ConnectionStringsDb" };
        var missing = required.Where(s => !configuration.GetSection(s).Exists()).ToArray();
        if (missing.Length > 0)
        {
            Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] WARNING: Missing configuration sections: {string.Join(", ", missing)}");
        }
    }
}
