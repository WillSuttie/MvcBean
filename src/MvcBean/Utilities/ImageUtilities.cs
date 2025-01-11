using System.ComponentModel.DataAnnotations;
using SkiaSharp;

namespace MvcBean.Utilities
{
    public class ImageUtilities : ValidationAttribute
    {
        private static readonly string[] ValidExtensions = { ".jpg", ".jpeg", ".png", ".gif" };

        public ImageUtilities() : base("Invalid image file type. Please upload a valid image.")
        {
        }

        // Validate model property (e.g., ImagePath)
        public override bool IsValid(object? value)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                // Null or empty values are considered valid (handled elsewhere)
                return true;
            }

            var extension = Path.GetExtension(value.ToString())?.ToLower();
            return !string.IsNullOrEmpty(extension) && ValidExtensions.Contains(extension);
        }

        // Static helper to validate IFormFile (e.g., in controllers)
        public static bool IsValidFile(IFormFile? file)
        {
            if (file == null || file.Length == 0)
            {
                // Null or empty file is considered valid (no upload case)
                return true;
            }

            var extension = Path.GetExtension(file.FileName)?.ToLower();
            return !string.IsNullOrEmpty(extension) && ValidExtensions.Contains(extension);
        }

        // **New Method**: Static helper to validate file extensions for strings
        public static bool IsValidFileExtension(string? filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return false;

            var extension = Path.GetExtension(filePath)?.ToLower();
            return !string.IsNullOrEmpty(extension) && ValidExtensions.Contains(extension);
        }

        // Static helper to get the allowed file extensions (for error messages or UI)
        public static string GetAllowedExtensionsMessage()
        {
            return string.Join(", ", ValidExtensions);
        }

        // Utility: Get average color from an image
        public static string GetAverageColorHex(string filePath)
        {
            try
            {
                // Decode the image using SKCodec for broader support (e.g., animated GIFs)
                using var codec = SKCodec.Create(filePath);
                var info = codec.Info;
                using var bitmap = new SKBitmap(info.Width, info.Height);

                // Get the pixels from the image
                codec.GetPixels(bitmap.Info, bitmap.GetPixels());

                long totalR = 0, totalG = 0, totalB = 0;
                int totalPixels = bitmap.Width * bitmap.Height;

                // Access all pixels
                var pixels = bitmap.Pixels;
                for (int i = 0; i < pixels.Length; i++)
                {
                    var pixel = pixels[i];
                    totalR += pixel.Red;
                    totalG += pixel.Green;
                    totalB += pixel.Blue;
                }

                // Calculate average RGB values
                int avgR = (int)(totalR / totalPixels);
                int avgG = (int)(totalG / totalPixels);
                int avgB = (int)(totalB / totalPixels);

                // Convert to hex and return
                return $"#{avgR:X2}{avgG:X2}{avgB:X2}";
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error processing image: {ex.Message}", ex);
            }
        }
    }
}
