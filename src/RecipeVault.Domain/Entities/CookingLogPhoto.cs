using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RecipeVault.Domain.Entities {
    [Index(nameof(CookingLogId))]
    [Table("CookingLogPhoto")]
    public class CookingLogPhoto {
        protected CookingLogPhoto() {
        }

        public CookingLogPhoto(int cookingLogId, string imageUrl, string caption, int sortOrder) {
            CookingLogId = cookingLogId;
            ImageUrl = imageUrl;
            Caption = caption;
            SortOrder = sortOrder;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CookingLogPhotoId { get; private set; }

        public int CookingLogId { get; private set; }
        public virtual CookingLog CookingLog { get; private set; }

        [Required]
        [StringLength(1000)]
        public string ImageUrl { get; private set; }

        [StringLength(500)]
        public string Caption { get; private set; }

        public int SortOrder { get; private set; }

        public void Update(string imageUrl, string caption, int sortOrder) {
            ImageUrl = imageUrl;
            Caption = caption;
            SortOrder = sortOrder;
        }
    }
}
