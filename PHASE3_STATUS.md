# Phase 3: Export + Batch Import + Multi-Image Import - Status

## ✅ Completed

### Database & Domain Layer
- [x] `ImportJob` entity with proper relationships
  - `ImportJobId` (int, primary key)
  - `ImportJobResourceId` (Guid, unique identifier)
  - `SubjectId` (Guid, user reference)
  - `Type` enum (Paprika/UrlBatch/Cookbook)
  - `Status` enum (Pending/Processing/Complete/Failed)
  - Progress tracking fields (TotalItems, ProcessedItems, SuccessCount, FailureCount)
  - `ResultsJson` for storing import results
  - Audit fields (CreatedDate, CompletedDate)

- [x] `ImportJobType` enum
- [x] `ImportJobStatus` enum
- [x] EF Core migration created (`AddImportJob` + `AddImportJobTable`)

### Repository Layer
- [x] `IImportJobRepository` interface
- [x] `ImportJobRepository` implementation with CRUD operations
- [x] Registered in DbContext (`IRecipeVaultDbContext` + `RecipeVaultDbContext`)

### Service Layer
- [x] Multi-image import in `ImportService.ImportFromMultipleImagesAsync`
  - Processes 1-4 images
  - Sequential OCR extraction via Gemini
  - Concatenates all text before parsing
  - Returns parsed recipe for preview

### API Layer
- [x] Multi-image import endpoint: `POST /api/v1/import/multi-image`
  - Accepts multipart form with images list
  - Optional processing mode parameter
  - Returns parsed `RecipeDto`

### Infrastructure
- [x] Build fixed and working
- [x] Line ending fixes (`stepNumber` vs `sortOrder` in RecipeInstruction)

---

## ⏳ Still Needed

### 1. Batch Import (Async Job Processing)
**Backend:**
- [ ] `ImportFacade.StartBatchImportAsync(IFormFile file, ImportJobType type)` → returns ImportJobDto
- [ ] `ImportController` endpoints:
  - `POST /api/v1/import/batch` - accepts file, starts async job
  - `GET /api/v1/import/{jobId}` - returns job status/progress
  - `GET /api/v1/import/{jobId}/results` - returns detailed results
  - `DELETE /api/v1/import/{jobId}` - cancels job
- [ ] Background job processor (use `BackgroundService` or similar)
- [ ] Store per-item results in `ResultsJson` field

**Design considerations:**
- Use `ImportJob` to track progress
- Store individual recipe import results (success/failure) in JSON
- Update progress fields as items are processed
- Handle cancellation gracefully

### 2. Export Features
**Backend:**
- [ ] `IExportService` + `ExportService`
  - `ExportRecipeAsJsonAsync(Recipe recipe)` → returns JSON string
  - `ExportRecipeAsTextAsync(Recipe recipe)` → returns formatted text
  - `ExportRecipeAsPaprikaAsync(Recipe recipe, bool includeImage)` → returns .paprikarecipes format
  
- [ ] `IExportFacade` + `ExportFacade`
  - `ExportSingleRecipeAsync(int recipeId, string format)` → returns file content
  - `ExportAllRecipesAsync(Guid subjectId, string format)` → starts async export job
  - `GetExportJobStatusAsync(Guid jobId)` → returns job status
  - `DownloadExportAsync(Guid jobId)` → returns file content

- [ ] `ExportController` endpoints:
  - `GET /api/v1/recipes/{id}/export?format=json|text|paprika` - single recipe export
  - `POST /api/v1/export/all` - starts bulk export job (async)
  - `GET /api/v1/export/{jobId}` - job status
  - `GET /api/v1/export/{jobId}/download` - download exported file

**Note:** Pre-written ExportService had fundamental errors (wrong property names, incompatible types). Needs rewrite from scratch following actual entity structure.

### 3. Frontend (Angular)
- [ ] Multi-image import component
  - Drag-to-reorder file upload UI
  - Preview selected images
  - Call `/api/v1/import/multi-image`
  - Show parsed recipe preview
  - Allow edit before saving

- [ ] Batch import page
  - File upload (Paprika, URL list, etc.)
  - Progress tracker (polling `/api/v1/import/{jobId}`)
  - Results display (per-item success/failure)

- [ ] Export UI
  - Single recipe export button on recipe detail page
  - Format selector (JSON, text, Paprika)
  - Bulk export page with format selection
  - Download link when ready

