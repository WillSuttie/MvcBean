using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using MvcBean.Utilities;


namespace MvcBean.Models
{
    [Index(nameof(SaleDate), IsUnique = true)]
    public class Bean
    {
        // Constants for error messages and string lengths
        private const string ValidStringRegex = @"^[a-zA-Z0-9\s&'()#!?,.-]{3,60}$";
        private const string ValidHexRegex = @"^#[0-9A-Fa-f]{6}$";

        private const string StringErrorMsg = "Entry may only include letters, numbers, spaces, symbols '()#!?,.- and must be between 3 and 60 characters long.";
        private const string ColourErrorMsg = "Invalid colour value";
        private const string PriceErrorMsg = "Price must be between £0.01 and £99,999.99";
        private const string NameErrorMsg = "A name is required";

        public const string PlaceholderImagePath = "/images/placeholder.jpg";

        //Id
        public int Id { get; set; }

        //Name - Duplicate names are valid. This allows the same bean to be sold on different dates at different prices, and stays within the defined business rules about 
        //       parameters. It feels a little pedantic, and maybe IRL I would ask the client if they wanted multiple sale dates for a bean, with built-in varying prices.
        private string? _name;
        [Required(ErrorMessage = NameErrorMsg)]
        [RegularExpression(ValidStringRegex, ErrorMessage = StringErrorMsg)]
        public string? Name
        {
            get => _name;
            set => _name = value?.TrimSafe();
        }

        //Sale Date
        [Display(Name = "Sale Date")]
        [DataType(DataType.Date)]
        public DateOnly SaleDate { get; set; }

        //Aroma
        [RegularExpression(ValidStringRegex, ErrorMessage = StringErrorMsg)]
        private string? _aroma;
        public string? Aroma
        {
            get => _aroma;
            set => _aroma = value?.TrimSafe();
        }

        //Colour
        [Display(Name = "Colour")]
        [RegularExpression(ValidHexRegex, ErrorMessage = ColourErrorMsg)]
        private string? _colourHex;
        public string? ColourHex
        {
            get => _colourHex;
            set => _colourHex = value?.TrimSafe();
        }

        //PricePer100g
        [Range(0.01, 10000, ErrorMessage = PriceErrorMsg)]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(7, 2)")]
        public decimal PricePer100g { get; set; }

        //Image
        [Display(Name = "Image")]
        [ImageUtilities]
        public string? ImagePath { get; set; }


    }
}