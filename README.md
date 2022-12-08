# TMNationForever Training Buddy! [![GPLv3 License](https://img.shields.io/badge/License-GPL%20v3-yellow.svg)](https://opensource.org/licenses/) 

Best tool for a fresh and experienced players that will allow you to manage and load replays in real-time!

[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/A0A0GM3N0)

# Installation

This project, usually does not need an installation. Extract Buddy into the game client folder and you are ready to go!

#### Project dependencies

Buddy will require to have a wpcap.dll dependency installed. Not every system have it already (f.e. Wirehark uses this .dll), therefore an installer is included in release package.

Properly associated .gbx files with a client is also required. If configured inccorectly, it will result to open another client instead of injecting to the current one! The easiest way to fix this issue is to remove all TM versions form pc, do a quick restart and then install a standalone TM client. Be sure that you selected an option with correct assosiation!

## FAQ

#### Is Buddy safe to play with? 

Yes! Nothing in client executable is being modified while using Buddy, therefore, there is no reason to be afraid of a ban. Every action that Buddy does can be easily done by hand by downloading a replay from TMX directly.

#### I have joined a map and there is no replay loaded. Why?

Map data is fetched directly from TMX. Sometimes map does not have any record recorded on TMX therefore replay canot be downloaded. Dedimania replay download is planned to be implemented in the future.

#### Buddy does not start at all!

Please, check the Project dependencies section.

#### I cannot login into my account after using Buddy!

You propably used one account for Buddy as well as for normal client. This is normal behavior because TM servers do not allow this kind of multi-client accounts. To avoid this in future, do not log any account on Buddy client or use a dummy one!
