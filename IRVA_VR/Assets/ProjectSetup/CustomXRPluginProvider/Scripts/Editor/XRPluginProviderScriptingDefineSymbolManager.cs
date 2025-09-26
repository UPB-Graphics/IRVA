#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;

namespace ProjectSetup.CustomXRPluginProvider.Scripts.Editor
{
    /// <summary>
    /// Script which adds or removes `UNITY_XR_MANAGEMENT` from the project's Scripting defines symbols if
    /// the XR Plug-In Manager is installed or not.
    /// </summary>
    [InitializeOnLoad]
    public class XRPluginProviderScriptingDefineSymbolManager
    {
        private static List<NamedBuildTarget> _namedBuildTargets = new() { NamedBuildTarget.Standalone, NamedBuildTarget.Android, NamedBuildTarget.iOS };
        
        static XRPluginProviderScriptingDefineSymbolManager()
        {
            var isXRManagementInstalled = Type.GetType("UnityEngine.XR.Management.XRGeneralSettings, Unity.XR.Management") != null;

            if (isXRManagementInstalled) AddDefineIfNeeded("UNITY_XR_MANAGEMENT");
            else RemoveDefineIfNeeded("UNITY_XR_MANAGEMENT");
        }

        private static void AddDefineIfNeeded(string define)
        {
            _namedBuildTargets.ForEach(btg =>
            {
                var definesString = PlayerSettings.GetScriptingDefineSymbols(btg);
                if (!definesString.Contains(define))
                {
                    PlayerSettings.SetScriptingDefineSymbols(btg, definesString + ";" + define);
                }
            });
        }

        private static void RemoveDefineIfNeeded(string define)
        {
            _namedBuildTargets.ForEach(btg =>
            {
                var definesString = PlayerSettings.GetScriptingDefineSymbols(btg);
                if (definesString.Contains(define))
                {
                    definesString = definesString.Replace(define, "");
                    PlayerSettings.SetScriptingDefineSymbols(btg, definesString);
                }
            });
        }
    }
}

#endif