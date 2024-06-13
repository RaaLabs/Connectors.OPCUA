// Copyright (c) RaaLabs. All rights reserved.
// Licensed under the GPLv2 License. See LICENSE file in the project root for full license information.

using Opc.Ua;

namespace RaaLabs.Edge.Connectors.OPCUA;

public record NodeValue(NodeId Node, DataValue Value);
