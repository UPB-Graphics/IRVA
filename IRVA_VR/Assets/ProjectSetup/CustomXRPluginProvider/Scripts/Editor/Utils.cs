using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProjectSetup.CustomXRPluginProvider.Scripts.Editor
{
    /// <summary>
    /// XR plug-in management provider helpers.
    /// </summary>
    public static class Utils
    {
        private const string XR_CRDBRD_LOADER = "Google.XR.Cardboard.XRLoader";
        private const string XR_OPENVR_LOADER = "Unity.XR.OpenVR.OpenVRLoader";
        private const string XR_OPENXR_LOADER = "UnityEngine.XR.OpenXR.OpenXRLoader";
        
        public static readonly Dictionary<TargetVR, string> StandaloneLoaders = new()
        {
            { TargetVR.CardboardXR, null},                // N/A.
            { TargetVR.SteamVR,     XR_OPENVR_LOADER},    // Native SteamVR.
            { TargetVR.MetaXR,      XR_OPENXR_LOADER}     // Meta's recommended.
        };        
        public static readonly Dictionary<TargetVR, string> AndroidLoaders = new()
        {
            { TargetVR.CardboardXR, XR_CRDBRD_LOADER},    // Native Cardboard.
            { TargetVR.SteamVR,     null},                // N/A.
            { TargetVR.MetaXR,      XR_OPENXR_LOADER}     // Native Quest.
        };
        public static readonly Dictionary<TargetVR, string> IOSLoaders = new()
        {
            { TargetVR.CardboardXR, XR_CRDBRD_LOADER},    // Native Cardboard.
            { TargetVR.SteamVR,     null},                // N/A.
            { TargetVR.MetaXR,      null}                 // N/A.
        };

        public static readonly List<string> AllXRLoaders = new()
        {
            XR_CRDBRD_LOADER,
            XR_OPENVR_LOADER,
            XR_OPENXR_LOADER,
        };
    
        public enum TargetVR
        {
            None = 0,
            CardboardXR = 1,
            SteamVR = 2,
            MetaXR = 3,
        }
        
        public static T FindScriptableObject<T>(string filter) where T : ScriptableObject
        {
            var guids = AssetDatabase.FindAssets(filter);
            if (guids.Length == 0)
            {
                // Debug.LogError($"[Utils] GUIDs not found for filter: {filter}");
                return null;
            }
            
            if (guids.Length > 1)
            {
                Debug.LogError($"[Utils] Too many GUIDs found for filter: {filter}");
                return null;
            }

            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            
            return (T)AssetDatabase.LoadAssetAtPath(path, typeof(T));
        }
    }
    
    #region Deprecated
    
    // public const string XR_ARCORE_LOADER = "UnityEngine.XR.ARCore.ARCoreLoader";
    // public const string XR_OCULUS_LOADER = "Unity.XR.Oculus.OculusLoader";
    // public const string XR_MOCK_LOADER   = "Unity.XR.MockHMD.MockHMDLoader";
    // public const string XR_ARKIT_LOADER  = "UnityEngine.XR.ARKit.ARKitLoader";
    
    #endregion
}
