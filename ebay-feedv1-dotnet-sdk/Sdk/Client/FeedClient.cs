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
using eBay.Sdk.Constants;
using eBay.Sdk.Model;
using eBay.Sdk.Util;
using eBay.Sdk.Validator;
using eBay.Sdk.Exceptions;


namespace eBay.Sdk.Client
{
    public class FeedClient
    {
       
        private readonly FeedUtil feedUtil = new();
        private FeedValidator feedValidator = new();

        /// <summary>
        /// Calls the GetFeedtype API to retrieve the feed type for a given marketplace.
        /// </summary>
        /// <param name="feedtype">The feed type to retrieve.</param>
        /// <param name="marketplaceId">The marketplace ID.</param>
        public string CallGetFeedtype(string feedtype, string marketplaceId)
        {
            try
            {
                var baseURL = ClientConstants.FEEDTYPE_BASE_URL + ClientConstants.SLASH + feedtype;

                var task = feedUtil.Get(marketplaceId, null, baseURL);
                task.Wait();
                HttpResponseMessage responseMsg = task.GetAwaiter().GetResult();
                if(feedValidator.ValidateResponse(responseMsg) == false)
                {
                    throw new ClientResponseException("Either response or response content is null, on CallGetFeedtype");
                }
                var contents = responseMsg.Content.ReadAsStringAsync();
                return contents.Result;
            }
            catch (Exception ex)
            {
                throw new ClientResponseException("Exception on CallGetFeedtype "+ ex.Message);

            }

        }

        /// <summary>
        /// Calls the Get Access API to retrieve access information.
        /// </summary>
        /// <returns>The response content as a string.</returns>
        public string CallGetAccess()
        {
           
            try
            {
            
                var task = feedUtil.Get(null, null, ClientConstants.ACCESS_BASE_URL);
                task.Wait();
                HttpResponseMessage responseMsg = task.GetAwaiter().GetResult();

                if (feedValidator.ValidateResponse(responseMsg) == false)
                {
                    throw new ClientResponseException("Either response or response content is null, on CallGetAccess");
                }
                var contents = responseMsg.Content.ReadAsStringAsync();
                return contents.Result;
            }
            catch (Exception ex)
            {
                throw new ClientResponseException("Exception on CallGetAccess "+ ex.Message);
            }
        }

        /// <summary>
        /// Calls the GetFeedtypes API to retrieve the available feed types for a specific marketplace.
        /// </summary>
        /// <param name="marketplaceId">The marketplace ID for which to retrieve the feed types.</param>
        /// 
        #nullable enable
        public string CallGetFeedtypes(string? marketplaceId)
        {
            
            try
            {
            
                var baseURL = ClientConstants.FEEDTYPE_BASE_URL + ClientConstants.SLASH;

                var task = feedUtil.Get(marketplaceId, null, baseURL);
                task.Wait();
                HttpResponseMessage responseMsg = task.GetAwaiter().GetResult();

                if (feedValidator.ValidateResponse(responseMsg) == false)
                {
                    throw new ClientResponseException("");
                }
                var contents = responseMsg.Content.ReadAsStringAsync();
                return contents.Result;

            }
            catch (Exception ex)
            {
                throw new ClientResponseException("Exception on CallGetFeedTypes "+ ex.Message);
            }

        }

        /// <summary>
        /// Calls the GetFiles API to retrieve the available files for a given feed type, category ID, and marketplace ID.
        /// </summary>
        /// <param name="feedtype">The feed type.</param>
        /// <param name="categoryId">The category ID.</param>
        /// <param name="marketplaceId">The marketplace ID.</param>
        /// <returns>The contents of the available files.</returns>
        public string CallGetFiles(string feedtype, string categoryId, string marketplaceId)
        {
            
            var url = ClientConstants.FILE_BASE_URL + ClientConstants.QUESTION_MARK+ ClientConstants.FEED_TYPE_ID+
            ClientConstants.EQUAL + feedtype + 
            ClientConstants.AND+ ClientConstants.CATEGORY_IDS+ ClientConstants.EQUAL + categoryId;
            var task = feedUtil.Get(marketplaceId, null, url);
            task.Wait();
            HttpResponseMessage responseMsg = task.GetAwaiter().GetResult();

            if (feedValidator.ValidateResponse(responseMsg) == false)
            {
                throw new ClientResponseException("");
            }
            var contents = responseMsg.Content.ReadAsStringAsync();
           
            Console.WriteLine("\n\n**************************");
            Console.WriteLine("Available files = " + contents.Result);
            Console.WriteLine("**************************\n\n");

            return contents.Result;
        }

        /// <summary>
        /// Calls the GetFile API to retrieve the contents of a file identified by the given file ID and marketplace ID.
        /// </summary>
        /// <param name="fileid">The ID of the file to retrieve.</param>
        /// <param name="marketplaceId">The marketplace ID associated with the file.</param>
        /// <returns>The contents of the file as a string.</returns>
        public string CallGetFile(String fileId, string marketplaceId) 
        {

            feedValidator.ValidateFileId(fileId);

            var baseURL = ClientConstants.FILE_BASE_URL + ClientConstants.SLASH + fileId;

            var task = feedUtil.Get(marketplaceId, null, baseURL);
            task.Wait();
            HttpResponseMessage responseMsg = task.GetAwaiter().GetResult();

            if (feedValidator.ValidateResponse(responseMsg) == false)
            {
                throw new ClientResponseException("");
            }
            var contents = responseMsg.Content.ReadAsStringAsync();
            Console.WriteLine("\n\n**************************");
            Console.WriteLine("CallGetFile: file metadata = " + contents.Result);
            Console.WriteLine("**************************\n\n");

            return contents.Result;
        }

        /// <summary>
        /// Calls the download file API to download a file from eBay Feed API.
        /// </summary>
        /// <param name="rangeValue">The range value for partial content download.</param>
        /// <param name="fileid">The ID of the file to be downloaded.</param>
        /// <param name="marketplaceId">The marketplace ID for the file.</param>
        /// <param name="outputFilename">The name of the output file.</param>
        public void CallDownloadFile(string rangeValue, string fileId, string marketplaceId, string outputFilename)
        {
            feedValidator.ValidateFileId(fileId);

            var baseURL = ClientConstants.FILE_BASE_URL + ClientConstants.SLASH + fileId + ClientConstants.DOWNLOAD;
            feedUtil.CallGetParallel(rangeValue, baseURL, marketplaceId, outputFilename);
        }

    }

}
