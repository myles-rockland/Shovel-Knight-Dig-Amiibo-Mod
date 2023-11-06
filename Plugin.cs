using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Rewired;
using TMPro;
using I2;
using BepInEx.Logging;
using I2.Loc;
using System.IO;
using static UnityEngine.UI.Image;
using System.Linq;

namespace AmiiboPlugin
{
    [BepInPlugin("rockm3000.skdig.amiibo", "Amiibo Mod", "1.0.0.0")]
    [BepInProcess("skDig64.exe")]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            //Applying the popup patch
            var harmony = new Harmony("rockm3000.skdig.amiibo");
            var original = typeof(TitleScreen).GetMethod(nameof(TitleScreen.TitleScreenJingle));
            var prefix = typeof(ShowingPopupPatch).GetMethod(nameof(ShowingPopupPatch.ChangeShowingPopup));
            harmony.Patch(original, prefix: new HarmonyMethod(prefix));

            //Applying the popup patch
            original = typeof(GenericMessagePopup).GetMethod(nameof(GenericMessagePopup.OpenQueue));
            prefix = typeof(AmiiboPopupPatch).GetMethod(nameof(AmiiboPopupPatch.SpawnFireballPopup));
            var postfix = typeof(AmiiboPopupPatch).GetMethod(nameof(AmiiboPopupPatch.ChangePopupText));
            harmony.Patch(original, prefix: new HarmonyMethod(prefix), postfix: new HarmonyMethod(postfix));

            //Applying the close popup patch
            original = typeof(GenericMessagePopup).GetMethod(nameof(GenericMessagePopup.Close));
            postfix = typeof(AmiiboClosePopupPatch).GetMethod(nameof(AmiiboClosePopupPatch.ChangeNextPopupText));
            harmony.Patch(original, postfix: new HarmonyMethod(postfix));

            //Applying the meeber patch
            original = typeof(Overworld).GetMethod(nameof(Overworld.PositionBackgroundElementsInWorld));
            postfix = typeof(MeeberPatch).GetMethod(nameof(MeeberPatch.SpawnMeeber));
            harmony.Patch(original, postfix: new HarmonyMethod(postfix));

            //Applying the meeber interaction patch
            original = typeof(MadamMeeber).GetMethod(nameof(MadamMeeber.OnInteract));
            prefix = typeof(MeeberInteractPatch).GetMethod(nameof(MeeberInteractPatch.InteractMeeber));
            harmony.Patch(original, prefix: new HarmonyMethod(prefix));

