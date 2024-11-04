# CrabGameOverseer
A small BepInEx mod for Crab Game adds some moderation commands.

Depends on [CustomCommands](https://github.com/lammas321/CrabGameCustomCommands/) and [PersistentData](https://github.com/lammas321/CrabGamePersistentData)

## What are the commands
There is warn, kick, ban, and unban, all taking a required player argument and an optional reason (except for unban which doesn't need a reason).

When a player is kicked or banned, they will be killed just before being kicked or banned, to fix an issue where they could continue to effect the game afterwards for a bit of time.

When a player is banned, their reason will be saved to their client data file, and will remain permanently banned from your future lobbies unless you unban them.
