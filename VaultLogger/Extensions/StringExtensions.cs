using System;
using System.Text;

namespace VaultLogger.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Extends Contains to allow for ignoring of case
        /// </summary>
        public static bool Contains(this string str, string substring, StringComparison comp)
        {
            if (string.IsNullOrWhiteSpace(str))
                return false;

            if (substring == null)
                throw new ArgumentNullException("substring", "substring cannot be null.");

            else if (!Enum.IsDefined(typeof(StringComparison), comp))
                throw new ArgumentException("comp is not a member of StringComparison", "comp");

            return str.IndexOf(substring, comp) >= 0;
        }

        /// <summary>
        /// Removes string value from full string, allowing for ignoring string case
        /// </summary>
        public static string Remove(this string str, string oldValue, StringComparison comparison)
        {
            StringBuilder sb = new StringBuilder();

            int previousIndex = 0;
            int index = str.IndexOf(oldValue, comparison);
            while (index != -1)
            {
                sb.Append(str.Substring(previousIndex, index - previousIndex));
                index += oldValue.Length;

                previousIndex = index;
                index = str.IndexOf(oldValue, index, comparison);
            }
            sb.Append(str.Substring(previousIndex));

            return sb.ToString();
        }
    }
}
