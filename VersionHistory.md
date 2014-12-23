### Previous Version History

### v2.0 - build 2.0.5466.37911 - 19 December 2014
* KSP 0.90 support
* Changed attachment mode toggling:
 * Changed Hotkey to "T", previous hotkey conflicted with new stock symmetry mode toggle (R)
 * Now toggles between surface attachment and node attachment. When surface attachment is enabled, part will not attach to regular nodes (green dots).
* Veritcal snap removed, new 0.90 editor does not have the same functionality. Currently working on new method of providing automatic vertical snap without having to use the translation gizmo.
* Removed launchpad/runway toggle due to incompatibility with KSP 0.90
* Removed redundant symmetry mode (radial/mirror) toggle, now in stock editor with R hotkey.
* This version is not compatible with any KSP versions prior to 0.90, for 0.25 use v1.4 which can be downloaded at the github page under releases.

#### 1.4 - 7 October 2014
  *   Recompiled against KSP 0.25 assemblies

#### 1.3 - 18 July 2014
  *   Recompiled against Mono/.NET 3.5 target to align with KSP runtime

#### #1.2 - 18 July 2014
*   Updated Text position for 0.24

#### #1.1 - 2 April 2014
*   Merged changes from forum user Ratzap
*   Updated plugin to use KSPAddonFixed
*   Code cleanup
*   Re-enabled VAB size increase

#### 1.0 beta 2 April 2014
*   Recompiled against 23.5 binaries and latest frameworks

#### 0.6 Beta - 5 August 2013
*   Symmetry mode will not wrap/loop around at maximum and minimum - stops at 50 on the way up with X, stops at 1 on the way down with Shift+X. Alt+X resets to 1

#### 0.5 Beta - 24 July 2013
*   Fixed compatibility with 0.21
*   Disabled VAB/SPH size modifications no longer working in 0.21
*   Removed feature to ingore hotkeys in ship name window no longer working in 0.21

#### 0.4 Alpha - 8 July 2013
*   Added on-screen messages when settings are toggled.
*   Space resets camera when no part is selected
*   Tab toggles between VAB and SPH editor modes while staying in the same building, and toggles launching to runway/launchpad.
*   Alt+C resets angle snap
*   Consolidated surface/radial attachment toggle to just use Alt+R and be context aware for when a part is selected

#### 0.3 Alpha - 5 July 2013
*   Added 1 degree to the angle snap options
*   Improved veritcal snap toggle behavior - automatically enables angle snap when vertical snap is enabled
*   Fixed radial/surface attachment toggling
-- Alt+R toggles normal surface attachment for all parts
-- Shift+Alt+R toggles surface attachment for the selected part, and also enables parts that usually can't be attached to a non-node surface (like hub connectors)

#### 0.2 Alpha - 4 July 2013
*   Added VAB/SPH interior space/camera limit increase
*   Known issues:
-- Radial/surface attachment not toggling properly.

#### 0.1 Alpha - 2 July 2013 - Initial release
*   Basic features implemented.
*   Known issues:
-- Radial/surface attachment not toggling properly.
-- Spaceplane hangar mode not tested
-- Mirror symmetry not tested
-- (Non-breaking/not user affecting) Debug log reports an error for out of bounds sprite animation when symmetry mode > 8
-- (Non-breaking/not user affecting) Debug log reports DivideByZero exception when angle snap is set to 0 and there is a part selected.
