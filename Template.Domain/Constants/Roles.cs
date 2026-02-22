namespace Template.Domain.Constants;

public abstract class Roles
{
    public const string Admin = nameof(Admin);
    public const string User = nameof(User);
    public const string TI = nameof(TI);
    public const string All = $"{Admin},{User},{TI}";

    public static Dictionary<string, string> GetRoles()
    {
        return new Dictionary<string, string>
        {
           { nameof(Admin), Admin },
           { nameof(User), User },
           { nameof(TI), TI }
        };
    }

    public static List<string> GetAllRoles()
    {
        return new List<string>
        {
            nameof(Admin),
            nameof(User),
            nameof(TI)
        };
    }
}