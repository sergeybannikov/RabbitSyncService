using System;

namespace RabbitSyncService.Extensions
{
    public static class ObjectExtensions
    {
        public static bool IsNotNull(this Object obj)
        {
            return obj != null;
        }
    }
}
