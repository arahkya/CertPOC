using System.Security.Cryptography.X509Certificates;
using Azure.Identity;
using Azure.Security.KeyVault.Certificates;

namespace CertPOC.Services;

public interface IAzureCertificateService
{
    X509Certificate2 GetCertificateFromAzureKeyVault(Uri azUri, ClientSecretCredential azClientCred, string certName);
}