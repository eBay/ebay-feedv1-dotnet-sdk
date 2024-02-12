
/*
 * *
 *  * Copyright 2024 eBay Inc.
 *  *
 *  * Licensed under the Apache License, Version 2.0 (the "License");
 *  * you may not use this file except in compliance with the License.
 *  * You may obtain a copy of the License at
 *  *
 *  *  http://www.apache.org/licenses/LICENSE-2.0
 *  *
 *  * Unless required by applicable law or agreed to in writing, software
 *  * distributed under the License is distributed on an "AS IS" BASIS,
 *  * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  * See the License for the specific language governing permissions and
 *  * limitations under the License.
 *  *
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using eBay.Sdk.Model;
using eBay.ApiClient.Auth.OAuth2;
using eBay.ApiClient.Auth.OAuth2.Model;
using YamlDotNet.Core.Tokens;
using eBay.Sdk.Exceptions;

namespace eBay.Sdk.Util
{
    public class FeedUtil
    {
        private const string BYTES_EQUAL = "bytes=";
        private const string BEARER = "Bearer ";

        private const string DASH = "-";
        private readonly HttpClient httpClient = new();
        private readonly OAuth2Api oauth2Api = new();

         private readonly IList<String> scopes = new List<String>()
            {
                "https://api.ebay.com/oauth/api_scope/buy.item.feed"
            };

        public long GetContentRange(string marketplaceId, string baseURL, string token)
        {
            string range = BYTES_EQUAL + 0 + DASH + 100;

            var task = Get(marketplaceId, range, baseURL);
            task.Wait();
            InvokeResponse invokeResponse = null;

            var respMessage = task.GetAwaiter().GetResult();

            if (respMessage.Content.Headers.TryGetValues("Content-Range", out IEnumerable<string> values))
            {
                invokeResponse = new InvokeResponse(respMessage.StatusCode, values.First());
            }
            return long.Parse(invokeResponse.contentRange.Split("/")[1]);
        }


        public void CallGetParallel(string rangeValue, string baseURL, string marketplaceId, string outputFilename)
        {
            // Make REST GET call once in order to get the content size
            long contentSize = GetContentRange(marketplaceId, baseURL);
            List<string> listOfRanges = GetRangeList(rangeValue, contentSize);
            CallGetChunks(marketplaceId, baseURL, listOfRanges, outputFilename);

        }

        private static List<string> GetRangeList(string rangeValue, long contentSize)
        {
            long requestRangeLowerLimit = 0;

            //create constant for 10240000
            long deltaIncrease = rangeValue == null ? 10240000 : long.Parse(rangeValue);
            long requestRangeUpperLimit = deltaIncrease > contentSize ? contentSize : deltaIncrease;
            string range = BYTES_EQUAL + requestRangeLowerLimit + DASH + requestRangeUpperLimit;

            Boolean iterateCall = true;
            List<string> ranges = new List<string>();

            while (iterateCall)
            {
                ranges.Add(range);
                if (requestRangeUpperLimit >= contentSize)
                {
                    break;
                }
                requestRangeLowerLimit = requestRangeUpperLimit + 1;
                requestRangeUpperLimit = requestRangeLowerLimit + deltaIncrease;
                range = BYTES_EQUAL + requestRangeLowerLimit + DASH + requestRangeUpperLimit;

            }

            return ranges;
        }

        private void CallGetChunks(string marketplaceId, string baseURL, List<string> ranges,
        string outputFilename)
        {

            //get 100 at a time in ranges
            //partition list of strings into 100 each
            Partition(ranges, 50).ForEach(subRanges =>
            {
                List<Task<HttpResponseMessage>> tasks = new List<Task<HttpResponseMessage>>();
                foreach (string range in subRanges)
                {
                    //add to tasks
                    tasks.Add(Get(marketplaceId, range, baseURL));
                };
                //wait for tasks
                Task.WaitAll(tasks.ToArray());
                AppendToFile(outputFilename, tasks);
            });

        }

        private void AppendToFile(string outputFilename, List<Task<HttpResponseMessage>> tasks)
        {
            using (Stream streamToWriteTo = File.Open(outputFilename, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            {
                foreach (Task<HttpResponseMessage> task in tasks)
                {
                    var respMessage = task.GetAwaiter().GetResult();
                    respMessage.Content.CopyToAsync(streamToWriteTo);
                }
            }
        }

        private static List<List<T>> Partition<T>(List<T> list, int chunkSize)
        {
            List<List<T>> partitions = new List<List<T>>();
            for (int i = 0; i < list.Count; i += chunkSize)
            {
                List<T> chunk = list.GetRange(i, Math.Min(chunkSize, list.Count - i));
                partitions.Add(chunk);
            }
            return partitions;
        }

        private long GetContentRange(string marketplaceId, string baseURL)
        {

            string range = BYTES_EQUAL + 0 + DASH + 100;

            var task = Get(marketplaceId, range, baseURL);
            task.Wait();
            InvokeResponse invokeResponse = null;

            var respMessage = task.GetAwaiter().GetResult();
            //Console.WriteLine("respMessage = " + respMessage.StatusCode);

            if (respMessage.Content.Headers.TryGetValues("Content-Range", out IEnumerable<string> values))
            {
                invokeResponse = new InvokeResponse(respMessage.StatusCode, values.First());
            }
            return long.Parse(invokeResponse.contentRange.Split("/")[1]);

        }

        public async Task<HttpResponseMessage> Get(string marketplaceId, string range, string url)
        {

            var token = FetchAppToken();

            if (!httpClient.DefaultRequestHeaders.TryGetValues("authorization",out IEnumerable<string> values) )
            {
                httpClient.DefaultRequestHeaders.Add("authorization", token);
            }
            if ( marketplaceId != null)
            {
                httpClient.DefaultRequestHeaders.Remove("X-EBAY-C-MARKETPLACE-ID");

                httpClient.DefaultRequestHeaders.Add("X-EBAY-C-MARKETPLACE-ID", marketplaceId);
            }
            if ( !httpClient.DefaultRequestHeaders.Contains("Accept")) {
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            }
            if (range != null)
            {
                httpClient.DefaultRequestHeaders.Remove("range");
                httpClient.DefaultRequestHeaders.Add("range", range);
            }

            var response = await httpClient.GetAsync(url);
            return response;
        }

        private string FetchAppToken()
        {
            OAuthResponse respons = oauth2Api.GetApplicationToken(OAuthEnvironment.PRODUCTION, scopes);
            if (respons == null ) {
                var message = "Fetch application token failed, with response null ";
                throw new FeedTokenException(message);

            }
            if (respons.AccessToken == null ) {
                var message = "Fetch application token failed, with access token null "+respons;
                throw new FeedTokenException(message);

            }
            if (respons.AccessToken.Token == null ) {
                var message = "Fetch application token failed, with token null "+respons;
                throw new FeedTokenException(message);

            }
            
                return BEARER + respons.AccessToken.Token;
            
            
        }

    }
}
