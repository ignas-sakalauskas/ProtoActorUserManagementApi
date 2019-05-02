using OpenTracing;

namespace UserManagement.Actors.Chaos
{
    public static class ChaosHelper
    {
        public static bool ShouldCreateUserCrash(ISpan activeSpan)
        {
            if (activeSpan == null)
                return false;

            return activeSpan.GetBaggageItem(Headers.ChaosType) == Headers.CreateUserDown;
        }
    }
}
