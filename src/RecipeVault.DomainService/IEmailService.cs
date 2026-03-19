using System.Threading.Tasks;

namespace RecipeVault.DomainService {
    public interface IEmailService {
        Task SendCircleInviteAsync(string toEmail, string circleName, string inviterName, string inviteToken);
    }
}
