namespace MedCore.Common.Authorization;

public static class AppRoles
{
    public const string Admin = "Admin";
    public const string Patient = "Patient";
    public const string Doctor = "Doctor";

    public static readonly IReadOnlySet<string> All 
        = new HashSet<string> { Admin, Patient, Doctor };
}