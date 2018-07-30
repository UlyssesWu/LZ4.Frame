# LZ4.Frame
[LZ4 Frame](https://github.com/lz4/lz4/blob/dev/doc/lz4_Frame_format.md) compress & decompress for .NET based on [lz4net](https://github.com/MiloszKrajewski/lz4net).

## Why not...

### [lz4net](https://github.com/MiloszKrajewski/lz4net)
lz4net is great, but [lacking](https://github.com/MiloszKrajewski/lz4net#compatibility) LZ4 Frame support. This project is based on lz4net and enable LZ4 Frame support.

### [lz4.net](https://github.com/IonKiwi/lz4.net)
lz4.net is a C++/CLI wrapper for native LZ4. I love C++/ClI solutions but that means it only works on Windows platforms. This project is fully managed, therefore it's easier to compile, modify, and go cross-platform.

## TODO
These features are not implemented:

* Block dependency (when Block Independence flag = 0)
* Dictionary (when Dictionary ID != 0)
* Skippable Frames
* Legacy frame

## LICENSE
**MIT**

## Reference
Rune Henriksen's [lz4 frame formatter](https://github.com/Pectojin/lz4_frame_formatter) (LICENSE: MIT) is the main reference of this project.

[LZ4 Frame Format Description](https://github.com/lz4/lz4/blob/dev/doc/lz4_Frame_format.md)

---
by Ulysses