﻿using System;
using ProtoBuf;

namespace HarmonyIOLoader.NPC
{
	[ProtoContract]
	[Serializable]
	public class SerializedNPCSpawner
	{
		[ProtoMember(1)]
		public int npcType;

		[ProtoMember(2)]
		public int respawnMin;

		[ProtoMember(3)]
		public int respawnMax;

		[ProtoMember(4)]
		public VectorData position;

		[ProtoMember(5)]
		public string category;
	}
}
