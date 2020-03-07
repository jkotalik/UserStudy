using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TodoApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TodosController : ControllerBase
    {
        [HttpGet]
        public async Task<IEnumerable<TodoItem>> Get()
        {
            using var db = new TodoDbContext();
            return await db.Todos.ToListAsync();
        }

        [HttpPost]
        public async Task<IActionResult> CreateTodo([FromBody]TodoItem todo)
        {
            using var db = new TodoDbContext();
            await db.Todos.AddAsync(todo);
            await db.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> UpdateCompleted(int id, [FromBody] TodoItem inputTodo)
        {
            using var db = new TodoDbContext();
            var todo = await db.Todos.FindAsync(id);

            if (todo == null)
            {
                return NotFound();
            }

            todo.IsComplete = inputTodo.IsComplete;

            await db.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTodo(int id)
        {
            using var db = new TodoDbContext();
            var todo = await db.Todos.FindAsync(id);
            if (todo == null)
            {
                return NotFound();
            }

            db.Todos.Remove(todo);
            await db.SaveChangesAsync();

            return Ok();
        }
    }
}
