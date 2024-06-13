using Opc.Ua;

namespace RaaLabs.Edge.Connectors.OPCUA;

public record NodeValue(NodeId Node, DataValue Value);
