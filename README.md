# MSIT: MapleStory Image Tool

A command-line utility that grabs images and animations from WZs. Requires .NET 3.5

## License

This project is licensed under the [GNU GPL](http://www.gnu.org/licenses/gpl.html) version 3.

## Usage

### Example

Animation: `MSIT /iwzp:D:\...\Effect.wz?BasicEff.img/LevelUp /iwzv:BMS /o:png /op:LevelUp.png`  
Multiple animation: `MSIT /iwzp:D:\...\Mob.wz?8510000.img/attack3*D:\...\Mob.wz?8510000.img/attack3/info/effect /iwzv:BMS /o:png /op:PianusBeam.png`  
Single image: `MSIT /iwzp:D:\...\Effect.wz?BasicEff.img/LevelUp/8 /iwzv:BMS /o:png /op:LevelUp8.png`

For more information, see `MSIT /?`.

### WZ Versions

* GMS v56+ & GMST: `GMS`
* EMS & KMS & KMST & MSEA: `EMS`
* BMS & GMS v55-: `BMS`
 
`MSEA`, `JMS` and `JMST` require the `/iwzne` flag.

## Libraries & Credits

* [NGif](http://www.codeproject.com/Articles/11505/NGif-Animated-GIF-Encoder-for-NET)
* [libpng](http://www.libpng.org/pub/png/libpng.html) 1.5.1 w/ [apng patch](http://littlesvr.ca/apng/)
* [SharpApng](http://code.google.com/p/sharpapng/)
* [Mono.Options](https://github.com/mono/mono/blob/master/mcs/class/Mono.Options/Mono.Options/Options.cs)

Algorithm taken from [HaRepacker](http://community.kryptodev.com/thread-release-hasuite-harepacker-and-hacreator).

### Thanks

* Fiel _... of course!_
* haha01haha01
* retep998 from [NoLifeStory](http://code.google.com/p/nolifestory/)

This project is a rewrite of MapleAnimator, by angelsl as well, which was never publically released.

MapleAnimator was written for the sole intent of providing Fiel's fantastic MapleStory community [Southperry](http://www.southperry.net/) with animations.
