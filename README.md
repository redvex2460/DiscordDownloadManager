# ![DiscoLoader](/images/discoloader.jpg)
DiscoLoader is a discord bot for managing downloads of Jdownloader2

you could add a command via chat, name it and autodownload it.

## Installation

For Unraid it´s an easy installation progress, just head to the Community Applications and search for Discoloader, download and install, after installation and first start go to your appdata folder and set the varaibles of the bot in the **settings.json** file.

For Docker it could be used as follow:

```bash
docker run -v <Path where to store the config file>:/app/config --name DiscoLoader redvex2460/discoloader:latest
```

## Discord Dev API

Head to https://discord.com/developers/applications and create a bot with the name you want - keep in mind to dont name it like the rolename you want to use otherwise you cant add users to that role

Then switch to the OAuth2-Page of that application and add the bot with the following permissions to your server.

### ![Permissions](/images/botsettings.jpg)

## Usage

```python
#links: space seperated links
#name: Name of the Package
#autodownload: true or false
/download [links:] [name:] [autodownload:]

#Checks the downloadstatus of JDownload2
/queryPackages

#Add a user to the DownloadManager-Role
/permit <username>
```

## License
[MIT](https://choosealicense.com/licenses/mit/)