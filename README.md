# NbtSharp
A .NET Standard 2.0 NBT (de)serialization library written in C#, based on [fNbt](https://github.com/mstefarov/fNbt) by [Matvei Stefarov](https://github.com/mstefarov).

[![Build status](https://img.shields.io/github/workflow/status/MelonSharp/fNbt/.NET)](https://github.com/MelonSharp/fNbt/actions/workflows/dotnet.yml)
![Repo size](https://img.shields.io/github/languages/code-size/MelonSharp/fNbt)
![GitHub](https://img.shields.io/github/license/MelonSharp/fNbt)

---

## Overview
[Named Binary Tag (NBT)](https://minecraft.gamepedia.com/NBT_format) is a structured binary file format developed by Mojang and used throughout Minecraft.

NbtSharp is a .NET Standard 2.0 library providing functionality to create, load, traverse, modify, and save NBT files and streams. It is based on [fNbt](https://github.com/mstefarov/fNbt) by [Matvei Stefarov](https://github.com/mstefarov), which itself is based on [LibNbt](https://github.com/aphistic/libnbt) by [Kristin Davidson (aphistic)](https://github.com/aphistic).

## Why not .NET 5?
.NET Standard 2.0 offers support for many more environments (including .NET 5 environments), such as for use with the Unity 3D engine, which does not yet support .NET 5.

## Features
- Load and save uncompressed, GZip-, and ZLib-compressed files/streams.
- Easily create, traverse, and modify NBT documents.
- Simple indexer-based syntax for accessing compound, list, and nested tags.
- Shortcut properties to access tags' values without unnecessary type casts.
- Compound tags implement `ICollection<T>` and List tags implement `IList<T>`, for easy traversal and LINQ integration.
- Good performance and low memory overhead.
- Built-in pretty-printing of individual tags or whole files.
- Every class and method are fully documented, annotated, and unit-tested.
- Can work with both big-endian and little-endian NBT data and systems.
- Optional high-performance reader/writer for working with streams directly.

## Examples
Coming soon

## Contributing
Contributions will be welcome in the near future. Please do not open a pull request at this time, it will be ignored. For issues with existing functionality, open an issue on the [fNbt issue tracker](https://github.com/mstefarov/fNbt/issues).

## License
NbtSharp is licensed under 3-Clause BSD; see [LICENSE.md](LICENSE.md) for more details.

fNbt is licensed under 3-Clause BSD; see [its LICENSE](https://github.com/MelonSharp/fNbt/blob/master/docs/LICENSE) for more details.

LibNbt is licensed under GNU GPL v3; see [its LICENSE](https://github.com/aphistic/libnbt/blob/master/docs/LICENSE) for more details.