# Hotsoup
**Home Theater Startup**, latest **release, with binary files**, can be found [here](https://github.com/HebronNor/Hotsoup/releases).

## What is Hotsoup?
Hotsoup lets you configure up to four applications, with custom icons, that will be shown as buttons when starting your home theater media center. If you have a user on [trakt.tv](http://trakt.tv) you can also
show air dates and lists of unwatched episodes for your shows. As well as trending movies and series. A application may be auto started after a configurable delay.

### How did it start?
I wanted something simple to choose between Netflix and Plex upon start-up, and get a short update on the series I was watching. Hotsoup is the love child of that.

### Why the name Hotsoup?
It was the first word that popped into my mind when looking at the full project name; Home Theater Start-up.

## Screenshots
<img src="https://mgpqvq.bn1302.livefilestore.com/y2pEH1AB0voCQcEvnePGl1bWm26dxCPf_mY75zHtMoGNEGxUVVKKVsc_kuo61qW2UtPltn_-vUwT9HRfbsZpfkBvJQT3PqfO9fQVY84cyzrmPE/Hotsoup_screen1.jpg?psid=1" width="400">
&nbsp;&nbsp;
<img src="https://mgpqvq.bn1.livefilestore.com/y2pEsT8zyhLjaFQ6PuSG6q4npSsdXPzpwTlpwfpesCUW10d3Uj6uLXjvatRq6YGLGUCqmkg1Yy3HD-VxtmjSzeGBMKDT9lPD3zom1Fg6JxcADg/Hotsoup_screen2.jpg?psid=1" width="400">

## Installation
No installation is required; simply unpack the release binary files (`Hotsoup_vX.X.X.zip`) to a folder of your choosing.

### Requirements
* Windows 7
* .NET Framework 4.5

## Upgrade
To upgrade your version of Hotsoup; unpack the release binary files (Hotsoup_vX.X.X.zip`) to the folder containing your current version. Some files will be overwritten, but your configuration will not be affected.
Make sure Hotsoup is not running during the upgrade.

## Configuration

### Settings
* **AutoStart**: The application, from the application section, to start after X seconds.
* **Shutdown**: Enable station shutdown, and how many seconds to wait after button is pressed.

### Applications
* **ApplicationKey**: Define up to four applications to show menu buttons for. Key cannot contain spaces, use attribute _Label_ as text to show.

### Trakt.tv
* **Connection**: Turn on/off trakt.tv connection, and how long to wait before timing out.
* **AutoUpdate**: Read from trakt.tv every X second, not just upon start-up.
* **Trending**: Show trending movies and series, and how many.
* **User**: User (with password) to display air dates and unwatched shows for.

## Wallpapers
Hotsoup will use JPG images in the _Wallpapers_ sub-folder as backgrounds, selecting a new one upon each start-up.

## Command-line arguments
* Open application in window with frame: `Hotsoup.exe /WindowMode`
* Use specific configuration file: `Hotsoup.exe full/path/to/configfile.xml`

## Issues
The application is still work in progress so there may be bugs. Please report all bugs to the issue tracker on this repository ([here](https://github.com/HebronNor/Hotsoup/issues)).
