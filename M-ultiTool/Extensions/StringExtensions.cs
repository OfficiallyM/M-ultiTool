using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiTool.Extensions
{
	public static class StringExtensions
	{
		/// <summary>
		/// Convert string to sentence case.
		/// </summary>
		/// <param name="s">String to convert</param>
		/// <returns>String in sentence case</returns>
		public static string ToSentenceCase(this string s)
		{
			return s.Substring(0, 1).ToUpper() + s.Substring(1);
		}

		/// <summary>
		/// Get if a string is in all upper case.
		/// </summary>
		/// <param name="s">String to check</param>
		/// <returns>True if the string is all in upper case, otherwise false</returns>
		public static bool IsAllUpper(this string s)
		{
			for (int i = 0; i < s.Length; i++)
			{
				if (char.IsLetter(s[i]) && !char.IsUpper(s[i]))
					return false;
			}
			return true;
		}

		/// <summary>
		/// Get if a string is in all lower case.
		/// </summary>
		/// <param name="s">String to check</param>
		/// <returns>True if the string is in all lower case, otherwise false</returns>
		public static bool IsAllLower(this string s)
		{
			for (int i = 0; i < s.Length; i++)
			{
				if (char.IsLetter(s[i]) && !char.IsLower(s[i]))
					return false;
			}
			return true;
		}
	}
}
