using Amazon;
using Amazon.SimpleEmail;


using Exam4.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Configure AWS SES
var awsOptions = builder.Configuration.GetSection("AWS");
var accessKey = awsOptions["AccessKey"];
var secretKey = awsOptions["SecretKey"];
var region = awsOptions["Region"];

// Register AWS SES client
builder.Services.AddSingleton<IAmazonSimpleEmailService>(provider =>
{
    var awsCredentials = new Amazon.Runtime.BasicAWSCredentials(accessKey, secretKey);
    var regionEndpoint = RegionEndpoint.GetBySystemName(region);
    return new AmazonSimpleEmailServiceClient(awsCredentials, regionEndpoint);
});

// Register our services as Singletons
// Singleton ensures the same instance (and list) is used across all requests
builder.Services.AddSingleton<ICredentialsService, CredentialsService>();
builder.Services.AddSingleton<IEmailService, EmailService>();
builder.Services.AddSingleton<ITestEmailService, TestEmailService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();