# Metro: Last Light Config Editor

## Description

This is a config editor for the classic Steam version of Metro: Last Light (not Redux).  
It allows you to edit most game settings as well as some hidden settings (such as FOV, cheats or windowed mode) without running the game.  
It was made in C# using .NET 4.6.2 and Visual Studio 2017. You may need to install the Microsoft [.NET Framework 4.6.2](https://www.microsoft.com/en-us/download/details.aspx?id=53344) to make it work.

## Installation

You can place the program anywhere on your computer. However, it'll be easier for Steam/game detection if it's placed where the game executable is located.

## Usage

1. Run the program.
2. Customize your settings.
3. Make sure all paths are specified.
4. Hit Save.
5. Run the game.

You can reload the config by pressing the Reload button.  
You can use the Start game (non-Steam) button to run the game from the specified path.  
You can use the Start game (Steam) button to run the game using the Steam browser protocol.  

## Arguments

- -log: when closing the program, it will create a LOG file in the same directory than the program.

## Credits

- [cdorst](http://twitch.tv/cdorst): helped for the update notification system
- [Jamedjo](https://github.com/Jamedjo): author of the Steam library function