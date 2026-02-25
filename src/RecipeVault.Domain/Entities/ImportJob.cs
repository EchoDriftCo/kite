using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cortside.AspNetCore.Auditable.Entities;
using Microsoft.EntityFrameworkCore;
using RecipeVault.Domain.Enums;
using UUIDNext;

namespace RecipeVault.Domain.Entities {
    [Index(nameof(ImportJobResourceId), IsUnique = true)]
    [Table("ImportJob")]
    public class ImportJob : AuditableEntity {
        protected ImportJob() {
        }

        public ImportJob(ImportJobType type, int totalItems) {
            ImportJobResourceId = Uuid.NewDatabaseFriendly(Database.SqlServer);
            Type = type;
            Status = ImportJobStatus.Pending;
            TotalItems = totalItems;
            ProcessedItems = 0;
            SuccessCount = 0;
            FailureCount = 0;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ImportJobId { get; private set; }

        public Guid ImportJobResourceId { get; private set; }

        public ImportJobType Type { get; private set; }

        public ImportJobStatus Status { get; private set; }

        public int TotalItems { get; private set; }

        public int ProcessedItems { get; private set; }

        public int SuccessCount { get; private set; }

        public int FailureCount { get; private set; }

        public DateTime? CompletedDate { get; private set; }

        [StringLength(int.MaxValue)]
        public string ResultsJson { get; private set; }

        [StringLength(2000)]
        public string ErrorMessage { get; private set; }

        public void UpdateProgress(int processed, int success, int failure) {
            ProcessedItems = processed;
            SuccessCount = success;
            FailureCount = failure;

            if (ProcessedItems >= TotalItems) {
                Status = ImportJobStatus.Complete;
                CompletedDate = DateTime.UtcNow;
            }
        }

        public void SetStatus(ImportJobStatus status) {
            Status = status;
            if (status == ImportJobStatus.Complete || status == ImportJobStatus.Failed) {
                CompletedDate = DateTime.UtcNow;
            }
        }

        public void SetResults(string resultsJson) {
            ResultsJson = resultsJson;
        }

        public void SetError(string errorMessage) {
            ErrorMessage = errorMessage?.Length > 2000 ? errorMessage.Substring(0, 2000) : errorMessage;
            Status = ImportJobStatus.Failed;
            CompletedDate = DateTime.UtcNow;
        }
    }
}