---

## Known Issues & Fixes Applied

1. **RecipeInstruction parameter mismatch**: Changed `sortOrder:` to `stepNumber:` in ImportService line 654
2. **ExportService property errors**: Removed incomplete ExportService/Facade/Controller due to:
   - `RecipeIngredient.Notes` doesn't exist
   - `RecipeInstruction.SortOrder` should be `StepNumber`
   - PaprikaRecipe model used lowercase properties but code tried camelCase
   - Multiple other type/property mismatches

3. **IRecipeVaultDbContext missing ImportJobs**: Added `DbSet<ImportJob>` to interface

---

## Testing Checklist (Once Complete)

### Multi-Image Import
- [ ] Upload 1 image → parses correctly
- [ ] Upload 4 images → concatenates text correctly
- [ ] Order matters (image 1 text before image 2 text)
- [ ] Preview shows correct recipe data
- [ ] Can save imported recipe

### Batch Import
- [ ] Can upload Paprika file → starts job
- [ ] Job status updates correctly
- [ ] Progress increments as items process
- [ ] Can view results (success/failure per recipe)
- [ ] Can cancel in-progress job

### Export
- [ ] Single recipe export (JSON) → valid JSON output
- [ ] Single recipe export (text) → readable format
- [ ] Single recipe export (Paprika) → valid .paprikarecipes file
- [ ] Bulk export → creates job, allows download
- [ ] Exported recipes can be re-imported

---

## Migration Commands

```bash
# Generate migration SQL for review
dotnet ef migrations script --idempotent --project src\RecipeVault.Data --startup-project src\RecipeVault.WebApi

# Apply migration locally
dotnet ef database update --project src\RecipeVault.Data --startup-project src\RecipeVault.WebApi

# Apply to production (Supabase)
# Run SQL from migrations folder via Supabase dashboard SQL editor
```

---

## Next Steps

1. **Priority 1:** Implement basic Export (single recipe, JSON/text formats)
   - Start with simple JSON serialization
   - Add text formatter
   - Defer Paprika format for later

2. **Priority 2:** Add batch import endpoints + background job
   - Use `IHostedService` or `BackgroundService`
   - Process items sequentially, update job progress
   - Store results in `ImportJob.ResultsJson`

3. **Priority 3:** Build Angular components
   - Start with multi-image import (already has backend)
   - Add export button to recipe detail page
   - Build batch import page last

4. **Priority 4:** Paprika export format
   - Research Paprika .paprikarecipes format
   - Implement compression/packaging
   - Handle image embedding

---

## Files Changed

### Created
- `src/RecipeVault.Domain/Entities/ImportJob.cs`
- `src/RecipeVault.Domain/Enums/ImportJobType.cs`
- `src/RecipeVault.Domain/Enums/ImportJobStatus.cs`
- `src/RecipeVault.Data/Repositories/IImportJobRepository.cs`
- `src/RecipeVault.Data/Repositories/ImportJobRepository.cs`
- `src/RecipeVault.Data/Migrations/20260225021338_AddImportJob.cs`
- `src/RecipeVault.Data/Migrations/20260225021339_AddImportJobTable.cs`
- `src/RecipeVault.Dto/Output/ImportJobDto.cs`
- `src/RecipeVault.Dto/Input/ImportMultiImageRequestDto.cs`
- `src/RecipeVault.Dto/Input/ExportRequestDto.cs`

### Modified
- `src/RecipeVault.Data/IRecipeVaultDbContext.cs` (added ImportJobs DbSet)
- `src/RecipeVault.Data/RecipeVaultDbContext.cs` (added ImportJobs DbSet)
- `src/RecipeVault.DomainService/ImportService.cs` (fixed stepNumber parameter, added multi-image method)
- `src/RecipeVault.Facade/ImportFacade.cs` (added multi-image method)
- `src/RecipeVault.WebApi/Controllers/ImportController.cs` (added multi-image endpoint)

### Removed (due to errors)
- `src/RecipeVault.DomainService/ExportService.cs`
- `src/RecipeVault.DomainService/IExportService.cs`
- `src/RecipeVault.Facade/ExportFacade.cs`
- `src/RecipeVault.Facade/IExportFacade.cs`
- `src/RecipeVault.WebApi/Controllers/ExportController.cs`
