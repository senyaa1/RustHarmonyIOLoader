using Facepunch;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace HarmonyIOLoader
{
    class DeployableProcessor
    {
        private static readonly Type[] typeArr = new Type[]
        {
            typeof(GroundWatch),
            typeof(DestroyOnGroundMissing)
        };

        public static List<BaseCombatEntity> entityList = new List<BaseCombatEntity>();
        public static readonly List<BaseEntityInfo> baseEntityInfoList = new List<BaseEntityInfo>();
        public static bool SaveLoaded = false;

        public struct BaseEntityInfo
        {
            public BaseEntityInfo(BaseEntity baseEntity)
            {
                prefabId = baseEntity.prefabID;
                position = baseEntity.transform.position;
            }

            public Vector3 position;
            public uint prefabId;
        }
        public struct BaseCombatEntityInfo
        {
            public string prefabPath;
            public Vector3 position;
            public Quaternion rotation;
            public BaseCombatEntityInfo(BaseCombatEntity baseCombatEntity)
            {
                prefabPath = baseCombatEntity.name;
                position = baseCombatEntity.transform.position;
                rotation = baseCombatEntity.transform.rotation;
            }
        }


        /*
		[Oxide.Core.Plugins.HookMethod("OnTrapTrigger")]
		private void OnTrapTrigger(BearTrap bearTrap)
		{
			if (!(bearTrap == null))
				bearTrap.Invoke(new Action(bearTrap.Arm), (float)(30 * 60));     
		}*/
        public static void SpawnEntities(BaseCombatEntityInfo baseCombatEntityInfo, int a = 1500, int b = 2400)
        {
            InvokeHandler.Invoke(SingletonComponent<ServerMgr>.Instance, delegate ()
            {
                List<BaseCombatEntity> list = Pool.GetList<BaseCombatEntity>();
                Vis.Entities(baseCombatEntityInfo.position, 1f, list, -1, (QueryTriggerInteraction)2);
                int count = list.Count;
                Pool.FreeList(ref list);

                if (count > 0)
                {
                    SpawnEntities(baseCombatEntityInfo, 300, 600);
                }
                else
                {
                    BaseCombatEntity baseCombatEntity = GameManager.server.CreateEntity(baseCombatEntityInfo.prefabPath, baseCombatEntityInfo.position, baseCombatEntityInfo.rotation, true) as BaseCombatEntity;
                    baseCombatEntity.enableSaving = false;
                    baseCombatEntity.Spawn();
                    entityList.Add(baseCombatEntity);
                    ProcessEntity(baseCombatEntity);
                }
            }, new System.Random().Next(a, b));
        }

        public static void ProcessEntity(BaseNetworkable bn)
        {
            if (bn == null) return;
            if ((!(bn is IOEntity) || bn is Signage) && !(bn is ResourceEntity) && !(bn is VendingMachine) && !(bn is LootContainer))
            {
                for (int i = 0; i < typeArr.Length; i++)
                {
                    Component component = bn.GetComponent(typeArr[i]);
                    if (component != null)
                    {
                        UnityEngine.Object.Destroy(component);
                    }
                }
                if (bn is SamSite)
                {
                    (bn as SamSite).staticRespawn = true;
                }
                if (bn is BearTrap)
                {
                    (bn as BearTrap).Arm();
                }
                if (bn is Barricade)
                {
                    (bn as Barricade).canNpcSmash = false;
                }
                var baseCombatEntity = bn as BaseCombatEntity;
                if (baseCombatEntity != null)
                {
                    baseCombatEntity.pickup.enabled = false;
                    entityList.Add(baseCombatEntity);
                }
                var stabilityEntity = bn as StabilityEntity;
                if (stabilityEntity != null)
                {
                    stabilityEntity.grounded = true;
                    stabilityEntity.cachedStability = 1f;
                    stabilityEntity.cachedDistanceFromGround = 1;
                }
                if (bn is Door)
                {
                    (bn as Door).CloseRequest();
                }
            }
        }
    }
}
