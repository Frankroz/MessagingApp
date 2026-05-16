using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ChatsController : ControllerBase
{
    private readonly FirestoreService _firestoreService;

    public ChatsController(FirestoreService firestoreService)
    {
        _firestoreService = firestoreService;
    }

    // CREATE
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Chat chat)
    {
        var id = await _firestoreService.CreateChatAsync(chat);
        return CreatedAtAction(nameof(Get), new { id = id }, chat);
    }

    // READ
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id)
    {
        var chat = await _firestoreService.GetChatAsync(id);
        return chat == null ? NotFound() : Ok(chat);
    }

    // UPDATE
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] Chat chat)
    {
        chat.chatId = id;
        await _firestoreService.UpdateChatAsync(chat);
        return NoContent();
    }

    // DELETE
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await _firestoreService.DeleteChatAsync(id);
        return NoContent();
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserChats(string userId)
    {
        var chats = await _firestoreService.GetUserChatsAsync(userId);
        return Ok(chats);
    }
}