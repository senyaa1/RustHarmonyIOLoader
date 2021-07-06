using System.Collections.Generic;
using HarmonyIOLoader.NPC;
using UnityEngine;

namespace HarmonyIOLoader
{
	class NPCProcessor
	{
		public static SerializedNPCData serializedNPCData;

		private static readonly List<NPC.NPCSpawner> npcSpawners = new List<NPC.NPCSpawner>();

		public static void Process()
		{
			if (serializedNPCData != null)
			{
				for (int i = 0; i < serializedNPCData.npcSpawners.Count; i++)
				{
					var npcSpawner = new GameObject("assets/prefabs/npc/scientist/scientistspawn.prefab").AddComponent<NPC.NPCSpawner>(); 
					npcSpawner.Initialize(serializedNPCData.npcSpawners[i]);
					npcSpawners.Add(npcSpawner);
				}
				Logger.LogMessage($"Sucessfully processed {serializedNPCData.npcSpawners.Count} NPC spawners");
			}
		}

		public static string GetPrefabFromType(NPCType npcType)
		{
            var dictionary = new Dictionary<NPCType, string>
            {
                [NPCType.Scientist] = "assets/prefabs/npc/scientist/scientist.prefab",
                [NPCType.Peacekeeper] = "assets/prefabs/npc/scientist/scientistpeacekeeper.prefab",
                [NPCType.HeavyScientist] = "assets/rust.ai/agents/npcplayer/humannpc/heavyscientist/heavyscientist.prefab",
                [NPCType.JunkpileScientist] = "assets/prefabs/npc/scientist/scientistjunkpile.prefab",
                [NPCType.Bandit] = "assets/prefabs/npc/bandit/guard/bandit_guard.prefab",
                [NPCType.Murderer] = "assets/prefabs/npc/murderer/murderer.prefab",
                [NPCType.Scarecrow] = "assets/prefabs/npc/scarecrow/scarecrow.prefab"
            };

            return dictionary[npcType];
		}
	}
}
