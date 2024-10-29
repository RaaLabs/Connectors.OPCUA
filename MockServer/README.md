# opcua-test-server

````bash
docker build -f Dockerfile.Server -t opcua-test-server .
docker build -f Dockerfile.Client -t opcue-test-client .

docker network create my-net
docker run --name opcuaserver --network my-net -p 4840:4840 docker.io/library/opcua-test-server
docker run --network my-net docker.io/library/opcua-test-client
````

Config for OPCUA connector (the host used in the serverUrl must match the name given to the Docker container):
````json
{
    "serverUrl": "opc.tcp://opcua-test-server:4840/freeopcua/server/",
    "publishingIntervalSeconds": 1.0,
    "nodes": [
        {
            "nodeId": "ns=2;i=2",
            "subscribeIntervalSeconds": 1.0,
            "readIntervalSeconds": 10.0
        }
    ]
}
````
