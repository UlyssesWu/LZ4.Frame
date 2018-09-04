# LZ4.Frame
[![NuGet version](https://badge.fury.io/nu/LZ4.Frame.svg)](https://badge.fury.io/nu/LZ4.Frame)

[LZ4 Frame](https://github.com/lz4/lz4/blob/dev/doc/lz4_Frame_format.md) compress & decompress for .NET based on [lz4net](https://github.com/MiloszKrajewski/lz4net).

## Install
`PM > Install-Package LZ4.Frame`

## Why not...

### [lz4net](https://github.com/MiloszKrajewski/lz4net)
lz4net is great, but [lacking](https://github.com/MiloszKrajewski/lz4net#compatibility) LZ4 Frame support. This project is based on lz4net and enables LZ4 Frame support.

### [lz4.net](https://github.com/IonKiwi/lz4.net)
lz4.net is a C++/CLI wrapper for native LZ4. I love C++/CLI solutions but that means it only works on Windows platforms. This project is fully managed, therefore it's easier to compile, modify, and go cross-platform.

## TODO
These features are not implemented:

* Linked Block (when Block Independence flag = 0)
* Dictionary (when Dictionary ID != 0)
* Skippable Frame
* Legacy frame

If you get `Unhandled ArgumentException: LZ4 block is corrupted, or invalid length has been given.`, your lz4 file is most likely using Linked Block and Dictionary. I currently couldn't find a workaround (please switch to lz4.net) for this because lz4net does not have any Dictionary support.

I'd appreciate it if someone could add Dictionary support for this.

## LICENSE
**MIT**

## Reference
Rune Henriksen's [lz4 frame formatter](https://github.com/Pectojin/lz4_frame_formatter) (LICENSE: MIT) is the main reference of this project.

[LZ4 Frame Format Description](https://github.com/lz4/lz4/blob/dev/doc/lz4_Frame_format.md)

## Thanks

Thanks @[vpenades](https://github.com/vpenades) for helping me improve this lib.

---
by Ulysses