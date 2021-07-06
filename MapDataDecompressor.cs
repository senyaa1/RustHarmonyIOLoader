using System;
using System.IO;
using System.Xml.Serialization;
using Harmony;

namespace HarmonyIOLoader
{
    static class MapDataDecompressor
    {
        public static string IOMapName { get; set; }
        public static string OceanPathMapName { get; set; }
        public static string NPCMapName { get; set; }
        public static string APCMapName { get; set; }

        public readonly static string VanillaMapNames = "heightbuildingblocksterrainsplatalphabiometopologywater";
        public static bool IsLoaded = false;
        public static T Deserialize<T>(byte[] byteArr, out bool notNull)
        {
            T result;
            try
            {
                using (MemoryStream memoryStream = new MemoryStream(byteArr))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                    T t = (T)xmlSerializer.Deserialize(memoryStream);
                    notNull = true;
                    result = t;
                }
            }
            catch (Exception ex)
            {
                FileLog.Log(ex.Message + ex.StackTrace);
                notNull = false;
                result = default;
            }
            return result;
        }
    }
}
