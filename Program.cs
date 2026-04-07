using Microsoft.EntityFrameworkCore;
using TarefasApi.Data;
using TarefasApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer("Server=MONISE\\SQLEXPRESS;Database=TarefasDb;Trusted_Connection=True;TrustServerCertificate=True;"));

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/tarefas", async (AppDbContext db) =>
{
    var tarefas = await db.Tarefas.ToListAsync();

    if (!tarefas.Any())
        return Results.NotFound("Nenhuma tarefa encontrada.");

    return Results.Ok(tarefas);
});

app.MapPost("/tarefas", async (AppDbContext db, Tarefa tarefa) =>
{
    if (string.IsNullOrWhiteSpace(tarefa.Titulo))
        return Results.BadRequest("O título é obrigatório.");

    if (string.IsNullOrWhiteSpace(tarefa.Status))
        return Results.BadRequest("O status é obrigatório.");

    tarefa.DataCriacao = DateTime.Now;

    db.Tarefas.Add(tarefa);
    await db.SaveChangesAsync();
    return Results.Created($"/tarefas/{tarefa.Id}", tarefa);
});

app.MapPut("/tarefas/{id}", async (AppDbContext db, int id, Tarefa tarefaAtualizada) =>
{
    var tarefa = await db.Tarefas.FindAsync(id);

    if (tarefa is null)
        return Results.NotFound("Tarefa não encontrada.");

    if (string.IsNullOrWhiteSpace(tarefaAtualizada.Titulo))
        return Results.BadRequest("O título é obrigatório.");

    if (string.IsNullOrWhiteSpace(tarefaAtualizada.Status))
        return Results.BadRequest("O status é obrigatório.");

    tarefa.Titulo = tarefaAtualizada.Titulo;
    tarefa.Descricao = tarefaAtualizada.Descricao;
    tarefa.Status = tarefaAtualizada.Status;

    await db.SaveChangesAsync();
    return Results.Ok(tarefa);
});

app.MapDelete("/tarefas/{id}", async (AppDbContext db, int id) =>
{
    var tarefa = await db.Tarefas.FindAsync(id);

    if (tarefa is null)
        return Results.NotFound("Tarefa não encontrada.");

    db.Tarefas.Remove(tarefa);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();
