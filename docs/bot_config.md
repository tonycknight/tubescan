# Configuration

You can configure the bot either through environment variables or a configuration file.

* [One-off TfL configuration](./tfl.md)
* [Discord Bot configuration](./discord_config.md)


## Environment variables

Environment variables may be used, for instance injection into a Docker container.

| Variable Name | Value |
| - | - |
| ``TubeScan_Discord_DiscordClientId`` | The bot's client ID |
| ``TubeScan_Discord_DiscordClientToken`` | The bot's client token|
| ``TubeScan_Mongo_Connection`` | Connection string to Mongo |
| ``TubeScan_Mongo_DatabaseName`` | The Mongo database name |
| ``TubeScan_Tfl_AppKey`` | The Tfl Application Key |

To use environment variables, start the bot without the ``-c`` option: 

``.\TubeScan.exe start``

## Configuration file

The bot can be configured by a single JSON configuration file. The file's schema is:

```json
{
  "discord": {
    "clientToken": "<bot token>",
    "clientId": "<client id>"
  },
  "mongoDb": {
    "connection": "<mongo DB connection string>",
    "databaseName": "<database name>"
  },
  "tfl": {
    "appKey": "<tfl app key>"
  },
  "telemetry": {
    "logMessageContent": false
  }
}
```

The configuration file's path is provided by the ``-c`` start option: 

``.\TubeScan.exe start -c <path to config file>``

