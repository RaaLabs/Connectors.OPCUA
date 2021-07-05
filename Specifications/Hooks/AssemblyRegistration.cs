// Copyright (c) RaaLabs. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using RaaLabs.Edge.Testing;
using TechTalk.SpecFlow;

namespace RaaLabs.Edge.ModuleTemplate.Specs.Hooks
{
    [Binding]
    class AssemblyRegistration
    {
        private readonly ComponentAssemblies _assemblies;

        public AssemblyRegistration(ComponentAssemblies assemblies)
        {
            _assemblies = assemblies;
        }

        [BeforeScenario]
        private void RegisterAssembly()
        {
            _assemblies.Add(GetType().Assembly);
        }
    }
}
