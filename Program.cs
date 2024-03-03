using System;
using Microsoft.EntityFrameworkCore;
using TodoApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<ToDoDbContext>(options =>
{
    options.UseMySql(builder.Configuration.GetConnectionString("ToDoDB"), new MySqlServerVersion(new Version(8, 0, 36)));
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAnyOrigin",builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();
app.MapGet("/items", async (ToDoDbContext dbContext) =>
{
    var tasks = await dbContext.Items.ToListAsync();
    return Results.Ok(tasks);
});

app.MapPost("/items", async (ToDoDbContext dbContext, Item newItem) =>
{
    dbContext.Items.Add(newItem);
    await dbContext.SaveChangesAsync();
    return Results.Created($"/items/{newItem.Id}", newItem);
});

app.MapPut("/items/${id}", async (ToDoDbContext dbContext, int taskId, Item updatedItem) =>
{
    var existingItem = await dbContext.Items.FindAsync(taskId);
    if (existingItem == null) return Results.NotFound();

    existingItem.Name = updatedItem.Name;
    await dbContext.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/items/${id}", async (int id,ToDoDbContext dbContext ) =>
{

    var itemToDelete = await dbContext.Items.FindAsync(id);
    if (itemToDelete == null) return Results.NotFound("Item not found");
    dbContext.Items.Remove(itemToDelete);
    await dbContext.SaveChangesAsync();
    return Results.NoContent();
});


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseRouting();
app.UseCors("AllowAnyOrigin");
app.UseEndpoints(endpoints =>
{
    _ = endpoints.MapControllers();
});
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
});

app.Run();
