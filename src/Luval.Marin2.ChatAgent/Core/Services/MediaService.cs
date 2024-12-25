﻿using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Luval.Marin2.ChatAgent.Infrastructure.Configuration;
using Luval.Marin2.ChatAgent.Infrastructure.Data;
using Luval.Marin2.ChatAgent.Infrastructure.Interfaces;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luval.Marin2.ChatAgent.Core.Services
{
    /// <summary>
    /// Service for handling media operations such as uploading files to blob storage.
    /// </summary>
    public class MediaService : IMediaService
    {
        private readonly MediaServiceConfig _config;
        private readonly ILogger<MediaService> _logger;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly BlobContainerClient _blobContainerClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaService"/> class.
        /// </summary>
        /// <param name="config">The configuration settings for the media service.</param>
        /// <param name="logger">The logger instance for logging information.</param>
        /// <exception cref="ArgumentNullException">Thrown when config or logger is null.</exception>
        public MediaService(MediaServiceConfig config, ILogger<MediaService> logger)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _blobServiceClient = new BlobServiceClient(_config.ConnectionString);
            _blobContainerClient = _blobServiceClient.GetBlobContainerClient(_config.ContainerName);
        }

        /// <summary>
        /// Uploads a media file to the blob storage.
        /// </summary>
        /// <param name="content">The byte array content of the media file.</param>
        /// <param name="fileName">The name of the file to be uploaded.</param>
        /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the media file information.</returns>
        /// <exception cref="ArgumentNullException">Thrown when content or fileName is null.</exception>
        /// <exception cref="Exception">Thrown when an error occurs during the upload process.</exception>
        public async Task<MediaFileInfo> UploadMediaAsync(byte[] content, string fileName, CancellationToken cancellationToken)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            return await UploadMediaAsync(new System.IO.MemoryStream(content), fileName, cancellationToken);
        }


        /// <summary>
        /// Uploads a media file to the blob storage.
        /// </summary>
        /// <param name="stream">The stream content of the media file.</param>
        /// <param name="fileName">The name of the file to be uploaded.</param>
        /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the media file information.</returns>
        /// <exception cref="ArgumentNullException">Thrown when stream or fileName is null.</exception>
        /// <exception cref="ArgumentException">Thrown when stream is not readable.</exception>
        /// <exception cref="Exception">Thrown when an error occurs during the upload process.</exception>
        public async Task<MediaFileInfo> UploadMediaAsync(Stream stream, string fileName, CancellationToken cancellationToken)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(nameof(fileName));
            if (!stream.CanRead)
            {
                _logger.LogError("Stream must be readable");
                throw new ArgumentException("Stream must be readable", nameof(stream));
            }
            try
            {
                var providerFileName = Guid.NewGuid().ToString().Replace("-", "").ToUpper();
                _logger.LogInformation("Starting upload of file {FileName}", fileName);

                _blobContainerClient.GetBlobClient(providerFileName);
                await _blobContainerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
                var blobClient = _blobContainerClient.GetBlobClient(fileName);
                var res = await blobClient.UploadAsync(stream, true, cancellationToken);
                var props = await blobClient.GetPropertiesAsync(cancellationToken: cancellationToken);

                _logger.LogInformation("Successfully uploaded file {FileName}", fileName);

                return new MediaFileInfo
                {
                    FileName = blobClient.Name,
                    ProviderFileName = providerFileName,
                    Uri = blobClient.Uri,
                    ContentMD5 = Convert.ToBase64String(res.Value.ContentHash),
                    ContentType = props.Value.ContentType
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while uploading file {FileName}", fileName);
                throw;
            }
        }

        /// <summary>
        /// Asynchronously gets the public URL for a media file.
        /// </summary>
        /// <param name="providerFileName">The provider file name of the media file.</param>
        /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the public URL of the media file.</returns>
        /// <exception cref="ArgumentNullException">Thrown when providerFileName is null or empty.</exception>
        /// <exception cref="Exception">Thrown when an error occurs while generating the public URL.</exception>
        public Task<string> GetPublicUrlAsync(string providerFileName, CancellationToken cancellationToken)
        {
            return Task.Run(() => GetPublicUrl(providerFileName), cancellationToken);
        }

        private string GetPublicUrl(string providerFileName)
        {
            if (string.IsNullOrEmpty(providerFileName))
            {
                _logger.LogError("Provider file name is null or empty");
                throw new ArgumentNullException(nameof(providerFileName), "Provider file name cannot be null or empty");
            }

            try
            {
                var blobClient = _blobContainerClient.GetBlobClient(providerFileName);
                var sasBuilder = new BlobSasBuilder
                {
                    BlobContainerName = _config.ContainerName,
                    BlobName = providerFileName,
                    Resource = "b",
                    StartsOn = DateTimeOffset.UtcNow,
                    ExpiresOn = DateTimeOffset.UtcNow.Add(_config.SASExpiration)
                };
                sasBuilder.SetPermissions(BlobSasPermissions.Read);
                var keyCredential = new Azure.Storage.StorageSharedKeyCredential(_blobContainerClient.AccountName, ExtractAccountKey(_config.ConnectionString));
                var sasToken = sasBuilder.ToSasQueryParameters(keyCredential).ToString();
                var uriBuilder = new UriBuilder(blobClient.Uri)
                {
                    Query = sasToken
                };
                return uriBuilder.Uri.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while generating public URL for file {ProviderFileName}", providerFileName);
                throw;
            }
        }

        public string ExtractAccountKey(string connectionString)
        {
            // Split the connection string into key-value pairs
            string[] pairs = connectionString.Split(';');
            Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();

            foreach (var pair in pairs)
            {
                if (!string.IsNullOrEmpty(pair))
                {
                    var keyValue = pair.Split('=', 2);
                    if (keyValue.Length == 2)
                    {
                        keyValuePairs[keyValue[0]] = keyValue[1];
                    }
                }
            }

            // Retrieve the AccountKey
            if (keyValuePairs.TryGetValue("AccountKey", out string accountKey))
            {
                return accountKey;
            }
            throw new Exception("AccountKey not found.");
        }
    }
}
