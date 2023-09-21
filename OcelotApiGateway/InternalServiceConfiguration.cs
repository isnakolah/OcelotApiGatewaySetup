internal enum InternalServiceType
{
    Onboarding,
    Ordering
}

internal static class InternalServicesConfiguration
{
    public const string Onboarding = "Onboarding";
    public const string Ordering = "Ordering";

    public static string GetName(this InternalServiceType serviceType)
    {
        return serviceType switch
        {
            InternalServiceType.Onboarding => Onboarding,
            InternalServiceType.Ordering => Ordering,
            _ => throw new ArgumentOutOfRangeException(nameof(serviceType), serviceType, null)
        };
    }
}
