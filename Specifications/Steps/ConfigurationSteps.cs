using BoDi;
using TechTalk.SpecFlow;
using RaaLabs.Edge.Connectors.OPCUA;


namespace RaaLabs.Edge.Connectors.OPCUA.Specs.Steps
{
    [Binding]
    public sealed class ConfigurationSteps
    {
        private readonly IObjectContainer _container;

        public ConfigurationSteps(IObjectContainer container)
        {
            _container = container;
        }

        [Given(@"(.*) as sample config value")]
        public void GivenThePrioritizedTags(string sampleConfigValue)
        {
            var OPCUAConfiguration = new OPCUAConfiguration
            {
                SampleConfigValue = int.Parse(sampleConfigValue)
            };
            _container.RegisterInstanceAs<OPCUAConfiguration>(OPCUAConfiguration);
        }
    }
}