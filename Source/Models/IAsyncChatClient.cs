using System.Net;

namespace HospitalAIChatbot.Source.Models
{
    public interface IAsyncChatClient : IDisposable
    {
        Task ConnectAsync(IPEndPoint ipEndPoint, CancellationToken cancellationToken = default);
        Task DisconnectAsync(CancellationToken cancellationToken = default);
        Task<string> ReceiveMessageAsync(CancellationToken cancellationToken = default);
        Task SendMessageAsync(string message, CancellationToken cancellationToken = default);
    }
}