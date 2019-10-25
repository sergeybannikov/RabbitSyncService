using RabbitSyncService.Hubs;

namespace RabbitSyncService.Dto
{
    public class MessageDto<TMessage>
    {
        public MessageStatus MessageStatus { get; set; }
        public TMessage Message { get; set; }
    }
}