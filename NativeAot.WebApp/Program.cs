using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer("Server=localhost;Database=TodoDb;User Id=sa;Password=password123!;TrustServerCertificate=true");
});


var app = builder.Build();

app.AddTodoEndpoints();

await app.RunAsync();

public class Todo
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public DateOnly DueBy { get; set; }
    public bool IsComplete { get; set; } = false;
}

[JsonSerializable(typeof(IEnumerable<Todo>))]
[JsonSerializable(typeof(Todo))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Todo> Todos { get; set; }
}

public static class TodoEndpoints
{
    public static void AddTodoEndpoints(this IEndpointRouteBuilder app)
    {
        var todosApi = app.MapGroup("/todos");
        todosApi.MapGet("/", GetAllAsync);
    }

    static async Task<IEnumerable<Todo>> GetAllAsync([FromServices] AppDbContext context)
    {
        var todos = new List<Todo>();
        await foreach (var todo in GetAllTodosAsync(context))
        {
            todos.Add(todo);
        }

        return todos;
    }

    static IAsyncEnumerable<Todo> GetAllTodosAsync(AppDbContext context) => context.Todos.OrderBy(b => b.Id).AsAsyncEnumerable();
}