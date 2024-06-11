// Copyright (c) RaaLabs. All rights reserved.
// Licensed under the GPLv2 License. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using RaaLabs.Edge.Modules.Configuration;

namespace RaaLabs.Edge.Connectors.OPCUA;

[Name("configuration.json"), RestartOnChange, ExcludeFromCodeCoverage]
public class OpcuaConfiguration : IConfiguration
{
    public string ServerUrl { get; init; }
    public ISet<string> NodeIds { get; init; }
}
