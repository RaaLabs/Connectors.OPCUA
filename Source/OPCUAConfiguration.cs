// Copyright (c) RaaLabs. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using RaaLabs.Edge.Modules.Configuration;


namespace RaaLabs.Edge.Connectors.OPCUA
{
    [Name("configuration.json")]
    [RestartOnChange]
    public class OPCUAConfiguration : IConfiguration
    {
        public int SampleConfigValue { get; set; }
    }
}
