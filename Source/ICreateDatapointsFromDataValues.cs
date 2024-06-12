using RaaLabs.Edge.Connectors.OPCUA.Events;

namespace RaaLabs.Edge.Connectors.OPCUA;

public interface ICreateDatapointsFromDataValues
{
    OpcuaDatapointOutput CreateDatapointFrom(NodeValue nodeValue);
    // Log warning if timestamp is old or in the future
}
