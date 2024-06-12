using Opc.Ua;

namespace RaaLabs.Edge.Connectors.OPCUA;

public record NodeValue(string NodeId, DataValue Value);
