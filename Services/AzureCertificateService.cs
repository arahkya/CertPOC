using System.Security.Cryptography.X509Certificates;
using Azure.Identity;
using Azure.Security.KeyVault.Certificates;

namespace CertPOC.Services;

public class AzureCertificateService : IAzureCertificateService
{
    public X509Certificate2 GetCertificateFromAzureKeyVault(Uri azUri, ClientSecretCredential azClientCred, string certName)
    {
        CertificateClient azCertCred = new(azUri, azClientCred);
        Azure.Response<X509Certificate2> azResponse = azCertCred.DownloadCertificate(certName);
        X509Certificate2 azCertificate = azResponse.Value;

        return azCertificate;
    } 
}