using System;
using System.Collections.Generic;
using ConVar;
using UnityEngine;

namespace HarmonyIOLoader.APC
{
	class CustomAPCSpawner : MonoBehaviour
	{
		private const string BRADLEYPREFAB = "assets/prefabs/npc/m2bradley/bradleyapc.prefab";

		public BasePath BasePath;
		public BradleyAPC BradleyAPC;

		private bool isDestroyed;

		public void InitializeBradley(SerializedAPCPath sAPCPath)
		{
			BasePath = new GameObject("BasePath").AddComponent<BasePath>();
			BasePath.nodes = new List<BasePathNode>();
			BasePath.interestZones = new List<PathInterestNode>();
			BasePath.speedZones = new List<PathSpeedZone>();
			for (int i = 0; i < sAPCPath.nodes.Count; i++)
			{
				BasePathNode basePathNode = new GameObject("BasePathNode").AddComponent<BasePathNode>();
				basePathNode.transform.position = sAPCPath.nodes[i];
				BasePath.nodes.Add(basePathNode);
			}
			for (int j = 0; j < BasePath.nodes.Count; j++)
			{
				BasePathNode basePathNode2 = BasePath.nodes[j];
				if (!(basePathNode2 == null))
				{
					basePathNode2.linked = new List<BasePathNode>();
					basePathNode2.linked.Add((j == 0) ? BasePath.nodes[BasePath.nodes.Count - 1] : BasePath.nodes[j - 1]);
					basePathNode2.linked.Add((j == BasePath.nodes.Count - 1) ? BasePath.nodes[0] : BasePath.nodes[j + 1]);
					basePathNode2.maxVelocityOnApproach = -1f;
				}
			}
			for (int k = 0; k < sAPCPath.interestNodes.Count; k++)
			{
				PathInterestNode pathInterestNode = new GameObject("PathInterestNode").AddComponent<PathInterestNode>();
				pathInterestNode.transform.position = sAPCPath.interestNodes[k];
				BasePath.interestZones.Add(pathInterestNode);
			}
			InvokeHandler.Invoke(this, new Action(SpawnBradley), 3f);
			InvokeHandler.InvokeRepeating(this, new Action(IsAlive), 5f, 5f);
			if (Config.DEBUG) Logger.LogMessage("Spawned Bradley APC!");
		}


		private void IsAlive()
		{
			if (!isDestroyed && (BradleyAPC == null || !BradleyAPC.IsAlive()))
			{
				InvokeHandler.CancelInvoke(this, new Action(SpawnBradley));
				InvokeHandler.Invoke(this, new Action(SpawnBradley), new System.Random().Next
					(
						Convert.ToInt32(Bradley.respawnDelayMinutes - Bradley.respawnDelayVariance),
						Convert.ToInt32((Bradley.respawnDelayMinutes + Bradley.respawnDelayVariance) * 60f)
					));
				isDestroyed = true;
			}
		}

		public void SpawnBradley()
		{
			if (!(BradleyAPC != null))
			{
				var rand = BasePath.interestZones.ToArray()[new System.Random().Next(0, BasePath.interestZones.Count - 1)];
				Vector3 vector = BasePath.interestZones.Count <= 0 ? BasePath.nodes[0].transform.position : rand.transform.position;
				BradleyAPC = GameManager.server.CreateEntity(BRADLEYPREFAB, vector, default(Quaternion), true).GetComponent<BradleyAPC>();
				BradleyAPC.enableSaving = false;
				BradleyAPC.Spawn();
				BradleyAPC.pathLooping = true;
				BradleyAPC.InstallPatrolPath(BasePath);
			}
			isDestroyed = false;
		}
	}
}
