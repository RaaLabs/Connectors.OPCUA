using BoDi;
using System.Linq;
using TechTalk.SpecFlow;
using System.Globalization;

namespace RaaLabs.Edge.ModuleTemplate.Specs.Steps
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
            var moduleTemplateConfiguration = new ModuleTemplateConfiguration
            {
                SampleConfigValue = int.Parse(sampleConfigValue)
            };
            _container.RegisterInstanceAs<ModuleTemplateConfiguration>(moduleTemplateConfiguration);
        }
    }
}