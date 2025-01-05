using System.ComponentModel.DataAnnotations;

namespace MovieGO.Models
{
    public class Cinema
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Nazwa kina jest wymagana.")]
        [StringLength(50, ErrorMessage = "Nazwa kina nie może przekroczyć 50 znaków.")]
        [Display(Name = "Nazwa kina")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Adres ulicy jest wymagany.")]
        [StringLength(50, ErrorMessage = "Adres ulicy nie może przekroczyć 50 znaków.")]
        [Display(Name = "Ulica")]
        public string StreetAddress { get; set; }
        [Required(ErrorMessage = "Kod pocztowy jest wymagany.")]
        [RegularExpression(@"^\d{2}-\d{3}$", ErrorMessage = "Nieprawidłowy format kodu pocztowego. Wprowadź w formacie XX-XXX.")]
        [Display(Name = "Kod pocztowy")]
        public string PostalCode { get; set; }
        [Required(ErrorMessage = "Miasto jest wymagane.")]
        [StringLength(50, ErrorMessage = "Nazwa miasta nie może przekroczyć 50 znaków.")]
        [Display(Name = "Miasto")]
        public string City { get; set; }
        public string? ImagePath { get; set; }
        public ICollection<Hall> Halls { get; set; } = new List<Hall>();
    }
}
