namespace BookCoverDownloader
{
    internal interface ISBNValidator
    {
        internal static bool IsValidIsbn13(string isbn13)
        {
            bool result = false;
            if (!string.IsNullOrEmpty(isbn13))
            {
                if (isbn13.Contains('-'))
                {
                    isbn13 = isbn13.Replace("-", "");
                }
                // Check if it contains any non numeric chars, if yes, return false

                if (!long.TryParse(isbn13, out long j))
                    return result;

                int sum = 0;
                // The calculation of an ISBN-13 check digit begins with the first 12 digits of the thirteen-digit ISBN (thus excluding the check digit itself).
                // Each digit, from left to right, is alternately multiplied by 1 or 3, then those products are summed modulo 10 to give a value ranging from 0 to 9.
                // Subtracted from 10, that leaves a result from 1 to 10. A zero (0) replaces a ten (10), so, in all cases, a single check digit results.
                for (int i = 0; i < 12; i++)
                {
                    sum += int.Parse(isbn13[i].ToString()) * (i % 2 == 1 ? 3 : 1);
                }

                int remainder = sum % 10;
                int checkDigit = 10 - remainder;
                if (checkDigit == 10) checkDigit = 0;
                result = (checkDigit == int.Parse(isbn13[12].ToString()));
            }
            return result;
        }
    }
}
