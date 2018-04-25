using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using TeamDynamix.Api.Assets;

namespace YourNamespace {
    public class clsTdAuth {

        private const string locationOrigin = "https://YOUR-COMPANY.teamdynamix.com/"; 
        private const string webApiBasePath = "SBTDWebApi/api/";
        private const string authPath = "auth/loginadmin/";
        private const string beid = "YOUR-BEID-KEY";
        private const string webServiceKey = "YOUR-WEB-SERVICE-KEY";
        private const string assetId = "909/";
        private const string assets = "assets/";
        private const string search = "search/";
        private const string statuses = "statuses/";
        private const string roles = "198009/"; 
        private const string people = "people/";
        private const string lookUp = "lookup?searchText=";
        private const string limitResults = "&maxResults=1";
        private const string locations = "locations/";
        private HttpClient httpClient = new HttpClient();
        private string httpResponse;
        private HttpResponseMessage responseMsg;

        /* Constructor class
         * Create the token when we instatiate this object
         * 
         */
        public clsTdAuth() {
            //set the http client headers when object created
            Task.Run(() => this.setBearerKey()).Wait();
        }

        private async Task setBearerKey() {
            var loginUri = new Uri(locationOrigin + webApiBasePath + authPath);
            responseMsg = await httpClient.PostAsJsonAsync(loginUri, new { BEID = beid, WebServicesKey = webServiceKey });
            httpResponse = responseMsg.Content.ReadAsStringAsync().Result;
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + httpResponse);
        }

        public async Task<JArray> getComputers(int[] statusIds) {
            Uri listOfAssetsUri = new Uri(locationOrigin + webApiBasePath + assetId + assets + search);
            return await returnJArrayFromPost(listOfAssetsUri, statusIds);
        }

        //return JArray of locations
        public async Task<JArray> getLocations() {
            Uri locationsUri = new Uri(locationOrigin + webApiBasePath + locations);
            return await returnJArrayFromGet(locationsUri);
        }

        //return JArray of asset statuses
        public async Task<JArray> getAssetStatuses() {
            Uri assetStatusesUri = new Uri(locationOrigin + webApiBasePath + assetId + assets + statuses);
            return await returnJArrayFromGet(assetStatusesUri);

        }

        public async Task<JArray> getListOfAssets() {
            Uri assetsUri = new Uri(locationOrigin + webApiBasePath + assetId + assets + search);
            return await returnJArrayFromPost(assetsUri);
        }

        public async Task<JObject> getAssetRoles() {
            Uri assetRolesUri = new Uri(locationOrigin + webApiBasePath + assetId + assets + roles);
            return await returnJObjectFromGet(assetRolesUri);
        }

        public async Task<JObject> getSingleAsset(string assetIdNum) {
            Uri singleAssetUri = new Uri(locationOrigin + webApiBasePath + assetId + assets + assetIdNum);
            return await returnJObjectFromGet(singleAssetUri);
        }

        //get their company specific ID based on the guid that exists in TD
        public async Task<JObject> getCompanyIdFromTd(Guid tdId) {
            Uri getUserUri = new Uri(locationOrigin + webApiBasePath + people + tdId.ToString());
            return await returnJObjectFromGet(getUserUri);
        }

        //get the TD id of a user (not the same as their company ID)
        public async Task<Guid> getTdUid(string searchText) {
            Uri getUidUri = new Uri(locationOrigin + webApiBasePath + people + lookUp + searchText + limitResults);
            JArray persons = await returnJArrayFromGet(getUidUri);

            if(persons.Count < 1) {
                return new Guid("00000000-0000-0000-0000-000000000000");
            }

            return (Guid)persons[0]["UID"];
        }

        public async Task<int> getBuildingId(string buildingName) {
            JArray buildings;
            Uri buildingsUri = new Uri(locationOrigin + webApiBasePath + locations + search);
            responseMsg = await httpClient.PostAsJsonAsync(buildingsUri, new { NameLike = buildingName });

            if (responseMsg.IsSuccessStatusCode) {
                httpResponse = responseMsg.Content.ReadAsStringAsync().Result;
                buildings = JArray.Parse(httpResponse);

                if (buildings.Count > 0) {
                    return (int)buildings[0]["ID"];
                }
            }

            return 0000;
        }

        public async Task<JObject> getRooms(string building) {
            Uri roomsUri = new Uri(locationOrigin + webApiBasePath + locations + (await getBuildingId(building)).ToString());
            return await returnJObjectFromGet(roomsUri);
        }

        //create a new asset in team dynamix
        public async Task<bool> createNewAsset(Asset asset) {
            Uri addNewAssetUri = new Uri(locationOrigin + webApiBasePath + assetId + assets);
            HttpResponseMessage res = await httpClient.PostAsJsonAsync(addNewAssetUri, asset);
            return res.IsSuccessStatusCode;
        }

        //update and existing asset in td
        public async Task<bool> updateAsset(Asset asset, string assetIdNumber) {
            Uri addNewAssetUri = new Uri(locationOrigin + webApiBasePath + assetId + assets + assetIdNumber);
            HttpResponseMessage res = await httpClient.PostAsJsonAsync(addNewAssetUri, asset);
            return res.IsSuccessStatusCode;
        }



        //private helper functions for this class

        //function to handle api gets to td that return json arrays
        private async Task<JArray> returnJArrayFromGet(Uri uri) {
            return returnJArray(await httpClient.GetAsync(uri));
        }

        //function to handle api posts to team dynamix that return json arrays
        private async Task<JArray> returnJArrayFromPost(Uri uri) {
            //responseMsg = await httpClient.PostAsJsonAsync(uri, new { StatusIDs = statusIds });
            return returnJArray(await httpClient.PostAsJsonAsync(uri, new { }));
        }

        //function to handle api posts to team dynamix that return json arrays
        private async Task<JArray> returnJArrayFromPost(Uri uri, int[] statusIds) {
            //responseMsg = await httpClient.PostAsJsonAsync(uri, new { StatusIDs = statusIds });
            return returnJArray(await httpClient.PostAsJsonAsync(uri, new { StatusIDs = statusIds }));
        }

        private async Task<JObject> returnJObjectFromGet(Uri uri) {
            return returnJObject(await httpClient.GetAsync(uri));
        }

        //check and return JObject
        private JObject returnJObject(HttpResponseMessage resMsg) {
            httpResponse = resMsg.Content.ReadAsStringAsync().Result;
            if (resMsg.IsSuccessStatusCode) { return JObject.Parse(httpResponse); }
            return null;
        }

        //check and return JsonArray
        private JArray returnJArray(HttpResponseMessage resMsg) {
            httpResponse = resMsg.Content.ReadAsStringAsync().Result;
            if (resMsg.IsSuccessStatusCode) { return JArray.Parse(httpResponse); }
            return null;
        }
    }
}