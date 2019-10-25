using System.Threading.Tasks;
using RabbitSyncService.Dto;

namespace RabbitSyncService.Requests
{
    public interface ITestRequest
    {
        Task SendAsync(MessageDto<TestDto> call);
    }
}