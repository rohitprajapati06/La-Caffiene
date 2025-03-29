namespace MyProject.Services
{
    public interface IOtpService
    {
        Task<int> GenerateOtpAsync();
    }
}
