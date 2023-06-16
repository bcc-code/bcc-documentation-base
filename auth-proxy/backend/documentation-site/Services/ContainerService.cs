using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using IdentityServer4.Services;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace BccCode.DocumentationSite.Services
{
    public class ContainerService : IContainerService
    {
        public ContainerService()
        {
        }

        public ContainerService(DefaultAzureCredential credential, string blobEndpoint, IMemoryCache cache)
        {
            //Get authenticatated token credential
            _blobEndpoint = new Uri(blobEndpoint);
            _blobClient = new BlobServiceClient(_blobEndpoint, credential);
            
            //Cache
            _cache = cache;
        }

        private readonly IMemoryCache? _cache;
        private BlobServiceClient? _blobClient;
        private Uri? _blobEndpoint;

        
        //Retrives The SASToken for the spesified container
        public Task<string> GetUserDelegationSasContainer(string containerName)
        {

            return _cache.GetOrCreateAsync(containerName + "SASToken", async c =>
            {
                //User delegation key
                UserDelegationKey key = _blobClient!.GetUserDelegationKey(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddMinutes(15));

                Uri containerUri = new Uri(_blobEndpoint!.ToString() + containerName);

                BlobContainerClient containerClient = new BlobContainerClient(blobContainerUri: containerUri);

                //Creates a SAS token
                BlobSasBuilder SASBuilder = new BlobSasBuilder()
                {
                    BlobContainerName = containerClient.Name,
                    Resource = "c",
                    StartsOn = DateTimeOffset.UtcNow,
                    ExpiresOn = DateTimeOffset.UtcNow.AddHours(1)
                };

                //Specify permissions for the SAS token
                SASBuilder.SetPermissions(BlobAccountSasPermissions.Read | BlobAccountSasPermissions.List);

                //Add SAS token to container URI
                BlobUriBuilder blobUri = new BlobUriBuilder(containerClient.Uri)
                {
                    //Specify User delegation key.
                    Sas = SASBuilder.ToSasQueryParameters(key, _blobClient.AccountName)
                };

                var SAStoken = blobUri.ToString().Substring(blobUri.ToString().IndexOf('?'));

                c.SetAbsoluteExpiration(TimeSpan.FromMinutes(15));
                //Returns user delegation SAS uri for the container
                return SAStoken;
            });

        }

        //Returns a list of all the containers that exsist in the storage account on azure
        public Task<List<string>> GetContainersList()
        {

            return _cache.GetOrCreateAsync("Containers", async c =>
            {

                List<string> exsistingContainers = new List<string>();
                var containers = _blobClient!.GetBlobContainersAsync(BlobContainerTraits.Metadata).AsPages(default);

                await foreach(Azure.Page<BlobContainerItem> containerPage in containers)
                {
                    foreach (BlobContainerItem containerItem in containerPage.Values)
                    {
                        exsistingContainers.Add(containerItem.Name);
                    }    
                }
                c.SetAbsoluteExpiration(TimeSpan.FromMinutes(15));
                return exsistingContainers;
            });
        }

        //Returns a list of all the blobs in the spesified container
        public Task<List<string>> GetBlobsList(string container)
        {

            return _cache.GetOrCreateAsync(container + "Files", async c =>
            {

                List<string> exsistingBlobs = new List<string>();
                var containerClient = _blobClient!.GetBlobContainerClient(container);
                var blobs = containerClient.GetBlobsAsync().AsPages(default);

                await foreach (Azure.Page<BlobItem> blobPage in blobs)
                {
                    foreach (BlobItem blobItem in blobPage.Values)
                    {
                        exsistingBlobs.Add("/" + blobItem.Name);
                    }
                }
                c.SetAbsoluteExpiration(TimeSpan.FromMinutes(15));
                return exsistingBlobs;
            });
        }

        //Updates the cache entry for the container
        public async Task UpdateBlobsList(string container)
        {
            if (_cache!.TryGetValue(container + "Files", out object value))
            {
                _cache.Remove(container + "Files");
            }
            await GetBlobsList(container);
        }

        //Check for "public" file in the container
        public Task<bool> IsPublic(string container)
        {
            return _cache.GetOrCreateAsync(container + "isPublic", async c =>
            {
                try
                {
                    BlobContainerClient containerClient = _blobClient!.GetBlobContainerClient(container);

                    var answer = (await containerClient.GetBlobClient("public").ExistsAsync()).Value;

                    c.SetAbsoluteExpiration(TimeSpan.FromMinutes(15));
                    return answer;

                }
                catch (Exception e)
                {
                    return false;
                }

            });
        }

        public Task<string> AuthProvider(string container)
        {
            return _cache.GetOrCreateAsync(container + "AuthProvider", async c =>
            {
                c.SetAbsoluteExpiration(TimeSpan.FromMinutes(15));
                try
                {
                    var authMethod = new List<string>() { "azuread", "portal" };
                  
                    BlobContainerClient containerClient = _blobClient!.GetBlobContainerClient(container);

                    foreach (var method in authMethod)
                    { 
                        var answer = (await containerClient.GetBlobClient(method).ExistsAsync()).Value;

                        if (answer)
                        {
                            return method;
                        }
                    }

                    return "github";

                }
                catch (Exception e)
                {
                    return "github";
                }

            });
        }
    }
}
