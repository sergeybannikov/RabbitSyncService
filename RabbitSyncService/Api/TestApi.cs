using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Rabbit.Bridge.Messages;
using Microsoft.AspNetCore.Mvc;
using Rebus.Bus;

namespace RabbitSyncService.Api
{
    [Route("api/test")]
    [ApiController]
    public class TestApi
    {
        private readonly IBus bus;


        public TestApi(IBus bus)
        {
            this.bus = bus;
        }

        [HttpGet("test-get")]
        public ActionResult TestGet() => new JsonResult(new TestGetResult
        {
            Name = "name",
            Description = "Description",
            CreatedAt = DateTime.Now,
            Version = GetFileVersion()
        });

        [HttpGet("test-rabbit-publish")]
        public async Task<ActionResult> TestRabbitPublish()
        {
            await bus.Publish(new Job {JobNumber = 123});
            return
                new JsonResult(new TestGetResult
                {
                    Name = "name",
                    Description = "Description",
                    CreatedAt = DateTime.Now,
                    Version = GetFileVersion()
                });
        }

        [HttpGet("test-rabbit-publish-retry")]
        public async Task<ActionResult> TestRabbitPublishRetry()
        {
            var message = new RetryExample();
            await bus.Publish(message);
            return
                new JsonResult("Ok");
        }

        [HttpGet("version")]
        public ActionResult GetVersion()
        {
            return new JsonResult(GetFileVersion());
        }

        public string GetFileVersion()
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);

            return fvi.FileVersion;
        }
    }

    public class TestGetResult
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Version { get; set; }
    }
}