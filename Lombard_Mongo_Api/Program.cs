using Lombard_Mongo_Api.Models;
using Lombard_Mongo_Api.MongoRepository.GenericRepository;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MongoDB.Driver;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
SymmetricSecurityKey GetSymmetricSecurityKey() =>
       new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"]));
builder.Services.AddControllers();


// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // óêàçûâàåò, áóäåò ëè âàëèäèðîâàòüñÿ èçäàòåëü ïðè âàëèäàöèè òîêåíà
            ValidateIssuer = true,
            // ñòðîêà, ïðåäñòàâëÿþùàÿ èçäàòåëÿ
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            // áóäåò ëè âàëèäèðîâàòüñÿ ïîòðåáèòåëü òîêåíà
            ValidateAudience = true,
            // óñòàíîâêà ïîòðåáèòåëÿ òîêåíà
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            // áóäåò ëè âàëèäèðîâàòüñÿ âðåìÿ ñóùåñòâîâàíèÿ
            ValidateLifetime = true,
            // óñòàíîâêà êëþ÷à áåçîïàñíîñòè
            IssuerSigningKey = GetSymmetricSecurityKey(),
            // âàëèäàöèÿ êëþ÷à áåçîïàñíîñòè
            ValidateIssuerSigningKey = true,
        };
    });
builder.Services.Configure<MongoSettings>(
    builder.Configuration.GetSection("Database"));

builder.Services.AddSingleton<IMongoSettings>(provider =>
    provider.GetRequiredService<IOptions<MongoSettings>>().Value);

builder.Services.AddScoped(typeof(IMongoRepository<>), typeof(MongoRepository<>));
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

    // Äîáàâüòå àâòîðèçàöèþ
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
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
            new string[] {}
        }
    });
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
