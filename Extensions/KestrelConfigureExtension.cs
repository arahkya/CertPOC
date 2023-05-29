using System.Net;
using System.Security.Cryptography.X509Certificates;
using Azure.Identity;
using CertPOC.Services;
using Microsoft.AspNetCore.Server.Kestrel.Https;

namespace CertPOC.Extensions;

public static class SetupKestrel
{
    public static IWebHostBuilder ConfigureAzKestrel(this IWebHostBuilder webhostBuilder, IServiceCollection services, int port)
    {
        return webhostBuilder.ConfigureKestrel((kestrelCfg =>
        {
            kestrelCfg.ConfigureHttpsDefaults(httpDefaultCfg =>
            {
                httpDefaultCfg.ClientCertificateMode = ClientCertificateMode.RequireCertificate;
                httpDefaultCfg.CheckCertificateRevocation = false;

                bool useLocalSSLFile = bool.TrueString.ToLower() == Environment.GetEnvironmentVariable("ASPNETCORE_SSL_LOCAL")?.ToLower();

                if (useLocalSSLFile)
                {
                    string localCertPath = webhostBuilder.GetSetting("ASPNETCORE_SSL_LOCAL_FILE")!;
                    string localCertPassword = webhostBuilder.GetSetting("ASPNETCORE_SSL_LOCAL_PASSWORD")!;

                    httpDefaultCfg.ServerCertificate = new X509Certificate2(localCertPath, localCertPassword);

                    return;
                }

#if DEBUG
                string? kvUri = webhostBuilder.GetSetting("ASPNETCORE_AZ_KV_URI");
                string? certName = webhostBuilder.GetSetting("ASPNETCORE_AZ_KV_CERT_NAME");
#else
                string? kvUri = Environment.GetEnvironmentVariable("ASPNETCORE_AZ_KV_URI");
                string? certName = Environment.GetEnvironmentVariable("ASPNETCORE_AZ_KV_CERT_NAME");
#endif
                Uri azUri = new(kvUri!);
                ClientSecretCredential azClientCred = CreateClientSecretCredentail(webhostBuilder);

                IServiceProvider serviceProvider = services.BuildServiceProvider();
                httpDefaultCfg.ServerCertificate = serviceProvider.GetRequiredService<IAzureCertificateService>().GetCertificateFromAzureKeyVault(azUri, azClientCred, certName!);
            });

            kestrelCfg.Listen(IPAddress.Loopback, port, listenOpt =>
            {
                listenOpt.UseHttps();
            });
        }));
    }

    private static ClientSecretCredential CreateClientSecretCredentail(IWebHostBuilder webhostBuilder)
    {
#if DEBUG       
        string? tenentId = webhostBuilder.GetSetting("ASPNETCORE_AZ_TENENT");
        string? clientId = webhostBuilder.GetSetting("ASPNETCORE_AZ_CLIENT_ID");
        string? clientSecret = webhostBuilder.GetSetting("ASPNETCORE_AZ_CLIENT_SECRET");
#else
        string? tenentId = Environment.GetEnvironmentVariable("ASPNETCORE_AZ_TENENT");
        string? clientId = Environment.GetEnvironmentVariable("ASPNETCORE_AZ_CLIENT_ID");
        string? clientSecret = Environment.GetEnvironmentVariable("ASPNETCORE_AZ_CLIENT_SECRET");
#endif

        ClientSecretCredential azClientCred = new(tenentId, clientId, clientSecret);

        return azClientCred;
    }
}