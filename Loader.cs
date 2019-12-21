﻿using ColossalFramework;
using ColossalFramework.Math;
using ColossalFramework.PlatformServices;
using ColossalFramework.Plugins;
using ColossalFramework.UI;
using CSURToolBox.CustomAI;
using CSURToolBox.CustomData;
using CSURToolBox.UI;
using CSURToolBox.Util;
using ICities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace CSURToolBox
{
    public class Loader : LoadingExtensionBase
    {
        public static UIView parentGuiView;
        public static MainUI mainUI;
        public static LoadMode CurrentLoadMode;
        public static bool isGuiRunning = false;
        public static MainButton mainButton;
        public static string m_atlasName = "CSUR_UI";
        public static string m_atlasName1 = "CSUR_UI1";
        public static string m_atlasName2 = "CSUR_UI2";
        public static string m_atlasNameHeader = "CSUR_UI_Header";
        public static string m_atlasNameBg = "CSUR_UI_Bg";
        public static string m_atlasNameNoAsset = "CSUR_UI_NoAssert";
        public static bool m_atlasLoaded;
        public static bool is583429740 = false;
        public static bool is1637663252 = false;
        public static bool is1806963141 = false;
        public static bool Done { get; private set; } // Only one Assets installation throughout the application
        public class Detour
        {
            public MethodInfo OriginalMethod;
            public MethodInfo CustomMethod;
            public RedirectCallsState Redirect;

            public Detour(MethodInfo originalMethod, MethodInfo customMethod)
            {
                this.OriginalMethod = originalMethod;
                this.CustomMethod = customMethod;
                this.Redirect = RedirectionHelper.RedirectCalls(originalMethod, customMethod);
            }
        }

        public static List<Detour> Detours { get; set; }
        public static bool DetourInited = false;
        public static bool isMoveItRunning = false;


        public override void OnCreated(ILoading loading)
        {
            Detours = new List<Detour>();
            base.OnCreated(loading);
        }
        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);
            CurrentLoadMode = mode;
            if (CSURToolBox.IsEnabled)
            {
                if (mode == LoadMode.LoadGame || mode == LoadMode.NewGame || mode == LoadMode.NewMap || mode == LoadMode.LoadMap || mode == LoadMode.NewAsset || mode == LoadMode.LoadAsset)
                {
                    OptionUI.LoadSetting();
                    SetupGui();
                    CheckTMPE();
                    InitDetour();
                    DebugLog.LogToFileOnly("OnLevelLoaded");
                    if (mode == LoadMode.NewGame)
                    {
                        //InitData();
                        DebugLog.LogToFileOnly("InitData");
                    }
                }
            }
        }

        public override void OnLevelUnloading()
        {
            base.OnLevelUnloading();
            is583429740 = false;
            is1637663252 = false;
            is1806963141 = false;
            if (CurrentLoadMode == LoadMode.LoadGame || CurrentLoadMode == LoadMode.NewGame || CurrentLoadMode == LoadMode.LoadMap || CurrentLoadMode == LoadMode.NewMap || CurrentLoadMode == LoadMode.LoadAsset || CurrentLoadMode == LoadMode.NewAsset)
            {
                if (CSURToolBox.IsEnabled)
                {
                    RevertDetour();
                    if (isGuiRunning)
                    {
                        RemoveGui();
                    }
                }
            }
        }

        private static void LoadSprites()
        {
            if (SpriteUtilities.GetAtlas(m_atlasName) != null) return;
            var modPath = PluginManager.instance.FindPluginInfo(Assembly.GetExecutingAssembly()).modPath;
            m_atlasLoaded = SpriteUtilities.InitialiseAtlas(Path.Combine(modPath, "Resources/CSUR.png"), m_atlasName);
            m_atlasLoaded &= SpriteUtilities.InitialiseAtlas(Path.Combine(modPath, "Resources/CSUR1.png"), m_atlasName1);
            m_atlasLoaded &= SpriteUtilities.InitialiseAtlas(Path.Combine(modPath, "Resources/CSUR2.png"), m_atlasName2);
            m_atlasLoaded &= SpriteUtilities.InitialiseAtlas(Path.Combine(modPath, "Resources/UIBG.png"), m_atlasNameBg);
            m_atlasLoaded &= SpriteUtilities.InitialiseAtlas(Path.Combine(modPath, "Resources/UITOP.png"), m_atlasNameHeader);
            m_atlasLoaded &= SpriteUtilities.InitialiseAtlas(Path.Combine(modPath, "Resources/Notfound.png"), m_atlasNameNoAsset);
            if (m_atlasLoaded)
            {
                var spriteSuccess = true;
                spriteSuccess = SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(2, 2), new Vector2(30, 30)), "0P_R", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(34, 2), new Vector2(30, 30)), "0P_L", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(66, 2), new Vector2(30, 30)), "0P_S", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(98, 2), new Vector2(30, 30)), "1_R", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(130, 2), new Vector2(30, 30)), "1_L", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(162, 2), new Vector2(30, 30)), "1_S", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(194, 2), new Vector2(30, 30)), "1P_R", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(226, 2), new Vector2(30, 30)), "1P_L", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(258, 2), new Vector2(30, 30)), "1P_S", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(290, 2), new Vector2(30, 30)), "2_R", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(322, 2), new Vector2(30, 30)), "2_L", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(354, 2), new Vector2(30, 30)), "2_S", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(386, 2), new Vector2(30, 30)), "2P_R", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(418, 2), new Vector2(30, 30)), "2P_S", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(452, 2), new Vector2(30, 30)), "3_R", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(482, 2), new Vector2(30, 30)), "3_S", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(514, 2), new Vector2(30, 30)), "3P_R", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(546, 2), new Vector2(30, 30)), "3P_S", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(578, 2), new Vector2(30, 30)), "4_R", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(610, 2), new Vector2(30, 30)), "4_S", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(642, 2), new Vector2(30, 30)), "4P_R", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(674, 2), new Vector2(30, 30)), "4P_S", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(706, 2), new Vector2(30, 30)), "5_R", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(738, 2), new Vector2(30, 30)), "5_S", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(770, 2), new Vector2(30, 30)), "5P_R", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(802, 2), new Vector2(30, 30)), "5P_S", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(834, 2), new Vector2(30, 30)), "6_R", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(866, 2), new Vector2(30, 30)), "6_S", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(898, 2), new Vector2(30, 30)), "6P_R", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(930, 2), new Vector2(30, 30)), "6P_S", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(962, 2), new Vector2(30, 30)), "7_R", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(994, 2), new Vector2(30, 30)), "7_S", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(1026, 2), new Vector2(30, 30)), "C_C", m_atlasName)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(1058, 2), new Vector2(30, 30)), "C_S", m_atlasName)
                             && spriteSuccess;
                spriteSuccess = SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(2, 2), new Vector2(30, 30)), "+0", m_atlasName1)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(34, 2), new Vector2(30, 30)), "+1", m_atlasName1)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(66, 2), new Vector2(30, 30)), "+2", m_atlasName1)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(98, 2), new Vector2(30, 30)), "SWAP", m_atlasName1)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(130, 2), new Vector2(30, 30)), "SWAP_S", m_atlasName1)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(162, 2), new Vector2(30, 30)), "SIDEWALK", m_atlasName1)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(194, 2), new Vector2(30, 30)), "0_S", m_atlasName1)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(226, 2), new Vector2(30, 30)), "0", m_atlasName1)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(258, 2), new Vector2(30, 30)), "COPY", m_atlasName1)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(290, 2), new Vector2(30, 30)), "COPY_S", m_atlasName1)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(322, 2), new Vector2(30, 30)), "UTURN", m_atlasName1)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(354, 2), new Vector2(30, 30)), "UTURN_S", m_atlasName1)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(386, 2), new Vector2(30, 30)), "NOSIDEWALK", m_atlasName1)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(418, 2), new Vector2(30, 30)), "CLEAR", m_atlasName1)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(450, 2), new Vector2(30, 30)), "CLEAR_S", m_atlasName1)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(482, 2), new Vector2(30, 30)), "BIKE", m_atlasName1)
                             && spriteSuccess;
                spriteSuccess = SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(2, 2), new Vector2(60, 50)), "CSUR_BUTTON_S", m_atlasName2)
                             && SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(64, 2), new Vector2(60, 50)), "CSUR_BUTTON", m_atlasName2)
                             && spriteSuccess;
                spriteSuccess &= SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(0, 0), new Vector2(566, 210)), "UIBG", m_atlasNameBg);
                spriteSuccess &= SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(0, 0), new Vector2(565, 35)), "UITOP", m_atlasNameHeader);
                spriteSuccess &= SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(0, 0), new Vector2(150, 150)), "NOASSET", m_atlasNameNoAsset);
                if (!spriteSuccess) DebugLog.LogToFileOnly("Error: Some sprites haven't been loaded. This is abnormal; you should probably report this to the mod creator.");
            }
            else DebugLog.LogToFileOnly("Error: The texture atlas (provides custom icons) has not loaded. All icons have reverted to text prompts.");
        }

        public void CheckTMPE()
        {
            if ((IsSteamWorkshopItemSubscribed(583429740) && IsSteamWorkshopItemSubscribed(1637663252)) || (IsSteamWorkshopItemSubscribed(1806963141) && IsSteamWorkshopItemSubscribed(1637663252)) || (IsSteamWorkshopItemSubscribed(583429740) && IsSteamWorkshopItemSubscribed(1806963141)))
            {
                UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel").SetMessage("Incompatibility Issue", "Can not sub two TM:PE, steamID:583429740 or 1637663252 or 1806963141", true);
            }
            else if (IsSteamWorkshopItemSubscribed(583429740))
            {
                is583429740 = true;
            }
            else if (IsSteamWorkshopItemSubscribed(1637663252))
            {
                is1637663252 = true;
            }
            else if (IsSteamWorkshopItemSubscribed(1806963141))
            {
                is1806963141 = true;
            }

            if (!this.Check3rdPartyModLoaded("TrafficManager", true))
            {
                //UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel").SetMessage("Incompatibility Issue", "Require TM:PE steamID:583429740 or 1637663252 or 1806963141", true);
                DebugLog.LogToFileOnly("We do not found TMPE");
            }
        }

        public static bool IsSteamWorkshopItemSubscribed(ulong itemId)
        {
            return ContentManagerPanel.subscribedItemsTable.Contains(new PublishedFileId(itemId));
        }
        public static void SetupGui()
        {
            LoadSprites();
            if (m_atlasLoaded)
            {
                parentGuiView = null;
                parentGuiView = UIView.GetAView();

                if (mainUI == null)
                {
                    mainUI = (MainUI)parentGuiView.AddUIComponent(typeof(MainUI));
                }

                SetupMainButton();
                isGuiRunning = true;
            }
        }

        public static void SetupMainButton()
        {
            if (mainButton == null)
            {
                mainButton = (parentGuiView.AddUIComponent(typeof(MainButton)) as MainButton);
            }
            mainButton.Show();
        }

        public static void RemoveGui()
        {
            isGuiRunning = false;
            if (parentGuiView != null)
            {
                parentGuiView = null;
                UnityEngine.Object.Destroy(mainUI);
                UnityEngine.Object.Destroy(mainButton);
                mainUI = null;
                mainButton = null;
            }
        }

        public void InitDetour()
        {
            if (!DetourInited)
            {
                DebugLog.LogToFileOnly("Init detours");
                bool detourFailed = false;

                //1
                DebugLog.LogToFileOnly("Detour NetAI::GetCollisionHalfWidth calls");
                try
                {
                    Detours.Add(new Detour(typeof(NetAI).GetMethod("GetCollisionHalfWidth", BindingFlags.Public | BindingFlags.Instance),
                                           typeof(CustomNetAI).GetMethod("CustomGetCollisionHalfWidth", BindingFlags.Public | BindingFlags.Instance)));
                }
                catch (Exception)
                {
                    DebugLog.LogToFileOnly("Could not detour NetAI::GetCollisionHalfWidth");
                    detourFailed = true;
                }
                //2
                //public static bool RayCast(ref NetSegment mysegment, ushort segmentID, Segment3 ray, float snapElevation, bool nameOnly, out float t, out float priority)
                DebugLog.LogToFileOnly("Detour NetSegment::RayCast calls");
                try
                {
                    Detours.Add(new Detour(typeof(NetSegment).GetMethod("RayCast", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(Segment3), typeof(float), typeof(bool), typeof(float).MakeByRefType(), typeof(float).MakeByRefType() }, null),
                                           typeof(CustomNetSegment).GetMethod("RayCast", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(NetSegment).MakeByRefType(), typeof(ushort), typeof(Segment3), typeof(float), typeof(bool), typeof(float).MakeByRefType(), typeof(float).MakeByRefType() }, null)));
                }
                catch (Exception)
                {
                    DebugLog.LogToFileOnly("Could not detour NetSegment::RayCast");
                    //detourFailed = true;
                }
                //3
                //public static bool RayCast(ref NetNode node, Segment3 ray, float snapElevation, out float t, out float priority)
                DebugLog.LogToFileOnly("Detour NetNode::RayCast calls");
                try
                {
                    Detours.Add(new Detour(typeof(NetNode).GetMethod("RayCast", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(Segment3), typeof(float), typeof(float).MakeByRefType(), typeof(float).MakeByRefType() }, null),
                                           typeof(CustomNetNode).GetMethod("RayCast", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(NetNode).MakeByRefType(), typeof(Segment3), typeof(float), typeof(float).MakeByRefType(), typeof(float).MakeByRefType() }, null)));
                }
                catch (Exception)
                {
                    DebugLog.LogToFileOnly("Could not detour NetNode::RayCast");
                    //detourFailed = true;
                }

                isMoveItRunning = CheckMoveItIsLoaded();

                if (detourFailed)
                {
                    DebugLog.LogToFileOnly("Detours failed");
                }
                else
                {
                    DebugLog.LogToFileOnly("Detours successful");
                }
                DetourInited = true;
            }
        }

        public void RevertDetour()
        {
            if (DetourInited)
            {
                DebugLog.LogToFileOnly("Revert detours");
                Detours.Reverse();
                foreach (Detour d in Detours)
                {
                    RedirectionHelper.RevertRedirect(d.OriginalMethod, d.Redirect);
                }
                DetourInited = false;
                Detours.Clear();
                DebugLog.LogToFileOnly("Reverting detours finished.");
            }
        }

        private bool Check3rdPartyModLoaded(string namespaceStr, bool printAll = false)
        {
            bool thirdPartyModLoaded = false;

            var loadingWrapperLoadingExtensionsField = typeof(LoadingWrapper).GetField("m_LoadingExtensions", BindingFlags.NonPublic | BindingFlags.Instance);
            List<ILoadingExtension> loadingExtensions = (List<ILoadingExtension>)loadingWrapperLoadingExtensionsField.GetValue(Singleton<LoadingManager>.instance.m_LoadingWrapper);

            if (loadingExtensions != null)
            {
                foreach (ILoadingExtension extension in loadingExtensions)
                {
                    if (printAll)
                        DebugLog.LogToFileOnly($"Detected extension: {extension.GetType().Name} in namespace {extension.GetType().Namespace}");
                    if (extension.GetType().Namespace == null)
                        continue;

                    var nsStr = extension.GetType().Namespace.ToString();
                    if (namespaceStr.Equals(nsStr))
                    {
                        DebugLog.LogToFileOnly($"The mod '{namespaceStr}' has been detected.");
                        thirdPartyModLoaded = true;
                        break;
                    }
                }
            }
            else
            {
                DebugLog.LogToFileOnly("Could not get loading extensions");
            }

            return thirdPartyModLoaded;
        }

        private bool CheckMoveItIsLoaded()
        {
            return this.Check3rdPartyModLoaded("MoveIt", true);
        }
    }
}