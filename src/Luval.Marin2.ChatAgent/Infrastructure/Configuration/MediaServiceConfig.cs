using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luval.Marin2.ChatAgent.Infrastructure.Configuration
{
    public class MediaServiceConfig
    {
        /// <summary>
        /// The connection string for the media service.
        /// </summary>
        public string ConnectionString { get; set; } = default!;

        /// <summary>
        /// The provider name for the media service.
        /// </summary>
        public string ProviderName { get; set; } = default!;

        /// <summary>
        /// The container name for the media service.
        /// </summary>
        public string ContainerName { get; set; } = default!;
    }
}
