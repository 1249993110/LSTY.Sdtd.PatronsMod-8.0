using LSTY.Sdtd.PatronsMod.Hubs;

namespace LSTY.Sdtd.PatronsMod
{
    public static class SkyChangeTrigger
    {
        private static int _lastDays;
        private static bool _isDark;

        public static void Callback()
        {
            var word = GameManager.Instance.World;

            if (word == null)
            {
                return;
            }

            int days = GameUtils.WorldTimeToDays(word.GetWorldTime());
            bool isDark = word.IsDark();

            // 首次 或 跨天
            if(_lastDays == 0 || _lastDays != days)
            {
                _isDark = isDark;
                _lastDays = days;
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
                    BloodMoonDaysRemaining = Utils.DaysRemaining(days),
                    DawnHour = word.DawnHour,
                    DuskHour = word.DuskHour,
                    SkyChangeEventType = _isDark ? SkyChangeEventType.Dusk : SkyChangeEventType.Dawn
                });
            }
        }
    }
}