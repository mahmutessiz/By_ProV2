using System;
using System.Text;

namespace By_ProV2.Helpers
{
    public static class DocumentNumberGenerator
    {
        private static readonly Random Random = new Random();
        
        /// <summary>
        /// Generates a unique document number in the format: SUT-YYYY-XXXXXXX
        /// Where SUT indicates Sut Alim, YYYY is the current year, and XXXXXXX is a 7-digit random number
        /// </summary>
        /// <returns>A unique document number string</returns>
        public static string GenerateSutAlimDocumentNumber()
        {
            int year = DateTime.Now.Year;
            int randomNumber = Random.Next(1000000, 9999999); // 7-digit random number
            
            return $"SUT-{year}-{randomNumber}";
        }
        
        /// <summary>
        /// Generates a document number with custom prefix
        /// </summary>
        /// <param name="prefix">Document prefix (e.g. "SUT", "SIP", etc.)</param>
        /// <returns>A unique document number string</returns>
        public static string GenerateDocumentNumber(string prefix = "DOC")
        {
            int year = DateTime.Now.Year;
            int randomNumber = Random.Next(1000000, 9999999); // 7-digit random number
            
            return $"{prefix}-{year}-{randomNumber}";
        }
    }
}