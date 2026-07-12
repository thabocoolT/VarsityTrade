using Microsoft.EntityFrameworkCore;
using VarsityTrade.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

//Databases
builder.Services.AddDbContext<VarsityTradeDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//Controllers 
builder.Services.AddControllers();

//Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if(app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();