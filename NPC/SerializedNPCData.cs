using System;
using System.Collections.Generic;
using ProtoBuf;

namespace HarmonyIOLoader.NPC
{
	[ProtoContract]
	[Serializable]
	public class SerializedNPCData
	{
		[ProtoMember(1)]
		public List<SerializedNPCSpawner> npcSpawners = new List<SerializedNPCSpawner>();
	}
}
