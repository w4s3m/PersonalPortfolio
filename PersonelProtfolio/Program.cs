using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.VisualBasic;
using PersonalProtfolioDataTier;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


//
string ?connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
PersonalProtfolioDataTier.clsConnectionString.connectionString = connectionString;
//


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // TokenValidationParameters define how incoming JWTs will be validated.
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // Ensures the token was issued by a trusted issuer.
            ValidateIssuer = true,


            // Ensures the token is intended for this API (audience check).
            ValidateAudience = true,


            // Ensures the token has not expired.
            ValidateLifetime = true,


            // Ensures the token signature is valid and was signed by the API.
            ValidateIssuerSigningKey = true,


            // The expected issuer value (must match the issuer used when creating the JWT).
            ValidIssuer = "PersonelProtfolio",


            // The expected audience value (must match the audience used when creating the JWT).
            ValidAudience = "PersonelProtfolioUsers",


            // The secret key used to validate the JWT signature.
            // This must be the same key used when generating the token.
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("THIS_IS_A_VERY_SECRET_KEY_123456"))
        };
    });

builder.Services.AddAuthorization();

// Register the custom authorization handler that checks if the user is either the owner of the resource or an admin.
builder.Services.AddSingleton<IAuthorizationHandler, UserOwnerOrAdminHandler>();

// Define authorization policies, including the custom "UserOwnerOrAdmin" policy that uses the UserOwnerOrAdminRequirement. 
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("UserOwnerOrAdmin", policy =>
        policy.Requirements.Add(new UserOwnerOrAdminRequirement()));
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

//builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(options =>
{
  
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
     
        Name = "Authorization",

        Type = SecuritySchemeType.Http,

        Scheme = "Bearer",

        BearerFormat = "JWT",
        In = ParameterLocation.Header,

        Description = "Enter: Bearer {your JWT token}"
    });
   
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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



//Define CORS policy Configuration to allow requests from the specified origins (e.g., your frontend application)
builder.Services.AddCors(options =>
{
    options.AddPolicy("PersonalProtfolioCorsPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:7000") //  √ﬂœ √‰ Â–« ÂÊ —«»ÿ «·‹ React ·œÌﬂ
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// 
var app = builder.Build();



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


// To redirect HTTP requests to HTTPS , Set before app.MapControllers();
app.UseHttpsRedirection();

// Define Cors Before MapControllers.
app.UseCors("PersonalProtfolioCorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
