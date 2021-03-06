# TwitchAlert

Note: Updating this Readme

Simple early(ish) version of an app that shows a popup whenever any user you follow goes Online/Offline.
(Online/Offline as in streaming)

Notes:
  Drag the popup from left to right to position it where on the horizontal you want it to appear 
  (I drag it to the centre) - it will then popup from bottom/centre. You can also Ctrl+Shift+C to
  center it. The Horizontal postion is saved.
  
  Right-click the notify icon and select Change Username to add your Twitch username.
  If you add an invalid username then, at the moment, it will just be ignored (there is no MessageBox or anything to    indicate failure yet).
  
  If you mouse-over the notify-icon it will display your username (if it was valid), the number of people you follow
  and how many of them are online.
  
  The first run of this will be slow depending on whether you follow a lot of people or not. This is
  because currently the app retrieves the thumbnails of all the people you follow. It caches these
  thumbnails to disk so subsequent runs won't have this delay (unless you change the username).
  
  The Who's Online menu option cycles through your followed Twitch users and shows all those who are currently
  streaming. (Double-clicking the Notify icon does the same job)
  
  If you follow a lot of users then when you first run the app (or when you double-click the NotifyIcon) there can be
  quite a few popups triggered if quite a few of them are Online. It can be a pain to sit through this everytime
  so pressing ESC will terminate this popup cycle. Popups for any future Online/Offline changes will still work
  though. There is now also a Menu option, 'Skip Popups At Start' to skip the initial popup cycle.
  
  Clicking on a streamers name will take you to their stream.
   
  Game and Status changes will also  now generate popups. These will have a yellow border. This option can also be
  turned off in the menu.
  
  The App polls Twitch every 20 seconds.

  If a streamers Name/Game/Status information can't all fit into the popup then a Tooltip will fire up if you mouse
  over it and display the full text. Be quick though as the popups don't stay open for long :-)
  
  There is a bit of a problem with the information Twitch gives whereby it sometimes indicates that a
  streamer has stopped streaming when in fact they haven't. This means there would be an Offline popup,
  followed 20 seconds later by an Online popup as it corrects itself. I've put in a bit of a kludge
  for this whereby the stream has to be 'Offline' twice in succession before I throw up the Offline popup.
  This seems to have reduced but not eliminated the rogue on/off popups.
  
  The 'Started' time at the bottom of the popup is simply the time string that Twitch sends. It takes no
  account of daylight saving so for here (UK) it gives GMT time rather than BST time. This means the time
  displayed is an hour earlier than it should be. I imagine this will be the case for people in other locations.
    
  As this is an early, rough and ready version the error detection isnt as robust as it should be yet but
  its has been running pretty stable here. Network errors - cant reach host, bad gateway etc - are just
  silently swallowed although if you run it through the debugger they will appear in the console output.
  
  Erm...thats it I think.
  
  
