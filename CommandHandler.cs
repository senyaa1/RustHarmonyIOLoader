using System;
using System.Collections.Generic;
using HarmonyIOLoader.APC;
using HarmonyIOLoader.Loot;
using UnityEngine;

namespace HarmonyIOLoader
{
    public static class CommandHandler
    {
        #region PathVisualization
        public static void ShowBradleyPath(BasePlayer player)
        {
            try
            {
                if (!player.IsAdmin || APCProcessor.serializedAPCPathList == null) return;
                Vector3 prevNode;

                foreach (var path in APCProcessor.serializedAPCPathList.paths)
                {
                    prevNode = new Vector3();
                    foreach (var pathNode in path.nodes)
                    {
                        Arrow(player, pathNode + new Vector3(0, 10f, 0), pathNode, 3f, Color.green, 100f);
                        Text(player, pathNode + new Vector3(0, 10f, 0), "Path node: " + path.nodes.IndexOf(pathNode), Color.green, 100f);
                        if (prevNode != new Vector3())
                            Line(player, prevNode, pathNode, Color.blue, 100f);
                        prevNode = pathNode;
                    }
                    Line(player, OceanPathProcessor.serializedPathList.vectorData[0], prevNode, Color.magenta, 100f);
                    foreach(var interestNode in path.interestNodes)
                    {
                        Arrow(player, interestNode + new Vector3(0, 10f, 0), interestNode, 3f, Color.red, 100f);
                        Text(player, interestNode, "Interest node: " + path.interestNodes.IndexOf(interestNode), Color.red, 100f);
                    }
                }
            }
            catch (Exception) { }
        }

        public static void ShowOceanPath(BasePlayer player)
        {
            try
            {
                if (!player.IsAdmin || OceanPathProcessor.serializedPathList == null) return;
                Vector3 prevNode = new Vector3();
                foreach (var sPathNode in OceanPathProcessor.serializedPathList.vectorData)
                {
                    Arrow(player, sPathNode + new Vector3(0, 10f, 0), sPathNode, 40f, Color.magenta, 100f);
                    Text(player, sPathNode + new Vector3(0, 10f, 0), "Path node : " + OceanPathProcessor.serializedPathList.vectorData.IndexOf(sPathNode), Color.magenta, 100f);
                    if (prevNode != new Vector3())
                        Line(player, prevNode, sPathNode, Color.magenta, 100f);
                    prevNode = sPathNode;
                }
                Line(player, OceanPathProcessor.serializedPathList.vectorData[0], prevNode, Color.magenta, 100f);
            }
            catch (Exception) { }
        }

        public static void ShowNPCSpawners(BasePlayer player)
        {
            try
            {
                if (!player.IsAdmin || NPCProcessor.serializedNPCData == null) return;
                foreach (var spawner in NPCProcessor.serializedNPCData.npcSpawners)
                {
                    Arrow(player, spawner.position + new Vector3(0, 2f, 0), spawner.position, 5f, Color.green, 100f);
                    Text(player, spawner.position + new Vector3(0, 2f, 0), "NPCType: " + spawner.npcType, Color.green, 100f);
                }
            }
            catch (Exception) { }
        }

        #endregion
        public static void SpawnBradley(BasePlayer player)
        {
            try
            {
                if (!player.IsAdmin || APCProcessor.customAPCSpawners == null) return;
                for (int i = 0; i < APCProcessor.customAPCSpawners.Count; i++)
                {
                    CustomAPCSpawner customAPCSpawner = APCProcessor.customAPCSpawners[i];
                    if (!(customAPCSpawner == null) && (customAPCSpawner.BradleyAPC == null || !customAPCSpawner.BradleyAPC.IsAlive()))
                    {
                        customAPCSpawner.SpawnBradley();
                    }
                }
            }
            catch (Exception) { }
        }
        public static void Line(BasePlayer player, Vector3 from, Vector3 to, Color color, float duration) => player.SendConsoleCommand("ddraw.line", duration, color, from, to);
		public static void Text(BasePlayer player, Vector3 pos, string text, Color color, float duration) => player.SendConsoleCommand("ddraw.text", duration, color, pos, text);
		public static void Box(BasePlayer player, Vector3 pos, float size, Color color, float duration) => player.SendConsoleCommand("ddraw.box", duration, color, pos, size);
		public static void Arrow(BasePlayer player, Vector3 from, Vector3 to, float headSize, Color color, float duration) => player.SendConsoleCommand("ddraw.arrow", duration, color, from, to, headSize);
        public static void Reply(BasePlayer player, string message) => player.SendConsoleCommand("chat.add", new object[] { 2, player.userID, message });
    }
}
