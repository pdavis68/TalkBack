using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TalkBackChatServer.Data;
using TalkBackChatServer.DTOs;
using TalkBackChatServer.Models;

namespace TalkBackChatServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConversationsController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger _logger;

        public ConversationsController(ILogger<ConversationsController> logger, ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [HttpPost("/api/Conversations/new")]
        public async Task<ActionResult> NewConversation(string provider, string model, string systemPrompt)
        {
            _logger.LogInformation("POST Conversations/new");
            try
            {
                var conversation = await _dbContext.Conversations.AddAsync(new Conversation()
                {
                    LLM = provider,
                    Model = model,
                    Title = Constants.Pending,
                    SystemMessage = systemPrompt
                });
                await _dbContext.SaveChangesAsync();
                return Ok(conversation.CurrentValues["Id"] is int id ? id : throw new Exception("This shouldn't happen!"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating new conversation");
                return StatusCode(500);
            }
        }

        [HttpDelete()]
        public async Task<ActionResult> DeleteConversation(int conversationId)
        {
            _logger.LogInformation("DELETE Conversations");
            try
            {
                var dbConversation = _dbContext.Conversations.FirstOrDefault(p => p.Id == conversationId);
                if (dbConversation is null)
                {
                    throw new Exception($"Conversation with id {conversationId} not found");
                }
                _dbContext.Conversations.Remove(dbConversation);
                _dbContext.Messages.RemoveRange(_dbContext.Messages.Where(m => m.ConversationId == conversationId));
                await _dbContext.SaveChangesAsync(); 
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting conversation");
                return StatusCode(500);
            }
        }
        // GET: api/conversations
        [HttpGet]
        public async Task<ActionResult<GetConversationsDto>> GetConversations()
        {
            _logger.LogInformation("GET conversations");
            return new GetConversationsDto() {
                Conversations = await _dbContext.Conversations
                .Select(c => new GetConversationItemDto
                {
                    Id = c.Id,
                    Title = c.Title,
                })
                .ToListAsync()
            };
        }

        // GET: api/conversations/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<GetConversationDto>> GetConversation(int id)
        {
            _logger.LogInformation($"GET conversation/{id}");
            var conversation = await _dbContext.Conversations
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (conversation == null)
            {
                _logger.LogWarning($"Conversation {id} not found");
                return NotFound();
            }

            return new GetConversationDto
            {
                Id = conversation.Id,
                Title = conversation.Title,
                LLM = conversation.LLM,
                Model = conversation.Model,
                SystemMessage = conversation.SystemMessage,
                Messages = conversation.Messages!.Select(m => new GetMessageDto
                {
                    Role = m.Role,
                    Content = m.Content,
                }).ToList()
            };
        }
    }
}
