using System;
using System.Linq;
using System.Text;

namespace TheRealCracking
{
    class StringUtilities
    {
        /// <summary>
        /// Capitalized the string
        /// </summary>
        /// <param name="str">The string that need to be capitalized</param>
        /// <returns>The capitalized string</returns>
        public static String Capitalize(String str)
        {
            if (str == null)
            {
                throw new ArgumentNullException("str");
            }
            if (str.Trim().Length == 0)
            {
                return str;
            }
            String firstLetterUppercase = str.Substring(0, 1).ToUpper();
            String theRest = str.Substring(1);
            return firstLetterUppercase + theRest;
        }

        /// <summary>
        /// Reserved the string
        /// </summary>
        /// <param name="str">The string that need to be reserved</param>
        /// <returns>The reserved string</returns>
        public static String Reverse(String str)
        {
            if (str == null)
            {
                throw new ArgumentNullException("str");
            }
            if (str.Trim().Length == 0)
            {
                return str;
            }
            StringBuilder reverseString = new StringBuilder();
            for (int i = 0; i < str.Length; i++)
            {
                reverseString.Append(str.ElementAt(str.Length - 1 - i));
            }
            return reverseString.ToString();
        }
    }
}
