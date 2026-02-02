# RecipeVault Postman Collections Setup Guide

This guide explains how to use the provided Postman collections for testing the RecipeVault API and Google Gemini API integration.

## Collections Included

1. **RecipeVault-API.postman_collection.json** - Complete RecipeVault API endpoints
2. **Gemini-API.postman_collection.json** - Google Gemini API for recipe parsing
3. **RecipeVault-Environment.postman_environment.json** - Environment variables for both collections

## Prerequisites

- **Postman** (latest version) - Download from https://www.postman.com/downloads/
- **RecipeVault API** running locally or on a server
- **Google Gemini API Key** (optional, for direct Gemini testing)

### Getting a Gemini API Key

1. Go to https://makersuite.google.com/app/apikey
2. Sign in with your Google account
3. Create a new API key
4. Copy the API key and save it securely

## Import Instructions

### Step 1: Import Collections

1. Open Postman
2. Click **Import** button (top-left)
3. Select **File** tab
4. Choose `RecipeVault-API.postman_collection.json`
5. Click **Import**
6. Repeat steps 2-5 for `Gemini-API.postman_collection.json`

### Step 2: Import Environment

1. Click **Import** in Postman
2. Select **File** tab
3. Choose `RecipeVault-Environment.postman_environment.json`
4. Click **Import**

### Step 3: Select Environment

1. In the top-right of Postman, find the **Environment** dropdown
2. Select **RecipeVault Environment**

## Configuration

### RecipeVault API Configuration

Update the environment variables:

| Variable | Default | Description |
|----------|---------|-------------|
| `base_url` | `https://localhost:5001` | Your RecipeVault API base URL |
| `access_token` | `your_jwt_token_here` | JWT token from your auth provider |
| `recipe_id` | UUID format | Recipe ID for GET/PUT/DELETE operations |

**To obtain a JWT token:**
1. Call your authentication endpoint
2. Copy the access_token from the response
3. Paste it into the `access_token` variable in the environment

### Gemini API Configuration

Update these variables for direct Gemini testing:

| Variable | Default | Description |
|----------|---------|-------------|
| `gemini_api_key` | `your_gemini_api_key_here` | Your Google Gemini API key |
| `gemini_model` | `gemini-1.5-flash` | Model name (flash or pro) |

## RecipeVault API Endpoints

### 1. Search Recipes
```
GET /api/v1/recipes
```
**Query Parameters:**
- `pageNumber` - Page number (default: 1)
- `pageSize` - Items per page (default: 10)
- `sort` - Sort field (e.g., "title")
- `title` - Filter by title (optional)

**Response:** Paginated list of recipes

### 2. Get Recipe by ID
```
GET /api/v1/recipes/{id}
```
**Path Parameters:**
- `id` - Recipe resource ID (UUID)

**Response:** Single recipe with all details

### 3. Create Recipe
```
POST /api/v1/recipes
```
**Request Body:**
```json
{
  "title": "Recipe Name",
  "description": "Description",
  "yield": 4,
  "prepTimeMinutes": 15,
  "cookTimeMinutes": 30,
  "source": "https://...",
  "originalImageUrl": "https://...",
  "ingredients": [
    {
      "sortOrder": 1,
      "quantity": 2.5,
      "unit": "cup",
      "item": "flour",
      "preparation": null,
      "rawText": "2 1/2 cups flour"
    }
  ],
  "instructions": [
    {
      "stepNumber": 1,
      "instruction": "Step description",
      "rawText": "Step description"
    }
  ]
}
```

**Response:** Created recipe with ID

### 4. Update Recipe
```
PUT /api/v1/recipes/{id}
```
**Request Body:** Same as Create (partial updates allowed)

**Response:** Updated recipe

### 5. Delete Recipe
```
DELETE /api/v1/recipes/{id}
```
**Response:** 204 No Content

### 6. Parse Recipe Image
```
POST /api/v1/recipes/parse
```
**Request Body:**
```json
{
  "image": "base64_encoded_image_data",
  "mimeType": "image/png"
}
```

**Response Codes:**
- **200 OK** - Successfully parsed
```json
{
  "confidence": 0.95,
  "parsed": {
    "title": "Chocolate Cake",
    "yield": 8,
    "prepTimeMinutes": 15,
    "cookTimeMinutes": 35,
    "ingredients": [...],
    "instructions": [...]
  },
  "warnings": []
}
```

- **422 Unprocessable Entity** - No recipe detected in image
- **503 Service Unavailable** - Gemini API failure

## Gemini API Direct Testing

### Parse Recipe from Image

Send multimodal request with image:

```bash
POST /v1beta/models/gemini-1.5-flash:generateContent?key=YOUR_API_KEY
```

**Supported Image Types:**
- image/jpeg
- image/png
- image/gif
- image/webp

**Base64 Encoding Example:**
```bash
# Linux/Mac
base64 < recipe.jpg

# PowerShell
[Convert]::ToBase64String([IO.File]::ReadAllBytes('recipe.jpg'))
```

### Rate Limits

