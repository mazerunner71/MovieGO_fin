using System.ComponentModel.DataAnnotations;

namespace MovieGO.Models
{
    public class Hall
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Nazwa sali jest wymagana.")]
        [StringLength(50, ErrorMessage = "Nazwa sali nie może przekroczyć 50 znaków.")]
        [Display(Name = "Nazwa sali")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Liczba rzędów jest wymagana.")]
        [Range(10, 20, ErrorMessage = "Liczba rzędów powinna być w zakresie od 10 do 20.")]
        [Display(Name = "Liczba rzędów")]
        public int RowCount { get; set; }
        [Required(ErrorMessage = "Liczba kolumn jest wymagana.")]
        [Range(10, 20, ErrorMessage = "Liczba kolumn powinna być w zakresie od 10 do 20.")]
        [Display(Name = "Liczba kolumn")]
        public int ColumnCount { get; set; }
        [Display(Name = "Kino")]
        [Required(ErrorMessage = "Kino jest wymagane.")]
        public int CinemaId { get; set; }
        public Cinema? Cinema { get; set; }
        public ICollection<Screening?> Screenings { get; set; } = new List<Screening>();
    }
}
