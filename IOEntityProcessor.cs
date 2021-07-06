using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace HarmonyIOLoader
{
    static class IOEntityProcessor
    {
        public static SerializedIOData SerializedIOData { get; set; }

        public static List<BaseNetworkable> entityList = new List<BaseNetworkable>();
        private static readonly List<IOEntity> alreadyProcessed = new List<IOEntity>();

        private static readonly Type[] typeArray = new Type[]
        {
            typeof(GroundWatch),
            typeof(DestroyOnGroundMissing),
            typeof(DeployableDecay)
        };

        public static void Process()
        {
            foreach (var sIOEnt in SerializedIOData.entities)
            {
                foreach (var ent in entityList)
                {
                    if (AreEqual(ent, sIOEnt))
                    {
                        var IOEnt = (IOEntity)ent;
                        DisableDecayAndStability(IOEnt);
                        ClearConnections(IOEnt);

                        IOEnt.pickup.enabled = false;
                        IOEnt.enableSaving = false;

                        if (BaseEntity.saveList.Contains(IOEnt))
                        {
                            BaseEntity.saveList.Remove(IOEnt);
                        }

                        for (int i = 0; i < sIOEnt.outputs.Length; i++)
                        {
                            try
                            {
                                if (sIOEnt.outputs[i]?.connectedTo != null)
                                    IOEnt.outputs[i].connectedToSlot = sIOEnt.outputs[i].connectedTo;
                                if (sIOEnt.outputs[i]?.type != null)
                                    IOEnt.outputs[i].type = (IOEntity.IOType)sIOEnt.outputs[i].type;

                                IOEntity target = null;
                                foreach (var targetEnt in entityList)
                                {
                                    if (targetEnt != null && sIOEnt.outputs[i] != null && AreEqual(targetEnt, sIOEnt.outputs[i]))
                                    {
                                        target = targetEnt as IOEntity;
                                        if (IOEnt is WheelSwitch)
                                        {
                                            if (!IOEnt.HasComponent<IOEntityToWheelSwitchConnection>())
                                                IOEnt.gameObject.AddComponent<IOEntityToWheelSwitchConnection>();

                                            var connection = IOEnt.gameObject.GetComponent<IOEntityToWheelSwitchConnection>();

                                            connection.SetTarget(target as WheelSwitch);
                                        }
                                    }
                                }
                                if (target != null)
                                {
                                    if(Config.DEBUG) Logger.LogMessage($"Found new connection: {IOEnt.ShortPrefabName} ---> {target.ShortPrefabName}");
                                    IOEnt.outputs[i].connectedTo.Set(target);
                                    /*if (IOEnt is Elevator)
                                    {
                                        ProcessElevator(IOEnt, target as Elevator, i, sIOEnt.outputs[i]);
                                    }*/
                                }
                            }
                            catch (Exception)
                            {
                                Logger.LogError($"Caught exception while prosessing {IOEnt.ShortPrefabName}: Try to connect Power Out of the button, instead of IO output");
                            }
                        }
                        
                        
                        ApplySerializedData(IOEnt, sIOEnt);

                        if (IOEnt.HasComponent<PuzzleReset>())
                        {
                            var puzzleReset = IOEnt.GetComponent<PuzzleReset>();
                            if (puzzleReset != null)
                            {
                                puzzleReset.ResetTimer();
                                puzzleReset.DoReset();
                            }
                        }
                        ResetIOEnt(IOEnt);
                    }
                }
            }
        }

        private static void ApplySerializedData(IOEntity IOEnt, SerializedIOEntity sIOEnt)
        {
            if (alreadyProcessed.Contains(IOEnt)) return;

            if (IOEnt is CardReader)
            {
                var cardReader = IOEnt as CardReader;
                if (sIOEnt?.accessLevel != null)
                {
                    cardReader.accessLevel = sIOEnt.accessLevel + 1;
                }
                if (!IOEnt.gameObject.HasComponent<CardReaderMonitor>())
                    IOEnt.gameObject.AddComponent<CardReaderMonitor>();

                if (sIOEnt?.timerLength != null && sIOEnt?.accessLevel != null)
                {
                    IOEnt.gameObject.GetComponent<CardReaderMonitor>().Init(sIOEnt.timerLength);

                    cardReader.AccessLevel1 = (BaseEntity.Flags)128;
                    cardReader.AccessLevel2 = (BaseEntity.Flags)256;
                    cardReader.AccessLevel3 = (BaseEntity.Flags)512;

                    if (sIOEnt.accessLevel + 1 == 1)
                        IOEnt.SetFlag((BaseEntity.Flags)128, true, false, true);
                    if (sIOEnt.accessLevel + 1 == 2)
                        IOEnt.SetFlag((BaseEntity.Flags)256, true, false, true);
                    if (sIOEnt.accessLevel + 1 == 3)
                        IOEnt.SetFlag((BaseEntity.Flags)512, true, false, true);

                    IOEnt.SendNetworkUpdate();
                }
            }

            if (IOEnt is ElectricalBranch)
            {
                if (sIOEnt?.branchAmount != null)
                    (IOEnt as ElectricalBranch).branchAmount = sIOEnt.branchAmount;
            }

            if (IOEnt is CCTV_RC)
            {
                if (sIOEnt?.rcIdentifier != null)
                    (IOEnt as CCTV_RC).UpdateIdentifier(sIOEnt.rcIdentifier);
                (IOEnt as CCTV_RC).UpdateRCAccess(true);
                (IOEnt as CCTV_RC).isStatic = true;
            }
            if (IOEnt is SamSite)
            {
                (IOEnt as SamSite).staticRespawn = true;
                (IOEnt as SamSite).InitializeHealth(1000f, 1000f);
            }

            if (IOEnt is TimerSwitch)
            {
                if (sIOEnt?.timerLength != null)
                {
                    if (sIOEnt.timerLength > 0f)
                    {
                        (IOEnt as TimerSwitch).timerLength = sIOEnt.timerLength;
                    }
                }
            }

            if (IOEnt is AutoTurret)
            {
                if (!(sIOEnt?.unlimitedAmmo == null || sIOEnt?.peaceKeeper == null || sIOEnt?.autoTurretWeapon == null)) {
                    if (!IOEnt.gameObject.HasComponent<AutoTurretManager>())
                        (IOEnt as AutoTurret).gameObject.AddComponent<AutoTurretManager>();

                    (IOEnt as AutoTurret).gameObject.GetComponent<AutoTurretManager>().Initialize(sIOEnt.unlimitedAmmo, sIOEnt.peaceKeeper, sIOEnt.autoTurretWeapon);
                }
            }

            if (IOEnt is DoorManipulator)
            {
                if (sIOEnt?.doorEffect != null)
                    (IOEnt as DoorManipulator).powerAction = (DoorManipulator.DoorEffect)sIOEnt.doorEffect;
                var door = (IOEnt as DoorManipulator).FindDoor(true);
                (IOEnt as DoorManipulator).SetTargetDoor(door);
            }

            if (IOEnt is PressButton) {
                if (sIOEnt?.timerLength != null)
                    (IOEnt as PressButton).pressDuration = sIOEnt.timerLength;
            }
            if (IOEnt is RFReceiver)
            {
                if (sIOEnt?.frequency != null)
                {
                    (IOEnt as RFReceiver).frequency = sIOEnt.frequency;
                    (IOEnt as RFReceiver).MarkDirty();
                    (IOEnt as RFReceiver).SendNetworkUpdate(0);
                    RFManager.ChangeFrequency((IOEnt as RFReceiver).frequency, sIOEnt.frequency, (IOEnt as RFReceiver), true, true);
                }
            }
            if (IOEnt is RFBroadcaster)
            {
                if (sIOEnt?.frequency != null)
                {
                    (IOEnt as RFBroadcaster).frequency = sIOEnt.frequency;
                    (IOEnt as RFBroadcaster).MarkDirty();
                    (IOEnt as RFBroadcaster).SendNetworkUpdate(0);
                    RFManager.ChangeFrequency((IOEnt as RFBroadcaster).frequency, sIOEnt.frequency, (IOEnt as RFBroadcaster), true, true);
                }
            }

            if (IOEnt is PowerCounter)
            {
                if(sIOEnt?.counterPassthrough != null && sIOEnt.counterPassthrough == true)
                    IOEnt.SetFlag((BaseEntity.Flags)256, true, false, true);
                if(sIOEnt?.targetCounterNumber != null)
                    typeof(PowerCounter).GetField("targetCounterNumber", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).SetValue(IOEnt as PowerCounter, Mathf.Clamp(sIOEnt.targetCounterNumber, 1, 100));
            }

            /*if(IOEnt is Elevator)
            {
                method_8(IOEnt as Elevator, sIOEnt);
            }*/
            

            if (IOEnt is Telephone)
            {
                if(sIOEnt?.phoneName != null)
                    (IOEnt as Telephone).Controller.PhoneName = sIOEnt.phoneName;
                (IOEnt as Telephone).SendNetworkUpdate(0);
            }
            alreadyProcessed.Add(IOEnt);

            if (SerializedIOData.entities.Count == alreadyProcessed.Count)
                Logger.LogMessage($"Sucessfully processed {alreadyProcessed.Count} IO entities");
        }
        public static void DisableDecayAndStability(IOEntity IOEntity)
        {
            for (int i = 0; i < typeArray.Length; i++)
            {
                Component component = IOEntity.GetComponent(typeArray[i]);
                if (component != null)
                {
                    UnityEngine.Object.Destroy(component);
                }
            }
            DecayEntity componentInParent = IOEntity.GetComponentInParent<DecayEntity>();
            if (componentInParent != null)
            {
                componentInParent.AddUpkeepTime(float.MaxValue);
            }
        }

        private static void ClearConnections(IOEntity IOEnt)
        {
            if (!(IOEnt == null))
            {
                for (int i = 0; i < IOEnt.inputs.Length; i++)
                {
                    IOEnt.inputs[i].Clear();
                }
                for (int j = 0; j < IOEnt.outputs.Length; j++)
                {
                    IOEnt.outputs[j].Clear();
                }
                IOEnt.ResetState();
            }
        }
        private static void ResetIOEnt(IOEntity IOEnt)
        {
            if (IOEnt is Elevator)
            {
                IOEnt.children.ForEach(delegate (BaseEntity x)
                {
                    x.EnableSaving(false);
                });
            }
            BaseEntity.Flags flags = IOEnt.flags;
            IOEnt.ResetState();
            IOEnt.flags = flags;
            IOEnt.ResetIOState();
            IOEnt.OnCircuitChanged(true);
            IOEnt.SendIONetworkUpdate();
            if (IOEnt is ReactiveTarget) (IOEnt as ReactiveTarget).ResetTarget();
        }

        private static bool IsConnectedTo(IOEntity IOEnt1, IOEntity IOEnt2, int slot)
        {
            for (int i = 0; i < IOEnt1.outputs.Length; i++)
            {
                IOEntity.IOSlot ioslot = IOEnt1.outputs[i];
                UnityEngine.Object obj;
                if (ioslot == null)
                {
                    obj = null;
                }
                else
                {
                    IOEntity.IORef connectedTo = ioslot.connectedTo;
                    obj = ((connectedTo != null) ? connectedTo.ioEnt : null);
                }
                if (!(obj == null) && ioslot.connectedTo.ioEnt == IOEnt2 && slot == ioslot.connectedToSlot)
                {
                    return true;
                }
            }
            return false;
        }
        public static bool AreEqual(BaseNetworkable bn, SerializedIOEntity IOEnt)
        {
            return bn != null && bn.PrefabName == IOEnt.fullPath && bn.transform.position == IOEnt.position;
        }
        public static bool AreEqual(BaseNetworkable bn, SerializedConnectionData sConnectionData)
        {
            return bn != null && bn.PrefabName == sConnectionData.fullPath && bn.transform.position == sConnectionData.position;
        }

        #region Elevator
        /*private static FieldInfo ioEntityField = typeof(Elevator).GetField("ioEntity", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        private static FieldInfo liftEntityField = typeof(Elevator).GetField("liftEntity", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        private static void method_8(Elevator e, SerializedIOEntity serializedIOEnt)
        {
            Elevator elevator = e;
            for (int i = 1; i < serializedIOEnt.floors; i++)
            {
                Elevator elevator2 = GameManager.server.CreateEntity(e.PrefabName, e.transform.position + e.transform.up * (3f * i), e.transform.rotation, true) as Elevator;
                elevator2.enableSaving = false;
                elevator2.pickup.enabled = false;
                elevator2.Spawn();
                DisableDecayAndStability(elevator2);
                entityList.Add(elevator2);
                elevator2.RefreshEntityLinks();
                elevator2.OnDeployed(null, null);
                elevator2.SetFlag((BaseEntity.Flags)256, true, false, false);
                elevator = elevator2;
            }
            elevator.SetFlag((BaseEntity.Flags)128, true, false, false);
            method_9(e, serializedIOEnt);
        }

        private static void method_9(Elevator elevator, SerializedIOEntity serializedIOEntity)
        {
            for (int i = 0; i < serializedIOEntity.inputs.Length; i++)
            {
                var serializedConnectionData = serializedIOEntity.inputs[i];
                if (serializedConnectionData != null)
                {
                    IOEntity ioEntity = null;
                    foreach(var ent in entityList)
                    {
                        if(AreEqual(ent, serializedIOEntity))
                        {
                            ioEntity = ent as IOEntity;
                        }
                    }
                    if (ioEntity != null)
                    {
                        if (i == 0)
                        {
                            method_11(elevator, ioEntity, serializedConnectionData.connectedTo);
                        }
                        else
                        {
                            method_17(elevator, DivideByHalfAndMinusOne(i));
                            IOEntity.IOSlot ioslot = elevator.inputs[i % 2];
                            ioslot.connectedTo.ioEnt = ioEntity;
                            ioslot.connectedTo.Set(ioEntity);
                            ioslot.connectedToSlot = serializedConnectionData.connectedTo;
                        }
                    }
                }
            }
        }

        private static void ProcessElevator(IOEntity ioEntity, Elevator elevator_0, int int_0, SerializedConnectionData serializedConnectionData)
        {
            int connectedTo = serializedConnectionData.connectedTo;
            if (connectedTo != 0)
            {
                Elevator elevator = method_17(elevator_0, DivideByHalfAndMinusOne(connectedTo));
                if (!IsConnectedTo(ioEntity, elevator, serializedConnectionData.connectedTo))
                {
                    IOEntity.IOSlot ioslot = ioEntity.outputs[int_0];
                    ioslot.connectedTo.ioEnt = elevator;
                    ioslot.connectedTo.Set(elevator);
                    ioslot.connectedToSlot = connectedTo % 2;
                    ioslot.type = 0;
                }
            }
            else
            {
                method_11(elevator_0, ioEntity, int_0);
            }
        }

        private static void method_11(Elevator elevator_0, IOEntity ioentity_0, int int_0)
        {
            try
            {
                Elevator elevator_ = method_18(elevator_0);
                method_13(elevator_);
                IOEntity ioentity = method_12(elevator_);
                IOEntity.IOSlot ioslot = ioentity.inputs[0];
                ioslot.connectedTo.ioEnt = ioentity_0;
                ioslot.connectedTo.Set(ioentity_0);
                ioslot.connectedToSlot = int_0;
                IOEntity.IOSlot ioslot2 = ioentity_0.outputs[int_0];
                ioslot2.connectedTo.ioEnt = ioentity;
                ioslot2.connectedTo.Set(ioentity);
                ioslot2.connectedToSlot = 0;
                ResetIOEnt(ioentity);
            } catch(Exception) { Logger.Log("Caught exception while method11 elevator"); }
        }

        private static IOEntity method_12(Elevator elevator)
        {
            var ioentity = ioEntityField.GetValue(elevator) as IOEntity;
            if (ioentity == null)
            {
                ioentity = GetIOEntity(elevator);
                if (ioentity == null)
                {
                    ioentity = (GameManager.server.CreateEntity(elevator.IoEntityPrefab.resourcePath, elevator.IoEntitySpawnPoint.position, elevator.IoEntitySpawnPoint.rotation, true) as IOEntity);
                    ioentity.enableSaving = false;
                    ioentity.SetParent(elevator, true, false);
                    ioentity.Spawn();
                    ioEntityField.SetValue(elevator, ioentity);
                }
            }
            if (ioentity != null)
            {
                ioentity.EnableSaving(false);
            }
            return ioentity;
        }

        private static void method_13(Elevator elevator)
        {
            ElevatorLift elevatorLift = liftEntityField.GetValue(elevator) as ElevatorLift;
            if (elevatorLift == null)
            {
                elevatorLift = GetElevatorLift(elevator);
                if (elevatorLift == null)
                {
                    elevatorLift = (GameManager.server.CreateEntity(elevator.LiftEntityPrefab.resourcePath, GetPosition(elevator, elevator.Floor), elevator.LiftRoot.rotation, true) as ElevatorLift);
                    elevatorLift.SetParent(elevator, true, false);
                    elevatorLift.enableSaving = false;
                    elevatorLift.Spawn();
                    liftEntityField.SetValue(elevator, elevatorLift);
                    return;
                }
            }
            if (elevatorLift != null)
            {
                elevatorLift.pickup.enabled = false;
                elevatorLift.SendNetworkUpdate(0);
                entityList.Add(elevatorLift);
            }
        }
        private static Vector3 GetPosition(Elevator elevator, int floor)
        {
            int num = elevator.Floor - floor;
            Vector3 vector = elevator.transform.up * ((float)num * 3f);
            vector.y -= 1f;
            return elevator.transform.position - vector;
        }
        private static IOEntity GetIOEntity(Elevator elevator)
        {
            foreach (BaseEntity baseEntity in elevator.children)
            {
                IOEntity ioentity = baseEntity as IOEntity;
                if (ioentity != null)
                {
                    return ioentity;
                }
            }
            return null;
        }

        private static ElevatorLift GetElevatorLift(Elevator elevator)
        {
            foreach (BaseEntity baseEntity in elevator.children)
            {
                ElevatorLift elevatorLift = baseEntity as ElevatorLift;
                if (elevatorLift != null)
                {
                    return elevatorLift;
                }
            }
            return null;
        }

        private static Elevator method_17(Elevator elevator_0, int int_0)
        {
            Elevator elevator = elevator_0;
            for (int i = 0; i < int_0; i++)
            {
                if (elevator == null)
                {
                    return null;
                }
                EntityLink entityLink = elevator.FindLink("elevator/sockets/elevator-male");
                object obj;
                if (entityLink == null)
                {
                    obj = null;
                }
                else
                {
                    EntityLink entityLink2 = entityLink.connections[0];
                    obj = ((entityLink2 != null) ? entityLink2.owner : null);
                }
                elevator = (obj as Elevator);
            }
            return elevator;
        }

        private static Elevator method_18(Elevator elevator)
        {
            for (int i = 0; i < 2147483647; i++)
            {
                EntityLink entityLink = elevator.FindLink("elevator/sockets/elevator-male");
                List<EntityLink> list = (entityLink != null) ? entityLink.connections : null;
                if (list.Count <= 0 || !(list[0].owner as Elevator != null))
                {
                    return elevator;
                }
                elevator = (list[0].owner as Elevator);
            }
            return elevator;
        }
        private static int DivideByHalfAndMinusOne(int num)
        {
            return Mathf.CeilToInt(num / 2f) - 1;
        }*/
        #endregion
    }
}
