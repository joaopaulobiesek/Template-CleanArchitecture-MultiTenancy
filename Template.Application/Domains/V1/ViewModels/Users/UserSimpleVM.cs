namespace Template.Application.Domains.V1.ViewModels.Users;

public class UserSimpleVM
{
    public string Id { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }

    public UserSimpleVM() { }

    public UserSimpleVM(string id, string? fullName, string? email)
    {
        Id = id;
        FullName = fullName;
        Email = email;
    }
}
