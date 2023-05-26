using System.Net;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Server.Kestrel.Https;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(kestrelCfg =>
{
    kestrelCfg.ConfigureHttpsDefaults(httpDefaultCfg =>
    {        
        httpDefaultCfg.ServerCertificate = new X509Certificate2(@"C:\Users\arahk\source\repos\arahkya\CertPOC\.certs\CertPOC.pfx", "Password123");
        httpDefaultCfg.ClientCertificateMode = ClientCertificateMode.RequireCertificate;
        httpDefaultCfg.CheckCertificateRevocation = false;
        httpDefaultCfg.ClientCertificateValidation = (cert, chain, policyErrors) =>
        {
            return true;
        };
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
