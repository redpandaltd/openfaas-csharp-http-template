using System;
using System.IO;

namespace Microsoft.Extensions.Configuration.OpenFaaSSecrets
{
    internal class SecretsConfigurationProvider : ConfigurationProvider
    {
        public override void Load()
        {
            try
            {
                var secrets = Directory.GetFiles( "/var/openfaas/secrets/" );

                foreach ( var secret in secrets )
                {
                    var secretName = string.Concat( "openfaas_secret_", Path.GetFileName( secret ) );
                    var secretValue = File.ReadAllBytes( secret );

                    Data.Add( secretName, Convert.ToBase64String( secretValue ) );
                }
            }
            catch ( Exception )
            { }
        }
    }
}
