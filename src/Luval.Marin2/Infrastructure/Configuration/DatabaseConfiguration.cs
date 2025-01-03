using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Luval.Marin2.Infrastructure.Configuration
{
    /// <summary>
    /// Provides methods to check various database configurations.
    /// </summary>
    public class DatabaseConfiguration
    {
        /// <summary>
        /// Checks if the specified configuration file exists in the Azure storage container.
        /// </summary>
        /// <param name="azureConnectionString">The Azure connection string.</param>
        /// <param name="container">The container name where the configuration file is stored. Default is "config".</param>
        /// <param name="fileName">The name of the configuration file. Default is "marin2.json".</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean value indicating whether the configuration file exists.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the azureConnectionString is null or empty.</exception>
        public static async Task<bool> CheckIfExistsAsync(string azureConnectionString, string container = "config", string fileName = "marin2.json")
        {
            return await GetPropertyValue(azureConnectionString, "checkIfExists", true, container, fileName);
        }

        /// <summary>
        /// Checks if the specified configuration file exists in the provided options.
        /// </summary>
        /// <param name="options">The configuration options as a JsonObject.</param>
        /// <param name="container">The container name where the configuration file is stored. Default is "config".</param>
        /// <param name="fileName">The name of the configuration file. Default is "marin2.json".</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean value indicating whether the configuration file exists.</returns>
        public static async Task<bool> CheckIfExistsAsync(JsonObject options, string container = "config", string fileName = "marin2.json")
        {
            return await GetPropertyValue(options, "checkIfExists", true, container, fileName);
        }

        /// <summary>
        /// Checks if the database should be deleted based on the configuration file.
        /// </summary>
        /// <param name="azureConnectionString">The Azure connection string.</param>
        /// <param name="container">The container name where the configuration file is stored. Default is "config".</param>
        /// <param name="fileName">The name of the configuration file. Default is "marin2.json".</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean value indicating whether the database should be deleted.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the azureConnectionString is null or empty.</exception>
        public static async Task<bool> CheckIfDeleteAsync(string azureConnectionString, string container = "config", string fileName = "marin2.json")
        {
            return await GetPropertyValue(azureConnectionString, "deleteDatabase", false, container, fileName);
        }

        /// <summary>
        /// Checks if the database should be deleted based on the provided options.
        /// </summary>
        /// <param name="options">The configuration options as a JsonObject.</param>
        /// <param name="container">The container name where the configuration file is stored. Default is "config".</param>
        /// <param name="fileName">The name of the configuration file. Default is "marin2.json".</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean value indicating whether the database should be deleted.</returns>
        public static async Task<bool> CheckIfDeleteAsync(JsonObject options, string container = "config", string fileName = "marin2.json")
        {
            return await GetPropertyValue(options, "deleteDatabase", false, container, fileName);
        }

        /// <summary>
        /// Checks if the database migration should be performed based on the configuration file.
        /// </summary>
        /// <param name="azureConnectionString">The Azure connection string.</param>
        /// <param name="container">The container name where the configuration file is stored. Default is "config".</param>
        /// <param name="fileName">The name of the configuration file. Default is "marin2.json".</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean value indicating whether the database migration should be performed.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the azureConnectionString is null or empty.</exception>
        public static async Task<bool> CheckIfDoMigrationAsync(string azureConnectionString, string container = "config", string fileName = "marin2.json")
        {
            return await GetPropertyValue(azureConnectionString, "doMigration", false, container, fileName);
        }

        /// <summary>
        /// Checks if the database migration should be performed based on the provided options.
        /// </summary>
        /// <param name="options">The configuration options as a JsonObject.</param>
        /// <param name="container">The container name where the configuration file is stored. Default is "config".</param>
        /// <param name="fileName">The name of the configuration file. Default is "marin2.json".</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean value indicating whether the database migration should be performed.</returns>
        public static async Task<bool> CheckIfDoMigrationAsync(JsonObject options, string container = "config", string fileName = "marin2.json")
        {
            return await GetPropertyValue(options, "doMigration", false, container, fileName);
        }

        /// <summary>
        /// Gets the application owner's email from the configuration file.
        /// </summary>
        /// <param name="azureConnectionString">The Azure connection string.</param>
        /// <param name="container">The container name where the configuration file is stored. Default is "config".</param>
        /// <param name="fileName">The name of the configuration file. Default is "marin2.json".</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the owner's email as a string.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the azureConnectionString is null or empty.</exception>
        public static async Task<string> GetAppOwnerEmail(string azureConnectionString, string container = "config", string fileName = "marin2.json")
        {
            return await GetPropertyValue(azureConnectionString, "ownerEmail", "oscar.marin.saenz@gmail.com", container, fileName);
        }

        /// <summary>
        /// Gets the application owner's email from the provided options.
        /// </summary>
        /// <param name="options">The configuration options as a JsonObject.</param>
        /// <param name="container">The container name where the configuration file is stored. Default is "config".</param>
        /// <param name="fileName">The name of the configuration file. Default is "marin2.json".</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the owner's email as a string.</returns>
        public static async Task<string> GetAppOwnerEmail(JsonObject options, string container = "config", string fileName = "marin2.json")
        {
            return await GetPropertyValue(options, "ownerEmail", "oscar.marin.saenz@gmail.com", container, fileName);
        }

        private static async Task<T> GetPropertyValue<T>(string azureConnectionString, string propertyName, T defaultValue, string container = "config", string fileName = "marin2.json")
        {
            if (string.IsNullOrEmpty(azureConnectionString))
                throw new ArgumentNullException(nameof(azureConnectionString), "Azure connection string cannot be null or empty.");

            var options = await WebConfiguration.GetObjectAsync(azureConnectionString, container, fileName);
            return await GetPropertyValue(options, propertyName, defaultValue, container, fileName);
        }

        private static async Task<T> GetPropertyValue<T>(JsonObject options, string propertyName, T defaultValue, string container = "config", string fileName = "marin2.json")
        {
            if (options == null) return defaultValue;
            if (options["Database"] != null && options["Database"][propertyName] != null)
            {
                return options["Database"][propertyName].GetValue<T>();
            }
            return defaultValue;
        }
    }
}
