using System.Threading.Tasks;
using RabbitSyncService.Clients;
using RabbitSyncService.Dto;
using RabbitSyncService.Requests;
using Microsoft.AspNetCore.SignalR;

namespace RabbitSyncService.Hubs
{
    public class TestHub : Hub<ITestClient>, ITestRequest
    {
        public async Task SendAsync(MessageDto<TestDto> dto)
            => await Clients.All.NewMessageAsync(dto.Message);
    }
}