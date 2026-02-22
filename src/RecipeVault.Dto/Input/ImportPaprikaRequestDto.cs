using System.IO;

namespace RecipeVault.Dto.Input {
    public class ImportPaprikaRequestDto {
        public Stream FileStream { get; set; }
        public string FileName { get; set; }
    }
}
