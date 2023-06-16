using Azure.Identity;
using Azure.Security.KeyVault.Keys.Cryptography;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using BccCode.DocumentationSite.Models;
using IdentityServer4.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel;
using System.IdentityModel.Tokens.Jwt;
using System.IO.Compression;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace BccCode.DocumentationSite.Services
{
    public class GetFiles : IGetFiles
    {
        private readonly IConfiguration config;
        private readonly IMemoryCache cache;

        public GetFiles(IConfiguration config, IMemoryCache cache)
        {
            this.config = config;
            this.cache = cache;
        }

        List<int> artifactid = new List<int>();

        public async Task<string> UploadPagesToStorage(string repo, IFormFile zip, bool isPublic = false, string auth = "github")
        {
            #region Azure vault pem file
            var envVar = new EnviromentVar(config);
            var credential = new DefaultAzureCredential();
            //The name of the Key in the vault
            var vaultName = envVar.GetEnviromentVariable("VaultName");
            var keyName = envVar.GetEnviromentVariable("KeyName");

            //Claims for the JWT where Iss is the github-app "App ID"
            var claims = new[] {
               new Claim(JwtRegisteredClaimNames.Iss,"269348"),
               new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.Now.AddSeconds(-60).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
               new Claim(JwtRegisteredClaimNames.Exp, DateTimeOffset.Now.AddMinutes(10).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            //Creating header and payload
            var header = @"{""alg"":""RS256"",""typ"":""JWT""}";
            var payload = JsonSerializer.Serialize(new JwtPayload(claims));
            var headerAndPayload = $"{Base64UrlEncoder.Encode(header)}.{Base64UrlEncoder.Encode(payload)}";

            //Encryption
            var hasher = SHA256.Create();
            var digest = hasher.ComputeHash(Encoding.ASCII.GetBytes(headerAndPayload));
            var cryptoclient = new CryptographyClient(new Uri($"https://{vaultName}.vault.azure.net/keys/{keyName}"), credential);
            var signature = await cryptoclient.SignAsync(SignatureAlgorithm.RS256, digest);

            //Github-App token
            var token = headerAndPayload + "." + Base64UrlEncoder.Encode(signature.Signature);
            #endregion

            #region Exchanging Tokens
            HttpClient client = new HttpClient();
            var handler = new JwtSecurityTokenHandler();
            //Can get installation id from the installation settings url
            var installationid = envVar.GetEnviromentVariable("InstallationID"); //bcc-code installation id 

            //Setting headers
            client.DefaultRequestHeaders.Add("User-Agent", "PostmanRuntime/7.29.2");
            client.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            //Exchanging token for "installation token"
            var getIntallToken = await client.PostAsync($"https://api.github.com/app/installations/{installationid}/access_tokens", null);
            var installToken = await getIntallToken.Content.ReadAsStringAsync();
            var newToken = installToken.Substring(installToken.IndexOf("token") + 8, (installToken.IndexOf(",") - 11));

            //Setting the new token in header
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", newToken);
            #endregion

            #region Check if repo exsists in bcc-code
            //API call to check if repo exsists
            string RepoExsist;
            if (repo == "community-toolsdiscord")
            {
                var getRepoExsist = await client.GetAsync($"https://api.github.com/repos/bcc-code/community-tools");
                RepoExsist = await getRepoExsist.Content.ReadAsStringAsync();
            }
            else
            {
                var getRepoExsist = await client.GetAsync($"https://api.github.com/repos/bcc-code/{repo}");
                RepoExsist = await getRepoExsist.Content.ReadAsStringAsync();
            }

            //Checks if repo exsists
            if (RepoExsist.Contains("Not Found"))
            {
                return "Repository doesn't exsist!";
            }
            #endregion

            #region Uploading files to container in azure
            else
            {
                #region Uploading zip file content to container in azure
                //Writing zip file to bytes
                byte[] bytes;
                using (var ms = new MemoryStream())
                {
                    await zip.CopyToAsync(ms);
                    bytes = ms.ToArray();
                }
                //Connecting to the Blob storage in azure
                string uri = envVar.GetEnviromentVariable("StorageUrl");

                // replacing '.' with '-' to avoid naming errors in azure
                if (repo.Contains('.'))
                {
                    repo = repo.Replace('.', '-');
                }

                //home page
                if (repo == envVar.GetEnviromentVariable("HomePageContainer"))
                {
                    repo = "home";
                }

                //Discord page
                if (repo == "community-toolsdiscord")
                {
                    repo = "discord";
                }

                Uri container = new Uri(uri + repo.ToLower());
                BlobContainerClient blobcontainer = new BlobContainerClient(container, credential);

                //Creates the container if it doesnt allready exsists in the storage account
                try
                {
                    await blobcontainer.CreateIfNotExistsAsync();
                }
                catch (Exception e)
                {
                    return e.Message;
                }

                //Uploading content to azure storage
                using (ZipArchive archive = new ZipArchive(new MemoryStream(bytes)))
                {
                    //Checks if Zip is not empty
                    if (archive.Entries.Count > 0)
                    {
                        foreach (ZipArchiveEntry entry in archive.Entries)
                        {
                            if (!entry.FullName.IsNullOrEmpty())
                            {
                                BlobClient blobclient = blobcontainer.GetBlobClient(entry.FullName);
                                #region Set file contentType and uploads file to storage
                                try
                                {
                                    var extention = entry.FullName.Substring(entry.FullName.LastIndexOf('.'));
                                    var blobSetting = new BlobHttpHeaders();
                                    switch (extention)
                                    {
                                        case ".md":
                                            blobSetting.ContentType = "text/markdown";
                                            break;
                                        case ".ico":
                                            blobSetting.ContentType = "image/vnd.microsoft.icon";
                                            break;
                                        case ".html":
                                            blobSetting.ContentType = "text/html";
                                            break;
                                        case ".png":
                                            blobSetting.ContentType = "image/png";
                                            break;
                                        case ".svg":
                                            blobSetting.ContentType = "image/svg+xml";
                                            break;
                                        case ".js":
                                            blobSetting.ContentType = "text/javascript";
                                            break;
                                        case ".css":
                                            blobSetting.ContentType = "text/css";
                                            break;
                                        case ".webmanifest":
                                            blobSetting.ContentType = "application/manifest+json";
                                            break;
                                        case ".xml":
                                            blobSetting.ContentType = "application/xml";
                                            break;
                                        default:
                                            blobSetting.ContentType = "application/octet-stream";
                                            break;
                                    }
                                    await blobclient.UploadAsync(entry.Open(), new BlobUploadOptions { HttpHeaders = blobSetting });
                                }
                                catch
                                {

                                }
                                #endregion
                            }
                        }
                    }
                    else
                    {
                        return "Zip file is Empty";
                    }

                }
                #endregion

                #region documentation settings

                #region publicazing documents

                try
                {
                    //Setting "Public" file in the container to indicate that the container have public access
                    if (isPublic)
                    {
                        BlobClient blobclient = blobcontainer.GetBlobClient("public");
                        var tmpFile = Path.GetTempFileName();
                        await blobclient.UploadAsync(File.Create(tmpFile), true);
                        File.Delete(tmpFile);

                    }
                    else
                    {
                        await blobcontainer.GetBlobClient("public").DeleteIfExistsAsync();
                    }
                }
                catch
                {
                    return "Failed to set public status";
                }

                #endregion

                #region Setting authentication method

                try
                {
                    //Setting "azuread" file in the container to indicate that the ducementation is accessed via azureAD
                    if (auth.ToLower() == "azuread")
                    {
                        BlobClient blobclient = blobcontainer.GetBlobClient("azuread");
                        var tmpFile = Path.GetTempFileName();
                        await blobclient.UploadAsync(File.Create(tmpFile), true);
                        File.Delete(tmpFile);

                    }
                    else
                    {
                        await blobcontainer.GetBlobClient("azuread").DeleteIfExistsAsync();
                    }
                }
                catch
                {
                    return "Failed to set azure authentication method";
                }

                try
                {
                    //Setting "portal" file in the container to indicate that the ducementation is accessed via bcc portal
                    if (auth.ToLower() == "portal")
                    {
                        BlobClient blobclient = blobcontainer.GetBlobClient("portal");
                        var tmpFile = Path.GetTempFileName();
                        await blobclient.UploadAsync(File.Create(tmpFile), true);
                        File.Delete(tmpFile);

                    }
                    else
                    {
                        await blobcontainer.GetBlobClient("portal").DeleteIfExistsAsync();
                    }
                }
                catch
                {
                    return "Failed to set bcc portal authentication method";
                }

                #endregion


                #endregion
            }
            #endregion

            #region update cache
            ContainerService blobs = new ContainerService(credential, new EnviromentVar(config).GetEnviromentVariable("StorageUrl"), cache);
            await blobs.UpdateBlobsList(repo);
            #endregion

            return "Done";
        }
    }
}
