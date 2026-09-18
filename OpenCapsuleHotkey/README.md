# Stardeus Mod Example: Open Capsule Hotkey

Example of a code (C#) mod that adds a new key binding to the game. Pressing **G** orders
your crew to open the storage capsules you have selected, the same thing the "Open Capsule"
button does. The key can be changed in Settings > Input, like any other key in the game.

Unlike the other examples, this mod contains C# code, so it has to be compiled into a DLL
file before it does anything.

## How to build

1. Install the .NET SDK (https://dotnet.microsoft.com/download)
2. Open `.vscode/mod.csproj` and make sure the `Reference` path points to the
   `Stardeus_Data/Managed` folder of your Stardeus installation
3. Run `dotnet build .vscode/mod.csproj` in the mod folder, or use
   "Tasks: Run Build Task" if you have opened the folder in Visual Studio Code
4. `Libraries/Stardeus_Mod_OpenCapsuleHotkey.dll` should appear. The game loads every DLL
   file it finds in the `Libraries` folder of an enabled mod

If you are using this as a starting point for your own mod, change `AssemblyName` in
`mod.csproj`. The game refuses to load a DLL that is called `Stardeus_Mod_Template`.

## How to install

1. Start Stardeus
2. Go to Main Menu > Mods > About Mods
3. Click "Open User Mod Directory"
4. A "Mods" folder will open ("%UserProfile%/AppData/LocalLow/Kodo Linija/Stardeus/Mods" on Windows)
5. Put OpenCapsuleHotkey folder (with the built `Libraries` folder in it) inside Mods folder
6. Exit Stardeus and start it again
7. Double check if Main Menu > Mods contains "Open Capsule Hotkey", and if the mod is enabled
8. Go to Settings > Input. "Shortcut: Open Selected Capsules" should be listed under Shortcuts
9. Load a game, select a storage capsule that has landed. The "Open Capsule" button should
   show the key next to it, and pressing the key should do the same as clicking the button

## How it works

### 1. Adding the binding (Code/OpenCapsuleBinding.cs)

The game calls every private static method marked with `[RuntimeInitializeOnLoadMethod]`
right after it loads the mod DLL. This moment matters: the default bindings of the game
already exist, but the keys the player has changed in the settings are not applied yet.
A binding that is added here is treated like one of the game's own:

- it is listed in Settings > Input, under the group given to `WithUIGroup`
- the player can rebind it there
- the new key is saved and restored the next time the game starts

Adding a binding later than this will log an error, and the saved key will be lost.

Every binding belongs to an `ActionType`. It is an enum inside the game code, so a mod
cannot add a named value to it. It is stored as a byte though, so the mod casts an unused
number to it instead:

```csharp
public const ActionType Action = (ActionType) 200;
```

This number is also the name of the binding. The label you see in the settings comes from
the translation key `input.200` (see `Translations/English.csv`), and the saved key is
stored under the same name.

### 2. Reacting to the key press (Code/OpenCapsulePatches.cs)

The game has no hook for mods to react to a key press, so the mod uses
[Harmony](https://harmony.pardeike.net/), which is included with the game, to run its own
code after `UserInputShortcutsHandler.HandleShortcuts`. That is the method where the game
checks its own shortcuts, once per frame.

Why there, and not in a per-frame signal like `S.Sig.EveryFrame`? The game does not call
`HandleShortcuts` while the player is typing into a text field, while a loading screen is
showing, or when the game window is not focused. Code that runs after it gets all of these
checks for free. With a per-frame signal, typing "g" into the search field would open
capsules.

The code runs every frame, so the first thing it does is the cheapest check possible: was
the key just pressed? Everything else happens only after that.

### 3. Reusing the game's own logic

`OrderOpenCapsuleAction.OrderOpen` is the method behind the "Open Capsule" button. It is
private, so the mod finds it with `AccessTools.Method` and turns it into a delegate once,
when the mod loads. Calling the delegate is as fast as calling the method directly, and
the mod does not have to copy (and keep up to date) the rules of which capsules can be
opened.

### 4. Showing the key on the button

Two more patches run after `OrderOpenCapsuleAction.Create` and `CreateMulti`, the methods
that create the button for one and for many selected capsules. `__result` is the button
the original method has returned, and the patch adds the key glyph to it, the same way the
Draft button shows its key. The glyph follows the key chosen in the settings, and it is
hidden if the player has removed the key.

### 5. Applying the patches

The game only applies Harmony patches from its own code. A mod has to apply its own:

```csharp
new Harmony("OpenCapsuleHotkey").PatchAll(typeof(OpenCapsuleBinding).Assembly);
```

Use an id that is unique to your mod.

## Things to keep in mind

- **The number can clash.** If two mods pick the same `ActionType` number, the second one
  will fail to load. Pick a high number (the game uses the low ones, the maximum is 255),
  and mention it in the description of your mod.
- **Pick a free default key.** Check Settings > Input before choosing one. Two actions on
  the same key cause a "Duplicate Binding" warning when the game starts. At the time of
  writing, G and Z are the only letters the game does not use.
- **Patches depend on method names.** `OrderOpen`, `Create` and `CreateMulti` are private
  methods of the game, they can be renamed or removed in a game update. If that happens,
  the mod throws an error while loading, and the game disables it and tells the player,
  instead of breaking in the middle of a game. The mod will need an update then.
- **Apply the patches only once.** Harmony patches stay applied until the game process
  exits, and applying them again does not replace them, it stacks them, so every patch
  would run twice. That is why the mod checks `Harmony.HasAnyPatches` first. The symptom
  of getting this wrong in this mod: the capsule opens, and you hear the "denied" sound
  too, because the second run finds a capsule that is already ordered to be opened.
- **Patch as little as possible.** Every patch is a place where your mod can break, or
  clash with another mod. This mod only uses postfixes, which run after the original
  method and do not change what it does.
