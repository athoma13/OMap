using System;

namespace OMap
{
    public interface IMappingConfigurationProvider
    {
        MappingConfiguration GetConfiguration();
    }

    public class MappingConfigurationProvider : IMappingConfigurationProvider
    {
        private readonly Action<ConfigurationBuilder> _builder;
        private readonly Lazy<MappingConfiguration> _configuration;

        public MappingConfigurationProvider(Action<ConfigurationBuilder> builder)
        {
            _builder = builder;
            _configuration = new Lazy<MappingConfiguration>(BuildConfiguration);
        }

        private MappingConfiguration BuildConfiguration()
        {
            var builder = new ConfigurationBuilder();
            _builder(builder);
            return builder.Build();
        }

        public MappingConfiguration GetConfiguration()
        {
            return _configuration.Value;
        }
    }
}