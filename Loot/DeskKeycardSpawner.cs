using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HarmonyIOLoader.Loot
{
	class DeskKeycardSpawner
	{
		public static HashSet<KeycardSpawnerManager> KeycardManagers = new HashSet<KeycardSpawnerManager>();

		public static readonly List<string> SpawnerNames = new List<string>()
		{
			"assets/bundled/prefabs/radtown/desk_greencard.prefab",
			"assets/bundled/prefabs/radtown/desk_bluecard.prefab",
			"assets/bundled/prefabs/radtown/desk_redcard.prefab",
			"assets/bundled/prefabs/hapis/desk_greencard_hapis.prefab"
		};

		public class KeycardSpawnerManager : FacepunchBehaviour
		{
			private SpawnGroup spawnGroup;
			private BaseSpawnPoint baseSpawnPoint;
			public bool isSpawned = false;

			[Serializable]
			private sealed class EntryWeight
			{
				public static readonly EntryWeight instance = new EntryWeight();
				public int GetSpawnEntryWeight(SpawnGroup.SpawnEntry spawnEntry)
				{
					return spawnEntry.weight;
				}
			}

			void Awake()
			{
				spawnGroup = GetComponent<SpawnGroup>();
				baseSpawnPoint = GetComponentInChildren<BaseSpawnPoint>();
				spawnGroup.Clear();
				Spawn();
			}

			public void OnDestroyed()
			{
				if (!isSpawned) InvokeHandler.Invoke(this, new Action(Spawn), 17f * 60);
			}

			public void SpawnImmidiate()
			{
				InvokeHandler.CancelInvoke(this, new Action(Spawn));
				Spawn();
			}

			private void Spawn()
			{
				baseSpawnPoint.GetLocation(out Vector3 position, out Quaternion rotation);

				var ent = GameManager.server.CreateEntity(GetResourcePath(), position, rotation, false);
				if (ent)
				{
					ent.enableSaving = false;
					PoolableEx.AwakeFromInstantiate(ent.gameObject);
					ent.Spawn();
					ent.gameObject.AddComponent<DestructionMonitor>().manager = this;
					isSpawned = true;
				}
			}

			private string GetResourcePath()
			{
				float num = spawnGroup.prefabs.Sum(new Func<SpawnGroup.SpawnEntry, int>(EntryWeight.instance.GetSpawnEntryWeight));
				if (num == 0f) return null;

				float num2 = new System.Random().Next(0, Convert.ToInt32(num));

				foreach (SpawnGroup.SpawnEntry spawnEntry in spawnGroup.prefabs)
					if ((num2 -= spawnEntry.weight) <= 0f)
						return spawnEntry.prefab.resourcePath;
					
				
				return spawnGroup.prefabs[spawnGroup.prefabs.Count - 1].prefab.resourcePath;
			}
		}

		private class DestructionMonitor : FacepunchBehaviour
		{
			public KeycardSpawnerManager manager;
			private void OnDestroy()
			{
				if (!Rust.Application.isQuitting)
				{
					manager.isSpawned = false;
					manager.OnDestroyed();
				}
			}
		}
	}
}
