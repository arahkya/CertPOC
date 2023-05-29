using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace CertPOC.Services;

public class AzureSecretService : IAzureSecretService
{
    public string GetSecretFromAzureKeyVault(IWebHostBuilder webhostBuilder, Uri azUri, ClientSecretCredential azClientCred, string secretName)
    {
        SecretClient client = new(azUri, azClientCred);
        Azure.Response<KeyVaultSecret> azSecret = client.GetSecret(secretName);
        string secret = azSecret.Value.Value;

        return secret;
    } 
}