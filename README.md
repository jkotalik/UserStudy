## Tutorial

**Goal**: In this exercise, the participants will be asked to build the backend of a TodoReact App.  The user will be exploring the functionality of .NET, a server-side framework. 

# Setup

1. Install [git](https://git-scm.com/downloads).
1. Install [.NET Core  3.1 SDK ](https://dotnet.microsoft.com/download)
1. Install [Node.js](https://nodejs.org/en/)
1. Clone this repository and navigate to the Tutorial folder, this consists of the frontend application `TodoReact` app.
    ```bash
    git clone https://github.com/glennc/UserStudy.git dotnet-study
    cd dotnet-study/Tutorial 
    ```

    > If using [Visual Studio Code](https://code.visualstudio.com/), install the [C# extension](https://marketplace.visualstudio.com/items?itemName=ms-vscode.csharp) for C# support.

**Task**:  Build the backend portion
-------------------------------------------------------
## Tasks
**Please Note: The completed exercise is available in the [samples folder](https://github.com/glennc/UserStudy/tree/master/Sample). Feel free to reference it at any point during the tutorial.**
###  Run the frontend application

1. Once you clone the Todo repo, navigate to the `TodoReact` folder and run the following commands 
```
\Tutorial\TodoReact> npm i 
\Tutorial\TodoReact> npm start
```
- The commands above
    - Restores packages `npm i `
    - Starts the react app `npm start`
1. The app will load but have no functionality
![image](https://user-images.githubusercontent.com/2546640/75070087-86307c80-54c0-11ea-8012-c78813f1dfd6.png)

    > Keep this React app running as we'll need it once we build the back-end in the upcoming steps

### Build backend 
**Create a new project**

1. Create a new API application and add the necessary packages in the `TodoApi` folder

```
Tutorial>dotnet new api -n TodoApi
Tutorial> cd TodoApi
Tutorial\TodoApi> dotnet add package Microsoft.EntityFrameworkCore.InMemory --version 3.1
```
   - The commands above
     - create a new API application `dotnet new api -n TodoApi`
     - Adds the NuGet packages  required in the next section `dotnet add package Microsoft.EntityFrameworkCore.InMemory --version 3.1`

2.  Open the `TodoApi` Folder in VS Code

## Create the database model

1. Create a file called  `TodoItem.cs` in the TodoApi folder. Add the content below:
   ```C#
   using System.Text.Json.Serialization;

    public class TodoItem
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("isComplete")]
        public bool IsComplete { get; set; }
    }
   ```
   The above model will be used for reading in JSON and storing todo items into the database.

1. Create a file called `TodoDbContext.cs` with the following contents:
    ```C#
    using Microsoft.EntityFrameworkCore;

    public class TodoDbContext : DbContext
    {
        public DbSet<TodoItem> Todos { get; set; }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase("Todos");
        }
    }
    ```
    This code does 2 things:
     - It exposes a `Todos` property which represents the list of todo items in the database.
     - The call to `UseInMemoryDatabase` wires up the in memory database storage. Data will only be persisted as long as the application is running.

1. Restart the server side application but this time we're going to use `dotnet watch`:
    ```
    dotnet watch run
    ```

    This will watch our application for source code changes and will restart the process as a result.

## Expose the list of todo items

1. Create a file called `TodosController.cs` with the following class:

    ```C#
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
        }
    }
    ```

    This method gets the list of todo items from the database and writes a JSON representation to the HTTP response. It also responds to a HTTP request on `/todos`.

1. Navigate to the URL https://localhost:5001/todos in the browser. It should return an empty JSON array.

    <img src="https://user-images.githubusercontent.com/2546640/75116317-1a235500-5635-11ea-9a73-e6fc30639865.png" alt="empty json array" style="text-align:center" width =70% />

## Adding a new todo item

1. In `TodosController.cs`, create another method called `CreateTodo` inside of the `TodosController` class:
    ```C#
    [HttpPost]
    public async Task<IActionResult> CreateTodo(TodoItem todo)
    {
        using var db = new TodoDbContext();
        await db.Todos.AddAsync(todo);
        await db.SaveChangesAsync();

        return Ok();
    }
    ```

    The above method accepts a `TodoItem` from the incoming HTTP request and adds it to the database. The web framework takes care of converting the incoming JSON to the TodoItem class.

1. Navigate to the `TodoReact` application which should be running on http://localhost:3000. The application should be able to add new todo items. Also, refreshing the page should show the stored todo items.
![image](https://user-images.githubusercontent.com/2546640/75119637-bc056a80-5652-11ea-81c8-71ea13d97a3c.png)

## Changing the state of todo items
1. In `TodosController.cs`, create another method called `UpdateTodoItem` inside of the `TodosController` class:
    ```C#
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
    ```

    The above logic retrives the id from the route parameter "id" and uses it to find the todo item in the database. It then sets the `IsComplete` property and updates the todo item in the database.

## Deleting a todo item

1. In `Program.cs` create another method called `DeleteTodo` inside of the `Program` class:
    ```C#
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
    ```

    The above logic is very similar to `UpdateTodoItem` but instead of udpdating it removes the todo item from the database after finding it.

## Test the application

The application should now be fully functional. 
![image](https://user-images.githubusercontent.com/2546640/75119891-08ea4080-5655-11ea-96be-adab4990ad65.png)
