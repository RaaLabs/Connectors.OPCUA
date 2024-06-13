// Copyright (c) RaaLabs. All rights reserved.
// Licensed under the GPLv2 License. See LICENSE file in the project root for full license information.

using RaaLabs.Edge.Connectors.OPCUA.Events;

namespace RaaLabs.Edge.Connectors.OPCUA;

public interface ICreateDatapointsFromDataValues
{
    OpcuaDatapointOutput CreateDatapointFrom(NodeValue nodeValue);
}
