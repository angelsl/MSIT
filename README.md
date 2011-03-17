# MSreinator

A command-line utility that converts frame-by-frame & offset animations in MapleStory's WZs into GIFs or APNGs.

## License

This project is licensed under the [GNU GPL](http://www.gnu.org/licenses/gpl.html) version 3.

## Usage

### Example
`MSreinator /i:wz /iwzf:D:\MapleStory\Effect.wz /iwzp:BasicEff.img/LevelUp /iwzv:BMS /o:apng /op:LevelUp.png`

For more information, see `MSreinator /?`.

## Todo

* Getting data from arguments

## Libraries & Credits

* [GifComponents & CommonForms](http://sourceforge.net/projects/gifcomponents/)
* [libpng](http://www.libpng.org/pub/png/libpng.html) 1.5.1 w/ [apng patch](http://littlesvr.ca/apng/)
* [SharpApng](http://code.google.com/p/sharpapng/)
* [MapleLib 2](http://code.google.com/p/maplelib2/)
* [Mono.Options](https://github.com/mono/mono/blob/master/mcs/class/Mono.Options/Mono.Options/Options.cs)

Algorithm taken from [HaRepacker](http://community.kryptodev.com/thread-release-hasuite-harepacker-and-hacreator).

### Thanks

* Fiel_... of course!_
* haha01haha01

This project is a rewrite of MapleAnimator, by angelsl as well, which was never publically released.

MapleAnimator was written for the sole intent of providing Fiel's fantastic MapleStory community [Southperry](http://www.southperry.net/) with animations.
