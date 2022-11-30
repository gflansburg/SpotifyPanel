# Spotify Panel

Display a VR compatibale Spotify in-game remote in a Microsoft Flight Simulator 2020 toolbar panel.

![Thumbnail](https://github.com/gflansburg/SpotifyPanel/blob/master/SpotifyPanel/Client/gafware-ingamepanel-spotify/PackageDefinitions/gafware-ingamepanel-spotify/ContentInfo/Thumbnail.jpg?raw=true)

## Installing

1. Download the latest package from the [releases](https://github.com/gflansburg/SpotifyPanel/releases) page.
2. Extract the contents of the "Client" folder into your MSFS 2020 Community folder.
3. Extract the contents of the "Server" folder into a location of your choosing.
4. Run SpotifyPanel.exe and click on the Settings icon. You will need to provide your own Spotify developer keys.
5. Visit the [Spotify Developer Dashboard](https://developer.spotify.com/dashboard/) to create or find your keys. Ensure that your Spotify application has a Redirect URI for http://localhost:52000 and http://localhost:52000/ (yes, you need one with and without the trailing slash). This URI is used to pass the Authentication Token from Spotify back to Spotify Panel.
6. (Optional) Add SpotifyPanel.exe to your EXE.xml so that it automatically launches when you run MSFS 2020.

## Running
Spotify Panel will run in the system tray. Right click on the system tray icon to show or close the server application. Additionaly, you may double-click on the system tray icon to show it. Once Spotify Panel is configured with your Spotify developer keys, subsequent launches of Spotify Panel will load minimized into the system tray. After entering your Spotify developer keys, Spotify Panel will open an embedded browser in order for you to authenticate and give permissions for it to pull information from your Spotify account. After that, refreshing the token should happen without any user intervention.

## F.A.Q.
Q. MSFS 2020 panels use HTML natively. Why do I need a server application?  
A. MSFS 2020 uses iFrames in their panels to serve up HTML. While this would be ideal, Spotify, in their infinite wisdom, has decided to prevent authentication from within iFrames. Perhaps someone smarter than me can explain the security concern they have.

Q. Why do I need to provide my Secret ID as well as my Client ID?  
A. By default Spotify authentication will pass the authorization token back to your web browser via a URL hash. Because we can't authenticate from within MSFS, because of the aforementioned iFrame issue, we have to get the authentication from a URL parameter. Since URL hashes are not visible to the server we must provice the Secret ID so that Spotify passes the token back as a standard URL parameter.

Q. What port does Spotify Panel use?  
A. 5150

Q. If I forward port 5150 from my router and open an incoming firewall rule, can an external browser access my data?  
A. No. Binding is to localhost (127.0.0.1) only.