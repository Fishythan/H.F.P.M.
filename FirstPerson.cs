using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zorro.Settings;
using Landfall.Modding;
using Landfall.Haste;
using UnityEngine.Localization;
using System.Collections;

namespace FirstPerson;

// var original = typeof(PlayerMovement).getMethod(nameof(Start));
[LandfallPlugin]
public class FirstPersonPlugin{
    public static readonly Harmony harmony = new Harmony("com.uhhhhFirstPersonMod.Fishy");
    public static bool Enabled = true;
    public static float HubDelayTime = 4;

    static FirstPersonPlugin(){
        harmony.PatchAll();
        Debug.Log("Haste in First Person is loaded!");
    }
    public static IEnumerator ApplyPerspective(PlayerCharacter inst, float delay){
        Debug.Log("HFPM: ApplyPerspective Start Time: "+Time.time);
        yield return new WaitForSeconds(delay);
        inst.data.firstPerson = Enabled;
        Debug.Log("HFPM: ApplyPerspective End Time: "+Time.time);
    }

}

[HarmonyPatch(typeof(PlayerCharacter))]
class FirstPersonPatch{

    [HarmonyPatch("Start")]
    [HarmonyPostfix]

    static void CharStartPost(PlayerCharacter __instance){
        Debug.Log("HFPM: OnPostfix starting coroutine");
        
        if(SceneManager.GetActiveScene().name == "FullHub"){
            Debug.Log("HFPS: Delaying Perspective due to hub by: "+ FirstPersonPlugin.HubDelayTime);
            __instance.StartCoroutine(FirstPersonPlugin.ApplyPerspective(__instance, FirstPersonPlugin.HubDelayTime));
        }else{
            __instance.StartCoroutine(FirstPersonPlugin.ApplyPerspective(__instance,0));
        }
        
    }
}

[HasteSetting]
public class HasteSetting : OffOnSetting, IExposedSetting {
    public override void ApplyValue()
    {
        FirstPersonPlugin.Enabled = base.Value==OffOnMode.ON;
        GameObject player = GameObject.Find("Player");
        if(player==null){
            Debug.LogError("HFPM: Failed to find player on settings change!");
            return;
        }
        player.GetComponent<PlayerCharacter>().StartCoroutine(FirstPersonPlugin.ApplyPerspective(player.GetComponent<PlayerCharacter>(),FirstPersonPlugin.HubDelayTime)); 
    }

    public string GetCategory() => "H.F.P.M.";

    // public override OffOnMode GetDefaultValue() {
    //     return OffOnMode.ON;
    // }

    public LocalizedString GetDisplayName() => new UnlocalizedString("Enable First Person?");

    public override List<LocalizedString> GetLocalizedChoices() {
        return new List<LocalizedString>
        {
            new LocalizedString("Settings", "DisabledGraphicOption"),
            new LocalizedString("Settings", "EnabledGraphicOption")
        };
    }

    protected override OffOnMode GetDefaultValue()
    {
        return OffOnMode.ON;
    }
}