## Editor Extensions v2.2

### BugFix
v2.1 caused a GUI error when opening the vessel loading screen. This was caused by a conflict between this mod's GUI skin and KSP's GUI skin.
This mod has been reverted to use the generic GUI skin to work around this issue. Otherwise no functional changes vs. v2.1

build 2.2.5470.2764 - 23 December 2014

### Changes in v2.1/v2.2
* Added new vertical snap (V)
 1. Place parts like normal.
 2. Once part is placed, hover over the part with your mouse and hit V. Part will align itself vertically to the middle of its parent part.
* Added new horizontal snap (H)
 1. Place parts like normal.
 2. Once part is placed, hover over the part with your mouse and hit H. Part will align itself horizontally to the middle of its parent part.
* Changed attachment mode toggling (T): now only toggles surface attachment of selected part.
* Added toolbar icon ("EEX") to bring up settings menu
 * Customize hotkeys
 * Customize angle snap values
 * Customize symmetry level
* Added KSP-AVC versioning support

A note regarding the new vertical/horizontal snap functions: These are currently very simplistic, they align themselves on a single axis to their parent part, and might clip into the parent part if the parent is rotated away from vertical/horizontal.
I'm working on a more sophisticated method that will align the parts to rotated parents and respect the collision meshes.

####[Download v2.2 for KSP 0.90](https://github.com/MachXXV/EditorExtensions/releases/download/v2.2/EditorExtensions_v2.2.zip)
This version is not compatible with any KSP versions prior to 0.90, for 0.25 use v1.4:
####[Download v1.4 for KSP 0.25](https://github.com/MachXXV/EditorExtensions/releases/download/v1.4/EditorExtensions_v1.4.zip)

### Features
* Allows custom levels of radial symmetry beyond the stock limitations.
* Horizontally and vertically center parts.
* Adds radial/angle snapping at 1,5,15,22.5,30,45,60, and 90 degrees. Angles are customizable.
* Toggle part clipping (From the cheat options)
* Toggle radial and node attachment of parts
* Reset hangar camera view

### Default Keybindings
* **V** 			- Vertically center the part under the mouse to its parent part
* **H** 			- Horizontally center the part under the mouse to its parent part
* **X, Shift+X** 	- Increase/Decrease symmetry level
* **Alt+X** 		- Reset symmetry level
* **C, Shift+C** 	- Increase/Decrease angle snap
* **Alt+C**			- Reset angle snap
* **T** 			- Attachment mode: Toggle between surface and node attachment modes for all parts, and when a part is selected, will toggle surface attachment even when that part's config usually does not allow it.
* **Alt+Z** 		- Toggle part clipping (CAUTION: This is a cheat option)
* **Space** 		- When no part is selected, resets camera pitch and heading (straight ahead and level)

###Installation
In your KSP GameData folder, delete any existing EditorExtensions folder.
Download the zip file to your KSP GameData folder and unzip.

Released under MIT license.
Source available at GitHub: [https://github.com/MachXXV/EditorExtensions](https://github.com/MachXXV/EditorExtensions)

