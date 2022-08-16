# Discord Bot commands

## Getting Started

``help`` Get command help

``start`` Start a private conversation with the Bot, and get continual line status notifications.

``about`` Get the bot's version & 3rd party copyright notices.

## Lines

``lines`` Get the current status of all Tube lines.

Once you've started a private conversation with the bot, you'll receive continual line status notifications to your private channel.

## Station tags

The bot commands use tags to identify stations. These tags are entirely specific to users: one user's ``home`` station is another user's ``work`` station, etc. 

``tags`` Show the user's station tags and their respective stations.

``tag <tag> <station search>`` Set a tag ``<tag>`` for a given ``<station>``.

``-tag <tag>`` Remove the ``<tag>`` tag.

Before using station status commands, set up the stations you need as tags.

## Station status

``station <station tag>`` Get the station's crowding status and the next arrivals per destination.

