using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.AI;

namespace HarmonyIOLoader.NPC
{
	public class NPCSpawner : MonoBehaviour
	{
		private SerializedNPCSpawner serializedNPCSpawner;
		private NPCType npcType;
		private string prefabPath;
		private float time;
		private bool isRespawnScheduled;

		internal BaseCombatEntity baseCombatEntity;

		public void Initialize(SerializedNPCSpawner serializedNPCSpawner)
		{
			this.serializedNPCSpawner = serializedNPCSpawner;
			npcType = (NPCType)serializedNPCSpawner.npcType;
			prefabPath = NPCProcessor.GetPrefabFromType(npcType);
			transform.position = serializedNPCSpawner.position;
			InvokeHandler.Invoke(this, new Action(DoRespawn), 3f);
			InvokeHandler.InvokeRepeating(this, new Action(CheckIfRespawnNeeded), 5f, 5f);
		}
		public void CheckIfRespawnNeeded()
		{
			if (isRespawnScheduled)
			{
				if (baseCombatEntity == null && Time.time >= time)
				{
					DoRespawn();
				}
			}
			else if (baseCombatEntity == null)
			{
				ScheduleRespawn();
			}
		}

		public void DoRespawn()
		{
			if (!Rust.Application.isLoading && !Rust.Application.isLoadingSave)
			{
				SpawnNPC();
			}
			isRespawnScheduled = false;
		}

		public void Fill() => DoRespawn();
		public void ScheduleRespawn()
		{
			time = Time.time + new System.Random().Next(serializedNPCSpawner.respawnMin, serializedNPCSpawner.respawnMax);
			isRespawnScheduled = true;
		}

		public void SpawnRepeating() => CheckIfRespawnNeeded();
		public void SpawnNPC()
		{
			ScientistAI sAI = new ScientistAI();
			Vector3 position = transform.position;
            bool flag = HasNavMesh(ref position, out int num);
            BaseEntity baseEntity = GameManager.server.CreateEntity(prefabPath, position, Quaternion.identity, false);
			if (!(baseEntity == null))
			{
				baseEntity.enableSaving = false;
				PoolableEx.AwakeFromInstantiate(baseEntity.gameObject);
				baseEntity.Spawn();
				sAI.scientist = (baseEntity as ScientistNPC);
				if (sAI.scientist != null)
				{
					sAI.scientist.NavAgent.areaMask = num;
					sAI.scientist.NavAgent.agentTypeID = ((num == 25 || num == 8) ? 0 : -1372625422);
					sAI.scientist.CancelInvoke(new Action(sAI.scientist.EnableNavAgent));
					if (!flag)
					{
						sAI.scientist.NavAgent.isStopped = true;
						sAI.scientist.NavAgent.enabled = false;
						typeof(HumanNPC).GetField("navmeshEnabled", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).SetValue(sAI.scientist, false);
					}
					else
					{
						sAI.scientist.NavAgent.Warp(sAI.scientist.transform.position);
						sAI.scientist.Invoke(new Action(sAI.EnableNav), 1f);
					}
				}
				Scientist scientist = baseEntity as Scientist;
				if (scientist != null)
				{
					scientist.SetFact((BaseNpc.Facts)51, (byte)((npcType == NPCType.Peacekeeper) ? 1 : 0), true, true);
					scientist.SetFact((BaseNpc.Facts)57, 0, true, true);
					scientist.SetFact((BaseNpc.Facts)5, 0, true, true);
					scientist.SetPlayerFlag((BasePlayer.PlayerFlags)65536, true);
					if (flag)
					{
						scientist.NavAgent.areaMask = num;
						scientist.NavAgent.agentTypeID = ((num == 25 || num == 8) ? 0 : -1372625422);
						scientist.NeverMove = false;
						scientist.SetFact((BaseNpc.Facts)18, 0, true, true);
					}
					else
					{
						scientist.NeverMove = true;
						scientist.SetFact((BaseNpc.Facts)18, 1, true, true);
						scientist.NavAgent.isStopped = true;
						scientist.NavAgent.enabled = false;
					}
				}
				baseCombatEntity = (baseEntity as BaseCombatEntity);
			}
		}

		private bool HasNavMesh(ref Vector3 position, out int mask)
		{
            bool result;
            if (NavMesh.SamplePosition(position, out NavMeshHit navMeshHit, 5f, -1))
			{
				mask = navMeshHit.mask;
				position = navMeshHit.position;
				result = true;
			}
			else
			{
				mask = -1;
				result = false;
			}
			return result;
		}
		private class ScientistAI
		{
			public ScientistNPC scientist;
			public void EnableNav()
			{
				scientist.NavAgent.isStopped = false;
				scientist.NavAgent.enabled = true;
				typeof(HumanNPC).GetField("navmeshEnabled", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).SetValue(scientist, true);
			}
		}
	}
}
