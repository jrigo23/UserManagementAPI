# API Testing Script
$baseUrl = "http://localhost:5048"
Write-Host "`n========================================" -ForegroundColor Magenta
Write-Host "     API TESTING SUITE" -ForegroundColor Magenta
Write-Host "========================================`n" -ForegroundColor Magenta

# Test 1: Login with valid credentials
Write-Host "`n=== TEST 1: Login with Valid Credentials ===" -ForegroundColor Cyan
try {
    $loginBody = @{
        username = "admin"
        password = "password123"
    } | ConvertTo-Json

    $loginResponse = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method POST -ContentType "application/json" -Body $loginBody
    $token = $loginResponse.token
    Write-Host "✓ PASSED - Status: 200" -ForegroundColor Green
    Write-Host "  Token: $($token.Substring(0,50))..." -ForegroundColor Yellow
} catch {
    Write-Host "✗ FAILED - $($_.Exception.Message)" -ForegroundColor Red
}

# Test 2: Login with invalid credentials
Write-Host "`n=== TEST 2: Login with Invalid Credentials ===" -ForegroundColor Cyan
try {
    $invalidLoginBody = @{
        username = "admin"
        password = "wrongpassword"
    } | ConvertTo-Json

    $response = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method POST -ContentType "application/json" -Body $invalidLoginBody
    Write-Host "✗ FAILED - Should have returned 401" -ForegroundColor Red
} catch {
    if ($_.Exception.Response.StatusCode -eq 401) {
        Write-Host "✓ PASSED - Correctly returned 401 Unauthorized" -ForegroundColor Green
    } else {
        Write-Host "✗ FAILED - Wrong status code: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
    }
}

# Test 3: Access protected endpoint without token
Write-Host "`n=== TEST 3: Access Protected Endpoint WITHOUT Token ===" -ForegroundColor Cyan
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/users" -Method GET
    Write-Host "✗ FAILED - Should have returned 401" -ForegroundColor Red
} catch {
    if ($_.Exception.Response.StatusCode -eq 401) {
        Write-Host "✓ PASSED - Correctly returned 401 Unauthorized" -ForegroundColor Green
    } else {
        Write-Host "✗ FAILED - Wrong status code: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
    }
}

# Test 4: Access protected endpoint with valid token
Write-Host "`n=== TEST 4: Access Protected Endpoint WITH Valid Token ===" -ForegroundColor Cyan
try {
    $headers = @{
        Authorization = "Bearer $token"
    }
    $response = Invoke-RestMethod -Uri "$baseUrl/api/users" -Method GET -Headers $headers
    Write-Host "✓ PASSED - Status: 200" -ForegroundColor Green
    Write-Host "  Retrieved $($response.Count) users" -ForegroundColor Yellow
} catch {
    Write-Host "✗ FAILED - $($_.Exception.Message)" -ForegroundColor Red
}

# Test 5: Access protected endpoint with invalid token
Write-Host "`n=== TEST 5: Access Protected Endpoint WITH Invalid Token ===" -ForegroundColor Cyan
try {
    $headers = @{
        Authorization = "Bearer invalid.token.here"
    }
    $response = Invoke-RestMethod -Uri "$baseUrl/api/users" -Method GET -Headers $headers
    Write-Host "✗ FAILED - Should have returned 401" -ForegroundColor Red
} catch {
    if ($_.Exception.Response.StatusCode -eq 401) {
        Write-Host "✓ PASSED - Correctly returned 401 Unauthorized" -ForegroundColor Green
    } else {
        Write-Host "✗ FAILED - Wrong status code: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
    }
}

# Test 6: GET user by ID with valid token
Write-Host "`n=== TEST 6: GET User by ID with Valid Token ===" -ForegroundColor Cyan
try {
    $headers = @{
        Authorization = "Bearer $token"
    }
    $response = Invoke-RestMethod -Uri "$baseUrl/api/users/1" -Method GET -Headers $headers
    Write-Host "✓ PASSED - Status: 200" -ForegroundColor Green
    Write-Host "  User: $($response.name) (Status: $($response.status))" -ForegroundColor Yellow
} catch {
    Write-Host "✗ FAILED - $($_.Exception.Message)" -ForegroundColor Red
}

