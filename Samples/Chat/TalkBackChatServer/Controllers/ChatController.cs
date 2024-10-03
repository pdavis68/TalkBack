using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TalkBackChatServer.Data;
using TalkBackChatServer.DTOs;
using TalkBackChatServer.Services;

namespace TalkBackChatServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;
        private readonly IChatService _chatService;
        public ChatController(ILogger<ChatController> logger, ApplicationDbContext context, IChatService chatService)
        {
            _context = context;
            _logger = logger;
            _chatService = chatService;
        }


        [HttpGet("Providers")]
        public Task<List<string>> GetProviders()
        {
            _logger.LogInformation("GET Providers");
            return Task.FromResult(_chatService.GetProviders());
        }

        [HttpGet("Models/{providerName}")]
        public async Task<List<string?>> GetModels(string providerName)
        {
            _logger.LogInformation($"GET Models/{providerName}");
            return (await _chatService.GetModelsAsync(providerName)).Select(p=>p.Name).ToList();
        }

        // GET: api/chat
        [HttpPost]
        public async Task Chat([FromBody] ChatMessageDto message)
        {
            _logger.LogInformation("POST chat");
            if (message is null || string.IsNullOrWhiteSpace(message.Message))
            {
                return;
            }

            var cts = new CancellationTokenSource();
            var stream = new MemoryStream();

            var httpContext = Request.HttpContext;
            await httpContext.Response.StartAsync(cts.Token);

            string response = string.Empty;
            bool doneReceived = false;
            await _chatService.ChatAsync(message.Message, message.ConversationId, (msgPart) =>
            {
                if (msgPart != Constants.Done)
                {
                    httpContext.Response.WriteAsync(msgPart, cts.Token);
                }
                else
                {
                    httpContext.Response.CompleteAsync();
                    doneReceived = true;

                }
            });

            while (!doneReceived)
            {
                Thread.Sleep(50);
            }
        }
    }
}