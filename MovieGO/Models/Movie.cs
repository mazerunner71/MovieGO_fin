using System.ComponentModel.DataAnnotations;

namespace MovieGO.Models
{
    public class Movie
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Tytuł filmu jest wymagany.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Tytuł filmu musi mieć między 3 a 100 znaków.")]
        [Display(Name = "Tytuł filmu")]
        public string Title { get; set; }
        public string? ImagePath { get; set; }
        [Display(Name = "Gatunek")]
        public int CategoryId { get; set; }
        [Display(Name = "Gatunek")]
        public Category? Category { get; set; }
        [Required(ErrorMessage = "Opis filmu jest wymagany.")]
        [StringLength(1000, ErrorMessage = "Opis filmu nie może przekroczyć 1000 znaków.")]
        [Display(Name = "Opis filmu")]
        public string Description { get; set; }
        [Required(ErrorMessage = "Długość filmu jest wymagana.")]
        [Range(20, 500, ErrorMessage = "Długość filmu powinna być w zakresie od 20 do 500 minut.")]
        [Display(Name = "Długość filmu (w minutach)")]
        public int Duration { get; set; }   
        public ICollection<Screening> Screenings { get; set; } = new List<Screening>();
    }
}
