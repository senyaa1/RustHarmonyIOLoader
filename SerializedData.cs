﻿using System;
using System.Collections.Generic;
using ProtoBuf;

namespace HarmonyIOLoader
{
    [ProtoContract]
    [Serializable]
    public class SerializedConnectionData
    {
        [ProtoMember(1)]
        public string fullPath;

        [ProtoMember(2)]
        public VectorData position;

        [ProtoMember(3)]
        public bool input;

        [ProtoMember(4)]
        public int connectedTo;

        [ProtoMember(5)]
        public int type;
    }

    [ProtoContract]
    [Serializable]
    public class SerializedIOData
    {
        [ProtoMember(1)]
        public List<SerializedIOEntity> entities = new List<SerializedIOEntity>();
    }

    [ProtoContract]
    [Serializable]
    public class SerializedIOEntity
    {
        [ProtoMember(1)]
        public string fullPath;

        [ProtoMember(2)]
        public VectorData position;

        [ProtoMember(3)]
        public SerializedConnectionData[] inputs;

        [ProtoMember(4)]
        public SerializedConnectionData[] outputs;

        [ProtoMember(5)]
        public int accessLevel;

        [ProtoMember(6)]
        public int doorEffect;

        [ProtoMember(7)]
        public float timerLength;

        [ProtoMember(8)]
        public int frequency;

        [ProtoMember(9)]
        public bool unlimitedAmmo;

        [ProtoMember(10)]
        public bool peaceKeeper;

        [ProtoMember(11)]
        public string autoTurretWeapon;

        [ProtoMember(12)]
        public int branchAmount;

        [ProtoMember(13)]
        public int targetCounterNumber;

        [ProtoMember(14)]
        public string rcIdentifier;

        [ProtoMember(15)]
        public bool counterPassthrough;

        [ProtoMember(16)]
        public int floors = 1;

        [ProtoMember(17)]
        public string phoneName;
    }
}
