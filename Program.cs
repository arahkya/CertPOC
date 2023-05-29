using System.Net;
using System.Security.Cryptography.X509Certificates;
using Azure.Identity;
using Azure.Security.KeyVault.Certificates;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Server.Kestrel.Https;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(kestrelCfg =>
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
            var localCertPath = builder.Configuration["ASPNETCORE_SSL_LOCAL_FILE"]!;
            var localCertPassword = builder.Configuration["ASPNETCORE_SSL_LOCAL_PASSWORD"];

            httpDefaultCfg.ServerCertificate = new X509Certificate2(localCertPath, localCertPassword);

            return;
        }
        else
        {

#if DEBUG
            var kvUri = builder.Configuration["ASPNETCORE_AZ_KV_URI"];
            var tenentId = builder.Configuration["ASPNETCORE_AZ_TENENT"];
            var clientId = builder.Configuration["ASPNETCORE_AZ_CLIENT_ID"];
            var clientSecret = builder.Configuration["ASPNETCORE_AZ_CLIENT_SECRET"];
            var secretName = builder.Configuration["ASPNETCORE_AZ_KV_SECRET_NAME"];
            var certName = builder.Configuration["ASPNETCORE_AZ_KV_CERT_NAME"];
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
});

// Add services to the container.
builder.Services.AddAuthentication(CertificateAuthenticationDefaults.AuthenticationScheme)
     .AddCertificate(CertificateAuthenticationDefaults.AuthenticationScheme, authCertCfg =>
    {
        authCertCfg.AllowedCertificateTypes = CertificateTypes.All;
        authCertCfg.Events = new CertificateAuthenticationEvents
        {
            OnAuthenticationFailed = context =>
            {
                return Task.CompletedTask;
            },
            OnCertificateValidated = context =>
            {
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
