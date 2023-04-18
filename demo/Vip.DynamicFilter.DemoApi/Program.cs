using Vip.DynamicFilter;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

#region Configuration

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

#endregion

#region Arrange

var clients = new List<Client>
{
    new("Batman Ribeiro", 10),
    new("Lindsey", 12),
    new("Joao", 15),
    new("Jose da Silva", 25),
    new("Bruce Wayne", 35),
    new("Amir", 56)
};

#endregion

#region Routes

app.MapPost("/clients", (FilterRequest filter) =>
{
    var teste = clients.GetFilterResponse(filter);
    return teste;
}).WithOpenApi();

app.Run();

#endregion

#region Classes

internal record Client(string Name, int Age)
{
    public Guid ClientId { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = Name;
    public int Age { get; set; } = Age;
}

#endregion