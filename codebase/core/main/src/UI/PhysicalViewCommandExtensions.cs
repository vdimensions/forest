using Axle.Verification;
    
namespace Forest.UI
{
    public static class PhysicalViewCommandExtensions
    {
        public static void Invoke(this IPhysicalViewCommand command)
        {
            command.VerifyArgument(nameof(command)).IsNotNull();
            command.Invoke(null);
        }
    }
}