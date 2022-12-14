<p align="center" width="100%">
    <img width="100%" src="Banner.png?raw=true">
</p>

### This server is completely free of intellectual property from the original game. If you see infringing code, please let me know and I will take it down to be rewritten.

My email is feroxfoxxo@gmail.com if you need to get into contact with me directly.

The logo above was made by Z6mbie through vector art. Big thanks to him for the design!

#### None of the repo, the tool, nor the repo owner is affiliated with, or sponsored or authorized by any affiliates of the original game.

MQReawaken is a private server meant for educational use only. It's built off the Ultima server emulator, written by the guys at [ServUO](https://github.com/ServUO/ServUO/), as well as [Asset Studio](https://github.com/Perfare/AssetStudio), by Perfare, and heavily modified.

**This project is for non-commercial, educational use only.**

I am completely unaffiliated with any prior remake attempts. This has just been a project to progress my own knowledge of C#.

### Prerequisites

1. You **must** have your own copy of the game and its associated asset bundles, set up on a private server which you can connect to. We cannot directly provide this due to copyright protection, so you'll have to find some way of doing this yourself. 

2. You **must** have the associated DLL for the game added to your server project. This is because, otherwise; the server would contain copywritten code. You can follow below to figure out how to do this.

- Drag the DLL files in `game -> [game]Data -> Managed` into `MQReawaken -> Server.Reawakened -> Dependencies (create if not exists)`

3. On building the server in Visual Studio, run through the questions stated. It'll require you to set up where you have the game's settings file stored. This is found in the root of your game directory. It'll also ask you for where your cache files for the game are stored - it wants the **root __info** file, which you can find at the bottom of your cache directory. You can create a blank one if need be. 

### Other information

- You'll also want to download ILSpy to peek around any dependencies of the original game to understand how they work, reverse engineering them to develop a server side implementation -> https://sourceforge.net/projects/ilspy.mirror . You can use this to peek into the DLL files mentioned above.

#### Versioning

Automatic versioning is planned for the future, but at the moment the version number needs to be manually updated in the ``Module`` class. The system to update the version numbers goes as follows:

|Number|Description|
|--:|:--|
|Major|Only updated by the repository administrators once a completely new version is deployed.|
|Minor|Updated once a batch of features (or a [project](https://github.com/FeroxFoxxo/MQReawaken/projects)) is fully implemented.|
|Patch|Updated once a new feature is added.|

#### Credits

- <a href="https://github.com/Perfare/AssetStudio/">AssetStudio</a>
- <a href="https://github.com/ServUO/ServUO/">ServUO</a>
- <a href="https://github.com/victti/">Victti</a>

#### License

GPL v2 (Code license in Server.Base)
MIT (Code license in Server.Reawakened)
