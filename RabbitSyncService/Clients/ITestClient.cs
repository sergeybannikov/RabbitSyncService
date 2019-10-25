using System.Threading.Tasks;
using RabbitSyncService.Dto;

namespace RabbitSyncService.Clients
{
    public interface ITestClient
    {
        Task NewMessageAsync(TestDto call);
    }
}