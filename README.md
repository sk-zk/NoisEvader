NoisEvader is a FOSS recreation of the musical bullet hell game Soundodger+.

Soundodger 2 actually happening after all has made this project kind of pointless, but I'll continue development anyway
in case I come up with something interesting I can do with this in the future.

At the moment, the main goal is to accurately recreate level playback, including glitches / unintentional behavior such as error bullets
or decimal spawners. There is also a 30fps mode for levels that require it (lasers work, but flicker a lot).

## Known issues
The bigger ones, anyway:
* The GUI is, uh, let's call it a work in progress.
* Exclusive fullscreen causes all sorts of problems, especially under DirectX, so Borderless is recommended.
* Soundodger+ appears to apply a seemingly random audio offset in every level. I don't know where that comes from
or how to compensate for it, which means you're stuck with the audio offset for now.
* Timestep is a never-ending source of fun so replays don't really work all that well yet.
* The Linux build has no AA. This will be fixed with the next MonoGame release.

## Dependencies

* CSCore
  * On Windows: [master branch](https://github.com/filoe/cscore/tree/master)
  * On Linux: [my fork of the netstandard branch](https://github.com/sk-zk/cscore/tree/netstandard)
* [Ionic.Zlib](https://www.nuget.org/packages/Ionic.Zlib.Core/)
* [Microsoft.Data.Sqlite](https://www.nuget.org/packages/Microsoft.Data.Sqlite)
* [MonoGame 2.8](https://www.monogame.net/)
* [MonoGame.Extended](https://github.com/craftworkgames/MonoGame.Extended)
* [LilyPath](https://github.com/sk-zk/LilyPath) (my fork of it)
* [Myra](https://github.com/rds1983/Myra)
* [NLog](https://nlog-project.org/)
* [SpriteFontPlus](https://github.com/rds1983/SpriteFontPlus)
* [vvvv/Svg](https://www.nuget.org/packages/Svg/3.1.1?_src=template)
* [XNAssets](https://github.com/rds1983/XNAssets)
