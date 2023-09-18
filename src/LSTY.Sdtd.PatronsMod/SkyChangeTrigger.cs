using IceCoffee.Common.Timers;
using LSTY.Sdtd.PatronsMod.Hubs;

namespace LSTY.Sdtd.PatronsMod
{
    public static class SkyChangeTrigger
    {
        private static bool _firstTime = true;
        private static bool _isDark;

        public static void Callback()
        {
            var word = GameManager.Instance.World;

            if (word == null)
            {
                return;
            }

            bool isDark = word.IsDark();

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

                ModEventHook.SkyChanged(new SkyChanged()
                {
                    BloodMoonDaysRemaining = Utils.DaysRemaining(GameUtils.WorldTimeToDays(word.GetWorldTime())),
                    DawnHour = word.DawnHour,
                    DuskHour = word.DuskHour,
                    CurrentHour = GameUtils.WorldTimeToHours(word.GetWorldTime()),
                    SkyChangeEventType = _isDark ? SkyChangeEventType.Dusk : SkyChangeEventType.Dawn
                });
            }
        }
    }
}