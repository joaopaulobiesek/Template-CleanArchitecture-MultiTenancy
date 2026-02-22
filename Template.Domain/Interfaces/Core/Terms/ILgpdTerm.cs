namespace Template.Domain.Interfaces.Core.Terms;

public interface ILgpdTerm
{
    string Version { get; set; }
    string TermsOfUseContent { get; set; }
    string PrivacyPolicyContent { get; set; }
}
