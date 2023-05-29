using System.Net;
using System.Security.Cryptography.X509Certificates;
using Azure.Identity;
using Azure.Security.KeyVault.Certificates;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Server.Kestrel.Https;

namespace CertPOC.Extenions;

public static class SetupKestrel
{
    public static IWebHostBuilder ConfigureAzKestrel(this IWebHostBuilder webhostBuilder)
    {
        return webhostBuilder.ConfigureKestrel((kestrelCfg =>
        {
            kestrelCfg.ConfigureHttpsDefaults(httpDefaultCfg =>
            {
                httpDefaultCfg.ClientCertificateMode = ClientCertificateMode.RequireCertificate;
                httpDefaultCfg.CheckCertificateRevocation = false;
                httpDefaultCfg.ClientCertificateValidation = (cert, chain, policyErrors) =>
                {
                    return true;
                };

                bool useLocalSSLFile = bool.TrueString.ToLower() == Environment.GetEnvironmentVariable("ASPNETCORE_SSL_LOCAL")?.ToLower();

                if (useLocalSSLFile)
                {
                    var localCertPath = webhostBuilder.GetSetting("ASPNETCORE_SSL_LOCAL_FILE")!;
                    var localCertPassword =  webhostBuilder.GetSetting("ASPNETCORE_SSL_LOCAL_PASSWORD");

                    httpDefaultCfg.ServerCertificate = new X509Certificate2(localCertPath, localCertPassword);

                    return;
                }
                else
                {

        #if DEBUG
                    var kvUri =  webhostBuilder.GetSetting("ASPNETCORE_AZ_KV_URI");
                    var tenentId =  webhostBuilder.GetSetting("ASPNETCORE_AZ_TENENT");
                    var clientId =  webhostBuilder.GetSetting("ASPNETCORE_AZ_CLIENT_ID");
                    var clientSecret =  webhostBuilder.GetSetting("ASPNETCORE_AZ_CLIENT_SECRET");
                    var secretName =  webhostBuilder.GetSetting("ASPNETCORE_AZ_KV_SECRET_NAME");
                    var certName =  webhostBuilder.GetSetting("ASPNETCORE_AZ_KV_CERT_NAME");
        #else
                    var kvUri = Environment.GetEnvironmentVariable("ASPNETCORE_AZ_KV_URI")!;
                    var tenentId = Environment.GetEnvironmentVariable("ASPNETCORE_AZ_TENENT")!;
                    var clientId = Environment.GetEnvironmentVariable("ASPNETCORE_AZ_CLIENT_ID")!;
                    var clientSecret = Environment.GetEnvironmentVariable("ASPNETCORE_AZ_CLIENT_SECRET")!;
                    var secretName = Environment.GetEnvironmentVariable("ASPNETCORE_AZ_KV_SECRET_NAME")!;
                    var certName = Environment.GetEnvironmentVariable("ASPNETCORE_AZ_KV_CERT_NAME")!;
        #endif

                    var azClientCred = new ClientSecretCredential(tenentId, clientId, clientSecret);
                    var client = new SecretClient(new Uri(kvUri!), azClientCred);
                    var azSecret = client.GetSecret(secretName);
                    var certPassword = azSecret.Value.Value;

                    var azCertCred = new CertificateClient(new Uri(kvUri!), azClientCred);
                    var azCertificate = azCertCred.DownloadCertificate(certName);
                    var azCert = azCertificate.Value;

                    httpDefaultCfg.ServerCertificate = azCert;
                }
            });

            kestrelCfg.Listen(IPAddress.Loopback, 7212, listenOpt =>
            {
                listenOpt.UseHttps();
            });
        }));
    }
}