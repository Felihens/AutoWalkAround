# Auto Walk Around

A client-side BepInEx mod for Valheim: auto-run steers itself around trees, rocks and fences, and sprints whenever your stamina is full.

Mod para Valheim (BepInEx, client-side): a corrida automática desvia sozinha de árvores, pedras e cercas, e corre rápido sempre que a stamina está cheia.

For players, see [package/README.md](package/README.md). That file is also the Thunderstore page.
Para jogadores, veja [package/README.md](package/README.md).

Sister mods / mods irmãos: [Bed Recall](https://github.com/Felihens/BedRecall), [Gather Tweaks](https://github.com/Felihens/GatherTweaks), [Build Tweaks](https://github.com/Felihens/BuildTweaks), [DigForHoe](https://github.com/Felihens/DigForHoe), [Resource Scanner](https://github.com/Felihens/ResourceScanner), [Ore Weak Spots](https://github.com/Felihens/OreWeakSpots), [Tree Weak Spots](https://github.com/Felihens/TreeWeakSpots).

## Building

`sh build.sh` compiles `src/*.cs` against the game's assemblies and installs the dll into `BepInEx/plugins/AutoWalkAround/` (`sh build.sh nocopy` only compiles). Then `powershell -File pack.ps1` builds `dist/AutoWalkAround-<version>.zip` from `package/`.

## License

MIT
