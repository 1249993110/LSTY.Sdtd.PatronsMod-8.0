using HarmonyLib;
using LSTY.Sdtd.PatronsMod.Hubs;

namespace LSTY.Sdtd.PatronsMod.HarmonyPatchers
{
    [HarmonyPatch(typeof(WorldEnvironment))]
    public static class WorldEnvironmentPatcher
    {
        private static bool _firstTime = true;
        private static bool _isDark;

        [HarmonyPostfix]
        [HarmonyPatch(nameof(WorldEnvironment.WorldTimeChanged))]
        public static void OnAfterWorldTimeChanged()
        {
            bool isDark = GameManager.Instance.World.IsDark();
            if (_firstTime)
            {
                _isDark = isDark;
                _firstTime = false;
                return;
            }

            if (_isDark == isDark)
            {
                return;
            }
            else
            {
                _isDark = isDark;

                var word = GameManager.Instance.World;
                ModEventHook.SkyChanged(new SkyChanged()
                {
                    BloodMoonDaysRemaining = Untils.DaysRemaining(GameUtils.WorldTimeToDays(word.GetWorldTime())),
                    DawnHour = word.DawnHour,
                    DuskHour = word.DuskHour,
                    SkyChangeEventType = _isDark ? SkyChangeEventType.Dusk : SkyChangeEventType.Dawn
                });
            }
        }
    }
}