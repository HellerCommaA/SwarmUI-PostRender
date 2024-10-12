# SwarmUI-PostRender

A [SwarmUI](https://github.com/mcmonkeyprojects/SwarmUI/) extension that adds parameters for [ProPost](https://github.com/digitaljohn/comfyui-propost/)

***If the nodes are not installed in the ComfyUI backend the parameters for that node won't show up in the generate tab.***

*** Shout out to Quaggles for their excellent work on [SwarmUI-FaceTools](https://github.com/Quaggles/SwarmUI-FaceTools/tree/master) who I lifted a lot of this work from ***

## Installation

1. Shutdown SwarmUI
2. Open a cmd/terminal window in `SwarmUI\src\Extensions`
3. Run `git clone https://github.com/HellerCommaA/SwarmUI-PostRender.git`
4. Run `SwarmUI\update-windows.bat` to recompile SwarmUI
5. Launch SwarmUI as usual, if the ProPost nodes are installed you should see parameter groups for them in the generate tab, if not follow the steps below.

## Installing ProPost Custom Nodes into SwarmUI
1. Install [ComfyUI-Manager](https://github.com/ltdrdata/ComfyUI-Manager) if not already installed by opening a cmd/terminal window in `SwarmUI/dlbackend/comfy/ComfyUI/custom_nodes` and running `git clone https://github.com/ltdrdata/ComfyUI-Manager.git`
2. Restart SwarmUI
3. Open the 'Comfy Workflow' tab and click on the manager button and then 'Custom Nodes Manager': ![image](https://github.com/user-attachments/assets/878878c1-e498-4e3c-922b-72efe382fb12)
4. In the Custom Nodes Manager find and install 'ProPost'
6. Click the red restart button at the bottom of the window then go to your SwarmUI Server/Logs tab and set the view to ComfyUI to view the download/install progress, **this will take a while**
7. When the logs show that the downloads have finished restart SwarmUI and the parameter groups should appear

## Updating
1. Shutdown SwarmUI
2. Open a cmd/terminal window in `SwarmUI\src\Extensions\SwarmUI-PostRender`
3. Run `git pull`
4. Run `SwarmUI\update-windows.bat` to recompile SwarmUI

## Usage
Check out the readme on [ProPost](https://github.com/digitaljohn/comfyui-propost/) for clear usage.