Gemini API Free Tier:
- **Requests per minute:** 15
- **Requests per day:** 500
- **RPM with API key:** 60

If you hit rate limits:
- Wait the specified time (check `Retry-After` header)
- Consider upgrading to Gemini Pro
- Implement exponential backoff in your application

## Testing Workflow

### Complete Recipe Creation Workflow

1. **Create a new recipe:**
   - Open "Create Recipe" request
   - Modify the body with your recipe data
   - Send request
   - Copy the recipe ID from response

2. **Set the recipe ID in environment:**
   - Update `recipe_id` variable with the copied ID

3. **Get the recipe:**
   - Open "Get Recipe by ID" request
   - Click Send
   - Verify all data was saved correctly

4. **Update the recipe:**
   - Open "Update Recipe" request
   - Modify the body with new data
   - Send request

5. **Delete the recipe:**
   - Open "Delete Recipe" request
   - Send request
   - Verify 204 response

### Recipe Parsing Workflow

1. **Prepare recipe image:**
   - Get a clear photo/scan of a recipe
   - Note the file path

2. **Encode image to Base64:**
   - Linux/Mac: `base64 recipe.jpg | tr -d '\n'`
   - PowerShell: `[Convert]::ToBase64String([IO.File]::ReadAllBytes('recipe.jpg'))`
   - Online tool: https://www.base64encode.org/

3. **Test with RecipeVault:**
   - Open "Parse Recipe Image" request
   - Replace `image` value with your Base64 data
   - Update `mimeType` if needed (image/jpeg, image/png)
   - Send request
   - Review parsed data and warnings

4. **Save parsed recipe:**
   - Copy the `parsed` object from response
   - Use it to create a recipe (modify the Create Recipe body)
   - Send Create Recipe request

### Direct Gemini Testing

1. **Get available models:**
   - Open "Get Available Models" request
   - Send to verify API key works
   - Note available model names

2. **Test token counting:**
   - Open "Count Tokens" request
   - Modify prompt if needed
   - Send to see token usage

3. **Parse image directly:**
   - Open "Parse Recipe Image - With Recipe" request
   - Replace Base64 image data in the body
   - Send request
   - View raw JSON response from Gemini

## Error Handling

### Common Errors

| Status | Error | Solution |
|--------|-------|----------|
| 401 | Unauthorized | Update `access_token` with valid JWT |
| 404 | Not Found | Verify `recipe_id` exists and is correct |
| 422 | Unprocessable Entity | Image doesn't contain a recipe |
| 429 | Too Many Requests | Wait before making more requests |
| 503 | Service Unavailable | Gemini API is down, retry later |

### Debug Requests

1. Click the request
2. Open **Pre-request Script** tab to see any setup
3. Open **Tests** tab to see validation
4. Check **Response** tab for:
   - Status code
   - Headers
   - Body (raw/pretty)

## Tips & Best Practices

1. **Use Variables:**
   - Store sensitive data in environment variables
   - Never commit actual API keys to version control
   - Use {{variable_name}} syntax in requests

2. **Organize Collections:**
   - Group related requests in folders
   - Use meaningful names for requests
   - Add descriptions for complex operations

3. **Save Responses:**
   - Right-click response → Save as example
   - Useful for documentation and testing

4. **Batch Operations:**
   - Use Collection Runner for automated testing
   - Click the folder icon next to collection name
   - Click "Run" to execute multiple requests

5. **Monitor Performance:**
   - Use Gemini API console to track usage
   - Set up alerts for quota limits
   - Consider caching frequently requested images

## Useful Resources

- **RecipeVault Documentation:** See Design.md in project root
- **Gemini API Docs:** https://ai.google.dev/docs
- **Postman Learning:** https://learning.postman.com/
- **JSON Schema:** https://json-schema.org/
- **Base64 Encoding:** https://www.base64encode.org/

## Troubleshooting

### "Variables are not being replaced"
- Select the correct environment from dropdown
- Ensure variables are spelled correctly (case-sensitive)
- Check that variables are enabled (green checkbox)

### "401 Unauthorized on RecipeVault API"
- Verify JWT token is still valid
- Get a new token from auth endpoint
- Check token format (should start with "Bearer ")

### "Invalid API Key for Gemini"
- Verify key is copied correctly (no spaces)
- Check key is enabled in Google Cloud Console
- Ensure billing is set up for the project

### "No recipe detected" (422 from parse endpoint)
- Image quality might be too low
- Text might be too small to read
- Try a different recipe image
- Ensure it's actually a recipe (not other text)

### Slow Response Times
- Check internet connection
- Verify API server is responsive
- Consider model size (flash vs pro)
- Check for rate limiting

## Security Notes

⚠️ **Important:**
- Never commit API keys or tokens to Git
- Use `.gitignore` for environment files with secrets
- Rotate API keys regularly
- Use separate keys for development/production
- Enable audit logging in Google Cloud Console
- Set up IP allowlists if possible

---

Happy testing! For issues or questions, refer to the project documentation or contact the development team.
