using SampleSinglePageApplication.Web.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();

builder.Services.AddMvc().AddNewtonsoftJson();

builder.Services.AddHttpContextAccessor();
 
string _connectionString = builder.Configuration.GetConnectionString("AppData");
if (String.IsNullOrEmpty(_connectionString)) {
    _connectionString = "";
}
SampleSinglePageApplication.GlobalSettings.DatabaseConnection = _connectionString;
builder.Services.AddTransient<SampleSinglePageApplication.IDataAccess>(x => ActivatorUtilities.CreateInstance<SampleSinglePageApplication.DataAccess>(x, _connectionString));

// Set up the Encryption library for injection.
builder.Services.AddSingleton<SampleSinglePageApplication.IEncryption>(x => ActivatorUtilities.CreateInstance<SampleSinglePageApplication.Encryption>(x, new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27, 0x28, 0x29, 0x30, 0x31, 0x32 }));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
} else {
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseEndpoints(endpoints => {
    endpoints.MapDefaultControllerRoute();
    endpoints.MapFallbackToController("Index", "Home");
    endpoints.MapHub<SignalRHub>("/signalrHub");
});

app.Run();