# Test 7: GET non-existent user (404 error)
Write-Host "`n=== TEST 7: GET Non-existent User (Test Error Handling) ===" -ForegroundColor Cyan
try {
    $headers = @{
        Authorization = "Bearer $token"
    }
    $response = Invoke-RestMethod -Uri "$baseUrl/api/users/999" -Method GET -Headers $headers
    Write-Host "✗ FAILED - Should have returned 404" -ForegroundColor Red
} catch {
    if ($_.Exception.Response.StatusCode -eq 404) {
        Write-Host "✓ PASSED - Correctly returned 404 Not Found" -ForegroundColor Green
        $errorStream = $_.Exception.Response.GetResponseStream()
        $reader = New-Object System.IO.StreamReader($errorStream)
        $errorBody = $reader.ReadToEnd() | ConvertFrom-Json
        Write-Host "  Error Response: $($errorBody.message)" -ForegroundColor Yellow
    } else {
        Write-Host "✗ FAILED - Wrong status code: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
    }
}

# Test 8: Create user with invalid name (validation error)
Write-Host "`n=== TEST 8: Create User with Invalid Name (Test Validation) ===" -ForegroundColor Cyan
try {
    $headers = @{
        Authorization = "Bearer $token"
    }
    $invalidUserBody = @{
        name = "John"  # Only one word - should fail
        status = "Active"
    } | ConvertTo-Json
    
    $response = Invoke-RestMethod -Uri "$baseUrl/api/users" -Method POST -Headers $headers -ContentType "application/json" -Body $invalidUserBody
    Write-Host "✗ FAILED - Should have returned 400" -ForegroundColor Red
} catch {
    if ($_.Exception.Response.StatusCode -eq 400) {
        Write-Host "✓ PASSED - Correctly returned 400 Bad Request" -ForegroundColor Green
        $errorStream = $_.Exception.Response.GetResponseStream()
        $reader = New-Object System.IO.StreamReader($errorStream)
        $errorBody = $reader.ReadToEnd() | ConvertFrom-Json
        Write-Host "  Validation Error: $($errorBody.details)" -ForegroundColor Yellow
    } else {
        Write-Host "✗ FAILED - Wrong status code: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
    }
}

# Test 9: Create user with valid data
Write-Host "`n=== TEST 9: Create User with Valid Data ===" -ForegroundColor Cyan
try {
    $headers = @{
        Authorization = "Bearer $token"
    }
    $validUserBody = @{
        name = "Alice Marie Johnson"
        status = "Active"
    } | ConvertTo-Json
    
    $response = Invoke-RestMethod -Uri "$baseUrl/api/users" -Method POST -Headers $headers -ContentType "application/json" -Body $validUserBody
    Write-Host "✓ PASSED - Status: 201 Created" -ForegroundColor Green
    Write-Host "  Created User: $($response.name) (ID: $($response.id), Status: $($response.status))" -ForegroundColor Yellow
    Write-Host "  Note: Status is 'Inactive' (forced by business rule)" -ForegroundColor Gray
} catch {
    Write-Host "✗ FAILED - $($_.Exception.Message)" -ForegroundColor Red
}

# Test 10: Update user with valid token
Write-Host "`n=== TEST 10: Update User with Valid Token ===" -ForegroundColor Cyan
try {
    $headers = @{
        Authorization = "Bearer $token"
    }
    $updateBody = @{
        name = "John Michael Updated"
        status = "Active"
    } | ConvertTo-Json
    
    $response = Invoke-RestMethod -Uri "$baseUrl/api/users/1" -Method PUT -Headers $headers -ContentType "application/json" -Body $updateBody
    Write-Host "✓ PASSED - Status: 200" -ForegroundColor Green
    Write-Host "  Updated User: $($response.name) (Status: $($response.status))" -ForegroundColor Yellow
} catch {
    Write-Host "✗ FAILED - $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n========================================" -ForegroundColor Magenta
Write-Host "     TESTING COMPLETE" -ForegroundColor Magenta
Write-Host "========================================`n" -ForegroundColor Magenta

Write-Host "`nCheck the server logs for detailed request/response logging!" -ForegroundColor Cyan
