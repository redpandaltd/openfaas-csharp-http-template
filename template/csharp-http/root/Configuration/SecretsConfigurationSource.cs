using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Configuration.OpenFaaSSecrets
{
    internal class SecretsConfigurationSource : IConfigurationSource
    {
        public IConfigurationProvider Build( IConfigurationBuilder builder )
        {
            return new SecretsConfigurationProvider();
        }
    }
}
