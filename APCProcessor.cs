using System.Collections.Generic;
using HarmonyIOLoader.APC;
using UnityEngine;

namespace HarmonyIOLoader
{
	static class APCProcessor
	{
		public static SerializedAPCPathList serializedAPCPathList;
		public static List<CustomAPCSpawner> customAPCSpawners = new List<CustomAPCSpawner>();

		public static void Process()
		{
			if (serializedAPCPathList == null || serializedAPCPathList.paths.Count == 0) return;
			Logger.LogMessage("Starting to process APC Path Data");
			int count = 0;
			int totalcount = 0;
			for (int i = serializedAPCPathList.paths.Count - 1; i >= 0; i--)
			{
				if (serializedAPCPathList.paths[i].interestNodes.Count == 0)
				{
					Logger.LogWarning("Not processing APC path without any interest nodes!");
				}
				else
				{
					count++;
					totalcount += serializedAPCPathList.paths[i].nodes.Count + serializedAPCPathList.paths[i].interestNodes.Count;
					var customAPCSpawner = new GameObject("CustomAPCSpawner").AddComponent<CustomAPCSpawner>();
					customAPCSpawner.InitializeBradley(serializedAPCPathList.paths[i]);
					customAPCSpawners.Add(customAPCSpawner);
				}
			}
			Logger.LogMessage($"Sucessfully processed {count} APC paths with the total of {totalcount} nodes!");
		}
	}
}
