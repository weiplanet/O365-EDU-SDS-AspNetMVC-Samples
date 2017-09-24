using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Text;
using System.IO;
using System;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Blob;

namespace OneRosterProviderDemo.Controllers
{
    [Authorize]
    [Route("sds")]
    public class SdsController : Controller
    {
        private static readonly HttpClient Client = new HttpClient();
        private readonly IConfiguration _config;

        public SdsController(IConfiguration config)
        {
            _config = config;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            ViewBag.url = $"{(Request.IsHttps ? "https" : "http")}://{Request.Host}/ims/oneroster/v1p1";
            return View();
        }

        [Route("csv")]
        public async Task<IActionResult> CsvAsync(List<IFormFile> files)
        {
            if (files.Count < 6)
            {
                return BadRequest();
            }

            var profile = await GetProfileAsync("csv");
            var profileId = (string)profile["id"];
            var uploadSas = await GetCsvUploadUrl(profileId);

            await UploadToUrl(files, uploadSas);
            await StartCsvSyncSafely(profileId);

            profile = await GetProfileAsync("csv");
            ViewBag.Message = $"CSV connector has id {profile["id"]} and status {profile["state"]}";

            return View("Csv");
        }

        [Route("rest")]
        public async Task<IActionResult> RestAsync()
        {
            var profile = await GetProfileAsync("rest");
            ViewBag.Message = $"OneRoster REST connector has id {profile["id"]} and status {profile["state"]}";
            return View("Rest");
        }

        private async Task<JObject> GetProfileAsync(string profileType)
        {
            // find existing matching profile
            var profileName = $"OneRoster{profileType.ToUpper()}Profile";
            HttpResponseMessage res = await QueryGraphAsync("/testsds/synchronizationProfiles");
            var profiles = (JArray)JObject.Parse(await res.Content.ReadAsStringAsync())["value"];

            foreach (var profile in profiles)
            {
                if ((string)profile["displayName"] == profileName)
                {
                    return (JObject)profile;
                }
            }

            // create new profile
            HttpResponseMessage res2 = await PostGraphAsync("/testsds/synchronizationProfiles", await GenerateProfileAsync(profileType));
            return JObject.Parse(await res2.Content.ReadAsStringAsync());
        }

        private async Task UploadToUrl(List<IFormFile> files, string sas)
        {
            CloudBlobContainer container = new CloudBlobContainer(new Uri(sas));

            foreach (var file in files)
            {
                var fileBlob = container.GetBlockBlobReference(file.FileName);
                await fileBlob.UploadFromStreamAsync(file.OpenReadStream());
            }
        }

        private async Task StartCsvSyncSafely(string profileId)
        {
            var profileIsReady = false;
            do
            {
                var res = await QueryGraphAsync($"/testsds/synchronizationProfiles/{profileId}");
                var responseText = await res.Content.ReadAsStringAsync();
                profileIsReady = (string)JObject.Parse(responseText)["state"] == "provisioned";

                if (!profileIsReady)
                {
                    await Task.Delay(5000);
                }
            } while (!profileIsReady);
            await StartCsvSync(profileId);
        }

        private async Task StartCsvSync(string profileId)
        {
            var req = new HttpRequestMessage(HttpMethod.Post, $"https://graph.microsoft.com/testsds/synchronizationProfiles/{profileId}/start");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await GetAccessTokenAsync());
            var response = await Client.SendAsync(req);
            var responseText = await response.Content.ReadAsStringAsync();
        }

        private async Task<string> GetCsvUploadUrl(string profileId)
        {
            var res = await QueryGraphAsync($"/testsds/synchronizationProfiles/{profileId}/uploadUrl");
            var parsed = JObject.Parse(await res.Content.ReadAsStringAsync());
            return (string)parsed["value"];
        }

        private async Task<HttpResponseMessage> PostGraphAsync(string relativeUrl, string requestBody)
        {
            var req = new HttpRequestMessage(HttpMethod.Post, $"https://graph.microsoft.com/{relativeUrl}")
            {
                Content = new StringContent(requestBody, Encoding.UTF8, "application/json")
            };

            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await GetAccessTokenAsync());

