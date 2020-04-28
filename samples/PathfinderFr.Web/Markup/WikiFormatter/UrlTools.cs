using System;
using System.Text;

namespace PathfinderFr.Markup.WikiFormatter
{
    internal static class UrlTools
    {
        internal static string UrlEncode(string fullName) => Uri.EscapeDataString(fullName);

        internal static string UrlDecode(string fullName) => Uri.UnescapeDataString(fullName);

        /// <summary>
        /// Obfuscates text, replacing each character with its HTML escaped sequence, for example a becomes <c>&amp;#97;</c>.
        /// </summary>
        /// <param name="input">The input text.</param>
        /// <returns>The output obfuscated text.</returns>
        public static string ObfuscateText(string input)
        {
            var buffer = new StringBuilder(input.Length * 4);

            foreach (var c in input)
            {
                buffer.Append("&#" + ((int)c).ToString("D2") + ";");
            }

            return buffer.ToString();
        }

        /// <summary>
        /// Builds a URL properly appendind the <b>NS</b> parameter if appropriate.
        /// </summary>
        /// <param name="destination">The destination <see cref="T:StringBuilder"/>.</param>
        /// <param name="chunks">The chunks to append.</param>
        public static void BuildUrl(StringBuilder destination, params string[] chunks)
        {
            if (destination == null) throw new ArgumentNullException("destination");

            destination.Append(BuildUrl(chunks));
        }

        /// <summary>
        /// Builds a URL properly prepending the namespace to the URL.
        /// </summary>
        /// <param name="chunks">The chunks used to build the URL.</param>
        /// <returns>The complete URL.</returns>
        public static string BuildUrl(params string[] chunks)
        {
            if (chunks == null) throw new ArgumentNullException("chunks");
            if (chunks.Length == 0) return ""; // Shortcut

            var temp = new StringBuilder(chunks.Length * 10);
            foreach (var chunk in chunks)
            {
                temp.Append(chunk);
            }

            var tempString = temp.ToString();

            if (tempString.StartsWith("++")) return tempString.Substring(2);

            string nspace = null;
            //if (HttpContext.Current != null)
            //{
            //    // HttpContext.Current can be null when executing asynchronous tasks
            //    // The point is that BuildUrl is called without namespace info only from the web application, so HttpContext is available in that case
            //    // When the context is not available, in all cases BuildUrl is called by the formatter, that has already included namespace info in the URL
            //    nspace = HttpContext.Current.Request["NS"];
            //    if (string.IsNullOrEmpty(nspace)) nspace = null;
            //    if (nspace == null) nspace = GetCurrentNamespace();
            //}
            if (string.IsNullOrEmpty(nspace)) nspace = null;
            //else nspace = new pages.FindNamespace(nspace).Name;

            if (nspace != null)
            {
                var tempStringLower = tempString.ToLowerInvariant();
                if ((tempStringLower.Contains(".ashx") || tempStringLower.Contains(".aspx")) && !tempString.StartsWith(UrlEncode(nspace) + ".")) temp.Insert(0, nspace + ".");
            }

            return temp.ToString();
        }
    }

}
