namespace MyProject.Services
{
    public class OtpService:IOtpService
    {
        private readonly Random random;

        public OtpService(Random random)
        {
            this.random = random;
        }

        public async Task<int> GenerateOtpAsync()
        {
            await Task.Delay(10);
            return random.Next(100000,1000000);
        }
    }
}
