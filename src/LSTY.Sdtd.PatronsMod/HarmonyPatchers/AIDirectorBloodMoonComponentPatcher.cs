using HarmonyLib;
using LSTY.Sdtd.PatronsMod.Hubs;

namespace LSTY.Sdtd.PatronsMod.HarmonyPatchers
{
    [HarmonyPatch(typeof(AIDirectorBloodMoonComponent))]
    public static class AIDirectorBloodMoonComponentPatcher
    {
        [HarmonyPostfix]
        [HarmonyPatch("StartBloodMoon")]
        public static void OnAfterStartBloodMoon()
        {
            var word = GameManager.Instance.World;
            ModEventHook.SkyChanged(new SkyChanged()
            {
                BloodMoonDaysRemaining = Untils.DaysRemaining(GameUtils.WorldTimeToDays(word.GetWorldTime())),
                DawnHour = word.DawnHour,
                DuskHour = word.DuskHour,
                SkyChangeEventType = SkyChangeEventType.BloodMoonStart
            });
        }

        [HarmonyPostfix]
        [HarmonyPatch("EndBloodMoon")]
        public static void OnAfterEndBloodMoon()
        {
            var word = GameManager.Instance.World;
            ModEventHook.SkyChanged(new SkyChanged()
            {
                BloodMoonDaysRemaining = Untils.DaysRemaining(GameUtils.WorldTimeToDays(word.GetWorldTime())),
                DawnHour = word.DawnHour,
                DuskHour = word.DuskHour,
                SkyChangeEventType = SkyChangeEventType.BloodMoonEnd
            });
        }
    }
}