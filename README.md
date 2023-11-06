# Shovel-Knight-Dig-Amiibo-Mod
A plugin/mod for Shovel Knight Dig that allows you to summon Amiibo fairies!

## How to install
1. Download BepInEx from here (direct download): https://github.com/BepInEx/BepInEx/releases/download/v5.4.21/BepInEx_x64_5.4.21.0.zip
2. Extract the zip into another folder
3. Copy all contents of the folder ("BepInEx" folder, "changelog.txt", "winhttp.dll", "doorstop_config.ini") into the Shovel Knight Dig game folder
4. Run Shovel Knight Dig, then quit using the in-game menu
5. Copy the "AmiiboPlugin.dll" file into "<game install folder>/BepInEx/plugins/"
 - "AmiiboPlugin.dll" is found in "./AmiiboPlugin/AmiiboPlugin/bin/Debug/netstandard2.1/"
6. Run the game and enjoy! 

When you have completed the steps above, your Shovel Knight Dig folder should look like the image below.
![Screenshot of the Shovel Knight Dig install folder layout once the steps above are completed.](./skdIntallFolder.png)

# How it works
Madam Meeber spawns at the left-most side of the camp. When you talk to her, a fairy will spawn based on the buttons you are holding:
- Nothing - Shovel Knight Fairy
- Left bumper - Plague Knight Fairy
- Right bumper - Golden Shovel Knight Fairy
- Left trigger - King Knight Fairy
- Right trigger - Specter Knight Fairy
You can only have 1 fairy equipped since that is the maximum allowed inside the well.

# Known issues
- Can't progress past title screen sometimes
- Limited functionality on keyboard