using Microsoft.AspNetCore.Mvc;
using TaskManagementSystem.Core.DTOs;
using TaskManagementSystem.Core.Entities;
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.API.Data;


namespace TaskManagementSystem.API.Controllers
{
     [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly TaskManagementContext _context;

        public UsersController(TaskManagementContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // POST: api/users/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDTO userDto)
        {
            try
            {
                // Check if username exists.
                if (await _context.Users.AnyAsync(u => u.Username == userDto.Username))
                    return BadRequest("Username already exists");

                // Check if email exists.
                if (await _context.Users.AnyAsync(u => u.Email == userDto.Email))
                    return BadRequest("Email already exists");

                // Create the user without hashing the password (plain text for testing only)
                var user = new User
                {
                    Username = userDto.Username,
                    Email = userDto.Email,
                    // Storing password as plain text.  (Not secure!)
                    Password = userDto.Password  
                };
                
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                var props = new UserRegDTO{
                    Id = user.Id,
                    Email = user.Email,
                    Username = user.Username

                };
                // Return the created user without the password (or you can return a safe DTO)
                return CreatedAtAction(nameof(GetUser), new { id = user.Id }, props);
            }
            catch (Exception ex)
            {
                // Log the exception as needed.
                return StatusCode(500, "An error occurred while registering the user");
            }
        }

        // POST: api/users/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserDTO userDto)
        {
            try
            {
                // Find the user by username; include the TaskSubscriptions and related TaskItem
                var user = await _context.Users
                    .Include(u => u.TaskSubscriptions)
                        .ThenInclude(ts => ts.TaskItem)
                    .FirstOrDefaultAsync(u => u.Username == userDto.Username);

                if (user == null)
                    return Unauthorized("Invalid credentials");

                // Validate password by direct string comparison (plain text; insecure)
                if (user.Password != userDto.Password)
                    return Unauthorized("Invalid credentials");

                // Map the user and its subscriptions to a response DTO.
                var response = new UserResponseDTO
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    SubscribedTasks = user.TaskSubscriptions
                        .Select(ts => new USERTaskItemDTO
                        {
                            Id = ts.TaskItem.Id,
                            Title = ts.TaskItem.Title,
                            Description = ts.TaskItem.Description,
                            DueDate = ts.TaskItem.DueDate,
                            Priority = ts.TaskItem.Priority,
                            Status = ts.TaskItem.Status.ToString()
                        }).ToList()
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                // Log exception as needed.
                return StatusCode(500, "An error occurred while logging in");
            }
        }

     // GET: api/users/{id}
[HttpGet("{id}")]
public async Task<ActionResult<UserResponseDTO>> GetUser(int id)
{
    var user = await _context.Users
        .Include(u => u.TaskSubscriptions)
        .ThenInclude(ts => ts.TaskItem)
        .FirstOrDefaultAsync(u => u.Id == id);

    if (user == null)
        return NotFound();

    var userDto = new UserResponseDTO
    {
        Id = user.Id,
        Username = user.Username,
        Email = user.Email,
        SubscribedTasks = user.TaskSubscriptions
            .Select(ts => new USERTaskItemDTO
            {
                Id = ts.TaskItem.Id,
                Title = ts.TaskItem.Title,
                Description = ts.TaskItem.Description,
                DueDate = ts.TaskItem.DueDate,
                Priority = ts.TaskItem.Priority,
                Status = ts.TaskItem.Status.ToString()
            })
            .OrderByDescending(t => t.Priority)
            .ToList()
    };

    return Ok(userDto);}
    }
    
    
    }
