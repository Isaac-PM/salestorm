using Inventory.Hubs;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();

builder.Services.AddCors(options =>
    options.AddPolicy("CorsPolicy", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()
    )
);

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.UseCors("CorsPolicy");

app.MapHub<InventoryHub>("/inventoryHub");
app.Run();