            // Plugin startup logic
            Logger.LogInfo("Plugin rockm3000.skdig.amiibo is loaded!");
        }
    }

    //Show popup patch
    [HarmonyPatch(typeof(TitleScreen), nameof(TitleScreen.TitleScreenJingle))]
    class ShowingPopupPatch
    {
        [HarmonyPrefix]
        public static void ChangeShowingPopup(ref bool ___m_ShowingPopups)
        {
            ___m_ShowingPopups = true;
        }
    }

    //Amiibo active mod popup patch
    [HarmonyPatch(typeof(GenericMessagePopup), nameof(GenericMessagePopup.OpenQueue))]
    class AmiiboPopupPatch
    {
        private static ManualLogSource popupPatchLog = BepInEx.Logging.Logger.CreateLogSource("PopupPatchLog");
        public static bool textChanged = false;
        [HarmonyPrefix]
        public static void SpawnFireballPopup(GenericMessagePopup __instance, ref Queue<GenericMessagePopup.QueuedPopup> popupQueue)
        {
            popupQueue.Enqueue(new GenericMessagePopup.QueuedPopup(GenericMessagePopup.TYPE.TRUE_ENDING_COMPLETE, new Action(OverworldEvents.SetShownKnightmareModePopup), string.Empty, true));
        }

        [HarmonyPostfix]
        public static void ChangePopupText(GenericMessagePopup __instance)
        {
            if (!textChanged)
            {
                Transform childByName = Utilities.GetChildByName(__instance.m_Pannels[4].transform, "title text");
                Transform childByName2 = Utilities.GetChildByName(__instance.m_Pannels[4].transform, "body text");
                childByName.GetComponent<TextMeshProUGUI>().text = "amiibo mod activated!";
                childByName2.GetComponent<TextMeshProUGUI>().text = "you gained a new <color=#DC00D4>fairy<color=#FFFFFF> ally! find her in the camp, my, oh, my!";
                popupPatchLog.LogInfo("Changed text to Amiibo mod text.");
                textChanged = true;
            }
        }
    }

    //Popup close patch
    [HarmonyPatch(typeof(GenericMessagePopup), nameof(GenericMessagePopup.Close))]
    class AmiiboClosePopupPatch
    {
        private static ManualLogSource nextPopupPatchLog = BepInEx.Logging.Logger.CreateLogSource("NextPopupPatchLog");
        [HarmonyPostfix]
        public static void ChangeNextPopupText(GenericMessagePopup __instance)
        {
            nextPopupPatchLog.LogInfo("Popup was closed.");
            if (!AmiiboPopupPatch.textChanged)
            {
                Transform childByName = Utilities.GetChildByName(__instance.m_Pannels[4].transform, "title text");
                Transform childByName2 = Utilities.GetChildByName(__instance.m_Pannels[4].transform, "body text");
                childByName.GetComponent<TextMeshProUGUI>().text = "amiibo mod activated!";
                childByName2.GetComponent<TextMeshProUGUI>().text = "you gained a new <color=#DC00D4>fairy<color=#FFFFFF> ally! find her in the camp, my, oh, my!";
                AmiiboPopupPatch.textChanged = true;
                nextPopupPatchLog.LogInfo("Changed text to Amiibo mod text.");
            }
        }
    }

    //Put Madam Meeber into overworld patch
    [HarmonyPatch(typeof(Overworld), nameof(Overworld.PositionBackgroundElementsInWorld))]
    class MeeberPatch
    {
        private static ManualLogSource meeberPatchLog = BepInEx.Logging.Logger.CreateLogSource("MeeberPatchLog");
        [HarmonyPostfix]
        public static void SpawnMeeber(Overworld __instance)
        {
            __instance.m_MadamMeeber.SetActive(true);
            meeberPatchLog.LogInfo("Spawned Madam Meeber into Overworld");

            //Unlock amiibo summons using console method
            SaveManager.SetInt("AMIIBO_FAIRY_SHOVEL", 1);
            //SaveManager.SetInt("AMIIBO_FAIRY_SPECTER", 1);
            //SaveManager.SetInt("AMIIBO_FAIRY_PLAGUE", 1);
            //SaveManager.SetInt("AMIIBO_FAIRY_KING", 1);
            //SaveManager.SetInt("AMIIBO_FAIRY_GOLD_SHOVEL", 1);
            //SaveManager.Save();
            meeberPatchLog.LogInfo("Unlocked Amiibo summons");
        }
    }

    //Madam Meeber interaction patch
    [HarmonyPatch(typeof(MadamMeeber), nameof(MadamMeeber.OnInteract))]
    class MeeberInteractPatch
    {
        private static ManualLogSource meeberPatchLog = BepInEx.Logging.Logger.CreateLogSource("MeeberInteractPatchLog");
        [HarmonyPrefix]
        public static bool InteractMeeber(MadamMeeber __instance, ref bool ___HasBeenOpenInThisSession) //Return type should be void if not skipping original
        {
            //Code to get all dialogue
            FileStream filestream = new FileStream("skdDialogue.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            filestream.Dispose();
            List<string> list = LocalisationToolPanel.GetList(LocalisationToolPanel.CATEGORY.DIALOGUES);
            for (int i = 0; i < list.Count; i++)
            {
                File.AppendAllText("skdDialogue.txt", list[i]);
            }
            //Ends here
            meeberPatchLog.LogInfo("Interacted with Madam Meeber");
            /*
            List<Familiar.FamiliarType> fairies = new List<Familiar.FamiliarType> { Familiar.FamiliarType.Amiibo_Shovel, Familiar.FamiliarType.Amiibo_Gold_Shovel, Familiar.FamiliarType.Amiibo_Plague, Familiar.FamiliarType.Amiibo_Specter, Familiar.FamiliarType.Amiibo_King };
            System.Random rnd = new System.Random();
            int randIndex = rnd.Next(fairies.Count);
            Familiar.FamiliarType fairy = fairies[randIndex];
            StageController.Instance.m_Player.m_FamiliarManager.AddFamiliar(fairy, true, null, true);
            SoundManager.GetInstance().PlaySound("ui_menu_confirm", 0.7f, false, 8, false, 1f, 128, -1, SoundManager.SourceData.SOUND_FLAGS.NONE, 0f);
            */
            var currentAmiibo = PlayerFamiliarManager.GetCurrentAmiibo();
            var inputAmiiboDict = new Dictionary<string, Familiar.FamiliarType>
            {
                { "R", Familiar.FamiliarType.Amiibo_Gold_Shovel },
                { "L", Familiar.FamiliarType.Amiibo_Plague },
                { "ZR", Familiar.FamiliarType.Amiibo_Specter },
                { "ZL", Familiar.FamiliarType.Amiibo_King },
                { "attack", Familiar.FamiliarType.Amiibo_Shovel }
            };
            if (currentAmiibo != Familiar.FamiliarType.None)
            {
                var amiiboFamiliars = StageController.Instance.m_Player.m_FamiliarManager.GetAmiiboFamiliars();
                for (int i = 0; i < amiiboFamiliars.Count; i++)
                {
                    StageController.Instance.m_Player.m_FamiliarManager.RemoveFamiliar(amiiboFamiliars[i], true, true, true);
                }
            }
            foreach (var item in inputAmiiboDict)
            {
                if (PlayerInput.instance.rewiredInput.GetButton(item.Key) && currentAmiibo != item.Value)
                {
                    StageController.Instance.m_Player.m_FamiliarManager.AddFamiliar(item.Value, true, null, true);
                    break;
                }
            }
            return false;
        }
    }
}
