using System;
using System.Collections.Generic;
using ProtoBuf;

namespace HarmonyIOLoader.APC
{
	[ProtoContract]
	[Serializable]
	public class SerializedAPCPathList
	{
		[ProtoMember(1)]
		public List<SerializedAPCPath> paths = new List<SerializedAPCPath>();
	}
}
