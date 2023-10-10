using Platform.Local;

namespace LSTY.Sdtd.PatronsMod
{
    public static class Utils
    {
        public readonly static ClientInfo CmdExecuteDelegate = new ClientInfo() 
        { 
            PlatformId = new UserIdentifierLocal(ModApi.ModIdentity),
        };

        public static int DaysRemaining(int daysUntilHorde)
        {
            int bloodmoonFrequency = GamePrefs.GetInt(EnumGamePrefs.BloodMoonFrequency);
            if (daysUntilHorde <= bloodmoonFrequency)
            {
                int daysLeft = bloodmoonFrequency - daysUntilHorde;
                return daysLeft;
            }
            else
            {
                int daysLeft = daysUntilHorde - bloodmoonFrequency;
                return DaysRemaining(daysLeft);
            }
        }
    }
}