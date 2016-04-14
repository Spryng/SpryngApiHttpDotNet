using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Spryng
{
    internal static class Utilities
    {
        /// <summary>
        /// Test if the number is MSIDN-numeric compliant.
        /// </summary>
        /// <param name="number">The number to check.</param>
        /// <returns>True if the number is complaint, False otherwise.</returns>
        public static bool IsMsidnCompliant(string number)
        {
            return Regex.IsMatch(number, "^[1-9]{1}[0-9]{3,14}");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsDigitsOnly(string str)
        {
            return !str.Any(c => c < '0' || c > '9');
        }
    }
}
