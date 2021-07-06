using System;
using System.Collections.Generic;
using ProtoBuf;

namespace HarmonyIOLoader
{
	[ProtoContract]
	[Serializable]
	public class SerializedPathList
	{
		[ProtoMember(1)]
		public List<VectorData> vectorData = new List<VectorData>();
	}
}
