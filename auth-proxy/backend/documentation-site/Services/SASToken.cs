﻿using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using IdentityServer4.Services;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Caching.Memory;
using System.Runtime.CompilerServices;

namespace BccCode.DocumentationSite.Services
{
    public class SASToken : ISASToken
    {
        public SASToken()
        {
        }

        public SASToken(DefaultAzureCredential credential, string blobEndpoint, IMemoryCache cache)
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
    }
}
