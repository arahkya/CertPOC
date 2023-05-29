using Microsoft.AspNetCore.Authentication.Certificate;
using CertPOC.Extenions;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureAzKestrel();

// Add services to the container.
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
