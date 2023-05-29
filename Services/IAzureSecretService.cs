using System.Security.Cryptography.X509Certificates;
using Azure.Identity;
using Azure.Security.KeyVault.Certificates;

namespace CertPOC.Services;

public interface IAzureSecretService
{
    string GetSecretFromAzureKeyVault(IWebHostBuilder webhostBuilder, Uri azUri, ClientSecretCredential azClientCred, string secretName);
}