# Connectors.OPCUA
[![dotnet build](https://github.com/RaaLabs/Connectors.OPCUA/actions/workflows/dotnet.yml/badge.svg)](https://github.com/RaaLabs/Connectors.OPCUA/actions/workflows/dotnet.yml)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=RaaLabs_Connectors.OPCUA&metric=sqale_rating&token=237aec8269dd7b80a5ef37b10b858152b085720e)](https://sonarcloud.io/dashboard?id=RaaLabs_Connectors.OPCUA)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=RaaLabs_Connectors.OPCUA&metric=coverage&token=237aec8269dd7b80a5ef37b10b858152b085720e)](https://sonarcloud.io/dashboard?id=RaaLabs_Connectors.OPCUA)

This document describes the Connectors.OPCUA module for RaaLabs Edge.

## What does it do?
The connector reads nodes from a OPC UA server. A node is identified by a node id, consisting of a namespace index and an identifier, e.g. `ns=3;i=1002`.

The connector are producing events of type [OutputName("output")] and should be routed to [IdentityMapper](https://github.com/RaaLabs/IdentityMapper).

## Configuration
The module is configured using a JSON file. `connector.json` represents the connection to the TCP or UDP stream using IP and port. The following example json works with the Prosys OPC UA Simulation Server.

```json
{
    "serverUrl": "opc.tcp://<HOST>:<PORT>/<SERVERNAME>",
    "publishingIntervalSeconds": 1.0,
    "nodes": [
        {
            "nodeId": "ns=3;i=1002",
            "subscribeIntervalSeconds": 1.0,
            "readIntervalSeconds": 1.0
        },
        {
            "nodeId": "ns=3;i=1001",
            "readIntervalSeconds": 1.0
        }
    ]
}
```

## Testing using Prosys OPC UA Simulation Server
Prosys offers a free simulation server, which can be downloaded here: <https://downloads.prosysopc.com/opc-ua-simulation-server-downloads.php>.

The OPC UA server starts automatically once you launch the application. Under `Objects`, you can browse the nodes present in the server, and navigating and clicking the individual nodes, you can find the nodes `NodeId`, which you can use in the `configuration.json` to test the connector using the simulator.

The simulator also displays the server url (connection address) once you start the simulator.
