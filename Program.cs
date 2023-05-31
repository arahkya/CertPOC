using Microsoft.AspNetCore.Authentication.Certificate;
using CertPOC.Extensions;
using CertPOC.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<IAzureCertificateService, AzureCertificateService>();
builder.Services.AddScoped<IAzureSecretService, AzureSecretService>();

builder.Services.AddAuthentication(CertificateAuthenticationDefaults.AuthenticationScheme)
     .AddCertificate(CertificateAuthenticationDefaults.AuthenticationScheme, authCertCfg =>
    {        
        authCertCfg.AllowedCertificateTypes = CertificateTypes.All;
        authCertCfg.Events = new CertificateAuthenticationEvents
        {
            OnCertificateValidated = context =>
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

builder.WebHost.ConfigureAzKestrel(builder.Services, 7212);

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
