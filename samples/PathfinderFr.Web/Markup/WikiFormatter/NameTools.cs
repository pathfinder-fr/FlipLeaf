using System;

namespace PathfinderFr.Markup.WikiFormatter
{
    /// <summary>
    ///     Implements useful tools for handling full object names.
    /// </summary>
    internal static class NameTools
    {
        /// <summary>
        ///     Gets the full name of a page from the namespace and local name.
        /// </summary>
        /// <param name="nspace">The namespace (<c>null</c> for the root).</param>
        /// <param name="name">The local name.</param>
        /// <returns>The full name.</returns>
        public static string GetFullName(string nspace, string name) => (!string.IsNullOrEmpty(nspace) ? nspace + "." : "") + name;

        /// <summary>
        ///     Expands a full name into the namespace and local name.
        /// </summary>
        /// <param name="fullName">The full name to expand.</param>
        /// <param name="nspace">The namespace.</param>
        /// <param name="name">The local name.</param>
        public static void ExpandFullName(string fullName, out string nspace, out string name)
        {
            if (fullName == null)
            {
                nspace = null;
                name = null;
            }
            else
            {
                var fields = fullName.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                if (fields.Length == 0)
                {
                    nspace = null;
                    name = null;
                }
                else if (fields.Length == 1)
                {
                    nspace = null;
                    name = fields[0];
                }
                else
                {
                    nspace = fields[0];
                    name = fields[1];
                }
            }
        }

        /// <summary>
        ///     Extracts the namespace from a full name.
        /// </summary>
        /// <param name="fullName">The full name.</param>
        /// <returns>The namespace, or <c>null</c>.</returns>
        public static string GetNamespace(string fullName)
        {
            string nspace, name;
            ExpandFullName(fullName, out nspace, out name);
            return nspace;
        }

        /// <summary>
        ///     Extracts the local name from a full name.
        /// </summary>
        /// <param name="fullName">The full name.</param>
        /// <returns>The local name.</returns>
        public static string GetLocalName(string fullName)
        {
            string nspace, name;
            ExpandFullName(fullName, out nspace, out name);
            return name;
        }

        /// <summary />
        public static bool AreNamespaceEquals(NamespaceInfo ns1, string ns2) => AreNamespaceEquals(ns1 != NamespaceInfo.Empty ? ns1.Name : null, ns2);

        /// <summary />
        public static bool AreNamespaceEquals(string ns1, NamespaceInfo ns2) => AreNamespaceEquals(ns1, ns2 != NamespaceInfo.Empty ? ns2.Name : null);

        /// <summary />
        public static bool AreNamespaceEquals(NamespaceInfo ns1, NamespaceInfo ns2) => AreNamespaceEquals(ns1 != NamespaceInfo.Empty ? ns1.Name : null, ns2 != NamespaceInfo.Empty ? ns2.Name : null);

        /// <summary />
        public static bool AreNamespaceEquals(string ns1, string ns2)
        {
            if (ns1 == null)
            {
                return ns2 == null;
            }

            if (ns2 == null)
            {
                return false;
            }
            
            return ns1.Equals(ns2, StringComparison.OrdinalIgnoreCase);
        }
    }

}
