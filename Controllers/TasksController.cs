using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DbContextType = pract_8.Models.Pract8Context;
using TaskEntity = pract_8.Models.Task;

namespace pract_8.Controllers;

[ApiController]
[Route("api/v1/tasks")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly DbContextType _db;

    public TasksController(DbContextType db)
    {
        _db = db;
    }

    public record CreateTaskRequest(string Title, string Description);
    public record UpdateTaskRequest(string Title, string Description, bool IsCompleted);

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var task = await _db.Tasks.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
        if (task is null)
            return NotFound();
        if (task.OwnerLogin != User.Identity!.Name && !User.IsInRole("Admin"))
            return Forbid();
        return Ok(task);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateTaskRequest request)
    {
        var task = new TaskEntity
        {
            Title = request.Title,
            Description = request.Description,
            OwnerLogin = User.Identity!.Name!,
            IsCompleted = false
        };

        _db.Tasks.Add(task);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateTaskRequest request)
    {
        var task = await _db.Tasks.FirstOrDefaultAsync(t => t.Id == id);
        if (task is null)
            return NotFound();
        if (task.OwnerLogin != User.Identity!.Name && !User.IsInRole("Admin"))
            return Forbid();

        task.Title = request.Title;
        task.Description = request.Description;
        task.IsCompleted = request.IsCompleted;
        await _db.SaveChangesAsync();

        return Ok(task);
    }
}