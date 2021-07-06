using System.Collections.Generic;
using UnityEngine;

namespace HarmonyIOLoader
{
    static class OceanPathProcessor
    {
        public static SerializedPathList serializedPathList;
        private static List<Vector3> pathNodesList = new List<Vector3>();

        public static List<Vector3> Process()
        {
			if (serializedPathList == null) return null;
			Logger.LogMessage("Starting to process ocean path nodes...");

			for (int i = 0; i < serializedPathList.vectorData.Count; i++)
				pathNodesList.Add(serializedPathList.vectorData[i]);
				
			if (pathNodesList.Count == 0) return null;

			Logger.LogMessage($"Sucessfully processed {pathNodesList.Count} ocean path nodes");
			return pathNodesList;
		}
    }
}
