# Connectors.OPCUA
[![.NET 5.0](https://github.com/RaaLabs/Connectors.OPCUA/actions/workflows/dotnet.yml/badge.svg?branch=main)](https://github.com/RaaLabs/Connectors.OPCUA/actions/workflows/dotnet.yml)

This document describes the Connectors.OPCUA module for RaaLabs Edge.

## What does it do?
The connector reads nodes from a OPC UA server. A node is identified by a node id, consisting of a namespace index and an identifier, e.g. `ns=3;i=1002`.

The connector are producing events of type [OutputName("output")] and should be routed to [IdentityMapper](https://github.com/RaaLabs/IdentityMapper).

## Configuration
The module is configured using a JSON file. `connector.json` represents the connection to the TCP or UDP stream using IP and port. The following example json works with the Prosys OPC UA Simulation Server.

```json
{
    "serverUrl": "opc.tcp://Rafaels-MacBook-Pro.local:53530/OPCUA/SimulationServer",
    "nodeIds": [
        "ns=3;i=1002",
        "ns=3;i=1001"
    ]
}
```

## IoT Edge Deployment

### $edgeAgent

In your `deployment.json` file, you will need to add the module. For more details on modules in IoT Edge, go [here](https://docs.microsoft.com/en-us/azure/iot-edge/module-composition).

The module has persistent state and it is assuming that this is in the `config` folder relative to where the binary is running.
Since this is running in a containerized environment, the state is not persistent between runs. To get this state persistent, you'll
need to configure the deployment to mount a folder on the host into the config folder.

In your `deployment.json` file where you added the module, inside the `HostConfig` property, you should add the volume binding.

```json
"Mounts": [
   {
        "Type": "volume",
        "Source": "raalabs-config-opcua",
        "Target": "/app/config",
        "RW": false
    }
]
```

```json
{
    "modulesContent": {
        "$edgeAgent": {
            "properties.desired.modules.OPCUA": {
                "settings": {
                    "image": "<repo-name>/connectors-opcua:<tag>",
                    "createOptions": {
                        "HostConfig": {
                            "Mounts": [
                                {
                                    "Type": "volume",
                                    "Source": "raalabs-config-opcua",
                                    "Target": "/app/config",
                                    "RW": false
                                }]}
                },
                "type": "docker",
                "version": "1.0",
                "status": "running",
                "restartPolicy": "always"
            }
        }
    }
}
```

### $edgeHub

The routes in edgeHub can be specified like the example below.

```json
{
    "$edgeHub": {
        "properties.desired.routes.OPCUAToIdentityMapper": "FROM /messages/modules/OPCUA/outputs/output INTO BrokeredEndpoint(\"/modules/IdentityMapper/inputs/events\")",
    }
}
```


## Testing using Prosys OPC UA Simulation Server
Prosys offers a free simulation server, which can be downloaded here: <https://downloads.prosysopc.com/opc-ua-simulation-server-downloads.php>.

The OPC UA server starts automatically once you launch the application. Under `Objects`, you can browse the nodes present in the server, and navigating and clicking the individual nodes, you can find the nodes `NodeId`, which you can use in the `configuration.json` to test the connector using the simulator.

The simulator also displays the server url (connection address) once you start the simulator.