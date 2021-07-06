# RustHarmonyIOLoader
An open-source Harmony mod, replacement for Oxide.Ext.RustEdit.dll 
*This mod is still in **development** and may lack some features that original mod provides.* 


## Debug

You can enable debug logging and commands by changing *Config.cs*
```csharp
public static class Config
{
    public const bool DEBUG = true; // Enable/Disable extended logging and console commands
}
```
**Available commands:**
 - loader.apc.spawn	- Force respawn Bradley                 
 - loader.apc.show		- Draw APC path
 - loader.ocean.show - Draw Cargoship path
 - loader.npc.show     - Show NPC spawners


