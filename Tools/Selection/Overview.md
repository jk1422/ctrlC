# Selection Tool Overview #

### Intro ###
When activating the mod by either using the UI button in the top left corner or by using the hotkey binding, the Selection Tool is activated. 

The Selection Tool is divided into separate files to maintain a cleaner and more organized project. 

Lets break each file down:

### SelectionTool.cs ###
This is the main file and contains all variables and core functions for the selection tool. It manages the overall state of the selection tool, including how selections are initiated, updated, and finalized. This file also contains logic for handling input and integrating with the other parts of the mod.

### SelectionTool.Helpers.cs ###
This file contains utility functions and helper methods that are used by the main selection tool. These functions are abstracted to keep the core file clean and focused on the primary logic. Common tasks like calculations or validation checks are handled here.

### SelectionTool.RaycastSelection.cs ###
This file manages the raycast-based selection. It handles all the logic related to selecting objects through raycasting, ensuring that objects are accurately selected based on the player's point of view and where they click within the game world.

### SelectionTool.CircleSelection.cs ###
This file handles the logic for circular area selection. It iterates through all entities in the game world, checking their transforms to determine if they fall within the desired selection radius. This is the core logic for selecting multiple entities based on their position relative to the circular selection area.

### EntitySelectionJob.cs ###
This file is responsible for multi-threaded processing of entity selection using jobs. It optimizes the circle selection by running in parallel, iterating through a large number of entities efficiently. Additionally, it leverages the Burst compiler to further boost performance, minimizing lag and ensuring smooth gameplay even when selecting a high volume of entities.
