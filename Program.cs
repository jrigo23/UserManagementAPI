using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserManagementAPI.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Configure JWT authentication
var jwtSecret = "YourSuperSecretKeyForJWTTokenGeneration123456";
var jwtIssuer = "UserManagementAPI";
var jwtAudience = "UserManagementAPIClients";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// 1. Global exception handling middleware (FIRST - catches all errors)
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An unhandled exception occurred");
        
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        
        var errorResponse = new ErrorResponse
        {
            StatusCode = 500,
            Message = "An internal server error occurred",
            Details = ex.Message,
            Timestamp = DateTime.UtcNow
        };
        
        await context.Response.WriteAsJsonAsync(errorResponse);
    }
});

// 2. HTTPS Redirection
app.UseHttpsRedirection();

// 3. Authentication (validates JWT tokens)
app.UseAuthentication();

// 4. Authorization (checks if user has required permissions)
app.UseAuthorization();

// 5. Request/Response logging middleware (LAST - logs after authentication)
app.UseRequestResponseLogging();

// In-memory storage for users
var users = new List<User>
{
    new User { Id = 1, Name = "John Michael Doe", Status = "Active" },
    new User { Id = 2, Name = "Jane Marie Smith", Status = "Inactive" }
};

// POST: Login endpoint to generate JWT token
app.MapPost("/api/auth/login", (LoginDto loginDto) => 
{
    // Simple authentication (in production, validate against a database)
    if (loginDto.Username == "admin" && loginDto.Password == "password123")
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(jwtSecret);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, loginDto.Username),
                new Claim(ClaimTypes.Role, "Admin")
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = jwtIssuer,
            Audience = jwtAudience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), 
                SecurityAlgorithms.HmacSha256Signature)
        };
        
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);
        
        return Results.Ok(new 
        { 
            token = tokenString,
            expiresAt = tokenDescriptor.Expires,
            message = "Login successful"
        });
    }
    
    return Results.Json(
        new ErrorResponse 
        { 
            StatusCode = 401, 
            Message = "Authentication failed",
            Details = "Invalid username or password",
            Timestamp = DateTime.UtcNow
        }, 
        statusCode: 401);
})
.WithName("Login")
.AllowAnonymous();

// GET: Get all users
app.MapGet("/api/users", () => 
{
    return Results.Ok(users);
})
.WithName("GetAllUsers")
.RequireAuthorization();

// GET: Get user by id
app.MapGet("/api/users/{id}", (int id) => 
{
    var user = users.FirstOrDefault(u => u.Id == id);
    if (user is null)
    {
        return Results.Json(
            new ErrorResponse 
            { 
                StatusCode = 404, 
                Message = "User not found",
                Details = $"No user found with ID {id}",
                Timestamp = DateTime.UtcNow
            }, 
            statusCode: 404);
    }
    return Results.Ok(user);
})
.WithName("GetUserById")
.RequireAuthorization();

// POST: Create a new user
app.MapPost("/api/users", (UserDto userDto) => 
{
    // Validate name has at least 3 words
    var wordCount = userDto.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
    if (wordCount < 3)
    {
        return Results.Json(
            new ErrorResponse 
            { 
                StatusCode = 400, 
                Message = "Validation failed",
                Details = "Name must contain at least 3 words",
                Timestamp = DateTime.UtcNow
            }, 
            statusCode: 400);
    }
    
    var newUser = new User 
    { 
        Id = users.Any() ? users.Max(u => u.Id) + 1 : 1,
        Name = userDto.Name,
        Status = "Inactive" // Initial status is always set to Inactive
    };
    users.Add(newUser);
    return Results.Created($"/api/users/{newUser.Id}", newUser);
})
.WithName("CreateUser")
.RequireAuthorization();

// PUT: Update an existing user
app.MapPut("/api/users/{id}", (int id, UserDto userDto) => 
{
    var user = users.FirstOrDefault(u => u.Id == id);
    if (user is null)
    {
        return Results.Json(
            new ErrorResponse 
            { 
                StatusCode = 404, 
                Message = "User not found",
                Details = $"No user found with ID {id}",
                Timestamp = DateTime.UtcNow
            }, 
            statusCode: 404);
    }
    
    // Validate name has at least 3 words
    var wordCount = userDto.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
    if (wordCount < 3)
    {
        return Results.Json(
            new ErrorResponse 
            { 
                StatusCode = 400, 
                Message = "Validation failed",
                Details = "Name must contain at least 3 words",
                Timestamp = DateTime.UtcNow
            }, 
            statusCode: 400);
    }
    
    user.Name = userDto.Name;
    user.Status = userDto.Status;
    return Results.Ok(user);
})
.WithName("UpdateUser")
.RequireAuthorization();

// DELETE: Delete a user
app.MapDelete("/api/users/{id}", (int id) => 
{
    var user = users.FirstOrDefault(u => u.Id == id);
    if (user is null)
    {
        return Results.Json(
            new ErrorResponse 
            { 
                StatusCode = 404, 
                Message = "User not found",
                Details = $"No user found with ID {id}",
                Timestamp = DateTime.UtcNow
            }, 
            statusCode: 404);
    }
    
    users.Remove(user);
    return Results.NoContent();
})
.WithName("DeleteUser")
.RequireAuthorization();

app.Run();

// User model
public class User
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Status { get; set; }
}

// DTO for creating/updating users
public class UserDto
{
    public required string Name { get; set; }
    public required string Status { get; set; }
}

// DTO for login
public class LoginDto
{
    public required string Username { get; set; }
    public required string Password { get; set; }
}

// Standardized error response model
public class ErrorResponse
{
    public int StatusCode { get; set; }
    public required string Message { get; set; }
    public string? Details { get; set; }
    public DateTime Timestamp { get; set; }
}

