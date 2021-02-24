## NoisEvader

<table border="0px">
 <tr>
   <td><img src="https://github.com/sk-zk/NoisEvader/blob/master/screen1.png"></td>
   <td><img src="https://github.com/sk-zk/NoisEvader/blob/master/screen2.png"></td>
   <td><img src="https://github.com/sk-zk/NoisEvader/blob/master/screen3.png"></td>
  </tr>
</table>

**NoisEvader** is a FOSS recreation of the bullet hell music game [Soundodger+](https://soundodger.com/plus/).

With Soundodger 2 actually happening after all, I suppose this project is not really all that interesting anymore, but I'll continue development anyway
in case I come up with something I can do with this in the future.

At the moment, the main goal is to accurately recreate level playback, including glitches / unintentional behavior such as error bullets
or decimal spawners. There is also a 30fps mode for levels that require it ("lasers" work, but flicker a lot).

(There are no levels included with this game, so you need to bring your own.)

## Features
* 🦀 FLASH IS GONE 🦀
* Replays
* Gameplay modifiers
* Linux support
* More audio formats

## Known issues
The bigger ones, anyway:
* The UI is, uh, well, it exists, I suppose.
* Exclusive fullscreen causes all sorts of problems, especially under DirectX, so Borderless is recommended.
* Soundodger+ appears to apply a seemingly random audio offset to each audio file. I don't know where that comes from
or how to compensate for it, which means you're stuck with the offset for now.
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
* [vvvv/Svg](https://www.nuget.org/packages/Svg/3.1.1?_src=template)
* [XNAssets](https://github.com/rds1983/XNAssets)
