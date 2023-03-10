using BccCode.DocumentationSite.Models;
using BccCode.DocumentationSite.Services;
using IdentityServer4.Extensions;
using Microsoft.OpenApi.Models;
using System.Diagnostics;
using System.IO;
using System.Net;
using Yarp.ReverseProxy.Forwarder;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

#region swagger
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "JWTToken_Auth_API",
        Version = "v1"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 1safsfsdfdfd\"",
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});
#endregion

#region Github token authentication
builder.Services.AddAuthentication().AddJwtBearer("github", options =>
{
    options.Authority = "https://token.actions.githubusercontent.com";
    options.Audience = "https://github.com/bcc-code";
});

builder.Services.AddAuthorization(policy =>
{
    policy.AddPolicy("githubpolicy", claims => claims.RequireClaim("repository_owner", "bcc-code").RequireClaim("repository"));
});
#endregion

//Https forwording in the container app
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedHost;
});

builder.Services.AddHttpForwarder();

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IGetMembersInterface, GetMembers>();
builder.Services.AddSingleton<IGetFiles, GetFiles>();
builder.Services.AddSingleton<ISASToken, SASToken>();
builder.Services.AddSingleton<EnviromentVar>();
builder.Services.AddSingleton<CustomTransformer>();
builder.Services.AddApplicationInsightsTelemetry(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]);

var app = builder.Build();

//Https forwarding in the container app
app.UseForwardedHeaders(new ForwardedHeadersOptions()
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
});

var httpClient = new HttpMessageInvoker(new SocketsHttpHandler()
{
    UseProxy = false,
    AllowAutoRedirect = false,
    AutomaticDecompression = DecompressionMethods.None,
    UseCookies = false,
    ActivityHeadersPropagator = new ReverseProxyPropagator(DistributedContextPropagator.Current),
    ConnectTimeout = TimeSpan.FromSeconds(15),
});

var requestConfig = new ForwarderRequestConfig { ActivityTimeout = TimeSpan.FromSeconds(100) };

app.UseRouting();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    var forwarder = app.Services.GetService<IHttpForwarder>();
    var transformer = app.Services.GetService<CustomTransformer>();  //HttpTransformer.Default; // or new CustomTransformer();
    var envVar = app.Services.GetService<EnviromentVar>();

    endpoints.Map("/{**catch-all}", async httpContext =>
    {
        var error = await forwarder!.SendAsync(httpContext, envVar!.GetEnviromentVariable("StorageUrl"), httpClient, requestConfig, transformer!);
        // Check if the operation was successful
        if (error != ForwarderError.None)
        {
            var errorFeature = httpContext.GetForwarderErrorFeature();
            var exception = errorFeature!.Exception;
        }
    });
});


// Configure the HTTP request pipeline. 
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCookiePolicy();

app.UseHttpsRedirection();

app.UseAuthentication();

app.MapControllers();

app.Run();