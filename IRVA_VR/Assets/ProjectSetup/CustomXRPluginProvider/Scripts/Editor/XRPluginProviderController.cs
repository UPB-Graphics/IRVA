#if UNITY_EDITOR
#if UNITY_XR_MANAGEMENT

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;
using UnityEngine.XR.Management;

namespace ProjectSetup.CustomXRPluginProvider.Scripts.Editor
{
    /// <summary>
    /// Logic related to XR plug-in management provider setup for either Android (Cardboard XR, Meta XR),
    /// Standalone (SteamVR, Oculus Link), or iOS (Cardboard XR).
    /// </summary>
    [InitializeOnLoad]
    public class XRPluginProviderController : IPreprocessBuildWithReport
    {
        private static BuildTargetGroup buildTargetGroup = BuildTargetGroup.Unknown;
        private static XRGeneralSettings xrGeneralSettings;
        private static XRPluginProviderControllerData pluginProviderData;
        private static XRGeneralSettingsPerBuildTarget xrGeneralSettingsPerBuildTarget;
        public int callbackOrder { get; }
        
        /// <summary>
        /// Called via [InitializeOnLoad].
        /// </summary>
        static XRPluginProviderController() => SetupAllXRPluginProviderSettings();

        /// <summary>
        /// Called when a build is started. Sets the Android or iOS XR plug-in provider.<br/>
        /// - Android for Cardboard XR and Meta XR builds.<br/>
        /// - iOS for Cardboard XR builds.
        /// </summary>
        /// <param name="report">Unused.</param>
        public void OnPreprocessBuild(BuildReport report)
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                SetupXRPluginProvider(BuildTargetGroup.Android, Utils.AndroidLoaders);
            }
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                SetupXRPluginProvider(BuildTargetGroup.iOS, Utils.IOSLoaders);
            }
        }
        
        [MenuItem("XR Loader Settings/None [won't affect XR settings]", priority = 0)]
        private static void SetNoPluginProvider()
        {
            pluginProviderData.TargetVR = Utils.TargetVR.None;
            SaveScriptableObjects();
            Debug.Log("[XRPluginProviderController] <color=\"#b5b3fc\"> No XR Loader selected. You can now freely change the settings <b>[Edit > Project Settings > XR Plug-In Management]</b>.</color>");
        }

        [MenuItem("XR Loader Settings/Set Cardboard XR loader", priority = 20)]
        private static void SetCardboardXRPluginProvider()
        {
            pluginProviderData.TargetVR = Utils.TargetVR.CardboardXR;
            SetupAllXRPluginProviderSettings();
            SaveScriptableObjects();
        }
        
        [MenuItem("XR Loader Settings/Set Steam VR loader", priority = 21)]
        private static void SetSteamVRPluginProvider()
        {
            pluginProviderData.TargetVR = Utils.TargetVR.SteamVR;
            SetupAllXRPluginProviderSettings();
            SaveScriptableObjects();
        }
        
        [MenuItem("XR Loader Settings/Set Meta XR loader", priority = 22)]
        private static void SetMetaXRPluginProvider()
        {
            pluginProviderData.TargetVR = Utils.TargetVR.MetaXR;
            SetupAllXRPluginProviderSettings();
            SaveScriptableObjects();
        }
        
        [MenuItem("XR Loader Settings/Refresh [reapply current settings]", priority = 40)]
        private static void RefreshPluginProvider()
        {
            SetupAllXRPluginProviderSettings();
            SaveScriptableObjects();
        }

        [MenuItem("XR Loader Settings/None [won't affect XR settings]", true)]
        private static bool SetNoPluginProviderValidate()
        {
            Menu.SetChecked("XR Loader Settings/None [won't affect XR settings]", pluginProviderData.TargetVR is Utils.TargetVR.None);
            return pluginProviderData.TargetVR != Utils.TargetVR.None;
        }
        
        [MenuItem("XR Loader Settings/Set Cardboard XR loader", true)]
        private static bool SetCardboardXRPluginProviderValidate()
        {
            Menu.SetChecked("XR Loader Settings/Set Cardboard XR loader", pluginProviderData.TargetVR is Utils.TargetVR.CardboardXR);
            return pluginProviderData.TargetVR != Utils.TargetVR.CardboardXR;
        }
        
        [MenuItem("XR Loader Settings/Set Steam VR loader", true)]
        private static bool SetSteamVRPluginProviderValidate()
        {
            Menu.SetChecked("XR Loader Settings/Set Steam VR loader", pluginProviderData.TargetVR is Utils.TargetVR.SteamVR);
            return pluginProviderData.TargetVR != Utils.TargetVR.SteamVR;
        }
        
        [MenuItem("XR Loader Settings/Set Meta XR loader", true)]
        private static bool SetMetaXRPluginProviderValidate()
        {
            Menu.SetChecked("XR Loader Settings/Set Meta XR loader", pluginProviderData.TargetVR is Utils.TargetVR.MetaXR);
            return pluginProviderData.TargetVR != Utils.TargetVR.MetaXR;
        }
        
        private static void SetupAllXRPluginProviderSettings()
        {
            LoadRequiredResources();
            
            if(pluginProviderData.TargetVR == Utils.TargetVR.None) return;
            
            InitializeXRSettingsForAllPlatforms();
            
            SetupXRPluginProvider(BuildTargetGroup.Standalone, Utils.StandaloneLoaders);
            SetupXRPluginProvider(BuildTargetGroup.Android, Utils.AndroidLoaders);
            SetupXRPluginProvider(BuildTargetGroup.iOS, Utils.IOSLoaders);
        }
        
        /// <summary>
        /// Load scriptable object which stores persistent data (for TargetVR).
        /// </summary>
        private static void LoadRequiredResources()
        { 
            if (pluginProviderData == null) pluginProviderData = Utils.FindScriptableObject<XRPluginProviderControllerData>("t:XRPluginProviderControllerData");
            if (xrGeneralSettingsPerBuildTarget == null) xrGeneralSettingsPerBuildTarget = Utils.FindScriptableObject<XRGeneralSettingsPerBuildTarget>("t:XRGeneralSettingsPerBuildTarget");
        }
        
        /// <summary>
        /// Makes sure that each target platform (Standalone, Android, iOS) has a valid set of XR-related settings.
        /// </summary>
        private static void InitializeXRSettingsForAllPlatforms()
        {
            var buildTargetGroups = new[] { BuildTargetGroup.Standalone, BuildTargetGroup.Android, BuildTargetGroup.iOS };
            foreach (var btg in buildTargetGroups)
            {
                if(xrGeneralSettingsPerBuildTarget == null) return;
                
                if (!xrGeneralSettingsPerBuildTarget.HasSettingsForBuildTarget(btg))
                {
                    xrGeneralSettingsPerBuildTarget.CreateDefaultManagerSettingsForBuildTarget(btg);
                }
            }
        }

        /// <summary>
        /// Sets the XR plug-in management provider as such:<br/>
        /// - BuildTargetGroup.Standalone for SteamVR, Quest Link.<br/>
        /// - BuildTargetGroup.Android for Cardboard XR, Meta XR.<br/>
        /// - BuildTargetGroup.iOS for Cardboard XR.<br/>
        /// </summary>
        /// <param name="btg">Target platform group.</param>
        /// <param name="loaders">Dictionary which contains loader settings.</param>
        private static void SetupXRPluginProvider(BuildTargetGroup btg, IReadOnlyDictionary<Utils.TargetVR, string> loaders)
        {
            if(Application.isPlaying || BuildPipeline.isBuildingPlayer || pluginProviderData.TargetVR == Utils.TargetVR.None)
            {
                return;
            }
            
            var settingsFetched = TryGetSettingsForBuildTarget(btg);
            if(!settingsFetched)
            {
                return;
            }
            
            // Clear all loaders first, regardless of what we're setting next.
            Utils.AllXRLoaders.ForEach(RemoveLoader);
            
            if (!loaders.TryGetValue(pluginProviderData.TargetVR, out var requiredLoader) || requiredLoader == null)
            {
                Debug.Log($"[XRPluginProviderController] <color=\"#fcb3b3\"> <b>{pluginProviderData.TargetVR}</b> is not supported on <b>{btg}</b>. No XR loader will be set.</color>");
                return;
            }
            
            if(IsLoaderAlreadySet(requiredLoader))
            {
                return;
            }
            
            AssignLoader(btg, requiredLoader);
        }

        // Check if loader attempted to be set is already contained in the active loader list.
        private static bool IsLoaderAlreadySet(string loaderStr) => xrGeneralSettings.AssignedSettings.activeLoaders.Any(loader => loader.ToString().Contains(loaderStr));

        private static bool TryGetSettingsForBuildTarget(BuildTargetGroup btg)
        {
            EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey, out XRGeneralSettingsPerBuildTarget buildTargetSettings);
            
            if (buildTargetSettings == null)
            {
                Debug.LogError("[XRPluginProviderController] This operation is not permitted until you open Edit > Project Settings > XR Plug-In Management editor window at least once.");
                SetNoPluginProvider();
                return false;
            }

            xrGeneralSettings = buildTargetSettings.SettingsForBuildTarget(buildTargetGroup = btg);
            return true;
        }

        private static void AssignLoader(BuildTargetGroup btg, string loader)
        {
            // We need to search for this loader type across all assemblies.
            Type loaderType = null;
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                loaderType = assembly.GetType(loader);
                
                // Abort loop if found.
                if (loaderType != null) break;
            }
            
            if (loaderType == null)
            {
                Debug.Log($"[XRPluginProviderController] <color=\"#fcecb3\"> Loader <b>{loader}</b> cannot be assigned for target <b>{btg}</b> (have you installed the required package for <b>{pluginProviderData.TargetVR}</b>?).</color>");
                return;
            }
            
            XRPackageMetadataStore.AssignLoader(xrGeneralSettings.Manager, loader, buildTargetGroup);

            Debug.Log($"[XRPluginProviderController] <color=#b3fcb5> Set loader <b>{loader}</b> for target <b>{buildTargetGroup}</b></color>.");
        }

        private static void RemoveLoader(string loader) => XRPackageMetadataStore.RemoveLoader(xrGeneralSettings.Manager, loader, buildTargetGroup);
        
        private static void SaveScriptableObjects()
        {
            EditorUtility.SetDirty(pluginProviderData);
            if (xrGeneralSettingsPerBuildTarget != null)
            {
                EditorUtility.SetDirty(xrGeneralSettingsPerBuildTarget);
            }
            AssetDatabase.SaveAssets();
        }
    }
}

#endif
#endif