## Editor Extensions v2.7 For KSP v1.0.2
2 May 2015

### Changes in v2.7
* Recompile against KSP v1.0.2 binaries
* Updated KSP-AVC version file to include wildcard for minor 1.0.* KSP versions
* Otherwise no changes from v2.6, not necessary to download if you are not getting messages from version-aware plugins.

### Changes in v2.6
* Recompile against KSP 1.0 libraries
* Changed angle snap (C) and symmetry (X) functions to use game's current key mapping
* Modifier keys Alt/Mod/Shift now use game's key mapping for better Linux support
* On screen messages now use the stock font/layout

####[Download v2.7](https://github.com/MachXXV/EditorExtensions/releases/download/v2.7/EditorExtensions_v2.7.zip)

### Features
* Allows custom levels of radial symmetry beyond the stock limitations.
* Horizontally and vertically center parts.
* Align struts and fuel lines radially from parent and vertically at 0° and 45°
* Adds radial/angle snapping at 1°,5°,15°,22.5°,30°,45°,60°, and 90°. Angles are customizable.
* Toggle part clipping (From the cheat options)
* Toggle radial and node attachment of parts
* Reset hangar camera view
* Customize hotkeys
* KSP-AVC versioning support

#### Vertical/Horizontal snap:
* Place the part, then once the part is placed, hover over the part with your mouse and press the Vertical or Horizontal snap hotkey.
* For vertical snap, part will center itself on the part lengthwise in the SPH

#### Strut & Fuel line alignment
* Place the strut, then hover over the base/start of the strut (the first end placed) with the mouse, and press the hotkey.
* Addon will align the strut's direction perpendicular from the part's surface.
* If the strut is close to level, it will be aligned flat at 0°
* if the strut is more than +/- 22.5° from level, strut will be aligned to +/- 45°

### Default Keybindings
* **V** 			- Vertically center a part. Place the part, hover over it with the mouse, and press the hotkey.
* **H** 			- Horizontally center the part. Place the part, hover over it with the mouse, and press the hotkey.
* **U** 			- Place the strut, then hover over the base/start of the strut (the first end placed) with the mouse, and press the hotkey.
* **X, Shift+X** 	- Increase/Decrease symmetry level (Based on KSP's key map)
* **Alt+X** 		- Reset symmetry level (Based on KSP's key map)
* **C, Shift+C** 	- Increase/Decrease angle snap (Based on KSP's key map)
* **Alt+C**			- Reset angle snap (Based on KSP's key map)
* **T** 			- Attachment mode: Toggle between surface and node attachment modes for all parts, and when a part is selected, will toggle surface attachment even when that part's config usually does not allow it.
* **Alt+Z** 		- Toggle part clipping (CAUTION: This is a cheat option)
* **Space** 		- When no part is selected, resets camera pitch and heading (straight ahead and level)

###Installation
In your KSP GameData folder, delete any existing EditorExtensions folder.
Download the zip file to your KSP GameData folder and unzip.

[KSP Forum Thread](http://forum.kerbalspaceprogram.com/threads/38768)

Released under MIT license.
Source available at GitHub: [https://github.com/MachXXV/EditorExtensions](https://github.com/MachXXV/EditorExtensions)

