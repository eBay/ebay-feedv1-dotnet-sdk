# Table of contents

* [Features](#features)
* [Usage](#usage)
* [Logging](#logging)
* [License](#license)

# Features

This SDK is intended to provide an easy way to filter for item in the latest feed file for a particular marketplace and category. It downloads the latest file and unzips it. It  filters the feed file for the given itemId and writes the filtered output to a file.
The item feed files provide a rich set of data regarding active ebay listings. The feed files for any supported marketplace can be downloaded through the Feed API V1.

This .NET SDK provides methods such as
* DownloadLatestFile(rangeValue, feedtype, categoryId, marketplaceId, zippedOutputFilename):
  It determines the latest file of the feedtype for the given marketplaceId and categoryId, and
  downloads the file in chunks of rangeValue.<br>
* Unzip(zippedOutputFilename, unzippedOutputFilename): It unzips the downloaded feed file.  <br>
* FilterByItem(itemId, unzippedOutputFilename, filteredOutputFilename): It downloads the latest file, unzips it, filters the feed file
  for the given itemId and writes the filtered output to a file.<br>

There are individual methods as well:
* CallGetFeedTypes : To get the list of feed types
* CallGetFiles : To get the list of files for a given feed type
* CallGetFile : To the file metadata
* CallGetAccess : To get the access configuration
* CallDownloadFile : To download the feed file

For more details on Feed V1 API, please refer to the [documentation](https://developer.ebay.com/api-docs/buy/feed/v1/static/overview.html).

# Usage

**Prerequisites**

```
Net 6
```
# Add the eBay.BuyFeedV1.Client NuGet Package

**Current Version** : 0.0.3

Use of this source code is governed by [Apache-2.0
license](https://opensource.org/licenses/Apache-2.0).If you’re looking
for the latest stable version (0.0.1), you can grab it directly from
NuGet.org.

``` xml
https://www.nuget.org/packages/eBay.BuyFeedV1.Client
```

## NuGet Package Manager UI

- In **Solution Explorer**, right-click NuGet in .csproj and choose
  **Add Package**.

- Search for **eBay.BuyFeedV1.Client**, select that package in the list, and
  click on **Add Package**

- **Accept** the License prompt

## Package Manager Console

- Use the following command in your project directory, to install the
  **eBay.BuyFeedV1.Client** package:

``` xml
Install-Package eBay.BuyFeedV1.Client -Version 0.0.3
```

- After the command completes, open the **.csproj** file to see the
  added reference:

``` xml
<ItemGroup>
   <PackageReference Include="eBay.BuyFeedV1.Client" Version="0.0.3" />
</ItemGroup>
```

## .NET CLI

- Use the following command in your project directory, to install the
  **eBay.BuyFeedV1.Client** package:

``` xml
dotnet add package eBay.BuyFeedV1.Client --version 0.0.3
```

- After the command completes, open the **.csproj** file to see the
  added reference:

``` xml
<ItemGroup>
   <PackageReference Include="eBay.BuyFeedV1.Client" Version="0.0.3" />
</ItemGroup>
```

## Paket CLI

- Use the following command in your project directory, to install the
  **eBay.BuyFeedV1.Client** package:

``` xml
paket add eBay.BuyFeedV1.Client --version 0.0.3
```

- After the command completes, open the **.csproj** file to see the
  added reference:

``` xml
<ItemGroup>
   <PackageReference Include="eBay.BuyFeedV1.Client" Version="0.0.3" />
</ItemGroup>
```
## Create config file
- Create a config file like [example-config.yaml](./examples/example-config.yaml) with your API Credentials.

## Load the config file
- Load the API Credentials using CredentialUtil


# Build eBay.OAuth.Client DLL from Source
- After cloning the project, you can build the package from the source
- **ebay-feedv1-dotnet-client.dll** will be created at
  ebay-feedv1-dotnet-client/bin/Debug/net6.0

# Running the examples
**Configure**

In order to run the example application the [example-config.yaml](./examples/example-config.yaml) needs to be updated.
This config file contains the parameters required to generate the token, in order to make the api call.

```yaml
api.ebay.com:
  appid: <appid>>
  certid: <certid>>
  devid: <devid>>
  redirecturi: <redirect_uri-from-developer-portal>

```
**Build**
You can build the examples from the source

**Run the examples**

To run the examples, run the following command from the examples directory of the repository:

* dotnet test --filter Files_Example

## Logging

Uses standard console logging.

## License

Copyright 2024 eBay Inc.

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

<https://www.apache.org/licenses/LICENSE-2.0>

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
