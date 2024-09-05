using Cardano.Sync.Extensions;
using Cardano.Sync.Example.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCardanoIndexer<CardanoTestDbContext>(builder.Configuration);
builder.Services.AddReducers<CardanoTestDbContext>(["BlockReducer,TestReducer"]);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.Run();
