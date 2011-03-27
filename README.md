# MSIT: MapleStory Image Tool

A command-line utility that grabs images and animations from WZs.

## License

This project is licensed under the [GNU GPL](http://www.gnu.org/licenses/gpl.html) version 3.

## Usage

### Example

Animation: `MSIT /iwzf:D:\MapleStory\Effect.wz /iwzp:BasicEff.img/LevelUp /iwzv:BMS /o:png /op:LevelUp.png`  
Single image: `MSIT /iwzf:D:\MapleStory\Effect.wz /iwzp:BasicEff.img/LevelUp/0 /iwzv:BMS /o:png /op:LevelUp.png`

For more information, see `MSIT /?`.

### WZ Versions

* GMS v56+ & GMST: `GMS`
* EMS & KMS & KMST & MSEA v91-: `EMS`
* BMS & GMS v55- & MSEA v92+: `BMS`

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
