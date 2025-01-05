using System.ComponentModel.DataAnnotations;

namespace MovieGO.Models
{
    public class Screening
    {
        public int Id { get; set; }
        public bool IsHidden { get; set; } = false;

        [Required(ErrorMessage = "Data seansu jest wymagana.")]
        [CustomValidation(typeof(Screening), nameof(ValidateScreeningDate))]
        [Display(Name = "Data seansu")]
        public DateTime ScreeningDate { get; set; }

        [Required(ErrorMessage = "Sala jest wymagana.")]
        [Display(Name = "Sala")]
        public int HallId { get; set; }
        [Display(Name = "Sala")]
        public Hall? Hall { get; set; }

        [Required(ErrorMessage = "Film jest wymagany.")]
        [Display(Name = "Film")]
        public int MovieId { get; set; }

        [Display(Name = "Film")]
        public Movie? Movie { get; set; }

        public Cinema? Cinema { get; set; }

        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

        public static ValidationResult? ValidateScreeningDate(DateTime screeningDate, ValidationContext context)
        {
            if (screeningDate.Date < DateTime.Now.Date.AddDays(1))
            {
                return new ValidationResult("Seans musi być zaplanowany na conajmniej 24h od teraz.");
            }
            return ValidationResult.Success;
        }
    }
}