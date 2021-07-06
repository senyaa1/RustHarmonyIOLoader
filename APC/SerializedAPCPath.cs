﻿using System;
using System.Collections.Generic;
using ProtoBuf;

namespace HarmonyIOLoader.APC
{
	[ProtoContract]
	[Serializable]
	public class SerializedAPCPath
	{
		[ProtoMember(1)]
		public List<VectorData> nodes = new List<VectorData>();

		[ProtoMember(2)]
		public List<VectorData> interestNodes = new List<VectorData>();
	}
}
