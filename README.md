# SwarmUI-PostRender

A [SwarmUI](https://github.com/mcmonkeyprojects/SwarmUI/) extension that adds parameters for [ProPost](https://github.com/digitaljohn/comfyui-propost/)

Most notably, LUTs are able to be applied during an image generation. If you're not familiar with what a LUT is, a quick 10,000 ft overview: high quality photo filters.

***If the nodes are not installed in the ComfyUI backend the parameters for that node won't show up in the generate tab.***

*** Shout out to Quaggles for their excellent work on [SwarmUI-FaceTools](https://github.com/Quaggles/SwarmUI-FaceTools/tree/master) who I lifted a lot of this work from ***

## Roadmap
1. All nodes are implemented, bug fixes

## Notes
LUTs are stored in SwarmUI/Models/luts

## Installation

#### Automatic Install (suggested):
1. Select the extension in the Server -> Extensions tab and click 'Install'
The extension will be kept up to date when `update-(your platform)` is run

#### Manual Install (not suggested):
1. Shutdown SwarmUI
2. Open a cmd/terminal window in `SwarmUI\src\Extensions`
3. Run `git clone https://github.com/HellerCommaA/SwarmUI-PostRender.git`
4. Run `SwarmUI\update-windows.bat` to recompile SwarmUI
5. Launch SwarmUI as usual, if the ProPost nodes are installed you should see parameter groups for them in the generate tab, if not go to step 6.
6. Expand the `Film Grain` group and click `Install ProPost Nodes`. Click OK to confirm. then once the install is finished, restart SwarmUI.

## Updating
1. Shutdown SwarmUI
2. Open a cmd/terminal window in `SwarmUI\src\Extensions\SwarmUI-PostRender`
3. Run `git pull`
4. Run `SwarmUI\update-windows.bat` to recompile SwarmUI

## Usage
Check out [ProPost](https://github.com/digitaljohn/comfyui-propost/) for clear usage.
