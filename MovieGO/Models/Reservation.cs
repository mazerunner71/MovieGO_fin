using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace MovieGO.Models
{
    public class Reservation
    {
        public int Id { get; set; }
        public int ScreeningId { get; set; }
        public Screening? Screening { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Nieprawidłowy rząd.")]
        [Display(Name = "Rząd")]
        public int Row { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Nieprawidłowa kolumna.")]
        [Display(Name = "Miejsce")]
        public int Column { get; set; }
        public string? UserId { get; set; }
        public IdentityUser? User { get; set; }
    }
}