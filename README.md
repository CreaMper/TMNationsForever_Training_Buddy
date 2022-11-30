# TMNationForever Training Buddy! [![GPLv3 License](https://img.shields.io/badge/License-GPL%20v3-yellow.svg)](https://opensource.org/licenses/) 

Best tool for new players that wants to start with some help!
With this tool, you are able to see a replay of a TOP1 player in a map using a separate game client!

[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/A0A0GM3N0)

# Installation
This project does not need an installation. Extract Configurator and Executor into the game client folder and you are ready to go!

NOTE: If Buddy won't run at all, please also use an installer which is added into the release package!

NOTE#2: Do not use any account on Buddy client (or just create a dummy one)!

## FAQ

#### I cannot set a game client!

At this moment Buddy supports only a standalone client as well as the steam version. Support for other types of client will be added in the future.

#### Is Buddy safe to play with? 

Yes! Nothing in client executable is being modified while using Buddy, therefore, there is no reason to be afraid of a ban. Every action that Buddy does can be easily done by hand by downloading a replay from TMX directly.

#### I have joined a map and there is no replay loaded. Why?

Map data is fetched directly from TMX. Sometimes map does not have any record recorded on TMX therefore replay canot be downloaded. Dedimania replay download is planned to be implemented in the future.

#### Why sometimes replay does not load automatically and I have to re-join my race?

Try to change a packet listening intensity. The higher value you set, the more accuracy Buddy will have with reading all map information but remember - more intensity requires more CPU resources. 

#### Buddy does not start at all!

Buddy requires a WinCap dependency to work properly. On most PC it is already installed but not allways (f.e if you previously installed WireShark you should already have all dependencies). Please, use an installer that is included in release package!

#### I cannot login into my account after using Buddy!

You propably used one account for Buddy as well as for normal client. This is normal behavior because TM servers do not allow this kind of multi-client accounts. To avoid this in future, do not log any account on Buddy client or use a dummy one!
