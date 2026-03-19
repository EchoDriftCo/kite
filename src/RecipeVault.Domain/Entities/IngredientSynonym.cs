using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RecipeVault.Domain.Entities {
    [Index(nameof(Synonym), IsUnique = true)]
    [Index(nameof(CanonicalName))]
    [Table("IngredientSynonym")]
    public class IngredientSynonym {
        protected IngredientSynonym() {
        }

        public IngredientSynonym(string canonicalName, string synonym, SynonymSource source = SynonymSource.Manual) {
            CanonicalName = canonicalName;
            Synonym = synonym;
            Source = source;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IngredientSynonymId { get; private set; }

        [Required]
        [StringLength(250)]
        public string CanonicalName { get; private set; }

        [Required]
        [StringLength(250)]
        public string Synonym { get; private set; }

        public SynonymSource Source { get; private set; }
    }

    public enum SynonymSource {
        Manual = 0,
        Imported = 1,
        UserSuggested = 2
    }
}
