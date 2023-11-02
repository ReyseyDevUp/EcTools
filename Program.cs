using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// builder.Services.AddSignalR();

var app = builder.Build();

app.UseWhen(context => context.Request.Path.StartsWithSegments("/FileToSQL"), appBuilder =>
{
    appBuilder.Use(async (context, next) =>
    {
        context.Features.Get<IHttpMaxRequestBodySizeFeature>().MaxRequestBodySize = 2L * 1024L * 1024L * 1024L; // 2 GB
        await next.Invoke();
    });
});

app.UseWhen(context => context.Request.Path.StartsWithSegments("/ToSQL"), appBuilder =>
{
    appBuilder.Use(async (context, next) =>
    {
        context.Features.Get<IHttpMaxRequestBodySizeFeature>().MaxRequestBodySize = 2L * 1024L * 1024L * 1024L; // 2 GB
        await next.Invoke();
    });
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();



