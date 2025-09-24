After the 1.3.0 update, there were some reports of the placement tool creating endlessly many copies of the instantiated prefab. (even happens with default ingame-prefabs such as signature buildings)
![lol](https://github.com/user-attachments/assets/3be9f24d-0b3f-472f-99f7-1c9fbee91f5a)

I cannot replecate this issue, and debugging an issue that doesnt exist on my machine is quite hard. So, if anyone wants to tackle this issue I'll write down what I know so far:

1: The issue happens without any other mods active. 
2: The issue happens even when trying to instantiate standard assets (as long as ctrlC is loaded)
3: According to one report, when instantiating a standard asset, the ctrlC's PlacementTool-log file doesnt log that its activated, so ctrlC is somehow affecting the games PlacementTool?

My first thought was that the updatePhase of the placementTool needed to change, but it threw an error when changing it:

"[CRITICAL]  System update error during Modification1->PlacementTool:

Exception: Trying to create EntityCommandBuffer when it's not allowed!"

As I mentioned, I can't replicate the issue so Its really hard for me to fix it. 

If someone can find a reliable way to replicate the issue, please let me know. 
