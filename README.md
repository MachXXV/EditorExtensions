## Editor Extensions v2.0

###Changes in v2.0
* KSP 0.90 support
* Changed attachment mode toggling:
-- Changed Hotkey to "T", previous hotkey conflicted with new stock symmetry mode toggle (R)
-- Now toggles between surface attachment and node attachment. When surface attachment is enabled, part will not attach to regular nodes (green dots).
* Veritcal snap removed, new 0.90 editor does not have the same functionality. Currently working on new method of providing automatic vertical snap without having to use the translation gizmo.
* Removed launchpad/runway toggle due to incompatibility with KSP 0.90
* Removed redundant symmetry mode (radial/mirror) toggle, now in stock editor with R hotkey.
* This version is not compatible with any KSP versions prior to 0.90, for 0.25 use v1.4 which can be downloaded at the github page under releases.

####[Download v2.0 for KSP 0.90](https://github.com/MachXXV/EditorExtensions/raw/master/Releases/EditorExtensions_v2.0.zip)

####[Download v1.4 for KSP 0.25](https://github.com/MachXXV/EditorExtensions/raw/master/Releases/EditorExtensions_v1.4.zip)

### Features
* Allows levels of radial symmetry from 1 to 99
* Adds radial/angle snapping at 1,5,15,30,45,60, and 90 degrees.
* Toggle part clipping (From the cheat options)
* Toggle radial and node attachment of parts
* Reset hangar camera view

### Keybindings
* **X, Shift+X** 	- Increase/Decrease symmetry level
* **Alt+X** 		- Reset symmetry level
* **C, Shift+C** 	- Increase/Decrease angle snap
* **Alt+C**			- Reset angle snap
* **T** 			- Attachment mode: Toggle between surface and node attachment modes for all parts, and when a part is selected, will toggle surface attachment even when that part's config usually does not allow it.
* **Alt+Z** 		- Toggle part clipping (CAUTION: This is a cheat option)
* **Space** 		- When no part is selected, resets camera pitch and heading (straight ahead and level)

Released under MIT license.
Source available at GitHub: [https://github.com/MachXXV/EditorExtensions](https://github.com/MachXXV/EditorExtensions)

