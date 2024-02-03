
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
using eBay.ApiClient.Auth.OAuth2;
using eBay.ApiClient.Auth.OAuth2.Model;
using eBay.Sdk.Client;
using eBay.Sdk.Model;
using eBay.Sdk.Validator;
using eBay.Sdk.Exceptions;
using Newtonsoft.Json.Linq;

namespace eBay.Sdk
{
    public class FeedV1SDK
    {
        private readonly FeedClient feedClient = new();
        private FeedValidator feedValidator = new();

        /// <summary>
        /// Filters the feed by item and performs the necessary operations to download, unzip, and find the specified item in the feed.
        /// </summary>
        /// <param name="rangeValue">The range value for filtering the feed.</param>
        /// <param name="feedtype">The type of feed.</param>
        /// <param name="categoryId">The category ID for filtering the feed.</param>
        /// <param name="marketplaceId">The marketplace ID for filtering the feed.</param>
        /// <param name="itemId">The ID of the item to find in the feed.</param>
        /// <param name="zippedOutputFilename">The filename of the zipped output file.</param>
        /// <param name="unzippedOutputFilename">The filename of the unzipped output file.</param>
        /// <param name="filteredOutputFilename">The filename of the filtered output file.</param>
        /// <returns>True if the item is found in the feed; otherwise, false.</returns>
        public Boolean FilterByItem(string rangeValue, string feedtype, string categoryId, string marketplaceId, string itemId,
        string zippedOutputFilename, string unzippedOutputFilename, string filteredOutputFilename)
        {
            feedValidator.ValidateSearchText(itemId);

            downloadUnzip(rangeValue, feedtype, categoryId, marketplaceId, zippedOutputFilename, unzippedOutputFilename);
            Console.WriteLine("Finding Item in Feed " + itemId);
            int index = findIndex(Constants.ClientConstants.ITEM_ID, feedtype, marketplaceId);

            return Find(index, itemId, unzippedOutputFilename, filteredOutputFilename);
        }

        public void FilterByItems(string rangeValue, string feedtype, string categoryId, string marketplaceId, List<string> itemIds,
            string zippedOutputFilename, string unzippedOutputFilename, string filteredOutputFilename)
        {
            feedValidator.ValidateData(itemIds);

            downloadUnzip(rangeValue, feedtype, categoryId, marketplaceId, zippedOutputFilename, unzippedOutputFilename);
            int index = findIndex(Constants.ClientConstants.ITEM_ID, feedtype, marketplaceId);

            FindSeveral(index, itemIds, unzippedOutputFilename, filteredOutputFilename);
        }

        public void FilterBySeller(string rangeValue, string feedtype, string categoryId, string marketplaceId, string sellerUsername,
        string zippedOutputFilename, string unzippedOutputFilename, string filteredOutputFilename)
        {
            feedValidator.ValidateSearchText(sellerUsername);
            downloadUnzip(rangeValue, feedtype, categoryId, marketplaceId, zippedOutputFilename, unzippedOutputFilename);
            int index = findIndex(Constants.ClientConstants.SELLER_USERNAME, feedtype, marketplaceId);
            FindAll(index, sellerUsername, unzippedOutputFilename, filteredOutputFilename);
        }

        private int findIndex(string fieldName, string feedtype, string marketplaceId)
        {
            //call getFeedType
            string response = feedClient.CallGetFeedtype(feedtype, marketplaceId);
            //get the json object
            JObject feedTypeJson = JObject.Parse(response);
            if(feedTypeJson == null) {
                throw new ClientResponseException("No feed type metadata for this categoryId  and marketplace");
            }

            JArray supprtFeedsArray = (JArray)feedTypeJson["supportedFeeds"];
            JObject supprtFeed = supprtFeedsArray.First.Value<JObject>();

            JArray supprtSchemaArray = (JArray)supprtFeed["supportedSchemas"];
            JObject supprtSchema = supprtSchemaArray.First.Value<JObject>();

            var definition = supprtSchema["definition"];
            if(definition == null) {
                throw new ClientResponseException("No feed type definition for this categoryId  and marketplace");
            }

            JObject dictObject = JObject.Parse(definition.Value<string>());
            int index = dictObject.Properties().ToList().FindIndex(p => p.Name.Equals(fieldName));
           
            Console.WriteLine("Index of "+fieldName+" is "+index);
            return index;
        }

        private void downloadUnzip(string rangeValue, string feedtype, string categoryId, string marketplaceId, string zippedOutputFilename, string unzippedOutputFilename)
        {
            feedValidator.ValidateRequest(feedtype, categoryId, marketplaceId);
            Console.WriteLine("Download Latest File-Start");
            DownloadLatestFile(rangeValue, feedtype, categoryId, marketplaceId, zippedOutputFilename);
            Console.WriteLine("Download Latest File-Complete");
            Console.WriteLine("Unzip Downloaded File-Start");
            Unzip(zippedOutputFilename, unzippedOutputFilename);
            Console.WriteLine("Unzip Downloaded File-Complete");
        }

