# User Management API

A RESTful API built with ASP.NET Core (.NET 10.0) for managing user records with JWT authentication, request/response logging, and standardized error handling.

## Features

- 🔐 **JWT Authentication** - Secure API endpoints with JSON Web Tokens
- 👥 **User CRUD Operations** - Create, Read, Update, and Delete user records
- ✅ **Input Validation** - Name validation requiring at least 3 words
- 📝 **Request/Response Logging** - Custom middleware for logging all API requests
- 🛡️ **Global Exception Handling** - Standardized error responses across all endpoints
- 📖 **OpenAPI/Swagger Documentation** - Interactive API documentation with Swagger UI
- 🚀 **Minimal API** - Built with .NET Minimal API architecture

## Technologies

- **.NET 10.0**
- **ASP.NET Core**
- **JWT Bearer Authentication**
- **Swashbuckle.AspNetCore** - OpenAPI/Swagger documentation
- **C#**

## Getting Started

### Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- A code editor (Visual Studio, VS Code, or Rider)

### Installation

1. Clone the repository:
```bash
git clone https://github.com/jrigo23/UserManagementAPI.git
cd UserManagementAPI
```

2. Restore dependencies:
```bash
dotnet restore
```

3. Build the project:
```bash
dotnet build
```

4. Run the application:
```bash
dotnet run
```

The API will start and be available at `https://localhost:5001` (or the port specified in your launch settings).

## API Documentation

The API includes interactive **Swagger/OpenAPI documentation** that is automatically generated and available when you run the application.

### Accessing Swagger UI

Once the application is running, navigate to:
- **Swagger UI**: `http://localhost:5048/` or `http://localhost:5048/index.html`
- **OpenAPI JSON**: `http://localhost:5048/swagger/v1/swagger.json`

The Swagger UI provides:
- 📖 Complete API documentation with descriptions for all endpoints
- 🧪 Interactive testing - try out API calls directly from the browser
- 🔐 JWT authentication support - use the "Authorize" button to add your JWT token
- 📝 Request/response examples and schemas
- 🏷️ Organized endpoints by tags (Authentication, Users)

### Using Authentication in Swagger UI

1. Call the `/api/auth/login` endpoint with the default credentials (username: `admin`, password: `password123`)
2. Copy the JWT token from the response
3. Click the "Authorize" button at the top of the Swagger UI
4. Paste the token in the "Value" field
5. Click "Authorize" and then "Close"
6. You can now test authenticated endpoints

## API Endpoints

### Authentication

#### Login
```http
POST /api/auth/login
```

**Request Body:**
```json
{
  "username": "admin",
  "password": "password123"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2026-01-31T12:00:00Z",
  "message": "Login successful"
}
```

### Users (All endpoints require authentication)

#### Get All Users
```http
GET /api/users
Authorization: Bearer {token}
```

#### Get User by ID
```http
GET /api/users/{id}
Authorization: Bearer {token}
```

#### Create User
```http
POST /api/users
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "John Michael Doe",
  "status": "Active"
}
```

**Validation Rules:**
- Name must contain at least 3 words
- New users are initially created with "Inactive" status

#### Update User
```http
PUT /api/users/{id}
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "Jane Marie Smith",
  "status": "Active"
}
```

**Validation Rules:**
- Name must contain at least 3 words

#### Delete User
```http
DELETE /api/users/{id}
Authorization: Bearer {token}
```

## Authentication Flow

1. Call the `/api/auth/login` endpoint with valid credentials
2. Receive a JWT token in the response
3. Include the token in the `Authorization` header for all subsequent requests:
   ```
   Authorization: Bearer {your-token-here}
   ```

## Project Structure

```
UserManagementAPI/
├── Middleware/
│   └── RequestResponseLoggingMiddleware.cs
├── Properties/
│   └── launchSettings.json
├── bin/
├── obj/
├── Program.cs                      # Main application entry point
├── UserManagementAPI.csproj        # Project configuration
├── UserManagementAPI.sln           # Solution file
├── UserManagementAPI.http          # HTTP test requests
├── appsettings.json                # Application configuration
├── appsettings.Development.json    # Development configuration
├── test-api.ps1                    # PowerShell test script
└── TEST_VALIDATION_REPORT.md       # Testing documentation
```

## Error Handling

All errors return a standardized JSON response:

```json
{
  "statusCode": 404,
  "message": "User not found",
  "details": "No user found with ID 5",
  "timestamp": "2026-01-31T10:30:00Z"
}
```

Common status codes:
- `400` - Bad Request (validation errors)
- `401` - Unauthorized (invalid credentials or missing token)
- `404` - Not Found (user doesn't exist)
- `500` - Internal Server Error (unhandled exceptions)

## Testing

The repository includes test utilities:

- **UserManagementAPI.http** - HTTP requests for testing with REST Client or similar tools
- **test-api.ps1** - PowerShell script for automated API testing
- **TEST_VALIDATION_REPORT.md** - Detailed testing documentation

### Running Tests with PowerShell

```powershell
.	est-api.ps1
```

## Security Notes

⚠️ **Important**: This is a demonstration project. For production use:

1. Store JWT secrets in environment variables or secure configuration
2. Implement proper user authentication against a database
3. Use HTTPS in production
4. Implement rate limiting
5. Add proper password hashing (bcrypt, Argon2)
6. Consider implementing refresh tokens
7. Add logging to a persistent store

## Default Credentials

For testing purposes only:
- **Username**: `admin`
- **Password**: `password123`

## License

This project is open source and available for educational purposes.

## Author

**jrigo23**

---

Built with ❤️ using ASP.NET Core