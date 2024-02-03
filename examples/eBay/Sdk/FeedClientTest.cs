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

using eBay.ApiClient.Auth.OAuth2;
using eBay.Sdk.Client;
using System;
using System.IO;
using Xunit;
using Newtonsoft.Json.Linq;


namespace eBay.Sdk
{
    public class FeedClientExample
    {
        private readonly FeedClient feedClient = new();
       
        [Fact]
        public void GetAccess_Example()
        {
            Console.WriteLine("STARTING TEST GetAccess_Positive");
            init();

            string response = feedClient.CallGetAccess();
            Assert.NotNull(response);
            Console.WriteLine(response);
            Console.WriteLine("ENDING TEST GetAccess_Positive");
        }

        [Fact]
        public void FeedTypes_Example()
        {
            Console.WriteLine("STARTING TEST GetFeedTypes_Positive: CallGetFeedtypes with no marketplace");
            init();
            string response = feedClient.CallGetFeedtypes(null);
            Assert.NotNull(response);
            Console.WriteLine("ENDING TEST GetFeedTypes_Positive");
            Console.WriteLine("STARTING TEST GetFeedTypes_Positive: CallGetFeedtypes with marketplace");
            response = feedClient.CallGetFeedtypes("EBAY_US");
            Assert.NotNull(response);
            Console.WriteLine("ENDING TEST GetFeedTypes_Positive");
            JObject feedTypesJson = JObject.Parse(response);
            JArray feedTypesArray = (JArray)feedTypesJson["feedTypes"];
            Assert.True(feedTypesArray.Count > 0);
            JObject feedTypeJson = feedTypesArray.First.Value<JObject>();
            Assert.NotNull(feedTypeJson);
            Assert.NotNull(feedTypeJson["feedTypeId"]);
            Console.WriteLine("STARTING TEST GetFeedTypes_Positive: CallGetFeedtype with feedTypeId: " + feedTypeJson["feedTypeId"].Value<string>());
            response = feedClient.CallGetFeedtype(feedTypeJson["feedTypeId"].Value<string>(), "EBAY_US");
            Assert.NotNull(response);
            Console.WriteLine("ENDING TEST GetFeedTypes_Positive");
        }

        [Fact]
        public void Files_ErrorExample()
        {
            Console.WriteLine("STARTING TEST Files_ErrorExample: CallGetFile");
            init();
            try {
                //fileId null test
                string response = feedClient.CallGetFile(null, "EBAY_US");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception on CallGetFile "+ex.Message);
            }
        
            Console.WriteLine("ENDING TEST Files_ErrorExample");
            Console.WriteLine("STARTING TEST Files_ErrorExample: CallDownloadFile");
            try {

            //fileId null test
            feedClient.CallDownloadFile(null, null, "EBAY_US", null);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception on CallDownloadFile "+ex.Message);
            }
            Console.WriteLine("ENDING TEST Files_ErrorExample");
          
        }

        [Fact]
        public void Files_Example()
        {
            Console.WriteLine("STARTING TEST GetFiles_Positive: CallGetFiles");
            init();

            string response = feedClient.CallGetFiles("CURATED_ITEM_FEED", "6000", "EBAY_US");
            Assert.NotNull(response);
            Console.WriteLine("ENDING TEST GetFiles_Positive");
            JObject filesJson = JObject.Parse(response);
            JArray filesMetadata = (JArray)filesJson["fileMetadata"];
            Assert.True(filesMetadata.Count > 0);
            JObject file = filesMetadata.First.Value<JObject>();
            Assert.NotNull(file);
            var fileIdToken = file["fileId"];
            Assert.NotNull(fileIdToken);
            var fileId = fileIdToken.Value<string>();
            Assert.NotNull(fileId);
            Console.WriteLine("STARTING TEST GetFiles_Positive: CallGetFile with fileId: " + fileId);
            response = feedClient.CallGetFile(fileId, "EBAY_US");
            Assert.NotNull(response);
            Console.WriteLine("ENDING TEST GetFiles_Positive: CallGetFile");

            string zippedOutputFilename = @"../../../feedClientDownloadTest.gz";

            Console.WriteLine("STARTING TEST GetFiles_Positive: CallDownloadFile with fileId: " + fileId);
            feedClient.CallDownloadFile(null, fileId, "EBAY_US",zippedOutputFilename);
            Console.WriteLine("ENDING TEST GetFiles_Positive: CallDownloadFile");
        }

        private static void init()
        {
            string path = @"../../../example-config.yaml";

            CredentialUtil.Load(path);
        }

        private static void deleteIfExists(string filename)
        {
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }
        }
    }
}