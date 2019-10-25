namespace RabbitSyncService.Helpers
{
    public static class Constants
    {
        public static class Hub
        {
            public const string Name = "/hub";

            public static class Test
            {
                public const string Name = "test";
                public static string Path => $"{Hub.Name}/{Name}";
            }
        }
    }
}