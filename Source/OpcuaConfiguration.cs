// Copyright (c) RaaLabs. All rights reserved.
// Licensed under the GPLv2 License. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using RaaLabs.Edge.Modules.Configuration;

namespace RaaLabs.Edge.Connectors.OPCUA;

[Name("configuration.json")]
[RestartOnChange]
[ExcludeFromCodeCoverage]
public class OpcuaConfiguration : IConfiguration
{
    public string ServerUrl { get; }
    public ISet<string> NodeIds { get; }
    public string OpcUaServerCertificateIssuer { get; }
    public string OpcUaServerCertificateSubject { get;}
    public bool OpcUaServerAutoAcceptUntrustedCertificates { get; }

    public OpcuaConfiguration(string serverUrl, ISet<string> nodeIds, string opcUaServerCertificateIssuer, string opcUaServerCertificateSubject, bool opcUaServerAutoAcceptUntrustedCertificates = false)
    {
        ServerUrl = serverUrl;
        NodeIds = nodeIds;
        OpcUaServerCertificateIssuer = opcUaServerCertificateIssuer;
        OpcUaServerCertificateSubject = opcUaServerCertificateSubject;
        OpcUaServerAutoAcceptUntrustedCertificates = opcUaServerAutoAcceptUntrustedCertificates;
    }
}
