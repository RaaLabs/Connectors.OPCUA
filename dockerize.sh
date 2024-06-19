# Docker container needs to be build with `docker build` command, because Docker compose does not
# allow secrets from environmental variables to be passed to the build context

export NUGET_GITHUB_PACKAGES_USERNAME=
export NUGET_GITHUB_PACKAGES_TOKEN= # GitHub token needs to have read right for packages 

DOCKER_BUILDKIT=1 docker build \
    --secret id=NUGET_GITHUB_PACKAGES_USERNAME,env=NUGET_GITHUB_PACKAGES_USERNAME \
    --secret id=NUGET_GITHUB_PACKAGES_TOKEN,env=NUGET_GITHUB_PACKAGES_TOKEN \
    -f Source/Dockerfile -t connectors-opcua:test .
