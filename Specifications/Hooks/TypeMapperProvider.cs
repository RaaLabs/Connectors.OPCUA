// Copyright (c) RaaLabs. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using TechTalk.SpecFlow;
using RaaLabs.Edge.Testing;
using RaaLabs.Edge.Connectors.OPCUA.Events;

namespace RaaLabs.Edge.Connectors.OPCUA.Specs.Hooks
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
            _typeMapping.Add("OPCUAHandler", typeof(OPCUAHandler));
            _typeMapping.Add("OPCUADatapointInput", typeof(OPCUADatapointInput));
            _typeMapping.Add("OPCUADatapointOutput", typeof(OPCUADatapointOutput));
        }
    }
}
