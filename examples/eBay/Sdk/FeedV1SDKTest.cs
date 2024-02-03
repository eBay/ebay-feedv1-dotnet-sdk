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

using System;
using System.Collections.Generic;
using System.IO;
using Xunit;


namespace eBay.Sdk
{
    public class FeedV1SDKTest
    {
        /// <summary>
        /// Test method to filter item by item ID.
        /// </summary>
        [Fact]
        public void FilterByItem_Success()
        {
            Console.WriteLine("STARTING TEST filterByItem_Success");
            string path = @"../../../example-config.yaml";
            string feedType = "CURATED_ITEM_FEED";
            string categoryId = "1249";
            string marketplaceId = "EBAY_US";
            string itemId = "v1|266188913615|0";
            string zippedOutputFilename = @"../../../feedDownloadTest.gz";
            string unzippedOutputFilename = @"../../../feedDownloadTest.tsv";
            string filteredOutputFilename = @"../../../feedFilteredItemTest.tsv";

            //delete if file exists
            deleteIfExists(zippedOutputFilename);
            deleteIfExists(unzippedOutputFilename);
            deleteIfExists(filteredOutputFilename);

            CredentialUtil.Load(path);

            FeedV1SDK sdk = new FeedV1SDK();
            Assert.True(sdk.FilterByItem("10240000", feedType, categoryId, marketplaceId, itemId, zippedOutputFilename, unzippedOutputFilename, filteredOutputFilename));
            Console.WriteLine("ENDING TEST FilterByItem_Success");
        }

         /// <summary>
        /// Test method to filter item by item ID.
        /// </summary>
        [Fact]
        public void FilterByItems_Success()
        {
            Console.WriteLine("STARTING TEST FilterByItems_Success");
            string path = @"../../../example-config.yaml";
            string feedType = "CURATED_ITEM_FEED";
            string categoryId = "6000";
            string marketplaceId = "EBAY_US";
            //init List
            //create list of itemIds    
            List<string> itemIds= getItemIds();
            string zippedOutputFilename = @"../../../feedItemsDownloadTest.gz";
            string unzippedOutputFilename = @"../../../feedItemsDownloadTest.tsv";
            string filteredOutputFilename = @"../../../feedFilteredItemsTest.tsv";

            //delete if file exists
            deleteIfExists(zippedOutputFilename);
            deleteIfExists(unzippedOutputFilename);
            deleteIfExists(filteredOutputFilename);

            CredentialUtil.Load(path);

            FeedV1SDK sdk = new FeedV1SDK();
            sdk.FilterByItems("10240000", feedType, categoryId, marketplaceId, itemIds, zippedOutputFilename, unzippedOutputFilename, filteredOutputFilename);
            Console.WriteLine("ENDING TEST FilterByItems_Success");
        }

        private static List<string> getItemIds()
        {
            List<string> itemIds = new List<string>();
            itemIds.Add("v1|334700166050|0");
            itemIds.Add("v1|222995287496|0");
            itemIds.Add("v1|261649893874|0");
            return itemIds;
        }

        [Fact]
        public void FilterBySeller_Success()
        {
            Console.WriteLine("STARTING TEST FilterBySeller_Success");
            string path = @"../../../example-config.yaml";
            string feedType = "CURATED_ITEM_FEED";
            string categoryId = "2984";
            string marketplaceId = "EBAY_US";
            string sellerUsername = "mopar-direct";
            string zippedOutputFilename = @"../../../feedSellerDownloadTest.gz";
            string unzippedOutputFilename = @"../../../feedSellerDownloadTest.tsv";
            string filteredOutputFilename = @"../../../feedFilteredSellerTest.tsv";

            //delete if file exists
            deleteIfExists(zippedOutputFilename);
            deleteIfExists(unzippedOutputFilename);
            deleteIfExists(filteredOutputFilename);

            CredentialUtil.Load(path);

            FeedV1SDK sdk = new FeedV1SDK();
            sdk.FilterBySeller("10240000", feedType, categoryId, marketplaceId, sellerUsername, zippedOutputFilename, unzippedOutputFilename, filteredOutputFilename);
            Console.WriteLine("ENDING TEST FilterBySeller_Success");
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