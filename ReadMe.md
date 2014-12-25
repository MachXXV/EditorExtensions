## Editor Extensions v2.3

build 2.3.5471.37348 - 24 December 2014

### Changes in v2.3
* Revised vertical/horizontal snap
 * Now works with rotated/skewed parts - centers the part along the parent part axis.
 * Vertical snap will center itself lengthwise for horizontal parts in the SPH
 * Snapping disabled while a gizmo is active
* Bugfixes
 * Part movements made with snaps now work with undo/redo in the editor
 * Fixed an issue where snapping, using a gizmo, and pressing space to reset the gizmo warped the part away by 2x its original position.

####[Download v2.3 for KSP 0.90](https://github.com/MachXXV/EditorExtensions/releases/download/v2.3/EditorExtensions_v2.3.zip)
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

