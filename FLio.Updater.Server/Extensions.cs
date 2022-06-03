using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace FLio.Updater.Server;

public static class Extensions
{
    public static string ToZipFileName(this Version version)
    {
        return version.ToString(3) + ".zip";
    }

    public static IConfigurationBuilder AddJsonEmbedded(this IConfigurationBuilder builder, string path)
    {
        var assembly = typeof(Extensions).Assembly;
        var resourceNames = assembly.GetManifestResourceNames();
        var resourceName = resourceNames.FirstOrDefault(n => n.Contains(path));
        if (resourceName == null)
            throw new FileNotFoundException();

        var assemblyStream = assembly.GetManifestResourceStream(resourceName)!;
        builder.AddJsonStream(assemblyStream);
        return builder;
    }

    public static void AddOptionsWithValidation<T>(
        this IServiceCollection services,
        HostBuilderContext builder
    ) where T : class
    {
        services
            .AddOptions<T>()
            .Bind(builder.Configuration.GetSection(typeof(T).Name[..^7]))
            .ValidateDataAnnotations();

        // Explicitly register the settings object by delegating to the IOptions object and performs initial validation
        services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<T>>().Value);
    }
}