            return await Client.SendAsync(req);
        }

        private async Task<HttpResponseMessage> QueryGraphAsync(string relativeUrl)
        {
            var req = new HttpRequestMessage(HttpMethod.Get, $"https://graph.microsoft.com/{relativeUrl}");

            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await GetAccessTokenAsync());

            return await Client.SendAsync(req);
        }

        private async Task<string> GetAccessTokenAsync()
        {
            return await HttpContext.GetTokenAsync("access_token");
        }

        // https://github.com/OfficeDev/O365-EDU-Tools/blob/master/SDSProfileManagementDocs/api/synchronizationProfile_create.md
        private async Task<string> GenerateProfileAsync(string profileType)
        {
            var skus = await GetOfficeSkusAsync();

            StringBuilder sb = new StringBuilder();
            using (JsonWriter writer = new JsonTextWriter(new StringWriter(sb)))
            {
                writer.WriteStartObject();

                writer.WritePropertyName("displayName");
                writer.WriteValue($"OneRoster{profileType.ToUpper()}Profile");

                writer.WritePropertyName("dataProvider");
                writer.WriteStartObject();

                writer.WritePropertyName("@odata.type");

                if (profileType == "csv")
                {
                    writer.WriteValue("#microsoft.graph.csvDataProvider");
                }
                else
                {
                    writer.WriteValue("#microsoft.graph.oneRosterApiDataProvider");

                    writer.WritePropertyName("connectionUrl");
                    writer.WriteValue($"{(Request.IsHttps ? "https" : "http")}://{Request.Host}/ims/oneroster/v1p1");

                    writer.WritePropertyName("clientId");
                    writer.WriteValue("contoso");

                    writer.WritePropertyName("clientSecret");
                    writer.WriteValue("contoso-secret");
                }

                writer.WriteEndObject();

                writer.WritePropertyName("identitySyncConfiguration");
                writer.WriteStartObject();

                writer.WritePropertyName("@odata.type");
                writer.WriteValue("#microsoft.graph.identityCreationConfiguration");

                writer.WritePropertyName("userDomains");
                writer.WriteStartArray();
                writer.WriteStartObject();
                writer.WritePropertyName("appliesTo");
                writer.WriteValue("student");

                writer.WritePropertyName("name");
                writer.WriteValue(_config.GetValue<string>("AzureDomain"));
                writer.WriteEndObject();

                writer.WriteStartObject();
                writer.WritePropertyName("appliesTo");
                writer.WriteValue("teacher");

                writer.WritePropertyName("name");
                writer.WriteValue(_config.GetValue<string>("AzureDomain"));
                writer.WriteEndObject();
                writer.WriteEndArray();

                writer.WriteEndObject();

                writer.WritePropertyName("licensesToAssign");
                writer.WriteStartArray();

                writer.WriteStartObject();
                writer.WritePropertyName("appliesTo");
                writer.WriteValue("teacher");

                writer.WritePropertyName("skuIds");
                writer.WriteStartArray();
                writer.WriteValue(skus.Item1);
                writer.WriteEndArray();
                writer.WriteEndObject();

                writer.WriteStartObject();
                writer.WritePropertyName("appliesTo");
                writer.WriteValue("student");

                writer.WritePropertyName("skuIds");
                writer.WriteStartArray();
                writer.WriteValue(skus.Item2);
                writer.WriteEndArray();
                writer.WriteEndObject();

                writer.WriteEndArray();

                writer.WriteEndObject();

                return sb.ToString();
            }
        }

        // https://developer.microsoft.com/en-us/graph/docs/api-reference/v1.0/api/subscribedsku_list
        private async Task<Tuple<string, string>> GetOfficeSkusAsync()
        {
            HttpResponseMessage res = await QueryGraphAsync("/v1.0/subscribedSkus");

            var skus = (JArray)JObject.Parse(await res.Content.ReadAsStringAsync())["value"];

            string studentSku = null;
            string teacherSku = null;

            foreach (var sku in skus)
            {
                if ((string)sku["skuPartNumber"] == "STANDARDWOFFPACK_FACULTY")
                {
                    teacherSku = (string)sku["skuId"];
                }
                if ((string)sku["skuPartNumber"] == "STANDARDWOFFPACK_STUDENT")
                {
                    studentSku = (string)sku["skuId"];
                }
            }

            return new Tuple<string, string>(teacherSku, studentSku);
        }
    }
}
