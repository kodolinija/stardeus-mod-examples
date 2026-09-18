# Stardeus Mod Example: Patched Traits

Example of JSON patching. Instead of overriding a whole Core file, a patch changes only
the parts of it that you care about, so your mod keeps working when the rest of the file
changes in a game update, and it can coexist with other mods patching the same file.

## How to install

1. Start Stardeus
2. Go to Main Menu > Mods > About Mods
3. Click "Open User Mod Directory"
4. A "Mods" folder will open ("%UserProfile%/AppData/LocalLow/Kodo Linija/Stardeus/Mods" on Windows)
5. Put PatchedTraits folder inside Mods folder
6. Exit Stardeus and start it again
7. Double check if Main Menu > Mods contains "Patch Example", and if the mod is enabled
8. After the game has loaded, look into the "Patched" folder next to the "Mods" folder. It contains the final patched files (Core/Config/Traits/Defs/CleaningOCD.json and Technophobe.json), with a header that lists which patches were applied
