using Azure.Storage.Blobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Luval.Marin2.Infrastructure.Configuration
{
    /// <summary>
    /// Provides the ability to download a configuration file from a cloud storage location
    /// </summary>
    public class WebConfiguration
    {
        /// <summary>
        /// Asynchronously retrieves a JSON file from an Azure Blob Storage container, parses its content,
        /// and returns it as a JsonObject.
        /// </summary>
        /// <param name="connectionString">The connection string to the Azure Blob Storage account.</param>
        /// <param name="container">The name of the Blob Storage container.</param>
        /// <param name="fileName">The name of the file in the container to retrieve.</param>
        /// <returns>A Task representing the asynchronous operation, containing the file's content as a JsonObject.</returns>
        /// <exception cref="ArgumentException">Thrown when any input parameter is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the file content cannot be parsed into a JsonObject.</exception>
        /// <exception cref="Exception">Thrown for any other errors encountered during the operation.</exception>
        public static async Task<JsonObject> GetObjectAsync(string connectionString, string container = "config", string fileName = "marin2.json")
        {
            // Input validation
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString));
            }

            if (string.IsNullOrWhiteSpace(container))
            {
                throw new ArgumentException("Container name cannot be null or empty.", nameof(container));
            }

            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("File name cannot be null or empty.", nameof(fileName));
            }

            try
            {
                // Create a BlobServiceClient to access the Blob Storage account
                var blobServiceClient = new BlobServiceClient(connectionString);

                // Get a reference to the container
                var blobContainerClient = blobServiceClient.GetBlobContainerClient(container);

                // Get a reference to the blob (file) in the container
                var blobClient = blobContainerClient.GetBlobClient(fileName);

                string content = null;
                // Check if the blob exists
                if (!await blobClient.ExistsAsync())
                {
                    content = "{}";
                }
                else
                {
                    // Download the blob content as a string
                    var downloadInfo = await blobClient.DownloadContentAsync();
                    content = downloadInfo.Value.Content.ToString();
                }

                // Parse the content into a JsonObject
                var jsonObject = JsonSerializer.Deserialize<JsonObject>(content);

                if (jsonObject == null)
                {
                    throw new InvalidOperationException("The content of the file could not be parsed into a valid JsonObject.");
                }

                return jsonObject;
            }
            catch (Exception ex)
            {
                // Log or rethrow exception as necessary
                throw new Exception($"An error occurred while retrieving the file '{fileName}' from the container '{container}': {ex.Message}", ex);
            }
        }
    }
}
