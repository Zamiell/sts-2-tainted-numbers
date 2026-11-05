# Tainted Numbers

A Slay the Spire 2 mod that adds an incoming-damage preview to Tainted cards:

```text
Gain 3 Tainted. (+3 damage)
```

The number is the **additional damage** from gaining that card's Tainted, summed over living enemies' current attack intents. Multi-hit attacks count every hit. The game's damage modifiers apply, including Weak, Vulnerable, existing Tainted, and damage caps, with rounding per hit. For example, a 10x3 attack while Weak becomes 7x3; adding 3 Tainted makes it 9x3, so the card shows `(+6 damage)`.

This is attack damage before Block, not predicted HP loss. It uses the current combat state, without predicting the card's other effects, future intent changes, or on-hit triggers. Non-attacking turns show `(+0 damage)`. Outside combat, card text is unchanged. Original text and formatting are preserved; the added suffix is in English.

## Build and install

Requires the .NET SDK with `net9.0` support and a local game installation.

```sh
dotnet build -c Release
./install.sh
```

Default Steam paths are supported on Windows, macOS, and WSL (using the Windows installation). Override a non-default location with `-p:GameDir="/path/to/Slay the Spire 2"` or `-p:GameDataDir="/path/to/data_sts2_platform"`.

Build output is in `dist/TaintedNumbers/`. To install manually, copy `TaintedNumbers.dll` and `TaintedNumbers.json` into the game's `mods/TaintedNumbers/` directory. On macOS, `mods/` is inside `SlayTheSpire2.app/Contents/MacOS/`.

Restart the game after installation. To uninstall, remove `mods/TaintedNumbers/`.

## Development checks

With the .NET 9 runtime installed:

```sh
dotnet run --project tests/Behavior
dotnet run --project tests/Compatibility -- \
  "/path/to/data_sts2_platform" \
  "dist/TaintedNumbers/TaintedNumbers.dll"
```

Behavior checks exercise the Harmony patches with small game stubs. Compatibility checks verify patch installation against the installed game assemblies. Visual layout still needs an in-game check.
