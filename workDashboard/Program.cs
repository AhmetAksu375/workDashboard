using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System;
using workDashboard.Data;
using workDashboard.Interfaces;
using workDashboard.Services;
using System.Text.Json.Serialization;
using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Identity;
using workDashboard.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container

// Register DinkToPdf converter service
builder.Services.AddSingleton<IConverter>(new SynchronizedConverter(new PdfTools()));

// Configure Swagger for API documentation with JWT Bearer authentication
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' followed by your token."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configure CORS policy to allow all origins, methods, and headers
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminPolicy", policy => policy.RequireClaim("aud", "admin"));
    options.AddPolicy("CompanyPolicy", policy => policy.RequireClaim("aud", "company"));
    options.AddPolicy("EmployeePolicy", policy => policy.RequireClaim("aud", "employee"));
    options.AddPolicy("CompanyOrEmployeePolicy", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim(c => c.Type == "aud" && (c.Value == "company" || c.Value == "employee"))));
    options.AddPolicy("AdminAccessPolicy", policy => policy.RequireClaim("aud", "admin"));
});

// Register services for dependency injection
// Register services for dependency injection
builder.Services.AddSingleton<EmailService>();
builder.Services.AddScoped<TaxService>();
builder.Services.AddScoped<InvoiceService>();
builder.Services.AddScoped<InvoicePdfService>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IPasswordHasher<Company>, PasswordHasher<Company>>();
builder.Services.AddScoped<IPasswordHasher<Admin>, PasswordHasher<Admin>>();
// Add this line
builder.Services.AddScoped<IPasswordHasher<Employee>, PasswordHasher<Employee>>();

// Configure authentication with JWT Bearer
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false; // Consider setting this to true in production
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudiences = builder.Configuration.GetSection("Jwt:Audiences").Get<string[]>(),
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"])),
            ClockSkew = TimeSpan.Zero
        };
    });

// Configure Entity Framework and database context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add support for controllers and configure JSON serialization options to avoid cycles
builder.Services.AddControllers().AddJsonOptions(x =>
    x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

// Add support for API exploration
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Apply the CORS policy globally
app.UseCors("AllowAll");

// Add authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Map controllers to handle requests
app.MapControllers();

// Set the URL for the application to listen on a specific IP and port
app.Urls.Add("https://10.16.17.62:7004"); // Ensure this IP and port are reachable

// Run the application
app.Run();