        /// <summary>
        /// Finds an item with the specified ID in the provided file and writes the matching item information to another file.
        /// </summary>
        /// <param name="itemId">The ID of the item to find.</param>
        /// <param name="unzippedOutputFilename">The path to the file containing the item information.</param>
        /// <param name="filteredOutputFilename">The path to the file where the matching item information will be written.</param>
        /// <returns>True if the item is found and written to the file, false otherwise.</returns>
        private static Boolean Find(int index, string text, string unzippedOutputFilename, string filteredOutputFilename)
        {
            //read content  
            using (StreamReader dataReader = new StreamReader(unzippedOutputFilename))
            {
                string itemInfo = string.Empty;

                while ((itemInfo = dataReader.ReadLine()) != null)
                {
                    string[] fields = itemInfo.Split('\t');
                    if(fields[index].ToLower().Trim().Equals(text.ToLower().Trim())) {
                        Console.WriteLine("Found "+text+" in Feed ");
                        Console.WriteLine("Writing to file "+filteredOutputFilename);
                        //append to file
                        using (StreamWriter sw = File.AppendText(filteredOutputFilename))
                        {
                            sw.WriteLine(itemInfo);
                            return true;
                        }
                    }
                }
            }
            return false;
            
        }

        //write method to search for text in tab separated file
        private static void FindAll(int index, string text, string unzippedOutputFilename, string filteredOutputFilename)
        {
            //read content  
            using (StreamReader dataReader = new StreamReader(unzippedOutputFilename))
            {
                string itemInfo = string.Empty;

                while ((itemInfo = dataReader.ReadLine()) != null)
                {
                    //create constant for tab
                    string[] fields = itemInfo.Split(Constants.ClientConstants.Separator);
                    
                    if(fields[index].ToLower().Trim().Equals(text.ToLower().Trim())) {
                        //append to file
                        using (StreamWriter sw = File.AppendText(filteredOutputFilename))
                        {
                            sw.WriteLine(itemInfo);
                        }
                    }
                }
            }
            
        }

        private static void FindSeveral(int index, List<string> values, string unzippedOutputFilename, string filteredOutputFilename)
        {

            //read content  
            using (StreamReader dataReader = new StreamReader(unzippedOutputFilename))
            {
                string itemInfo = string.Empty;

                while ((itemInfo = dataReader.ReadLine()) != null)
                {
                    string[] fields = itemInfo.Split('\t');
                    if(values.Contains(fields[index])) {
                        //append to file
                        using (StreamWriter sw = File.AppendText(filteredOutputFilename))
                        {
                            sw.WriteLine(itemInfo);
                        }
                    }
                }
            }
            
        }

        /// <summary>
        /// Unzips a GZip file and saves the decompressed content to a specified file.
        /// </summary>
        /// <param name="zippedOutputFilename">The path of the GZip file to unzip.</param>
        /// <param name="unzippedOutputFilename">The path where the decompressed content will be saved.</param>
        public static void Unzip(string zippedOutputFilename, string unzippedOutputFilename)
        {
            //unzip gz file and read content
            // load the GZip archive
            using (var fileStream = File.OpenRead(zippedOutputFilename))
            using (var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress))
            using (var extracted = File.Create(unzippedOutputFilename))
            {
                gzipStream.CopyTo(extracted);
            }
        }

        /// <summary>
        /// Downloads the latest file for the specified feed type, category, and marketplace.
        /// </summary>
        /// <param name="rangeValue">The range value for the file download.</param>
        /// <param name="feedtype">The feed type.</param>
        /// <param name="categoryId">The category ID.</param>
        /// <param name="marketplaceId">The marketplace ID.</param>
        /// <param name="zippedOutputFilename">The filename for the downloaded file.</param>
        public void DownloadLatestFile(string rangeValue, string feedtype, string categoryId, string marketplaceId, string zippedOutputFilename)
        {
            //get all the files metadata
            String files = feedClient.CallGetFiles(feedtype, categoryId, marketplaceId);
            if(files == null)
            {
                return;//the reason is already printed in CallGetFiles
            }
            //parse json string to read fileId
            JObject filesJson = JObject.Parse(files);
            if (filesJson == null)
            {
                Console.WriteLine("No files found for the given feedtype and category");
                return;
            }
            JArray fileArray = (JArray)filesJson[Constants.ClientConstants.FILE_METADATA];
            if (fileArray == null)
            {
                Console.WriteLine("No files found for the given feedtype and category");
                return;
            }
            //get the latest file
            JObject file = (JObject)fileArray[0];
            string fileId = (string)file[Constants.ClientConstants.FILE_ID];
            if (fileId != null)
            {
                feedClient.CallGetFile(fileId, marketplaceId);
                feedClient.CallDownloadFile(rangeValue, fileId, marketplaceId, zippedOutputFilename);
            } else {
                Console.WriteLine("No files found for the given feedtype and category");
            }
        }

    }

}
