namespace MyProject.Services
{
    public interface IEmailServices
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
