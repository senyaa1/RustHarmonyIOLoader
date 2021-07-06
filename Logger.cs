using Harmony;

namespace HarmonyIOLoader
{
    static class Logger
    {
        public static void LogMessage(string msg)
        {
            UnityEngine.Debug.Log("[HarmonyIOLoader] " + msg);
            FileLog.Log(msg);
            FileLog.FlushBuffer();
        }

        public static void LogWarning(string msg)
        {
            UnityEngine.Debug.LogWarning("[HarmonyIOLoader] " + msg);
            FileLog.Log("WARNING: " + msg);
            FileLog.FlushBuffer();
        }

        public static void LogError(string msg)
        {
            UnityEngine.Debug.LogError("[HarmonyIOLoader] " + msg);
            FileLog.Log("ERROR: " + msg);
            FileLog.FlushBuffer();
        }
    }
}
