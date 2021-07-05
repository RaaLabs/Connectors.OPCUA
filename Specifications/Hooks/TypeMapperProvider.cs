// Copyright (c) RaaLabs. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using TechTalk.SpecFlow;
using RaaLabs.Edge.Testing;

namespace RaaLabs.Edge.ModuleTemplate.Specs.Hooks
{
    [Binding]
    public sealed class TypeMapperProvider
    {
        private readonly TypeMapping _typeMapping;

        public TypeMapperProvider(TypeMapping typeMapping)
        {
            _typeMapping = typeMapping;
        }

        [BeforeScenario]
        public void SetupTypes()
        {
            _typeMapping.Add("ModuleTemplateHandler", typeof(ModuleTemplateHandler));
            _typeMapping.Add("ModuleTemplateDatapointInput", typeof(Events.ModuleTemplateDatapointInput));
            _typeMapping.Add("ModuleTemplateDatapointOutput", typeof(Events.ModuleTemplateDatapointOutput));
        }
    }
}
