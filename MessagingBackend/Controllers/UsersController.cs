using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly FirestoreService _firestoreService;

    // The framework automatically provides the instance here
    public UsersController(FirestoreService firestoreService)
    {
        _firestoreService = firestoreService;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        try
        {
            var users = await _firestoreService.GetAllUsersAsync();
            return Ok(users);
        }
        catch (Exception ex)
        {
            // Log the error in a real app
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}