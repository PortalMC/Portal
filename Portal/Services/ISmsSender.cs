using System.Threading.Tasks;

namespace Portal.Services
{
    public interface ISmsSender
    {
        Task SendSmsAsync(string number, string message);
    }
}