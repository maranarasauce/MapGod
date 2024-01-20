# Map God
A toolset for ULTRAKILL mapping that automates wave and room creation. Currently works with both Tundra and Rude.

## Installation
Download the `.unitypackage` from the Releases tab and drag it into your project.

## Usage
**Starting Out**
Open up the Map God Inspector Window via the `Tools/Map God` tab. From here is everything related to the Map God. When first starting a map, it will tell you *GOD IS DEAD*. Fear not, we are going to bring to life your new Map God. Click the `Do it for me` button, and you'll find a new Map God component on the same GameObject as your Map Info component. You don't have to touch the Map God component anytime after this, as it is all automated by the Map God Inspector Window.

**Creating a Room**
To create a new Room, press the *Add New Room* button. This will set up an empty hierarchy where you can place your static objects, any dynamic objects and the Start and End doors.
Once you've made any changes to a room, including enemies, press *Validate All Rooms* or *Validate Selected Room* to tell the Map God to automatically fill in any fields related to your changes.

**Checkpoints**
All you need to do when setting up Checkpoints is drop the Checkpoint prefab in and set the `To Activate` field to the room that contains that checkpoint. *Validate Checkpoints* goes through each checkpoint and finds every room between the last checkpoint and itself to see what rooms and doors it needs to inherit.

Finally, *Toggle Wave Enemies* toggles the enemies in the current wave you have selected. 