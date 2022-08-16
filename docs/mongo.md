# MongoDB

Tubescan uses MongoDB for data persistance. MongoDB is not included in the Docker image: you'll have to set up a database account separately. 

The data volumes and request frequencies aren't high, so a free [Mongo Atlas](https://www.mongodb.com/atlas/database) account should suit most installations.

Once the account and database are created, the Bot will need:
- network access, so whitelist your server
- database connection, with read/write access.

The Bot will initialise containers and indexes on startup.

