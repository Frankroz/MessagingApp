using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/chats/{chatId}/messages")]
public class MessagesController : ControllerBase
{
    private readonly FirestoreService _firestoreService;

    public MessagesController(FirestoreService firestoreService)
    {
        _firestoreService = firestoreService;
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage(string chatId, [FromBody] Message message)
    {
        await _firestoreService.SendMessageAsync(chatId, message);
        return Ok(new { messageId = message.messageId });
    }

    [HttpGet]
    public async Task<IActionResult> GetMessages(string chatId)
    {
        var messages = await _firestoreService.GetChatMessagesAsync(chatId);
        return Ok(messages);
    }

    [HttpPut("{messageId}")]
    public async Task<IActionResult> UpdateMessage(string chatId, string messageId, [FromBody] Message message)
    {
        try
        {
            // Ensure the ID in the URL matches the ID in the body object
            message.messageId = messageId;
            message.chatId = chatId; // Keeps the relationship intact

            await _firestoreService.UpdateMessageAsync(message);
            return NoContent(); // 204 Success, no content to return
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpDelete("{messageId}")]
    public async Task<IActionResult> DeleteMessage(string chatId, string messageId)
    {
        try
        {
            await _firestoreService.DeleteMessageAsync(chatId, messageId);
            return NoContent(); // 204 Success, nothing to return
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}

