namespace PatientManagementSystem.Modules.Identity.Domain.Roles;

public static class IdentityRoles
{
    public const string Admin = "Admin";
    public const string Patient = "Patient";
    public const string Doctor = "Doctor";
    
    public static readonly IReadOnlyList<string> All = [Admin, Patient, Doctor];
}