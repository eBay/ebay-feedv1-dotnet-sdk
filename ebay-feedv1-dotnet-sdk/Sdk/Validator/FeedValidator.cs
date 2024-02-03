
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
using System.Net.Http;

using eBay.Sdk.Exceptions;

namespace eBay.Sdk.Validator
{
    public class FeedValidator
    {
        public Boolean ValidateResponse(HttpResponseMessage responseMsg)
        {
            if (responseMsg == null)
            {
                Console.WriteLine("No response for this feedtype from the API");
                return false;
            }
            if (responseMsg.Content == null)
            {
                Console.WriteLine("No response content for this feedtype from the API");
                return false;
            }
            if (responseMsg.Content.ReadAsStringAsync().Result.Contains("errors"))
            {
                Console.WriteLine("Errors in the response ");
                Console.WriteLine("Response "+responseMsg.Content.ReadAsStringAsync().Result);

                return false;
            }
            return true;
        }

        public void ValidateRequest(string feedType, string categoryId, string marketplaceId)
        {

            ValidateFeedType(feedType);
            ValidateMarketplace(marketplaceId);
            ValidateCategoryId(categoryId);
        }

        public void ValidateFileId(string fileId)
        {

            if (fileId == null)
            {
                var message = "Missing fileId in the request";
                throw new ClientRequestException(message);
            }
        }

        public void ValidateSearchText(string searchText)
        {

            if (searchText == null)
            {
                var message = "Missing searchText in the request";
                throw new ClientRequestException(message);
            }
        }

        public void ValidateData(List<string> values)
        {

            if (values == null)
            {
                var message = "Missing values in the request";
                throw new ClientRequestException(message);
            }
        }

        private void ValidateFeedType(string feedType)
        {
            if (feedType == null)
            {
                var message = "Missing feedType in the request";
                throw new ClientRequestException(message);
            }
        }

        private Boolean ValidateMarketplace(String marketplaceId)
        {
            if (marketplaceId == null)
            {
                throw new ClientRequestException("Missing marketplaceId in the request");
            }
            return true;
        }

        public static Boolean ValidateCategoryId(String categoryId)
        {
            if (categoryId == null)
            {
                throw new ClientRequestException("Missing categoryId in the request");
            }
            return true;
        }
    }
}