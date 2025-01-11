namespace MvcBean.Utilities
{
    public static class StringUtilities
    {
        // Method to safely trim strings
        public static string? TrimSafe(this string? str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return string.Empty;
            }
            return str.Trim();
        }

        // Method to clean the currency input
        public static decimal CleanCurrencyInput(string input)
        {
            // Remove currency symbols (e.g., £, $) and other non-numeric characters (except for the decimal point)
            var cleanedInput = input.Replace("£", "").Replace("$", "").Replace(",", "");
            if (decimal.TryParse(cleanedInput, out var result))
            {
                return Math.Round(result, 2); // Ensures the result has 2 decimal places
            }
            else
            {
                throw new ArgumentException("Invalid price format.");
            }
        }
    }
}