using System.ComponentModel.DataAnnotations;

namespace MovieGO.Models
{
        public class Category
        {
            public int Id { get; set; }
            [Required(ErrorMessage = "Nazwa gatunku jest wymagana.")]
            [StringLength(50, ErrorMessage = "Nazwa gatunku nie może przekroczyć 50 znaków.")]
            [Display(Name = "Nazwa gatunku")]
            public string Name { get; set; }
            public ICollection<Movie> Movies { get; set; } = new List<Movie>();
        }
}
