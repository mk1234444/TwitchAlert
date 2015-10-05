# TwitchAlert

Simple early(is) version of app that shows a popup whenever any user you follow goes Online/Offline.
(Online/Offline as in streaming)

Notes:
  Drag the popup from left to right to position it where on the horizontal you want it to appear (I drag it to the centre)
  - it will then popup from bottom/centre. The Horizontal postion is saved.
  
  Right-click the notify icon and select Change Username to add your Twitch username.
  If you add an invalid username then, at the moment, it will just be ignored (there is no MessageBox or anything to indicate
  failure yet).
  
  If you mouse-over the notify-icon it will display your username (if it was valid), the number of people you follow
  and how many of them are online.
  
  The first run of this will be slow depending on whether you follow a lot of people or not. This is because currently 
  the app retrieves all of the thumbnails of your followers. It caches these thumbnails to disk so subsequent runs
  won't have this delay (unless you change the username).
  
  The App polls Twitch every 20 seconds.
  
  As this is an early, rough and ready version the error detection isnt as robust as it should be yet but its has been
  running pretty stable here. Network errors - cant reach host, bad gateway etc - are just silently swallowed although
  if you run it through the debugger they will appear in the console output.
  
  The Who's Online menu option cycles through your followed Twitch users and shows all those who are currently
  streaming.
  
  There is a bit of a problem with the information Twitch gives whereby it sometimes indicates that a streamer has
  stopped streaming when in fact they haven't. This would there would be a Offline popup, followed 20 seconds later
  an Online popup as it corrects itself. I've put in a bit of a kludge for this whereby the stream has to be 'Offline'
  twice in succession before I throw up the Offline popup. This seems to have reduced the on/off popups to (up till now
  , zero). Although more testing needed.
  
  Erm...thats it I think.
  
  
