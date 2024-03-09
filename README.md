# tubescan
A Discord bot for TfL tube data

[![Build & Release](https://github.com/tonycknight/tubescan/actions/workflows/build.yml/badge.svg)](https://github.com/tonycknight/tubescan/actions/workflows/build.yml)

# Features

* Get overall lines' status

* Get a station's crowding and arrival information

A full list of features and commands is [here](./docs/bot_actions.md).

# Getting Started

## Build

You'll the .Net 8 SDK installed


```
dotnet tool restore
dotnet restore
dotnet build
```


## Bot Configuration

Before you start, you'll need to first set up [Mongo database](/docs/mongo.md), obtain a [TfL App key](./docs/tfl.md) and set up a [Discord bot](/docs/discord_config.md).


## Docker image

An Ubuntu image is [prepared every release](https://github.com/users/tonycknight/packages/container/package/tubescan).

To install:

``docker pull ghcr.io/tonycknight/tubescan:<latest tag>``

To run:

```
docker run -it --rm   -e TubeScan_Discord_DiscordClientId=<Discord bot Client ID> \
                        -e TubeScan_Discord_DiscordClientToken=<Discord bot Client Token> \
                        -e TubeScan_Mongo_Connection=<Mongo DB connection string> \
                        -e TubeScan_Mongo_DatabaseName=<Mongo DB database name> \
                        -e TubeScan_Tfl_AppKey=<TFL bot app key>  
                        ghcr.io/tonycknight/tubescan:<image tag>
```
