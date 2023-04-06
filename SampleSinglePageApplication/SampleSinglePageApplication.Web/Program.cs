using SampleSinglePageApplication;
using SampleSinglePageApplication.Web.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddMvc().AddNewtonsoftJson(options => {
    options.SerializerSettings.DateParseHandling = Newtonsoft.Json.DateParseHandling.DateTimeOffset;
    options.SerializerSettings.Converters.Add(new UTCDateTimeConverter());
});
builder.Services.AddHttpContextAccessor();

var _connectionString = String.Empty + builder.Configuration.GetConnectionString("AppData");
var _databaseType = String.Empty + builder.Configuration.GetValue<string>("DatabaseType");
builder.Services.AddTransient<IDataAccess>(x => ActivatorUtilities.CreateInstance<DataAccess>(x, _connectionString, _databaseType));

var useAuthorization = CustomAuthenticationProviders.UseAuthorization(builder);
builder.Services.AddTransient<ICustomAuthentication>(x => ActivatorUtilities.CreateInstance<CustomAuthentication>(x, useAuthorization));

var app = builder.Build();

if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Error");
    app.UseHsts();
} else {
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
if (useAuthorization.Enabled) {
    app.UseAuthorization();
}

app.UseRequestLocalization();

app.UseEndpoints(endpoints => {
    endpoints.MapDefaultControllerRoute();
    endpoints.MapFallbackToController("Index", "Home");
    endpoints.MapHub<SignalRHub>("/signalrHub");
});

app.Run();