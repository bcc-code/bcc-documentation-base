using Azure.Identity;
using Azure.Security.KeyVault.Keys.Cryptography;
using BccCode.DocumentationSite.Models;
using IdentityServer4.Endpoints.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using System.Collections;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;
using System.Security;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Text.Json;

namespace BccCode.DocumentationSite.Services
{
    public class GetMembers : IGetMembersInterface
    {

        public GetMembers(IMemoryCache cache, IConfiguration config)
        {
            _cache = cache;
            _config = config;
        }

        private readonly IMemoryCache _cache;
        private readonly IConfiguration _config;

        private List<int> users;

        public Task<string> GetTokenFromAzurePem()
        {
            return _cache.GetOrCreateAsync("token", async c =>
                {
                    #region using azure vault pem file to get a github token
                    var credential = new DefaultAzureCredential();
                    var envVar = new EnviromentVar(_config);
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

                    c.SetAbsoluteExpiration(TimeSpan.FromMinutes(15));
                    return token;
                });
        }

        public Task<List<int>> GetUsersInRepo(string token = "", string repo = "")
        {

            if (token == "")
            {
                //Checks if a cache for that repo exsist/expired else returns cached result
                _cache.TryGetValue(repo + "getMembers", out users);
                return Task.FromResult(users);
            }


            return _cache.GetOrCreateAsync(repo + "getMembers", async c =>
            {

                #region Exchanging Tokens
                var envVar = new EnviromentVar(_config);
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

                #region Checks for valid repo

                //API call to check repo properties
                var getRepoState = await client.GetAsync($"https://api.github.com/repos/bcc-code/{repo}");
                var repoState = await getRepoState.Content.ReadAsStringAsync();

                //Gets visibility of repo
                MemberUrl? visibility = (MemberUrl?)JsonSerializer.Deserialize(repoState, typeof(MemberUrl));

                if (visibility!.visibility!.Contains("public"))
                {
                    //If repo is public returns an array of size 1 with a 404 element
                    return new List<int>(){ 404 };
                }
                    #endregion 

                #region installation api calls
                else
                {
                    //Getting contributors of the repo
                    var getInstallCont = await client.GetAsync($"https://api.github.com/repos/bcc-code/{repo}/contributors");
                    var installCont = await getInstallCont.Content.ReadAsStringAsync();

                    Members[]? parsedJson = (Members[]?)JsonSerializer.Deserialize(installCont, typeof(Members[]));

                    //Initialization of the users list
                    users = new List<int>();

                    //Adding contributors to the users list
                    foreach (var login in parsedJson!)
                    {
                        users.Add(login.id);
                    }

                    //Getting teams members api
                    var getInstallTeam = await client.GetAsync($"https://api.github.com/repos/bcc-code/{repo}/teams");
                    var installTeam = await getInstallTeam.Content.ReadAsStringAsync();
                    MemberUrl[]? parsedMemberurl = (MemberUrl[]?)JsonSerializer.Deserialize(installTeam, typeof(MemberUrl[]));

                    //Extracting the team members api url
                    foreach (var url in parsedMemberurl!)
                    {
                        url.members_url = url?.members_url?.Replace("{/member}", "");
                    }
                 #endregion

                #region members api
                    //Calling all the teams api to retrive all thier members
                    foreach (var url in parsedMemberurl)
                    {
                        var getInstallMembers = await client.GetAsync(url.members_url);
                        var installMembers = await getInstallMembers.Content.ReadAsStringAsync();
                        parsedJson = (Members[]?)JsonSerializer.Deserialize(installMembers, typeof(Members[]));

                        //Adding team members to the users list
                        foreach (var login in parsedJson!)
                        {
                            //Prevent adding users who allready exsist
                            if (!users.Contains(login.id))
                            {
                                users.Add(login.id);
                            }
                        }
                    }
                #endregion

                #region bcc-code members
                    //Calling the api to retrive all bcc-code members
                    var getBccCodeMembers = await client.GetAsync("https://api.github.com/orgs/bcc-code/members?per_page=100");
                    var bccCodeMembers = await getBccCodeMembers.Content.ReadAsStringAsync();

                    //Parsing result
                    Members[]? parsedMembersJson = (Members[]?)JsonSerializer.Deserialize(bccCodeMembers, typeof(Members[]));

                    //Adding all the members to the list
                    foreach (var member in parsedMembersJson!)
                    {
                        //Prevent adding users who allready exsist
                        if (!users.Contains(member.id))
                        {
                            users.Add(member.id);
                        }
                    }
                #endregion

                    c.SetAbsoluteExpiration(TimeSpan.FromMinutes(30));

                    //Return users list
                    return users;
                }
            });
        }

    }
}