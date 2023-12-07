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
	}
}
