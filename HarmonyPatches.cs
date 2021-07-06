using Harmony;
using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using HarmonyIOLoader.NPC;
using HarmonyIOLoader.Loot;
using HarmonyIOLoader.APC;
using Network;
using System.Reflection;

namespace HarmonyIOLoader
{
    [HarmonyPatch(typeof(WorldSerialization), "GetMap", new Type[] { typeof(string) })]
    class GetMapPatch
    {
        static void Prefix(ref WorldSerialization __instance)
        {
            if (MapDataDecompressor.IsLoaded) return;
            if (__instance == null) return;
            foreach (var map in __instance.world.maps)
            {
                if (map != null && !MapDataDecompressor.VanillaMapNames.Contains(map.name))
                {
                    var ioData = MapDataDecompressor.Deserialize<SerializedIOData>(map.data, out bool flag);
                    if (flag)
                    {
                        MapDataDecompressor.IOMapName = map.name;
                        IOEntityProcessor.SerializedIOData = ioData;
                        if (Config.DEBUG) Logger.LogMessage($"Found matching IO map name - {map.name}");
                        Logger.LogMessage($"Sucessfully found {IOEntityProcessor.SerializedIOData.entities.Count} IO entities");
                        continue;
                    }
                    var pathData = MapDataDecompressor.Deserialize<SerializedPathList>(map.data, out flag);
                    if (flag)
                    {
                        MapDataDecompressor.OceanPathMapName = map.name;
                        OceanPathProcessor.serializedPathList = pathData;
                        if (Config.DEBUG) Logger.LogMessage($"Found matching ocean path map name - {map.name}");
                        Logger.LogMessage($"Sucessfully found {pathData.vectorData.Count} ocean path nodes");
                        continue;
                    }
                    var npcData = MapDataDecompressor.Deserialize<SerializedNPCData>(map.data, out flag);
                    if(flag)
                    {
                        MapDataDecompressor.NPCMapName = map.name;
                        NPCProcessor.serializedNPCData = npcData;
                        if(Config.DEBUG) Logger.LogMessage($"Found matching NPC spawner map name - {map.name}");
                        Logger.LogMessage($"Sucessfully found {npcData.npcSpawners.Count} NPC spawners");
                        NPCProcessor.Process();
                        continue;
                    }
                    var apcData = MapDataDecompressor.Deserialize<SerializedAPCPathList>(map.data, out flag);
                    if(flag)
                    {
                        MapDataDecompressor.APCMapName = map.name;
                        APCProcessor.serializedAPCPathList = apcData;
                        if (Config.DEBUG) Logger.LogMessage($"Found matching APC spawner map name - {map.name}");
                        Logger.LogMessage($"Successfully found {apcData.paths.Count} APC spawners");
                        APCProcessor.Process();
                        continue;
                    }
                }
            }
            MapDataDecompressor.IsLoaded = true;
        }
    }
    #region Spawn
    [HarmonyPatch(typeof(BaseNetworkable), "Spawn")]
    class SpawnPatch
    {
        static void Postfix(ref BaseNetworkable __instance)
        {
            if (!DeployableProcessor.SaveLoaded && DeployableProcessor.baseEntityInfoList != null)
            {
                bool isInList = false;
                for (int i = 0; i < DeployableProcessor.baseEntityInfoList.Count; i++)
                {
                    DeployableProcessor.BaseEntityInfo elem = DeployableProcessor.baseEntityInfoList[i];
                    if (__instance.prefabID == elem.prefabId && __instance.transform.position == elem.position)
                    {
                        isInList = true;
                    }
                }

                if (isInList)
                    DeployableProcessor.ProcessEntity(__instance);
            }
            if (__instance != null && !(__instance is Signage) && __instance is IOEntity && IOEntityProcessor.SerializedIOData != null)
            {
                foreach (var sIOEnt in IOEntityProcessor.SerializedIOData.entities)
                {
                    if (IOEntityProcessor.AreEqual(__instance, sIOEnt))
                    {
                        IOEntityProcessor.entityList.Add(__instance);
                        if (IOEntityProcessor.entityList.Count == IOEntityProcessor.SerializedIOData?.entities.Count)
                        {
                            Logger.LogMessage("Starting to process IO entities...");
                            IOEntityProcessor.Process();
                        }
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(World), "Spawn", new Type[] { typeof(string), typeof(Prefab), typeof(Vector3), typeof(Quaternion), typeof(Vector3) })]
    class PrefabSpawnPatch
    {
        static bool Prefix(ref string category, ref Prefab prefab, ref Vector3 position, ref Quaternion rotation, ref Vector3 scale)
        {
            if (!prefab.Object) return false;
            if (!World.Cached)
            {
                prefab.ApplyTerrainPlacements(position, rotation, scale);
                prefab.ApplyTerrainModifiers(position, rotation, scale);
            }
            var go = prefab.Spawn(position, rotation, scale, true);
            if (go)
            {
                go.SetHierarchyGroup(category, true, false);
            }

            var entity = go.GetComponent<BaseEntity>();

            if (entity != null && !DeployableProcessor.SaveLoaded && !(entity is LootContainer))
            {
                entity.enableSaving = false;
                DeployableProcessor.ProcessEntity(entity);
                DeployableProcessor.baseEntityInfoList.Add(new DeployableProcessor.BaseEntityInfo(entity));
            }

            if (DeskKeycardSpawner.SpawnerNames.Contains(go.name))
            {
                int i = 0;
                while (i < go.transform.childCount)
                {

                    Transform child = go.transform.GetChild(i);
                    if (!StringEx.Contains(child.name, "card_spawner", CompareOptions.IgnoreCase))
                    {
                        i++;
                    }
                    else
                    {
                        var component = child.GetComponent<SpawnGroup>();
                        if (component != null)
                        {
                            var spawnClock = typeof(SpawnGroup).GetField("spawnClock", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(component) as LocalClock;
                            if (spawnClock != null && component.WantsTimedSpawn()) spawnClock.events.Clear();
                            var spawnGroups = typeof(SpawnHandler).GetField("SpawnGroups", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(SingletonComponent<SpawnHandler>.Instance) as List<ISpawnGroup>;
                            if(spawnGroups != null) spawnGroups.Remove(component);

                            DeskKeycardSpawner.KeycardManagers.Add(component.gameObject.AddComponent<DeskKeycardSpawner.KeycardSpawnerManager>());
                            break;
                        }
                        break;
                    }
                }
            }
            return false;
        }
    }
    #endregion

    #region Kill
    [HarmonyPatch(typeof(BaseCombatEntity), "OnAttacked", new Type[] { typeof(HitInfo) })]
    class OnAttackedPatch
    {
        static bool Prefix(ref BaseCombatEntity __instance, ref HitInfo info)
        {
            try
            {
                if (__instance != null && info != null)
                {
                    if (__instance is SamSite || __instance is Landmine || __instance is BearTrap || __instance is Barricade)
                        return true;
                    if (DeployableProcessor.entityList.Contains(__instance))
                        return false;
                    if (__instance is AutoTurret && IOEntityProcessor.entityList.Contains(__instance as AutoTurret) && __instance.HasComponent<AutoTurretManager>())
                        return false;   
                }

                if (IOEntityProcessor.entityList.Contains(__instance)) return false;
                return true;
            } catch(Exception ex)
            {
                Logger.LogMessage("Caught exception " + ex);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(BaseNetworkable), "Kill", new Type[] { typeof(BaseNetworkable.DestroyMode) })]
    class KillPatch
    {
        static bool Prefix(ref BaseNetworkable __instance)
        {
            try {
                //JunkPileProcessor.OnEntityKilled(__instance);
                if (!(__instance is BaseCombatEntity)) return true;
                var entity = __instance as BaseCombatEntity;


                if (entity != null && DeployableProcessor.entityList.Contains(entity) && (entity is Landmine || entity is BearTrap || entity is Barricade))
                {
                    if (!DeployableProcessor.SaveLoaded)
                    {
                        DeployableProcessor.SpawnEntities(new DeployableProcessor.BaseCombatEntityInfo(entity));
                    }
                    else
                    {
                        DeployableProcessor.SpawnEntities(new DeployableProcessor.BaseCombatEntityInfo(entity), 120, 360);
                    }
                    DeployableProcessor.entityList.Remove(entity);
                }

                if (DeployableProcessor.entityList.Contains((BaseCombatEntity)__instance)) return false;

                //IO
                if (!(__instance is IOEntity)) return true;
                var IOEnt = __instance as IOEntity;
                if (IOEnt is Signage) return true;
                if (IOEntityProcessor.entityList.Contains(IOEnt)) return false;

                for (int i = 0; i < IOEntityProcessor.SerializedIOData.entities.Count; i++)
                {
                    if (IOEntityProcessor.AreEqual(IOEnt, IOEntityProcessor.SerializedIOData.entities[i]))
                    {
                        return false;
                    }
                }
                return true;
            } catch(Exception ex)
            {
                Logger.LogMessage("Caught exception: " + ex);
                return true;
            }
        }
    }
    #endregion

    #region AutoTurret

    [HarmonyPatch(typeof(Item), "UseItem", new Type[] { typeof(int) })]
    class UseItemPatch
    {
        static void Prefix(ref Item __instance, ref int amountToConsume)
        {
            if (amountToConsume <= 0) return;

            var parent = __instance.parent;
            var ent = (parent?.entityOwner);

            if (!(ent == null) && (ent is AutoTurret && IOEntityProcessor.entityList.Contains(ent as AutoTurret)))
            {
                AutoTurretManager component = ent.GetComponent<AutoTurretManager>();
                if (component != null && component.UnlimitedAmmo)
                {
                    __instance.amount += amountToConsume * 2;
                }
            }
        }
    }

    [HarmonyPatch(typeof(AutoTurret), "AddSelfAuthorize", new Type[] { typeof(BaseEntity.RPCMessage) })]
    class TurretAuthorizePatch
    {
        static bool Prefix(ref AutoTurret __instance, ref BaseEntity.RPCMessage rpc)
        {
            if (IOEntityProcessor.entityList.Contains(__instance) && !rpc.player.IsAdmin) return false;
            return true;
        }
    }
    #endregion

    #region CargoshipPath
    [HarmonyPatch(typeof(BaseBoat), "GenerateOceanPatrolPath", new Type[] { typeof(float), typeof(float) })]
    class GenerateOceanPatrolPathPatch
    {
        static bool Prefix(ref List<Vector3> __result)
        {
            var pathNodesList = OceanPathProcessor.Process();
            if (pathNodesList == null) return true;
            __result = pathNodesList;
            return false;
        }
    }
    #endregion

    #region Misc
    [HarmonyPatch(typeof(SaveRestore), "Load", new Type[] { typeof(string), typeof(bool) })]
    class OnSaveLoadPatch
    {
        static void Postfix(ref bool __result)
        {
            DeployableProcessor.SaveLoaded = true;
            if(__result) foreach(var manager in DeskKeycardSpawner.KeycardManagers) manager.SpawnImmidiate();
            if (Config.DEBUG) Logger.LogWarning("! HarmonyIOLoader is compiled with debug ON!");
        }
    }


    [HarmonyPatch(typeof(StabilityEntity), "StabilityCheck")]
    class OnStabilityCheckPatch
    {
        static bool Prefix(ref StabilityEntity __instance)
        {
            if (__instance != null && DeployableProcessor.entityList.Contains(__instance))
                return false;
            return true;
        }
    }

    #endregion

    #region Debugging
    [HarmonyPatch(typeof(ConsoleNetwork), "OnClientCommand", new Type[] { typeof(Message) })]
    class OnClientCommandPatch
    {
        static bool Prefix(ref Message packet)
        {
            if (packet.read.Unread > ConVar.Server.maxpacketsize_command)
            {
                Debug.LogWarning("Dropping client command due to size");
                return false;
            }
            string text = packet.read.StringRaw(1048576U);
            if (packet.connection == null || !packet.connection.connected)
            {
                Debug.LogWarning("Client without connection tried to run command: " + text);
                return false;
            }

            var player = BasePlayer.FindByID(packet.connection.userid);
            if (player == null) return false;

            if (Config.DEBUG)
            {
                switch (text)
                {
                    case "loader.apc.spawn":
                        CommandHandler.SpawnBradley(player);
                        return false;
                    case "loader.apc.show":
                        CommandHandler.ShowBradleyPath(player);
                        return false;
                    case "loader.ocean.show":
                        CommandHandler.ShowOceanPath(player);
                        return false;
                    case "loader.npc.show":
                        CommandHandler.ShowNPCSpawners(player);
                        return false;
                }
            }

            string text2 = ConsoleSystem.Run(ConsoleSystem.Option.Server.FromConnection(packet.connection).Quiet(), text, Array.Empty<object>());
            if (!string.IsNullOrEmpty(text2))
                typeof(ConsoleNetwork).GetMethod("SendClientReply", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { packet.connection, text2 });
            return false;
        }
    }
    #endregion
}
