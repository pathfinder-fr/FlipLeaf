using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using PathfinderFr.Docs;

namespace PathfinderFr.Markup.WikiFormatter
{

    /// <summary>
    /// Performs all the text formatting and parsing operations.
    /// </summary>
    internal class Formatter
    {
        private static readonly Regex NoWikiRegex = new Regex(@"\<nowiki\>(.|\n|\r)+?\<\/nowiki\>", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        private static readonly Regex NoSingleBr = new Regex(@"\<nobr\>(.|\n|\r)+?\<\/nobr\>", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        private static readonly Regex LinkRegex = new Regex(@"(\[\[.{0,120}?\]\])|(\[.{0,120}?\])", RegexOptions.Compiled);
        private static readonly Regex DoubleSquareLinkRegex = new Regex(@"(\[\[.{0,120}?\]\])", RegexOptions.Compiled);
        private static readonly Regex RedirectionRegex = new Regex(@"^\ *\>\>\>\ *.+\ *$", RegexOptions.Compiled | RegexOptions.Multiline);
        private static readonly Regex H1Regex = new Regex(@"^==.+?==\n?", RegexOptions.Compiled | RegexOptions.Multiline);
        private static readonly Regex H2Regex = new Regex(@"^===.+?===\n?", RegexOptions.Compiled | RegexOptions.Multiline);
        private static readonly Regex H3Regex = new Regex(@"^====.+?====\n?", RegexOptions.Compiled | RegexOptions.Multiline);
        private static readonly Regex H4Regex = new Regex(@"^=====.+?=====\n?", RegexOptions.Compiled | RegexOptions.Multiline);
        private static readonly Regex BoldRegex = new Regex(@"'''.+?'''", RegexOptions.Compiled | RegexOptions.Singleline);
        private static readonly Regex ItalicRegex = new Regex(@"''.+?''", RegexOptions.Compiled | RegexOptions.Singleline);
        private static readonly Regex BoldItalicRegex = new Regex(@"'''''.+?'''''", RegexOptions.Compiled | RegexOptions.Singleline);
        private static readonly Regex ApexRegex = new Regex(@"\&lt;sup\&gt(.+?)\&lt;/sup\&gt;", RegexOptions.Compiled | RegexOptions.Singleline);
        private static readonly Regex SubscribeRegex = new Regex(@"\&lt;sub\&gt(.+?)\&lt;/sub\&gt;", RegexOptions.Compiled | RegexOptions.Singleline);
        private static readonly Regex UnderlinedRegex = new Regex(@"__.+?__", RegexOptions.Compiled | RegexOptions.Singleline);
        private static readonly Regex StrikedRegex = new Regex(@"(?<!(\<\!|\&lt;))(\-\-(?!\>).+?\-\-)(?!(\>|\&gt;))", RegexOptions.Compiled | RegexOptions.Singleline);
        private static readonly Regex CodeRegex = new Regex(@"\{\{.+?\}\}", RegexOptions.Compiled | RegexOptions.Singleline);
        private static readonly Regex PreRegex = new Regex(@"\{\{\{\{.+?\}\}\}\}", RegexOptions.Compiled | RegexOptions.Singleline);
        private static readonly Regex BoxRegex = new Regex(@"\(\(\(.+?\)\)\)", RegexOptions.Compiled | RegexOptions.Singleline);
        private static readonly Regex BaseRegex = new Regex(@"\{base\}", RegexOptions.IgnoreCase);
        private static readonly Regex UploadRegex = new Regex(@"\{updir\}", RegexOptions.IgnoreCase);
        private static readonly Regex ExtendedUpRegex = new Regex(@"\{up((\:|\().+?)?\}", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex SpecialTagRegex = new Regex(@"\{(wikititle|wikiversion|mainurl|rsspage|themepath|clear|top|searchbox|pagecount|pagecount\(\*\)|categories|cloud|orphans|wanted|namespacelist)\}", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        private static readonly Regex SpecialTagBRRegex = new Regex(@"\{(br)\}", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        // Sueetie Modified - Adding Sueetieusername for match and profile link
        private static readonly Regex Phase3SpecialTagRegex = new Regex(@"\{(sueetieusername|username|pagename|loginlogout|namespace|namespacedropdown|incoming|outgoing)\}", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        //private static readonly Regex Phase3SpecialTagRegex = new Regex(@"\{(username|pagename|loginlogout|namespace|namespacedropdown|incoming|outgoing)\}", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        private static readonly Regex RecentChangesRegex = new Regex(@"\{recentchanges(\(\*\))?\}", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        private static readonly Regex ListRegex = new Regex(@"(?<=(\n|^))((\*|\#)+(\ )?.+?\n)+(\n|\z)", RegexOptions.Compiled | RegexOptions.Singleline); // Singleline to matche list elements on multiple lines
        private static readonly Regex TocRegex = new Regex(@"\{toc\}", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        private static readonly Regex TransclusionRegex = new Regex(@"\{T(\:|\|).+?\}", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        private static readonly Regex HRRegex = new Regex(@"(?<=(\n|^))(\ )*----(\ )*\n", RegexOptions.Compiled);
        private static readonly Regex SnippetRegex = new Regex(@"\{s\:(.+?)(\|.*?)*\}", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Singleline);
        private static readonly Regex SnippetParametersRegex = new Regex("\\?[a-zA-Z0-9_-]+\\?", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex ClassicSnippetVerifier = new Regex(@"\|\ *[\w\d]+\ *\=", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex TableRegex = new Regex(@"\{\|(\ [^\n]*)?\n.+?\|\}", RegexOptions.Compiled | RegexOptions.Singleline);
        private static readonly Regex IndentRegex = new Regex(@"(?<=(\n|^))\:+(\ )?.+?\n", RegexOptions.Compiled);
        private static readonly Regex EscRegex = new Regex(@"\<esc\>(.|\n|\r)*?\<\/esc\>", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Singleline);
        private static readonly Regex SignRegex = new Regex(@"§§\(.+?\)§§", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        // This regex is duplicated in Edit.aspx.cs
        private static readonly Regex FullCodeRegex = new Regex(@"@@.+?@@", RegexOptions.Compiled | RegexOptions.Singleline);
        private static readonly Regex JavascriptRegex = new Regex(@"\<script.*?\>.*?\<\/script\>", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);
        private static readonly Regex CommentRegex = new Regex(@"(?<!(\<script.*?\>[\s\n]*))\<\!\-\-.*?\-\-\>(?!([\s\n]*\<\/script\>))", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);
        private static readonly Regex HardLineBreak = new Regex(@"(\\|  )$", RegexOptions.Multiline | RegexOptions.CultureInvariant);

        /// <summary>
        /// The section editing button placeholder.
        /// </summary>
        private const string EditSectionPlaceHolder = "%%%%EditSectionPlaceHolder%%%%"; // This string is also used in History.aspx.cs
        private const string TocTitlePlaceHolder = "%%%%TocTitlePlaceHolder%%%%";
        private const string UpReplacement = "GetFile.aspx?File=";
        private const string ExtendedUpReplacement = "GetFile.aspx?$File=";
        private const string ExtendedUpReplacementForAttachment = "GetFile.aspx?$Page=@&File=";
        private const string SingleBrPlaceHolder = "%%%%SingleBrPlaceHolder%%%%";
        private const string SectionLinkTextPlaceHolder = "%%%%SectionLinkTextPlaceHolder%%%%";

        private IWikiPagesProvider _pages;

        public Formatter(IWikiPagesProvider pages)
        {
            _pages = pages ?? throw new ArgumentNullException(nameof(pages));
        }

        /// <summary>
        /// Gets the Regex that should be used to detect links based on <see cref="Settings.IgnoreSingleSquareBrackets"/> setting.
        /// </summary>
        private static Regex CurrentLinkRegex
        {
            get { return DoubleSquareLinkRegex; }
        }

        /// <summary>
        /// Detects the current namespace.
        /// </summary>
        /// <param name="currentPage">The current page, if any.</param>
        /// <returns>The current namespace (<c>null</c> for the root).</returns>
        private NamespaceInfo DetectNamespaceInfo(PageInfo currentPage)
        {
            if (currentPage == null)
            {
                throw new NotImplementedException();
            }
            else
            {
                return new NamespaceInfo(NameTools.GetNamespace(currentPage.FullName));
            }
        }

        /// <summary>
        /// Formats WikiMarkup, converting it into XHTML.
        /// </summary>
        /// <param name="raw">The raw WikiMarkup text.</param>
        /// <param name="forIndexing">A value indicating whether the formatting is being done for content indexing.</param>
        /// <param name="context">The formatting context.</param>
        /// <param name="current">The current Page (can be null).</param>
        /// <param name="linkedPages">The linked pages, both existent and inexistent.</param>
        /// <returns>The formatted text.</returns>
        public string Format(
            string raw,
            bool forIndexing,
            FormattingContext context,
            PageInfo current,
            out IList<string> linkedPages,
            out WikiName redirect)
        {
            redirect = null;

            linkedPages = new string[0];
            var tempLinkedPages = new List<string>(10);

            var sb = new StringBuilder(raw);
            Match match;
            string tmp, a, n, url, title, bigUrl;
            StringBuilder dummy; // Used for temporary string manipulation inside formatting cycles
            List<int> noWikiBegin = new List<int>(), noWikiEnd = new List<int>();
            string sbToString;

            sb.Replace("\r", "");
            var addedNewLineAtEnd = false;
            if (!sb.ToString().EndsWith("\n"))
            {
                sb.Append("\n"); // Very important to make Regular Expressions work!
                addedNewLineAtEnd = true;
            }

            sbToString = sb.ToString();

            // Remove all double- or single-LF in JavaScript tags
            var singleLine = Settings.ProcessSingleLineBreaks;
            match = JavascriptRegex.Match(sbToString);
            while (match.Success)
            {
                sb.Remove(match.Index, match.Length);
                if (singleLine) sb.Insert(match.Index, match.Value.Replace("\n", ""));
                else sb.Insert(match.Index, match.Value.Replace("\n\n", "\n"));
                sbToString = sb.ToString();
                match = JavascriptRegex.Match(sbToString, match.Index + 1);
            }

            // Remove empty NoWiki and NoBr tags
            sb.Replace("<nowiki></nowiki>", "");
            sb.Replace("<nobr></nobr>", "");
            sbToString = sb.ToString();

            ComputeNoWiki(sbToString, ref noWikiBegin, ref noWikiEnd);

            // Before Producing HTML
            match = FullCodeRegex.Match(sbToString);
            int end;
            while (match.Success)
            {
                if (!IsNoWikied(match.Index, noWikiBegin, noWikiEnd, out end))
                {
                    sb.Remove(match.Index, match.Length);
                    var content = match.Value.Substring(2, match.Length - 4);
                    dummy = new StringBuilder();
                    dummy.Append("<pre><nobr>");
                    // IE needs \r\n for line breaks
                    dummy.Append(EscapeWikiMarkup(content).Replace("\n", "\r\n"));
                    dummy.Append("</nobr></pre>");
                    sb.Insert(match.Index, dummy.ToString());
                    sbToString = sb.ToString();
                }
                ComputeNoWiki(sbToString, ref noWikiBegin, ref noWikiEnd);
                match = FullCodeRegex.Match(sbToString, end);
            }

            if (current != null)
            {
                // Check redirection
                match = RedirectionRegex.Match(sbToString);
                if (match.Success)
                {
                    if (!IsNoWikied(match.Index, noWikiBegin, noWikiEnd, out end))
                    {
                        sb.Remove(match.Index, match.Length);
                        var destination = match.Value.Trim().Substring(4).Trim();
                        while (destination.StartsWith("[") && destination.EndsWith("]"))
                        {
                            destination = destination.Substring(1, destination.Length - 2);
                        }

                        while (sb[match.Index] == '\n' && match.Index < sb.Length - 1)
                        {
                            sb.Remove(match.Index, 1);
                        }

                        if (!destination.StartsWith("++") && !destination.Contains(".") && current.FullName.Contains("."))
                        {
                            // Adjust namespace
                            destination = NameTools.GetFullName(NameTools.GetNamespace(current.FullName), destination);
                        }

                        destination = destination.Trim('+');

                        var destinationName = new WikiName(destination);

                        var dest = _pages.FindPage(destinationName.FullName);
                        if (dest != null)
                        {
                            Redirections.AddRedirection(current, dest);
                            redirect = destinationName;
                        }

                        sbToString = sb.ToString();
                    }

                    ComputeNoWiki(sbToString, ref noWikiBegin, ref noWikiEnd);
                }
            }

            // No more needed (Striked Regex modified)
            // Temporarily "escape" comments
            //sb.Replace("<!--", "($_^)");
            //sb.Replace("-->", "(^_$)");

            ComputeNoWiki(sbToString, ref noWikiBegin, ref noWikiEnd);

            // Before Producing HTML
            match = EscRegex.Match(sbToString);
            while (match.Success)
            {
                if (!IsNoWikied(match.Index, noWikiBegin, noWikiEnd, out end))
                {
                    sb.Remove(match.Index, match.Length);
                    sb.Insert(match.Index, match.Value.Substring(5, match.Length - 11).Replace("<", "&lt;").Replace(">", "&gt;"));
                    sbToString = sb.ToString();
                }
                ComputeNoWiki(sbToString, ref noWikiBegin, ref noWikiEnd);
                match = EscRegex.Match(sbToString, end);
            }

            // Snippets and tables processing was here

            match = IndentRegex.Match(sbToString);
            while (match.Success)
            {
                if (!IsNoWikied(match.Index, noWikiBegin, noWikiEnd, out end))
                {
                    sb.Remove(match.Index, match.Length);
                    sb.Insert(match.Index, BuildIndent(match.Value) + "\n");
                    sbToString = sb.ToString();
                }
                ComputeNoWiki(sbToString, ref noWikiBegin, ref noWikiEnd);
                match = IndentRegex.Match(sbToString, end);
            }

            // replace {base} special tag
            match = BaseRegex.Match(sbToString);
            while (match.Success)
            {
                if (!IsNoWikied(match.Index, noWikiBegin, noWikiEnd, out end))
                {
                    sb.Remove(match.Index, match.Length);
                    sb.Insert(match.Index, Settings.BaseUrl);
                    sbToString = sb.ToString();
                }

                ComputeNoWiki(sbToString, ref noWikiBegin, ref noWikiEnd);
                match = BaseRegex.Match(sbToString, end);
            }

            // replace {updir} special tag
            match = UploadRegex.Match(sbToString);
            while (match.Success)
            {
                if (!IsNoWikied(match.Index, noWikiBegin, noWikiEnd, out end))
                {
                    sb.Remove(match.Index, match.Length);
                    sb.Insert(match.Index, Settings.UploadUrl);
                    sbToString = sb.ToString();
                }

                ComputeNoWiki(sbToString, ref noWikiBegin, ref noWikiEnd);
                match = UploadRegex.Match(sbToString, end);
            }

            // Process extended UP before standard UP
            match = ExtendedUpRegex.Match(sbToString);
            while (match.Success)
            {
                if (!IsNoWikied(match.Index, noWikiBegin, noWikiEnd, out end))
                {
                    // Encode filename only if it's used inside a link,
                    // i.e. check if {UP} is used just after a '['
                    // This works because links are processed afterwards
                    var sbString = sbToString;
                    if (match.Index > 0 && (sbString[match.Index - 1] == '[' || sbString[match.Index - 1] == '^'))
                    {
                        EncodeFilename(sb, match.Index + match.Length);
                    }

                    sb.Remove(match.Index, match.Length);
                    var prov = match.Groups[1].Value.StartsWith(":") ? match.Value.Substring(4, match.Value.Length - 5) : match.Value.Substring(3, match.Value.Length - 4);
                    string page = null;
                    // prov - Full.Provider.Type.Name(PageName)
                    // (PageName) is optional, but it can contain brackets, for example (Page(WithBrackets))
                    if (prov.EndsWith(")") && prov.Contains("("))
                    {
                        page = prov.Substring(prov.IndexOf("(") + 1);
                        page = page.Substring(0, page.Length - 1);
                        page = UrlTools.UrlEncode(page);
                        prov = prov.Substring(0, prov.IndexOf("("));
                    }
                    if (page == null)
                    {
                        // Normal file
                        sb.Insert(match.Index, ExtendedUpReplacement.Replace("$", (prov != "") ? "Provider=" + prov + "&" : ""));
                    }
                    else
                    {
                        // Page attachment
                        sb.Insert(match.Index,
                            ExtendedUpReplacementForAttachment.Replace("$", (prov != "") ? "Provider=" + prov + "&" : "").Replace("@", page));
                    }
                    sbToString = sb.ToString();
                }
                ComputeNoWiki(sbToString, ref noWikiBegin, ref noWikiEnd);
                match = ExtendedUpRegex.Match(sbToString, end);
            }

            match = SpecialTagBRRegex.Match(sbToString); // solved by introducing a new regex call SpecialTagBR
            while (match.Success)
            {
                if (!IsNoWikied(match.Index, noWikiBegin, noWikiEnd, out end))
                {
                    sb.Remove(match.Index, match.Length);
                    if (!forIndexing)
                    {
                        switch (match.Value.Substring(1, match.Value.Length - 2).ToUpperInvariant())
                        {
                            case "BR":
                                sb.Insert(match.Index, "<br />");
                                break;
                        }
                    }
                    sbToString = sb.ToString();
                }
                ComputeNoWiki(sbToString, ref noWikiBegin, ref noWikiEnd);
                match = SpecialTagBRRegex.Match(sbToString, end);
            }

            // Hard line break
            // two spaces or backslash at the end of the line => <br />
            match = HardLineBreak.Match(sbToString);
            while (match.Success)
            {
                if (!IsNoWikied(match.Index, noWikiBegin, noWikiEnd, out end))
                {
                    sb.Remove(match.Index, match.Length);
                    sb.Insert(match.Index, "<br />");
                    sbToString = sb.ToString();
                }

                ComputeNoWiki(sbToString, ref noWikiBegin, ref noWikiEnd);
                match = HardLineBreak.Match(sbToString, end);
            }

            NamespaceInfo ns = DetectNamespaceInfo(current);
            match = SpecialTagRegex.Match(sbToString);
            while (match.Success)
            {
                if (!IsNoWikied(match.Index, noWikiBegin, noWikiEnd, out end))
                {
                    sb.Remove(match.Index, match.Length);
                    if (!forIndexing)
                    {
                        switch (match.Value.Substring(1, match.Value.Length - 2).ToUpperInvariant())
                        {

                            case "CLEAR":
                                sb.Insert(match.Index, @"<div style=""clear: both;""></div>");
                                break;
                            case "TOP":
                                sb.Insert(match.Index, @"<a href=""#PageTop"">" + Resources.Top + "</a>");
                                break;
                        }
                    }
                    sbToString = sb.ToString();
                }

                ComputeNoWiki(sbToString, ref noWikiBegin, ref noWikiEnd);
                match = SpecialTagRegex.Match(sbToString, end);
            }

            match = ListRegex.Match(sbToString);
            while (match.Success)
            {
                if (!IsNoWikied(match.Index, noWikiBegin, noWikiEnd, out end))
                {
                    sb.Remove(match.Index, match.Length);
                    var d = 0;
                    try
                    {
                        var lines = new List<string>(match.Value.Split('\n'));

                        // Inline multi-line list elements
                        var tempLines = new List<string>(lines);
                        for (var i = tempLines.Count - 1; i >= 1; i--)
                        {
                            // Skip first line
                            var trimmedLine = tempLines[i].Trim();
                            if (!trimmedLine.StartsWith("*") && !trimmedLine.StartsWith("#"))
                            {
                                //if(i != tempLines.Count - 1 && tempLines[i].Length > 0) {
                                if (!tempLines[i - 1].EndsWith("<br />") && !string.IsNullOrEmpty(tempLines[i - 1]) && !string.IsNullOrEmpty(trimmedLine))
                                {
                                    trimmedLine = "<br />" + trimmedLine;
                                    tempLines[i - 1] += trimmedLine;
                                }
                                //}
                                tempLines.RemoveAt(i);
                            }
                        }

                        lines = tempLines;

                        sb.Insert(match.Index, GenerateList(lines, 0, 0, ref d) + "\n");
                    }
                    catch
                    {
                        sb.Insert(match.Index,
                            @"<b style=""color: #FF0000;"">FORMATTER ERROR (Malformed List)</b><br />");
                    }

                    sbToString = sb.ToString();
                }
                ComputeNoWiki(sbToString, ref noWikiBegin, ref noWikiEnd);
                match = ListRegex.Match(sbToString, end);
            }

            match = HRRegex.Match(sbToString);
            while (match.Success)
            {
                if (!IsNoWikied(match.Index, noWikiBegin, noWikiEnd, out end))
                {
                    sb.Remove(match.Index, match.Length);
                    sb.Insert(match.Index, @"<h1 class=""separator""> </h1>" + "\n");
                    sbToString = sb.ToString();
                }
                ComputeNoWiki(sbToString, ref noWikiBegin, ref noWikiEnd);
                match = HRRegex.Match(sbToString, end);
            }

            // Replace \n with BR was here

            ComputeNoWiki(sbToString, ref noWikiBegin, ref noWikiEnd);

            var attachments = new List<string>();

            // Links and images
            match = CurrentLinkRegex.Match(sbToString);
            while (match.Success)
            {
                if (IsNoWikied(match.Index, noWikiBegin, noWikiEnd, out end))
                {
                    match = CurrentLinkRegex.Match(sbToString, end);
                    continue;
                }

                // [], [[]] and [[] can occur when empty links are processed
                if (match.Value.Equals("[]") || match.Value.Equals("[[]]") || match.Value.Equals("[[]"))
                {
                    sb.Remove(match.Index, match.Length);
                    sbToString = sb.ToString();
                    match = CurrentLinkRegex.Match(sbToString, end);
                    continue; // Prevents formatting emtpy links
                }

                sb.Remove(match.Index, match.Length);

                var done = false;
                if (match.Value.StartsWith("[["))
                {
                    tmp = match.Value.Substring(2, match.Length - 4).Trim();
                }
                else
                {
                    tmp = match.Value.Substring(1, match.Length - 2).Trim();
                }

                a = "";
                n = "";
                if (tmp.IndexOf("|") != -1)
                {
                    // There are some fields
                    var fields = tmp.Split('|');
                    if (fields.Length == 2)
                    {
                        // Link with title
                        a = fields[0];
                        n = fields[1];
                    }
                    else
                    {
                        done = true;
                        var img = new StringBuilder();
                        // Image
                        if (fields[0].ToLowerInvariant().Equals("imageleft") || fields[0].ToLowerInvariant().Equals("imageright") || fields[0].ToLowerInvariant().Equals("imageauto"))
                        {
                            var c = "";
                            switch (fields[0].ToLowerInvariant())
                            {
                                case "imageleft":
                                    c = "imageleft";
                                    break;
                                case "imageright":
                                    c = "imageright";
                                    break;
                                case "imageauto":
                                    c = "imageauto";
                                    break;
                                default:
                                    c = "image";
                                    break;
                            }
                            title = fields[1];
                            url = fields[2];
                            if (fields.Length == 4) bigUrl = fields[3];
                            else bigUrl = "";
                            url = EscapeUrl(url);
                            // bigUrl = EscapeUrl(bigUrl); The url is already escaped by BuildUrl
                            if (c.Equals("imageauto"))
                            {
                                img.Append(@"<table class=""imageauto"" cellpadding=""0"" cellspacing=""0""><tr><td>");
                            }
                            else
                            {
                                img.Append(@"<div class=""");
                                img.Append(c);
                                img.Append(@""">");
                            }
                            if (bigUrl.Length > 0)
                            {
                                dummy = new StringBuilder(200);
                                dummy.Append(@"<img class=""image"" src=""");
                                dummy.Append(url);
                                dummy.Append(@""" alt=""");
                                if (title.Length > 0) dummy.Append(StripWikiMarkup(StripHtml(title.TrimStart('#'))));
                                else dummy.Append(Resources.Image);
                                dummy.Append(@""" />");
                                img.Append(BuildLink(bigUrl, dummy.ToString(), true, title, forIndexing, false, context, null, tempLinkedPages));
                            }
                            else
                            {
                                img.Append(@"<img class=""image"" src=""");
                                img.Append(url);
                                img.Append(@""" alt=""");
                                if (title.Length > 0) img.Append(StripWikiMarkup(StripHtml(title.TrimStart('#'))));
                                else img.Append(Resources.Image);
                                img.Append(@""" />");
                            }
                            if (title.Length > 0 && !title.StartsWith("#"))
                            {
                                img.Append(@"<p class=""imagedescription"">");
                                img.Append(title);
                                img.Append("</p>");
                            }
                            if (c.Equals("imageauto"))
                            {
                                img.Append("</td></tr></table>");
                            }
                            else
                            {
                                img.Append("</div>");
                            }
                            sb.Insert(match.Index, img);
                        }
                        else if (fields[0].ToLowerInvariant().Equals("image"))
                        {
                            title = fields[1];
                            url = fields[2];
                            if (fields.Length == 4) bigUrl = fields[3];
                            else bigUrl = "";
                            url = EscapeUrl(url);
                            // bigUrl = EscapeUrl(bigUrl); The url is already escaped by BuildUrl
                            if (bigUrl.Length > 0)
                            {
                                dummy = new StringBuilder();
                                dummy.Append(@"<img src=""");
                                dummy.Append(url);
                                dummy.Append(@""" alt=""");
                                if (title.Length > 0) dummy.Append(StripWikiMarkup(StripHtml(title.TrimStart('#'))));
                                else dummy.Append(Resources.Image);
                                dummy.Append(@""" />");
                                img.Append(BuildLink(bigUrl, dummy.ToString(), true, title, forIndexing, false, context, null, tempLinkedPages));
                            }
                            else
                            {
                                img.Append(@"<img src=""");
                                img.Append(url);
                                img.Append(@""" alt=""");
                                if (title.Length > 0) img.Append(StripWikiMarkup(StripHtml(title.TrimStart('#'))));
                                else img.Append(Resources.Image);
                                img.Append(@""" />");
                            }
                            sb.Insert(match.Index, img.ToString());
                        }
                        else
                        {
                            sb.Insert(match.Index, @"<b style=""color: #FF0000;"">FORMATTER ERROR (Malformed Image Tag)</b>");

                            done = true;
                        }
                    }
                }
                else if (tmp.ToLowerInvariant().StartsWith("attachment:"))
                {
                    // This is an attachment
                    done = true;
                    var f = tmp.Substring("attachment:".Length);
                    if (f.StartsWith("{up}")) f = f.Substring(4);
                    if (f.ToLowerInvariant().StartsWith(UpReplacement.ToLowerInvariant())) f = f.Substring(UpReplacement.Length);
                    attachments.Add(UrlTools.UrlDecode(f));
                    // Remove all trailing \n, so that attachments have no effect on the output in any case
                    while (sb[match.Index] == '\n' && match.Index < sb.Length - 1)
                    {
                        sb.Remove(match.Index, 1);
                    }
                }
                else
                {
                    a = tmp;
                    n = "";
                }
                if (!done)
                {
                    sb.Insert(match.Index, BuildLink(a, n, false, "", forIndexing, false, context, current, tempLinkedPages));
                }

                sbToString = sb.ToString();
                ComputeNoWiki(sbToString, ref noWikiBegin, ref noWikiEnd);
                match = LinkRegex.Match(sbToString, end);
            }

            match = BoldItalicRegex.Match(sbToString);
            while (match.Success)
            {
                if (!IsNoWikied(match.Index, noWikiBegin, noWikiEnd, out end))
                {
                    sb.Remove(match.Index, match.Length);
                    dummy = new StringBuilder("<b><i>");
                    dummy.Append(match.Value.Substring(5, match.Value.Length - 10));
                    dummy.Append("</i></b>");
                    sb.Insert(match.Index, dummy.ToString());
                    sbToString = sb.ToString();
                }
                ComputeNoWiki(sbToString, ref noWikiBegin, ref noWikiEnd);
                match = BoldItalicRegex.Match(sbToString, end);
            }

            match = BoldRegex.Match(sbToString);
            while (match.Success)
            {
                if (!IsNoWikied(match.Index, noWikiBegin, noWikiEnd, out end))
                {
                    sb.Remove(match.Index, match.Length);
                    dummy = new StringBuilder("<b>");
                    dummy.Append(match.Value.Substring(3, match.Value.Length - 6));
                    dummy.Append("</b>");
                    sb.Insert(match.Index, dummy.ToString());
                    sbToString = sb.ToString();
                }
                ComputeNoWiki(sbToString, ref noWikiBegin, ref noWikiEnd);
                match = BoldRegex.Match(sbToString, end);
            }

            match = ItalicRegex.Match(sbToString);
            while (match.Success)
            {
                if (!IsNoWikied(match.Index, noWikiBegin, noWikiEnd, out end))
                {
                    sb.Remove(match.Index, match.Length);
                    dummy = new StringBuilder("<i>");
                    dummy.Append(match.Value.Substring(2, match.Value.Length - 4));
                    dummy.Append("</i>");
                    sb.Insert(match.Index, dummy.ToString());
                    sbToString = sb.ToString();
                }
                ComputeNoWiki(sbToString, ref noWikiBegin, ref noWikiEnd);
                match = ItalicRegex.Match(sbToString, end);
            }

            match = UnderlinedRegex.Match(sbToString);
            while (match.Success)
            {
                if (!IsNoWikied(match.Index, noWikiBegin, noWikiEnd, out end))
                {
                    sb.Remove(match.Index, match.Length);
                    dummy = new StringBuilder("<u>");
                    dummy.Append(match.Value.Substring(2, match.Value.Length - 4));
                    dummy.Append("</u>");
                    sb.Insert(match.Index, dummy.ToString());
                    sbToString = sb.ToString();
                }
                ComputeNoWiki(sbToString, ref noWikiBegin, ref noWikiEnd);
                match = UnderlinedRegex.Match(sbToString, end);
            }

            match = ApexRegex.Match(sbToString);
            while (match.Success)
            {
                if (!IsNoWikied(match.Index, noWikiBegin, noWikiBegin, out end))
                {
                    sb.Remove(match.Index, match.Length);
                    dummy = new StringBuilder("<sup>");
                    dummy.Append(match.Value.Substring(11, match.Value.Length - 23));
                    dummy.Append("</sup>");
                    sb.Insert(match.Index, dummy.ToString());
                    sbToString = sb.ToString();
                }
                ComputeNoWiki(sbToString, ref noWikiBegin, ref noWikiEnd);
                match = ApexRegex.Match(sbToString, end);
            }

            match = SubscribeRegex.Match(sbToString);
            while (match.Success)
            {
                if (!IsNoWikied(match.Index, noWikiBegin, noWikiBegin, out end))
                {
                    sb.Remove(match.Index, match.Length);
                    dummy = new StringBuilder("<sub>");
                    dummy.Append(match.Value.Substring(11, match.Value.Length - 23));
                    dummy.Append("</sub>");
                    sb.Insert(match.Index, dummy.ToString());
                    sbToString = sb.ToString();
                }
                ComputeNoWiki(sbToString, ref noWikiBegin, ref noWikiEnd);
                match = SubscribeRegex.Match(sbToString, end);
            }

            match = StrikedRegex.Match(sbToString);
            while (match.Success)
            {
                if (!IsNoWikied(match.Index, noWikiBegin, noWikiEnd, out end))
                {
                    sb.Remove(match.Index, match.Length);
                    dummy = new StringBuilder("<strike>");
                    dummy.Append(match.Value.Substring(2, match.Value.Length - 4));
                    dummy.Append("</strike>");
                    sb.Insert(match.Index, dummy.ToString());
                    sbToString = sb.ToString();
                }
                ComputeNoWiki(sbToString, ref noWikiBegin, ref noWikiEnd);
                match = StrikedRegex.Match(sbToString, end);
            }

            match = PreRegex.Match(sbToString);
            while (match.Success)
            {
                if (!IsNoWikied(match.Index, noWikiBegin, noWikiEnd, out end))
                {
                    sb.Remove(match.Index, match.Length);
                    dummy = new StringBuilder("<pre>");
                    // IE needs \r\n for line breaks
                    dummy.Append(match.Value.Substring(4, match.Value.Length - 8).Replace("\n", "\r\n"));
                    dummy.Append("</pre>");
                    sb.Insert(match.Index, dummy.ToString());
                    sbToString = sb.ToString();
                }
                ComputeNoWiki(sbToString, ref noWikiBegin, ref noWikiEnd);
                match = PreRegex.Match(sbToString, end);
            }

            match = CodeRegex.Match(sbToString);
            while (match.Success)
            {
                if (!IsNoWikied(match.Index, noWikiBegin, noWikiEnd, out end))
                {
                    sb.Remove(match.Index, match.Length);
                    dummy = new StringBuilder("<code>");
                    dummy.Append(match.Value.Substring(2, match.Value.Length - 4));
                    dummy.Append("</code>");
                    sb.Insert(match.Index, dummy.ToString());
                    sbToString = sb.ToString();
                }
                ComputeNoWiki(sbToString, ref noWikiBegin, ref noWikiEnd);
                match = CodeRegex.Match(sbToString, end);
            }

            string h;

            // Hx: detection pass (used for the TOC generation and section editing)
            List<HPosition> hPos = DetectHeaders(sbToString);

            // Hx: formatting pass

            var count = 0;

            match = H4Regex.Match(sbToString);
            while (match.Success)
            {
                if (!IsNoWikied(match.Index, noWikiBegin, noWikiEnd, out end))
                {
                    sb.Remove(match.Index, match.Length);
                    h = match.Value.Substring(5, match.Value.Length - 10 - (match.Value.EndsWith("\n") ? 1 : 0));
                    dummy = new StringBuilder(200);
                    dummy.Append(@"<h4 class=""separator"">");
                    dummy.Append(h);
                    if (!forIndexing)
                    {
                        var id = BuildHAnchor(h, count.ToString());
                        BuildHeaderAnchor(dummy, id);
                    }
                    dummy.Append("</h4>");
                    sb.Insert(match.Index, dummy.ToString());
                    sbToString = sb.ToString();
                    count++;
                }
                ComputeNoWiki(sbToString, ref noWikiBegin, ref noWikiEnd);
                match = H4Regex.Match(sbToString, end);
            }

            match = H3Regex.Match(sbToString);
            while (match.Success)
            {
                if (!IsNoWikied(match.Index, noWikiBegin, noWikiEnd, out end))
                {
                    sb.Remove(match.Index, match.Length);
                    h = match.Value.Substring(4, match.Value.Length - 8 - (match.Value.EndsWith("\n") ? 1 : 0));
                    dummy = new StringBuilder(200);
                    if (current != null && !forIndexing) dummy.Append(BuildEditSectionLink(count, current.FullName));
                    dummy.Append(@"<h3 class=""separator"">");
                    dummy.Append(h);
                    if (!forIndexing)
                    {
                        var id = BuildHAnchor(h, count.ToString());
                        BuildHeaderAnchor(dummy, id);
                    }
                    dummy.Append("</h3>");
                    sb.Insert(match.Index, dummy.ToString());
                    sbToString = sb.ToString();
                    count++;
                }
                ComputeNoWiki(sbToString, ref noWikiBegin, ref noWikiEnd);
                match = H3Regex.Match(sbToString, end);
            }

            match = H2Regex.Match(sbToString);
            while (match.Success)
            {
                if (!IsNoWikied(match.Index, noWikiBegin, noWikiEnd, out end))
                {
                    sb.Remove(match.Index, match.Length);
                    h = match.Value.Substring(3, match.Value.Length - 6 - (match.Value.EndsWith("\n") ? 1 : 0));
                    dummy = new StringBuilder(200);
                    if (current != null && !forIndexing) dummy.Append(BuildEditSectionLink(count, current.FullName));
                    dummy.Append(@"<h2 class=""separator"">");
                    dummy.Append(h);
                    if (!forIndexing)
                    {
                        var id = BuildHAnchor(h, count.ToString());
                        BuildHeaderAnchor(dummy, id);
                    }
                    dummy.Append("</h2>");
                    sb.Insert(match.Index, dummy.ToString());
                    sbToString = sb.ToString();
                    count++;
                }
                ComputeNoWiki(sbToString, ref noWikiBegin, ref noWikiEnd);
                match = H2Regex.Match(sbToString, end);
            }

            match = H1Regex.Match(sbToString);
            while (match.Success)
            {
                if (!IsNoWikied(match.Index, noWikiBegin, noWikiEnd, out end))
                {
                    sb.Remove(match.Index, match.Length);
                    h = match.Value.Substring(2, match.Value.Length - 4 - (match.Value.EndsWith("\n") ? 1 : 0));
                    dummy = new StringBuilder(200);
                    if (current != null && !forIndexing) dummy.Append(BuildEditSectionLink(count, current.FullName));
                    dummy.Append(@"<h1 class=""separator"">");
                    dummy.Append(h);
                    if (!forIndexing)
                    {
                        var id = BuildHAnchor(h, count.ToString());
                        BuildHeaderAnchor(dummy, id);
                    }
                    dummy.Append("</h1>");
                    sb.Insert(match.Index, dummy.ToString());
                    sbToString = sb.ToString();
                    count++;
                }
                ComputeNoWiki(sbToString, ref noWikiBegin, ref noWikiEnd);
                match = H1Regex.Match(sbToString, end);
            }

            match = BoxRegex.Match(sbToString);
            while (match.Success)
            {
                if (!IsNoWikied(match.Index, noWikiBegin, noWikiEnd, out end))
                {
                    sb.Remove(match.Index, match.Length);
                    dummy = new StringBuilder(@"<div class=""box"">");
                    dummy.Append(match.Value.Substring(3, match.Value.Length - 6));
                    dummy.Append("</div>");
                    sb.Insert(match.Index, dummy.ToString());
                    sbToString = sb.ToString();
                }
                ComputeNoWiki(sbToString, ref noWikiBegin, ref noWikiEnd);
                match = BoxRegex.Match(sbToString, end);
            }

            var tocString = BuildToc(hPos);

            if (current != null)
            {
                match = TocRegex.Match(sbToString);
                while (match.Success)
                {
                    if (!IsNoWikied(match.Index, noWikiBegin, noWikiEnd, out end))
                    {
                        sb.Remove(match.Index, match.Length);
                        if (!forIndexing) sb.Insert(match.Index, tocString);
                    }
                    sbToString = sb.ToString();
                    ComputeNoWiki(sbToString, ref noWikiBegin, ref noWikiEnd);
                    match = TocRegex.Match(sbToString, end);
                }
            }

            match = SnippetRegex.Match(sbToString);
            while (match.Success)
            {
                if (!IsNoWikied(match.Index, noWikiBegin, noWikiEnd, out end))
                {
                    string? balanced = null;
                    try
                    {
                        // If the snippet is malformed this can explode
                        balanced = ExpandToBalanceBrackets(sbToString, match.Index, match.Value);
                    }
                    catch { }

                    if (balanced == null)
                    {
                        // Replace brackets with escaped values so that the snippets regex does not trigger anymore
                        sb.Replace("{", "&#123;", match.Index, match.Length);
                        sb.Replace("}", "&#125;", match.Index, match.Length);
                        sbToString = sb.ToString();
                        break; // Give up
                    }
                    else
                    {
                        sb.Remove(match.Index, balanced.Length);
                    }

                    if (balanced.IndexOf("}") == balanced.Length - 1)
                    {
                        // Single-level snippet
                        var snippet = FormatSnippet(balanced, tocString);
                        snippet = Format(snippet, forIndexing, context, current, out var innerLinkedPages, out var _);
                        snippet = snippet.Trim('\n');
                        sb.Insert(match.Index, snippet);
                        if (innerLinkedPages != null) tempLinkedPages.AddRange(innerLinkedPages);
                    }
                    else
                    {
                        // Nested snippet
                        int lastOpen;
                        do
                        {
                            lastOpen = balanced.LastIndexOf("{");
                            var firstClosedAfterLastOpen = balanced.IndexOf("}", lastOpen + 1);

                            if (lastOpen < 0 || firstClosedAfterLastOpen <= lastOpen) break; // Give up

                            var internalSnippet = balanced.Substring(lastOpen, firstClosedAfterLastOpen - lastOpen + 1);
                            balanced = balanced.Remove(lastOpen, firstClosedAfterLastOpen - lastOpen + 1);

                            // This check allows to ignore special tags (especially Phase3)
                            if (!internalSnippet.ToLowerInvariant().StartsWith("{s:"))
                            {
                                internalSnippet = internalSnippet.Replace("{", "$$$$$$$$OPEN$$$$$$$$").Replace("}", "$$$$$$$$CLOSE$$$$$$$$");
                                balanced = balanced.Insert(lastOpen, internalSnippet);
                                sbToString = sb.ToString();
                                continue;
                            }

                            var formattedInternalSnippet = FormatSnippet(internalSnippet, tocString);
                            formattedInternalSnippet = Format(formattedInternalSnippet, forIndexing, context, current, out var innerLinkedPages, out _).Trim('\n');
                            if (innerLinkedPages != null) tempLinkedPages.AddRange(innerLinkedPages);

                            balanced = balanced.Insert(lastOpen, formattedInternalSnippet);
                        } while (lastOpen != -1);

                        sb.Insert(match.Index, balanced.Replace("$$$$$$$$OPEN$$$$$$$$", "{").Replace("$$$$$$$$CLOSE$$$$$$$$", "}"));
                    }

                    sbToString = sb.ToString();
                }

                ComputeNoWiki(sbToString, ref noWikiBegin, ref noWikiEnd);
                match = SnippetRegex.Match(sbToString, end);
            }

            match = TableRegex.Match(sbToString);
            while (match.Success)
            {
                if (!IsNoWikied(match.Index, noWikiBegin, noWikiEnd, out end))
                {
                    sb.Remove(match.Index, match.Length);
                    sb.Insert(match.Index, BuildTable(match.Value));
                    sbToString = sb.ToString();
                }
                ComputeNoWiki(sbToString, ref noWikiBegin, ref noWikiEnd);
                match = TableRegex.Match(sbToString, end);
            }

            // Strip out all comments
            match = CommentRegex.Match(sbToString);
            while (match.Success)
            {
                sb.Remove(match.Index, match.Length);
                sb.Insert(match.Index, " "); // This prevents the creation of additional blank lines
                match = CommentRegex.Match(sbToString, match.Index + 1);
            }

            // Remove <nowiki> tags
            sb.Replace("<nowiki>", "");
            sb.Replace("</nowiki>", "");

            ProcessLineBreaks(sb, false);

            if (addedNewLineAtEnd)
            {
                if (sb.ToString().EndsWith("<br />"))
                {
                    sb.Remove(sb.Length - 6, 6);
                }
            }

            // Append Attachments
            if (attachments.Count > 0)
            {
                sb.Append(@"<div id=""AttachmentsDiv"">");
                for (var i = 0; i < attachments.Count; i++)
                {
                    sb.Append(@"<a href=""");
                    sb.Append(UpReplacement);
                    sb.Append(UrlTools.UrlEncode(attachments[i]));
                    sb.Append(@""" class=""attachment"">");
                    sb.Append(attachments[i]);
                    sb.Append("</a>");
                    if (i != attachments.Count - 1) sb.Append(" - ");
                }
                sb.Append("</div>");
            }

            linkedPages = tempLinkedPages.ToArray();

            return sb.ToString();
        }

        /// <summary>
        /// Encodes a filename used in combination with {UP} tags.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="startIndex">The index where to start working.</param>
        private static void EncodeFilename(StringBuilder buffer, int startIndex)
        {
            // 1. Find end of the filename (first pipe or closed square bracket)
            // 2. Decode the string, so that it does not break if it was already encoded
            // 3. Encode the string

            var allData = buffer.ToString();

            var endIndex = allData.IndexOfAny(new[] { '|', ']' }, startIndex);
            if (endIndex > startIndex)
            {
                var len = endIndex - startIndex;
                // {, : and } are used in snippets which are useful in links
                var input = UrlTools.UrlDecode(allData.Substring(startIndex, len));
                var value = UrlTools.UrlEncode(input).Replace("%7b", "{").Replace("%7B", "{").Replace("%7d", "}").Replace("%7D", "}").Replace("%3a", ":").Replace("%3A", ":");
                buffer.Remove(startIndex, len);
                buffer.Insert(startIndex, value);
            }
        }

        /// <summary>
        /// Builds the anchor markup for a header.
        /// </summary>
        /// <param name="buffer">The string builder.</param>
        /// <param name="id">The anchor ID.</param>
        private static void BuildHeaderAnchor(StringBuilder buffer, string id)
        {
            buffer.Append(@"<a class=""headeranchor"" id=""");
            buffer.Append(id);
            buffer.Append(@""" href=""#");
            buffer.Append(id);
            buffer.Append(@""" title=""");
            buffer.Append(SectionLinkTextPlaceHolder);
            if (Settings.EnableSectionAnchors) buffer.Append(@""">&#0182;</a>");
            else buffer.Append(@""" style=""visibility: hidden;"">&nbsp;</a>");
        }

        /// <summary>
        /// Builds the link to a namespace.
        /// </summary>
        /// <param name="nspace">The namespace (<c>null</c> for the root).</param>
        /// <returns>The link.</returns>
        private static string BuildNamespaceLink(string nspace)
        {
            return "<a href=\"" + (string.IsNullOrEmpty(nspace) ? "" : UrlTools.UrlEncode(nspace) + ".") +
                "Default.aspx\" class=\"pagelink\" title=\"" + (string.IsNullOrEmpty(nspace) ? "" : UrlTools.UrlEncode(nspace)) + "\">" +
                (string.IsNullOrEmpty(nspace) ? "&lt;root&gt;" : nspace) + "</a>";
        }

        /// <summary>
        /// Processes line breaks.
        /// </summary>
        /// <param name="sb">The <see cref="T:StringBuilder" /> containing the text to process.</param>
        /// <param name="bareBones">A value indicating whether the formatting is being done in bare-bones mode.</param>
        private static void ProcessLineBreaks(StringBuilder sb, bool bareBones)
        {
            if (AreSingleLineBreaksToBeProcessed())
            {
                // Replace new-lines only when not enclosed in <nobr> tags
                Match match = NoSingleBr.Match(sb.ToString());
                while (match.Success)
                {
                    sb.Remove(match.Index, match.Length);
                    sb.Insert(match.Index, match.Value.Replace("\n", SingleBrPlaceHolder));
                    //sb.Insert(match.Index, match.Value.Replace("\n", "<br />"));

                    match = NoSingleBr.Match(sb.ToString(), match.Index + 1);
                }

                sb.Replace("\n", "<br />");

                sb.Replace(SingleBrPlaceHolder, "\n");
                //sb.Replace(SingleBrPlaceHolder, "<br />");
            }
            else
            {
                // Replace new-lines only when not enclosed in <nobr> tags
                Match match = NoSingleBr.Match(sb.ToString());
                while (match.Success)
                {
                    sb.Remove(match.Index, match.Length);
                    sb.Insert(match.Index, match.Value.Replace("\n", SingleBrPlaceHolder));
                    //sb.Insert(match.Index, match.Value.Replace("\n", "<br />"));
                    match = NoSingleBr.Match(sb.ToString(), match.Index + 1);
                }

                sb.Replace("\n\n", "<br /><br />");

                sb.Replace(SingleBrPlaceHolder, "\n");//Replace <br /><br /> with <br />

            }

            sb.Replace("<br>", "<br />");

            // BR Hacks
            sb.Replace("</ul><br /><br />", "</ul><br />");
            sb.Replace("</ol><br /><br />", "</ol><br />");
            sb.Replace("</table><br /><br />", "</table><br />");
            sb.Replace("</pre><br /><br />", "</pre><br />");
            if (AreSingleLineBreaksToBeProcessed())
            {
                sb.Replace("</h1><br />", "</h1>");
                sb.Replace("</h2><br />", "</h2>");
                sb.Replace("</h3><br />", "</h3>");
                sb.Replace("</h4><br />", "</h4>");
                sb.Replace("</h5><br />", "</h5>");
                sb.Replace("</h6><br />", "</h6>");
                sb.Replace("</div><br />", "</div>");
            }
            else
            {
                sb.Replace("</div><br /><br />", "</div><br />");
            }

            sb.Replace("<nobr>", "");
            sb.Replace("</nobr>", "");
        }

        /// <summary>
        /// Gets a value indicating whether or not to process single line breaks.
        /// </summary>
        /// <returns><c>true</c> if SLB are to be processed, <c>false</c> otherwise.</returns>
        private static bool AreSingleLineBreaksToBeProcessed()
        {
            return Settings.ProcessSingleLineBreaks;
        }

        /// <summary>
        /// Replaces the {TOC} markup in a string.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <param name="cachedToc">The TOC replacement.</param>
        /// <returns>The final string.</returns>
        private static string ReplaceToc(string input, string cachedToc)
        {
            // HACK: this method is used to trick the formatter so that it works when a {TOC} tag is placed in a trigger
            // Basically, the Snippet content is formatted in the same context as the main content, but without
            // The headers list available, thus the need for this special treatment

            Match match = TocRegex.Match(input);
            while (match.Success)
            {
                input = input.Remove(match.Index, match.Length);
                input = input.Insert(match.Index, cachedToc);
                match = TocRegex.Match(input, match.Index + 1);
            }

            return input;
        }

        /// <summary>
        /// Expands a regex selection to match the number of open curly brackets.
        /// </summary>
        /// <param name="sb">The buffer.</param>
        /// <param name="index">The match start index.</param>
        /// <param name="value">The match value.</param>
        /// <returns>The balanced string, or <c>null</c> if the brackets could not be balanced.</returns>
        private static string? ExpandToBalanceBrackets(string bigString, int index, string value)
        {
            var tempIndex = -1;

            var openCount = 0;
            do
            {
                tempIndex = value.IndexOf("{", tempIndex + 1);
                if (tempIndex >= 0) openCount++;
            } while (tempIndex != -1 && tempIndex < value.Length - 1);

            var closeCount = 0;
            tempIndex = -1;
            do
            {
                tempIndex = value.IndexOf("}", tempIndex + 1);
                if (tempIndex >= 0) closeCount++;
            } while (tempIndex != -1 && tempIndex < value.Length - 1);

            // Already balanced
            if (openCount == closeCount) return value;

            tempIndex = index + value.Length - 1;

            do
            {
                var dummy = bigString.IndexOf("{", tempIndex + 1);
                if (dummy != -1) openCount++;
                tempIndex = bigString.IndexOf("}", tempIndex + 1);
                if (tempIndex != -1) closeCount++;

                tempIndex = Math.Max(dummy, tempIndex);

                if (closeCount == openCount)
                {
                    // Balanced
                    return bigString.Substring(index, tempIndex - index + 1);
                }
            } while (tempIndex != -1 && tempIndex < bigString.Length - 1);

            return null;
        }

        /// <summary>
        /// Formats a snippet.
        /// </summary>
        /// <param name="capturedMarkup">The captured markup.</param>
        /// <param name="cachedToc">The TOC content (trick to allow {TOC} to be inserted in snippets).</param>
        /// <returns>The formatted result.</returns>
        private string FormatSnippet(string capturedMarkup, string cachedToc)
        {
            // If the markup does not contain equal signs, process it using the classic method, assuming there are only positional parameters
            //if(capturedMarkup.IndexOf("=") == -1) {
            if (!ClassicSnippetVerifier.IsMatch(capturedMarkup))
            {
                var tempRes = FormatClassicSnippet(capturedMarkup);
                return ReplaceToc(tempRes, cachedToc);
            }
            // If the markup contains "=" but not new lines, simulate the required structure as shown below
            if (capturedMarkup.IndexOf("\n") == -1)
            {
                capturedMarkup = capturedMarkup.Replace("|", "\n|");
            }

            // The format is:
            // {s:Name | param = value			-- OR
            // | param = value					-- OR
            // | param = value
            //
            // which continues on next line, preceded by a blank
            // }

            // End bracket can be on a line on its own or on the last content line

            // 0. Find snippet object
            var snippetName = capturedMarkup.Substring(3, capturedMarkup.IndexOf("\n") - 3).Trim();
            var snippet = _pages.FindSnippet(snippetName);
            if (snippet == null) return @"<b style=""color: #FF0000;"">FORMATTER ERROR (Snippet Not Found)</b>";

            // 1. Strip all useless data at the beginning and end of the text ({s| and }, plus all whitespaces)

            // Strip opening and closing tags
            var sb = new StringBuilder(capturedMarkup);
            sb.Remove(0, 3);
            sb.Remove(sb.Length - 1, 1);

            // Strip all whitespaces at the end
            while (char.IsWhiteSpace(sb[sb.Length - 1])) sb.Remove(sb.Length - 1, 1);

            // 2. Split into lines, preserving empty lines
            var lines = sb.ToString().Split('\n');

            // 3. Find all lines starting with a pipe and containing an equal sign -> those are the ones that define params values
            var parametersLines = new List<int>(lines.Length);
            for (var i = 0; i < lines.Length; i++)
            {
                if (lines[i].Trim().StartsWith("|") && lines[i].Contains("="))
                {
                    parametersLines.Add(i);
                }
            }

            // 4. For each parameter line, extract the parameter value, spanning through all subsequent non-parameter lines
            //    Build a name->value dictionary for parameters
            var values = new Dictionary<string, string>(parametersLines.Count);
            for (var i = 0; i < parametersLines.Count; i++)
            {
                // Extract parameter name
                var equalSignIndex = lines[parametersLines[i]].IndexOf("=");
                var parameterName = lines[parametersLines[i]].Substring(0, equalSignIndex);
                parameterName = parameterName.Trim(' ', '|').ToLowerInvariant();

                var currentValue = new StringBuilder(100);
                currentValue.Append(lines[parametersLines[i]].Substring(equalSignIndex + 1).TrimStart(' '));

                // Span all subsequent lines
                if (i < parametersLines.Count - 1)
                {
                    for (var span = parametersLines[i] + 1; span < parametersLines[i + 1]; span++)
                    {
                        currentValue.Append("\n");
                        currentValue.Append(lines[span]);
                    }
                }
                else if (parametersLines[i] < lines.Length - 1)
                {
                    // All remaining lines belong to the last parameter
                    for (var span = parametersLines[i] + 1; span < lines.Length; span++)
                    {
                        currentValue.Append("\n");
                        currentValue.Append(lines[span]);
                    }
                }

                if (!values.ContainsKey(parameterName)) values.Add(parameterName, currentValue.ToString());
                else values[parameterName] = currentValue.ToString();
            }

            // 5. Prepare the snippet output, replacing parameters placeholders with their values
            //    Use a lowercase version of the snippet to identify parameters locations

            var lowercaseSnippetContent = snippet.Content.ToLowerInvariant();
            var output = new StringBuilder(snippet.Content);

            foreach (KeyValuePair<string, string> pair in values)
            {
                var index = lowercaseSnippetContent.IndexOf("?" + pair.Key + "?");
                while (index >= 0)
                {
                    output.Remove(index, pair.Key.Length + 2);
                    output.Insert(index, pair.Value);

                    // Need to update lowercase representation because the parameters values alter the length
                    lowercaseSnippetContent = output.ToString().ToLowerInvariant();
                    index = lowercaseSnippetContent.IndexOf("?" + pair.Key + "?");
                }
            }

            // Remove all remaining parameters and return
            var tempResult = SnippetParametersRegex.Replace(output.ToString(), "");
            return ReplaceToc(tempResult, cachedToc);
        }

        /// <summary>
        /// Format classic number-parameterized snippets.
        /// </summary>
        /// <param name="capturedMarkup">The captured markup to process.</param>
        /// <returns>The formatted result.</returns>
        private string FormatClassicSnippet(string capturedMarkup)
        {
            var secondPipe = capturedMarkup.Substring(3).IndexOf("|");
            var name = "";
            if (secondPipe == -1) name = capturedMarkup.Substring(3, capturedMarkup.Length - 4); // No parameters
            else name = capturedMarkup.Substring(3, secondPipe);
            var snippet = _pages.FindSnippet(name);
            if (snippet != null)
            {
                var parameters = CustomSplit(capturedMarkup.Substring(3 + secondPipe + 1, capturedMarkup.Length - secondPipe - 5));
                var fs = PrepareSnippet(parameters, snippet.Content);
                return fs.Trim('\n');
            }
            else
            {
                return @"<b style=""color: #FF0000;"">FORMATTER ERROR (Snippet Not Found)</b>";
            }
        }

        /// <summary>
        /// Splits a string at pipe characters, taking into account square brackets for links and images.
        /// </summary>
        /// <param name="data">The input data.</param>
        /// <returns>The resuling splitted strings.</returns>
        private static string[] CustomSplit(string data)
        {
            // 1. Find all pipes that are not enclosed in square brackets
            var indices = new List<int>(10);
            var index = 0;
            var openBrackets = 0; // Account for links with two brackets, e.g. [[link]]
            while (index < data.Length)
            {
                if (data[index] == '|')
                {
                    if (openBrackets == 0) indices.Add(index);
                }
                else if (data[index] == '[') openBrackets++;
                else if (data[index] == ']') openBrackets--;
                if (openBrackets < 0) openBrackets = 0;
                index++;
            }

            // 2. Split string at reported indices
            indices.Insert(0, -1);
            indices.Add(data.Length);

            var result = new List<string>(indices.Count);
            for (var i = 0; i < indices.Count - 1; i++)
            {
                result.Add(data.Substring(indices[i] + 1, indices[i + 1] - indices[i] - 1));
            }

            return result.ToArray();
        }

        /// <summary>
        /// Prepares the content of a snippet, properly managing parameters.
        /// </summary>
        /// <param name="parameters">The snippet parameters.</param>
        /// <param name="snippet">The snippet original text.</param>
        /// <returns>The prepared snippet text.</returns>
        private static string PrepareSnippet(string[] parameters, string snippet)
        {
            var sb = new StringBuilder(snippet);

            for (var i = 0; i < parameters.Length; i++)
            {
                sb.Replace(string.Format("?{0}?", i + 1), parameters[i]);
            }

            // Remove all remaining parameters that have no value
            return SnippetParametersRegex.Replace(sb.ToString(), "");
        }

        /// <summary>
        /// Escapes all the characters used by the WikiMarkup.
        /// </summary>
        /// <param name="content">The Content.</param>
        /// <returns>The escaped Content.</returns>
        private static string EscapeWikiMarkup(string content)
        {
            var sb = new StringBuilder(content);
            sb.Replace("&", "&amp;"); // Before all other escapes!
            sb.Replace("#", "&#35;");
            sb.Replace("*", "&#42;");
            sb.Replace("<", "&lt;");
            sb.Replace(">", "&gt;");
            sb.Replace("[", "&#91;");
            sb.Replace("]", "&#93;");
            sb.Replace("{", "&#123;");
            sb.Replace("}", "&#125;");
            sb.Replace("'''", "&#39;&#39;&#39;");
            sb.Replace("''", "&#39;&#39;");
            sb.Replace("=====", "&#61;&#61;&#61;&#61;&#61;");
            sb.Replace("====", "&#61;&#61;&#61;&#61;");
            sb.Replace("===", "&#61;&#61;&#61;");
            sb.Replace("==", "&#61;&#61;");
            sb.Replace("§§", "&#167;&#167;");
            sb.Replace("__", "&#95;&#95;");
            sb.Replace("--", "&#45;&#45;");
            sb.Replace("@@", "&#64;&#64;");
            sb.Replace(":", "&#58;");
            return sb.ToString();
        }

        /// <summary>
        /// Removes all the characters used by the WikiMarkup.
        /// </summary>
        /// <param name="content">The Content.</param>
        /// <returns>The stripped Content.</returns>
        private static string StripWikiMarkup(string content)
        {
            if (string.IsNullOrEmpty(content)) return "";

            var sb = new StringBuilder(content);
            sb.Replace("*", "");
            sb.Replace("<", "");
            sb.Replace(">", "");
            sb.Replace("[", "");
            sb.Replace("]", "");
            sb.Replace("{", "");
            sb.Replace("}", "");
            sb.Replace("'''", "");
            sb.Replace("''", "");
            sb.Replace("=====", "");
            sb.Replace("====", "");
            sb.Replace("===", "");
            sb.Replace("==", "");
            sb.Replace("§§", "");
            sb.Replace("__", "");
            sb.Replace("--", "");
            sb.Replace("@@", "");
            return sb.ToString();
        }

        /// <summary>
        /// Removes all HTML markup from a string.
        /// </summary>
        /// <param name="content">The string.</param>
        /// <returns>The result.</returns>
        public static string StripHtml(string content)
        {
            if (string.IsNullOrEmpty(content)) return "";

            var sb = new StringBuilder(Regex.Replace(content, "<[^>]*>", " "));
            sb.Replace("&nbsp;", "");
            sb.Replace("  ", " ");
            return sb.ToString();
        }

        /// <summary>
        /// Prepares the title of an item for safe display.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <returns>The sanitized title.</returns>
        private static string PrepareItemTitle(string title)
        {
            return StripHtml(title)
                .Replace("\"", "&quot;")
                .Replace("'", "&#39;")
                .Replace("<", "&lt;").Replace(">", "&gt;")
                .Replace("[", "&#91;").Replace("]", "&#93;"); // This avoid endless loops in Formatter
        }

        /// <summary>
        /// Builds a Link.
        /// </summary>
        /// <param name="targetUrl">The (raw) HREF.</param>
        /// <param name="title">The name/title.</param>
        /// <param name="isImage">True if the link contains an Image as "visible content".</param>
        /// <param name="imageTitle">The title of the image.</param>
        /// <param name="forIndexing">A value indicating whether the formatting is being done for content indexing.</param>
        /// <param name="bareBones">A value indicating whether the formatting is being done in bare-bones mode.</param>
        /// <param name="context">The formatting context.</param>
        /// <param name="currentPage">The current page, or <c>null</c>.</param>
        /// <param name="linkedPages">The linked pages list (both existent and inexistent).</param>
        /// <returns>The formatted Link.</returns>
        private string BuildLink(string targetUrl, string title, bool isImage, string imageTitle,
            bool forIndexing, bool bareBones, FormattingContext context, PageInfo currentPage, List<string> linkedPages)
        {

            if (targetUrl == null) targetUrl = "";
            if (title == null) title = "";
            if (imageTitle == null) imageTitle = "";

            var blank = false;
            if (targetUrl.StartsWith("^"))
            {
                blank = true;
                targetUrl = targetUrl.Substring(1);
            }
            targetUrl = EscapeUrl(targetUrl);
            var nstripped = StripWikiMarkup(StripHtml(title));
            var imageTitleStripped = StripWikiMarkup(StripHtml(imageTitle));

            var sb = new StringBuilder(150);

            if (targetUrl.ToLowerInvariant().Equals("anchor") && title.StartsWith("#"))
            {
                sb.Append(@"<a id=""");
                sb.Append(title.Substring(1));
                sb.Append(@"""> </a>");
            }
            else if (targetUrl.StartsWith("#"))
            {
                sb.Append(@"<a");
                if (!isImage) sb.Append(@" class=""internallink""");
                if (blank) sb.Append(@" target=""_blank""");
                sb.Append(@" href=""");
                //sb.Append(a);
                UrlTools.BuildUrl(sb, targetUrl);
                sb.Append(@""" title=""");
                if (!isImage && title.Length > 0) sb.Append(nstripped);
                else if (isImage && imageTitle.Length > 0) sb.Append(imageTitleStripped);
                else sb.Append(targetUrl.Substring(1));
                sb.Append(@""">");
                if (title.Length > 0) sb.Append(title);
                else sb.Append(targetUrl.Substring(1));
                sb.Append("</a>");
            }
            else if (targetUrl.StartsWith("http://") || targetUrl.StartsWith("https://") || targetUrl.StartsWith("ftp://") || targetUrl.StartsWith("file://"))
            {
                // The link is complete
                sb.Append(@"<a");
                if (!isImage) sb.Append(@" class=""externallink""");
                sb.Append(@" href=""");
                sb.Append(targetUrl);
                sb.Append(@""" title=""");
                if (!isImage && title.Length > 0) sb.Append(nstripped);
                else if (isImage && imageTitle.Length > 0) sb.Append(imageTitleStripped);
                else sb.Append(targetUrl);
                sb.Append(@""" target=""_blank"">");
                if (title.Length > 0) sb.Append(title);
                else sb.Append(targetUrl);
                sb.Append("</a>");
            }
            else if (targetUrl.StartsWith(@"\\") || targetUrl.StartsWith("//"))
            {
                // The link is a UNC path
                sb.Append(@"<a");
                if (!isImage) sb.Append(@" class=""externallink""");
                sb.Append(@" href=""file://///");
                sb.Append(targetUrl.Substring(2));
                sb.Append(@""" title=""");
                if (!isImage && title.Length > 0) sb.Append(nstripped);
                else if (isImage && imageTitle.Length > 0) sb.Append(imageTitleStripped);
                else sb.Append(targetUrl);
                sb.Append(@""" target=""_blank"">");
                if (title.Length > 0) sb.Append(title);
                else sb.Append(targetUrl);
                sb.Append("</a>");
            }
            else if (targetUrl.IndexOf("@") != -1 && targetUrl.IndexOf(".") != -1)
            {
                // Email
                sb.Append(@"<a");
                if (!isImage) sb.Append(@" class=""emaillink""");
                if (blank) sb.Append(@" target=""_blank""");
                if (targetUrl.StartsWith(@"mailto:"))
                {
                    sb.Append(@" href=""");
                }
                else
                {
                    sb.Append(@" href=""mailto:");
                }
                sb.Append(UrlTools.ObfuscateText(targetUrl.Replace("&amp;", "%26"))); // Trick to let ampersands work in email addresses
                sb.Append(@""" title=""");
                if (!isImage && title.Length > 0) sb.Append(nstripped);
                else if (isImage && imageTitle.Length > 0) sb.Append(imageTitleStripped);
                else sb.Append(UrlTools.ObfuscateText(targetUrl));
                sb.Append(@""">");
                if (title.Length > 0) sb.Append(title);
                else sb.Append(UrlTools.ObfuscateText(targetUrl));
                sb.Append("</a>");
            }
            else if (((targetUrl.IndexOf(".") != -1 && !targetUrl.ToLowerInvariant().EndsWith(".aspx")) || targetUrl.EndsWith("/")) &&
                !targetUrl.StartsWith("++") && _pages.FindPage(targetUrl) == null &&
                !targetUrl.StartsWith("c:") && !targetUrl.StartsWith("C:"))
            {
                // Link to an internal file or subdirectory, or link to GetFile.aspx
                sb.Append(@"<a");
                if (!isImage) sb.Append(@" class=""internallink""");
                if (blank) sb.Append(@" target=""_blank""");
                sb.Append(@" href=""");
                //sb.Append(a);
                if (targetUrl.ToLowerInvariant().StartsWith("getfile.aspx"))
                {
                    sb.Append(targetUrl);
                }
                else
                {
                    UrlTools.BuildUrl(sb, targetUrl);
                }

                sb.Append(@""" title=""");
                if (!isImage && title.Length > 0) sb.Append(nstripped);
                else if (isImage && imageTitle.Length > 0) sb.Append(imageTitleStripped);
                else sb.Append(targetUrl);
                sb.Append(@""">");
                if (title.Length > 0) sb.Append(title);
                else sb.Append(targetUrl);
                sb.Append("</a>");
            }
            else if (targetUrl.IndexOf(".aspx") != -1)
            {
                // The link points to a "system" page
                sb.Append(@"<a");
                if (!isImage) sb.Append(@" class=""systemlink""");
                if (blank) sb.Append(@" target=""_blank""");
                sb.Append(@" href=""");
                //sb.Append(a);
                UrlTools.BuildUrl(sb, targetUrl);
                sb.Append(@""" title=""");
                if (!isImage && title.Length > 0) sb.Append(nstripped);
                else if (isImage && imageTitle.Length > 0) sb.Append(imageTitleStripped);
                else sb.Append(targetUrl);
                sb.Append(@""">");
                if (title.Length > 0) sb.Append(title);
                else sb.Append(targetUrl);
                sb.Append("</a>");
            }
            else if (targetUrl.StartsWith("c:") || targetUrl.StartsWith("C:"))
            {
                // Category link
                //sb.Append(@"<a href=""AllPages.aspx?Cat=");
                //sb.Append(UrlTools.UrlEncode(a.Substring(2)));
                sb.Append(@"<a href=""");
                UrlTools.BuildUrl(sb, "category.html?name=", UrlTools.UrlEncode(targetUrl.Substring(2)));

                sb.Append(@""" class=""systemlink"" title=""");
                if (!isImage && title.Length > 0) sb.Append(nstripped);
                else if (isImage && imageTitle.Length > 0) sb.Append(imageTitleStripped);
                else sb.Append(targetUrl.Substring(2));
                sb.Append(@""">");
                if (title.Length > 0) sb.Append(title);
                else sb.Append(targetUrl.Substring(2));
                sb.Append("</a>");
            }
            else if (targetUrl.Contains(":") || targetUrl.ToLowerInvariant().Contains("%3a") || targetUrl.Contains("&") || targetUrl.Contains("%26"))
            {
                sb.Append(@"<b style=""color: #FF0000;"">FORMATTER ERROR ("":"" and ""&"" not supported in Page Names)</b>");
            }
            else
            {
                // The link points to a wiki page
                var explicitNamespace = false;
                var tempLink = targetUrl;
                if (tempLink.StartsWith("++"))
                {
                    tempLink = tempLink.Substring(2);
                    targetUrl = targetUrl.Substring(2);
                    explicitNamespace = true;
                }

                if (targetUrl.IndexOf("#") != -1)
                {
                    tempLink = targetUrl.Substring(0, targetUrl.IndexOf("#"));
                    targetUrl = UrlTools.UrlEncode(targetUrl.Substring(0, targetUrl.IndexOf("#"))) + Settings.PageExtension + targetUrl.Substring(targetUrl.IndexOf("#"));
                }
                else
                {
                    targetUrl += Settings.PageExtension;
                    // #468: Preserve ++ for ReverseFormatter
                    targetUrl = (bareBones && explicitNamespace ? "++" : "") + UrlTools.UrlEncode(targetUrl);
                }

                var fullName = "";
                if (!explicitNamespace)
                {
                    fullName = currentPage != null ?
                         NameTools.GetFullName(NameTools.GetNamespace(currentPage.FullName), NameTools.GetLocalName(tempLink)) :
                         tempLink;
                }
                else
                {
                    fullName = tempLink;
                }

                // Add linked page without repetitions
                //linkedPages.Add(fullName);
                var info = _pages.FindPage(fullName);
                if (info != null)
                {
                    if (!linkedPages.Contains(info.Name.FullName))
                    {
                        linkedPages.Add(info.Name.FullName);
                    }
                }
                else
                {
                    var lowercaseFullName = fullName.ToLowerInvariant();
                    if (linkedPages.Find(p => { return p.ToLowerInvariant() == lowercaseFullName; }) == null)
                    {
                        linkedPages.Add(fullName);
                    }
                }

                if (info == null)
                {
                    sb.Append(@"<a");
                    if (!isImage) sb.Append(@" class=""unknownlink""");
                    if (blank) sb.Append(@" target=""_blank""");
                    sb.Append(@" href=""");
                    //sb.Append(a);
                    UrlTools.BuildUrl(sb, explicitNamespace ? "++" : "", targetUrl);
                    sb.Append(@""" title=""");
                    /*if(!isImage && title.Length > 0) sb.Append(nstripped);
                    else if(isImage && imageTitle.Length > 0) sb.Append(imageTitleStripped);
                    else sb.Append(tempLink);*/
                    sb.Append(fullName);
                    sb.Append(@""">");
                    if (title.Length > 0) sb.Append(title);
                    else sb.Append(tempLink);
                    sb.Append("</a>");
                }
                else
                {
                    sb.Append(@"<a");
                    if (!isImage) sb.Append(@" class=""pagelink""");
                    if (blank) sb.Append(@" target=""_blank""");
                    sb.Append(@" href=""");
                    //sb.Append(a);
                    if (explicitNamespace)
                    {
                        UrlTools.BuildUrl(sb, "++", targetUrl);
                    }
                    else if (!string.IsNullOrEmpty(info.Name.Namespace))
                    {
                        UrlTools.BuildUrl(sb, info.Name.Namespace, ".", targetUrl);
                    }
                    else
                    {
                        UrlTools.BuildUrl(sb, targetUrl);
                    }

                    sb.Append(@""" title=""");
                    /*if(!isImage && title.Length > 0) sb.Append(nstripped);
                    else if(isImage && imageTitle.Length > 0) sb.Append(imageTitleStripped);
                    else sb.Append(FormattingPipeline.PrepareTitle(Content.GetPageContent(info, false).Title, context, currentPage));*/

                    if (forIndexing)
                    {
                        // When saving a page, the SQL Server provider calls IHost.PrepareContentForIndexing
                        // If the content contains a reference to the page being saved, the formatter will call GetPageContent on SQL Server,
                        // resulting in a transaction deadlock (the save transaction waits for the read transaction, while the latter
                        // waits the the locks on the PageContent table being released)
                        // See also Content.GetPageContent

                        if (currentPage != null && currentPage.FullName == info.Name.FullName)
                        {
                            // Do not format title
                            sb.Append(info.Name.FullName);
                        }
                        else
                        {
                            // Try to format title
                            var titleText = _pages.GetPageTitle(info, false);
                            if (!string.IsNullOrEmpty(titleText))
                            {
                                sb.Append(PrepareItemTitle(titleText));
                            }
                            else sb.Append(info.Name.FullName);
                        }
                    }
                    else
                    {
                        var titleText = _pages.GetPageTitle(info, false);
                        titleText = PrepareItemTitle(titleText);
                        sb.Append(titleText);
                    }

                    sb.Append(@""">");
                    if (title.Length > 0) sb.Append(title);
                    else sb.Append(tempLink);
                    sb.Append("</a>");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Detects all the Headers in a block of text (H1, H2, H3, H4).
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The List of Header objects, in the same order as they are in the text.</returns>
        public static List<HPosition> DetectHeaders(string text)
        {
            Match match;
            string h;
            var end = 0;
            List<int> noWikiBegin = new List<int>(), noWikiEnd = new List<int>();
            var hPos = new List<HPosition>();
            var sb = new StringBuilder(text);

            ComputeNoWiki(sb.ToString(), ref noWikiBegin, ref noWikiEnd);

            var count = 0;

            match = H4Regex.Match(sb.ToString());
            while (match.Success)
            {
                if (!IsNoWikied(match.Index, noWikiBegin, noWikiEnd, out end))
                {
                    h = match.Value.Substring(5, match.Value.Length - 10 - (match.Value.EndsWith("\n") ? 1 : 0));
                    hPos.Add(new HPosition(match.Index, h, 4, count));
                    end = match.Index + match.Length;
                    count++;
                }
                ComputeNoWiki(sb.ToString(), ref noWikiBegin, ref noWikiEnd);
                match = H4Regex.Match(sb.ToString(), end);
            }

            match = H3Regex.Match(sb.ToString());
            while (match.Success)
            {
                if (!IsNoWikied(match.Index, noWikiBegin, noWikiEnd, out end))
                {
                    h = match.Value.Substring(4, match.Value.Length - 8 - (match.Value.EndsWith("\n") ? 1 : 0));
                    var found = false;
                    for (var i = 0; i < hPos.Count; i++)
                    {
                        if (match.Index == hPos[i].Index)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        hPos.Add(new HPosition(match.Index, h, 3, count));
                        count++;
                    }
                    end = match.Index + match.Length;
                }
                ComputeNoWiki(sb.ToString(), ref noWikiBegin, ref noWikiEnd);
                match = H3Regex.Match(sb.ToString(), end);
            }

            match = H2Regex.Match(sb.ToString());
            while (match.Success)
            {
                if (!IsNoWikied(match.Index, noWikiBegin, noWikiEnd, out end))
                {
                    h = match.Value.Substring(3, match.Value.Length - 6 - (match.Value.EndsWith("\n") ? 1 : 0));
                    var found = false;
                    for (var i = 0; i < hPos.Count; i++)
                    {
                        if (match.Index == hPos[i].Index)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        hPos.Add(new HPosition(match.Index, h, 2, count));
                        count++;
                    }
                    end = match.Index + match.Length;
                }
                ComputeNoWiki(sb.ToString(), ref noWikiBegin, ref noWikiEnd);
                match = H2Regex.Match(sb.ToString(), end);
            }

            match = H1Regex.Match(sb.ToString());
            while (match.Success)
            {
                if (!IsNoWikied(match.Index, noWikiBegin, noWikiEnd, out end))
                {
                    h = match.Value.Substring(2, match.Value.Length - 4 - (match.Value.EndsWith("\n") ? 1 : 0));
                    var found = false;
                    for (var i = 0; i < hPos.Count; i++)
                    {
                        // A special treatment is needed in this case
                        // because =====xxx===== matches also 2 H1 headers (=='='==)
                        if (match.Index >= hPos[i].Index && match.Index <= hPos[i].Index + hPos[i].Text.Length + 5)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        hPos.Add(new HPosition(match.Index, h, 1, count));
                        count++;
                    }
                    end = match.Index + match.Length;
                }
                ComputeNoWiki(sb.ToString(), ref noWikiBegin, ref noWikiEnd);
                match = H1Regex.Match(sb.ToString(), end);
            }

            return hPos;
        }

        /// <summary>
        /// Builds the "Edit" links for page sections.
        /// </summary>
        /// <param name="id">The section ID.</param>
        /// <param name="page">The page name.</param>
        /// <returns>The link.</returns>
        private static string BuildEditSectionLink(int id, string page)
        {
            if (!Settings.EnableSectionEditing) return "";

            var sb = new StringBuilder(100);
            sb.Append(@"<a href=""");
            UrlTools.BuildUrl(sb, "Edit.aspx?Page=", UrlTools.UrlEncode(page), "&amp;Section=", id.ToString());
            sb.Append(@""" class=""editsectionlink"">");
            sb.Append(EditSectionPlaceHolder);
            sb.Append("</a>");
            return sb.ToString();
        }

        /// <summary>
        /// Generates list HTML markup.
        /// </summary>
        /// <param name="lines">The lines in the list WikiMarkup.</param>
        /// <param name="line">The current line.</param>
        /// <param name="level">The current level.</param>
        /// <param name="currLine">The current line.</param>
        /// <returns>The correct HTML markup.</returns>
        private static string GenerateList(List<string> lines, int line, int level, ref int currLine)
        {
            var sb = new StringBuilder(200);
            if (lines[currLine][level] == '*') sb.Append("<ul>");
            else if (lines[currLine][level] == '#') sb.Append("<ol>");
            while (currLine <= lines.Count - 1 && CountBullets(lines[currLine]) >= level + 1)
            {
                if (CountBullets(lines[currLine]) == level + 1)
                {
                    sb.Append("<li>");
                    sb.Append(lines[currLine].Substring(CountBullets(lines[currLine])).Trim());
                    sb.Append("</li>");
                    currLine++;
                }
                else
                {
                    sb.Remove(sb.Length - 5, 5);
                    sb.Append(GenerateList(lines, currLine, level + 1, ref currLine));
                    sb.Append("</li>");
                }
            }
            if (lines[line][level] == '*') sb.Append("</ul>");
            else if (lines[line][level] == '#') sb.Append("</ol>");
            return sb.ToString();
        }

        /// <summary>
        /// Counts the bullets in a list line.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <returns>The number of bullets.</returns>
        private static int CountBullets(string line)
        {
            int res = 0, count = 0;
            while (line[count] == '*' || line[count] == '#')
            {
                res++;
                count++;
            }
            return res;
        }

        /// <summary>
        /// Extracts the bullets from a list line.
        /// </summary>
        /// <param name="value">The line.</param>
        /// <returns>The bullets.</returns>
        private static string ExtractBullets(string value)
        {
            var res = "";
            for (var i = 0; i < value.Length; i++)
            {
                if (value[i] == '*' || value[i] == '#') res += value[i];
                else break;
            }
            return res;
        }

        /// <summary>
        /// Builds the TOC of a document.
        /// </summary>
        /// <param name="hPos">The positions of headers.</param>
        /// <returns>The TOC HTML markup.</returns>
        private static string BuildToc(List<HPosition> hPos)
        {
            var sb = new StringBuilder();

            hPos.Sort(new HPositionComparer());

            // Table only used to workaround IE idiosyncrasies - use TocCointainer for styling
            sb.Append(@"<table id=""TocContainerTable""><tr><td>");
            sb.Append(@"<div id=""TocContainer"">");
            sb.Append(@"<p class=""small"">");
            sb.Append(TocTitlePlaceHolder);
            sb.Append("</p>");

            sb.Append(@"<div id=""Toc"">");
            sb.Append("<p><br />");
            for (var i = 0; i < hPos.Count; i++)
            {
                var tocEntry = hPos[i];

                // Indent
                for (var j = 1; j < tocEntry.Level; j++)
                {
                    sb.Append("&nbsp;&nbsp;&nbsp;");
                }

                // Formatting
                switch (tocEntry.Level)
                {
                    case 1: sb.Append("<strong>"); break;
                    case 4: sb.Append("<small>"); break;
                }

                sb.Append(@"<a href=""#")
                .Append(BuildHAnchor(tocEntry.Text, tocEntry.ID.ToString()))
                .Append(@""">")
                .Append(StripWikiMarkup(StripHtml(tocEntry.Text)).Trim())
                .Append("</a>");

                switch (tocEntry.Level)
                {
                    case 1: sb.Append("</strong>"); break;
                    case 4: sb.Append("</small>"); break;
                }

                sb.Append("<br />");
            }
            sb.Append("</p>");
            sb.Append("</div>");

            sb.Append("</div>");
            sb.Append("</td></tr></table>");

            return sb.ToString();
        }

        /// <summary>
        /// Builds a valid anchor name from a string.
        /// </summary>
        /// <param name="h">The string, usually a header (Hx).</param>
        /// <returns>The anchor ID.</returns>
        public static string BuildHAnchor(string h)
        {
            // Remove any extra spaces around the heading title:
            // '=== Title ===' results in '<a id="Title">' instead of '<a id="_Title_">'
            h = h.Trim();

            var sb = new StringBuilder(StripWikiMarkup(StripHtml(h)));
            sb.Replace(" ", "_");
            sb.Replace(".", "");
            sb.Replace(",", "");
            sb.Replace(";", "");
            sb.Replace("\"", "");
            sb.Replace("/", "");
            sb.Replace("\\", "");
            sb.Replace("'", "");
            sb.Replace("(", "");
            sb.Replace(")", "");
            sb.Replace("[", "");
            sb.Replace("]", "");
            sb.Replace("{", "");
            sb.Replace("}", "");
            sb.Replace("<", "");
            sb.Replace(">", "");
            sb.Replace("#", "");
            sb.Replace("\n", "");
            sb.Replace("?", "");
            sb.Replace("&", "");
            sb.Replace("0", "A");
            sb.Replace("1", "B");
            sb.Replace("2", "C");
            sb.Replace("3", "D");
            sb.Replace("4", "E");
            sb.Replace("5", "F");
            sb.Replace("6", "G");
            sb.Replace("7", "H");
            sb.Replace("8", "I");
            sb.Replace("9", "J");
            return sb.ToString();
        }

        /// <summary>
        /// Builds a valid and unique anchor name from a string.
        /// </summary>
        /// <param name="h">The string, usually a header (Hx).</param>
        /// <param name="uid">The unique ID.</param>
        /// <returns>The anchor ID.</returns>
        public static string BuildHAnchor(string h, string uid)
        {
            return BuildHAnchor(h) + "_" + uid;
        }

        /// <summary>
        /// Escapes ampersands in a URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>The escaped URL.</returns>
        private static string EscapeUrl(string url)
        {
            return url.Replace("&", "&amp;");
        }

        /// <summary>
        /// Builds and return a HTML table from WikiMarkup.
        /// </summary>
        /// <param name="table">The WikiMarkup.</param>
        /// <returns>The HTML.</returns>
        private static string BuildTable(string table)
        {
            // Proceed line-by-line, ignoring the first and last one
            var lines = table.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length < 3)
            {
                return "<b>FORMATTER ERROR (Malformed Table)</b>";
            }
            var sb = new StringBuilder();
            sb.Append("<table");
            if (lines[0].Length > 2)
            {
                sb.Append(" ");
                sb.Append(lines[0].Substring(3));
            }
            sb.Append(">");
            var count = 1;
            if (lines[1].Length >= 3 && lines[1].Trim().StartsWith("|+"))
            {
                // Table caption
                sb.Append("<caption>");
                sb.Append(lines[1].Substring(3));
                sb.Append("</caption>");
                count++;
            }

            if (!lines[count].StartsWith("|-")) sb.Append("<tr>");

            var thAdded = false;

            string item;
            for (var i = count; i < lines.Length - 1; i++)
            {
                var line = lines[i].Trim();


                // Nouvelle ligne (<tr>)
                var isNewLine = line.StartsWith("|-");

                // Nouvelle cellule (<td>)
                var isCellLine = !isNewLine && line.StartsWith("|");

                // Nouvelle ligne + nouvelle cellule (<tr> + <td>)
                var isNewCellLine = line.StartsWith("|/");

                // Nouvelle cellule d'entête (<th>)
                var isHeadCell = line.StartsWith("!");

                if (isNewLine || isNewCellLine)
                {
                    // New line
                    if (i != count)
                        sb.Append("</tr>");

                    sb.Append("<tr");

                    if (isNewLine && line.Length > 2)
                    {
                        // |-style
                        var style = line.Substring(3).Trim();
                        if (style.All(c => c == '-'))
                        {
                            // On ignore les lignes qui sont uniquement composées de "|--------------------", utilisé pour faire des tableaux plus jolis
                            // Exemple : 
                            // {|
                            // | Cell 1 || Cell 2
                            // |------------------
                            // | Cell 3 || Cell 4
                            // |}
                        }
                        else if (style.Length > 0)
                        {
                            // append HTML TR tag attributes
                            // |- class="test" cellpadding="0"
                            sb.Append(" ").Append(style);
                        }
                    }

                    sb.Append(">");
                }

                if (isCellLine || isNewCellLine)
                {
                    // Cell
                    if (line.Length < 3)
                        continue;

                    item = line.Substring(isCellLine ? 2 : 3);

                    if (item.IndexOf(" || ") != -1)
                    {
                        sb.Append("<td>");
                        sb.Append(item.Replace(" || ", "</td><td>"));
                        sb.Append("</td>");
                    }
                    else if (item.IndexOf(" | ") != -1)
                    {
                        sb.Append("<td ");
                        sb.Append(item.Substring(0, item.IndexOf(" | ")));
                        sb.Append(">");
                        sb.Append(item.Substring(item.IndexOf(" | ") + 3));
                        sb.Append("</td>");
                    }
                    else
                    {
                        sb.Append("<td>");
                        sb.Append(item);
                        sb.Append("</td>");
                    }
                }

                if (line.StartsWith("!"))
                {
                    // Header
                    if (line.Length < 3)
                        continue;

                    // only if ! is found in the first row of the table, it is an header
                    if (lines[i + 1] == "|-")
                        thAdded = true;

                    item = lines[i].Substring(2);
                    if (item.IndexOf(" !! ") != -1)
                    {
                        sb.Append("<th>");
                        sb.Append(item.Replace(" !! ", "</th><th>"));
                        sb.Append("</th>");
                    }
                    else if (item.IndexOf(" ! ") != -1)
                    {
                        sb.Append("<th ");
                        sb.Append(item.Substring(0, item.IndexOf(" ! ")));
                        sb.Append(">");
                        sb.Append(item.Substring(item.IndexOf(" ! ") + 3));
                        sb.Append("</th>");
                    }
                    else
                    {
                        sb.Append("<th>");
                        sb.Append(item);
                        sb.Append("</th>");
                    }
                }
            }

            if (sb.ToString().EndsWith("<tr>"))
            {
                sb.Remove(sb.Length - 4 - 1, 4);
                sb.Append("</table>");
            }
            else
            {
                sb.Append("</tr></table>");
            }
            sb.Replace("<tr></tr>", "");

            // Add <thead>, <tbody> tags, if table contains header
            if (thAdded)
            {
                var thIndex = sb.ToString().IndexOf("<th");
                //if(thIndex >= 4) sb.Insert(thIndex - 4, "<thead>");
                sb.Insert(thIndex - 4, "<thead>");

                // search for the last </th> tag in the first row of the table
                var thCloseIndex = -1;
                var thCloseIndex_temp = -1;
                do
                {
                    thCloseIndex = thCloseIndex_temp;
                    thCloseIndex_temp = sb.ToString().IndexOf("</th>", thCloseIndex + 1);
                }
                while (thCloseIndex_temp != -1/* && thCloseIndex_temp < sb.ToString().IndexOf("</tr>") #443, but disables row-header support */);

                sb.Insert(thCloseIndex + 10, "</thead><tbody>");
                sb.Insert(sb.Length - 8, "</tbody>");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Builds an indented text block.
        /// </summary>
        /// <param name="indent">The input text.</param>
        /// <returns>The result.</returns>
        private static string BuildIndent(string indent)
        {
            var colons = 0;
            indent = indent.Trim();
            while (colons < indent.Length && indent[colons] == ':') colons++;
            indent = indent.Substring(colons).Trim();
            return @"<div class=""indent"" style=""margin: 0px; padding: 0px; padding-left: " + (colons * 15).ToString() + @"px"">" + indent + "</div>";
        }

        /// <summary>
        /// Computes the positions of all NOWIKI tags.
        /// </summary>
        /// <param name="text">The input text.</param>
        /// <param name="noWikiBegin">The output list of begin indexes of NOWIKI tags.</param>
        /// <param name="noWikiEnd">The output list of end indexes of NOWIKI tags.</param>
        private static void ComputeNoWiki(string text, ref List<int> noWikiBegin, ref List<int> noWikiEnd)
        {
            Match match;
            noWikiBegin.Clear();
            noWikiEnd.Clear();

            match = NoWikiRegex.Match(text);
            while (match.Success)
            {
                noWikiBegin.Add(match.Index);
                noWikiEnd.Add(match.Index + match.Length);
                match = NoWikiRegex.Match(text, match.Index + match.Length);
            }
        }

        /// <summary>
        /// Determines whether a character is enclosed in a NOWIKI tag.
        /// </summary>
        /// <param name="index">The index of the character.</param>
        /// <param name="noWikiBegin">The list of begin indexes of NOWIKI tags.</param>
        /// <param name="noWikiEnd">The list of end indexes of NOWIKI tags.</param>
        /// <param name="end">The end index of the NOWIKI tag that encloses the specified character, or zero.</param>
        /// <returns><c>true</c> if the specified character is enclosed in a NOWIKI tag, <c>false</c> otherwise.</returns>
        private static bool IsNoWikied(int index, List<int> noWikiBegin, List<int> noWikiEnd, out int end)
        {
            for (var i = 0; i < noWikiBegin.Count; i++)
            {
                if (index > noWikiBegin[i] && index < noWikiEnd[i])
                {
                    end = noWikiEnd[i];
                    return true;
                }
            }
            end = 0;
            return false;
        }

        /// <summary>
        /// Performs the internal Phase 3 of the Formatting pipeline.
        /// </summary>
        /// <param name="raw">The raw data.</param>
        /// <param name="context">The formatting context.</param>
        /// <param name="current">The current PageInfo, if any.</param>
        /// <returns>The formatted content.</returns>
        public string FormatPhase3(string raw, FormattingContext context, PageInfo current)
        {
            var sb = new StringBuilder(raw.Length);
            StringBuilder dummy;
            sb.Append(raw);

            Match match;

            // Format other Phase3 special tags
            match = Phase3SpecialTagRegex.Match(sb.ToString());
            while (match.Success)
            {
                sb.Remove(match.Index, match.Length);
                match = Phase3SpecialTagRegex.Match(sb.ToString());
            }

            sb.Replace(SectionLinkTextPlaceHolder, Resources.LinkToThisSection);

            dummy = new StringBuilder("<b>");
            dummy.Append(Resources.TableOfContents);
            dummy.Append(@"</b><span id=""ExpandTocSpan""> [<a href=""#"" onclick=""javascript:if(document.getElementById('Toc').style['display']=='none') document.getElementById('Toc').style['display']=''; else document.getElementById('Toc').style['display']='none'; return false;"">");
            dummy.Append(Resources.HideShow);
            dummy.Append("</a>]</span>");
            sb.Replace(TocTitlePlaceHolder, dummy.ToString());

            // Display edit links only when formatting page content (and not transcluded page content)
            if (current != null && context == FormattingContext.PageContent)
            {
            }

            // Remove all placeholders left in the page and their wrapping link
            try
            {
                var editSectionPhIdx = 0;
                do
                {
                    var tempString = sb.ToString();
                    editSectionPhIdx = tempString.IndexOf(EditSectionPlaceHolder);
                    if (editSectionPhIdx >= 0)
                    {
                        // Find first '<' before index, and first '>' after index
                        var openingIndex = editSectionPhIdx;
                        while (openingIndex > 0 && tempString[openingIndex] != '<')
                        {
                            openingIndex--;
                        }
                        var closingIndex = tempString.IndexOf('>', editSectionPhIdx);

                        sb.Remove(openingIndex, closingIndex - openingIndex + 1);
                    }
                } while (editSectionPhIdx >= 0);
            }
            catch
            {
                // Just in case
                sb.Replace(EditSectionPlaceHolder, "");
            }

            //match = SignRegex.Match(sb.ToString());
            //while (match.Success)
            //{
            //    sb.Remove(match.Index, match.Length);
            //    try
            //    {
            //        // Avoid that malformed tags cause a crash
            //        var txt = match.Value.Substring(3, match.Length - 6);
            //        var idx = txt.LastIndexOf(",");
            //        var fields = new string[] { txt.Substring(0, idx), txt.Substring(idx + 1) };
            //        dummy = new StringBuilder();
            //        dummy.Append(@"<span class=""signature"">");
            //        dummy.Append(Users.UserLink(fields[0]));
            //        dummy.Append(", ");
            //        dummy.Append(Preferences.AlignWithTimezone(DateTime.Parse(fields[1])).ToString(Settings.DateTimeFormat));
            //        dummy.Append("</span>");
            //        sb.Insert(match.Index, dummy.ToString());
            //    }
            //    catch { }
            //    match = SignRegex.Match(sb.ToString());
            //}

            //// Transclusion
            //match = TransclusionRegex.Match(sb.ToString());
            //while (match.Success)
            //{
            //    sb.Remove(match.Index, match.Length);
            //    var pageName = match.Value.Substring(3, match.Value.Length - 4);
            //    if (pageName.StartsWith("++")) pageName = pageName.Substring(2);
            //    else
            //    {
            //        // Add current namespace, if not present
            //        var tsNamespace = NameTools.GetNamespace(pageName);
            //        var currentNamespace = current != null ? NameTools.GetNamespace(current.FullName) : null;
            //        if (string.IsNullOrEmpty(tsNamespace) && !string.IsNullOrEmpty(currentNamespace))
            //        {
            //            pageName = NameTools.GetFullName(currentNamespace, pageName);
            //        }
            //    }
            //    PageInfo transcludedPage = Pages.FindPage(pageName);

            //    // Avoid circular transclusion
            //    var transclusionAllowed =
            //        transcludedPage != null &&
            //            (current != null &&
            //             transcludedPage.FullName != current.FullName ||
            //             context != FormattingContext.PageContent && context != FormattingContext.TranscludedPageContent);

            //    if (transclusionAllowed)
            //    {
            //        var currentUsername = SessionFacade.GetCurrentUsername();
            //        var currentGroups = SessionFacade.GetCurrentGroupNames();

            //        var canView = AuthChecker.CheckActionForPage(transcludedPage, Actions.ForPages.ReadPage, currentUsername, currentGroups);
            //        if (canView)
            //        {
            //            dummy = new StringBuilder();
            //            dummy.Append(@"<div class=""transcludedpage"">");
            //            dummy.Append(FormattingPipeline.FormatWithPhase3(
            //                FormattingPipeline.FormatWithPhase1And2(Content.GetPageContent(transcludedPage, true).Content, false, FormattingContext.TranscludedPageContent, transcludedPage), FormattingContext.TranscludedPageContent, transcludedPage));
            //            dummy.Append("</div>");
            //            sb.Insert(match.Index, dummy.ToString());
            //        }
            //        else
            //        {
            //            var formatterErrorString = @"<b style=""color: #FF0000;"">PERMISSION ERROR (You are not allowed to see transcluded page)</b>";
            //            sb.Insert(match.Index, formatterErrorString);
            //        }
            //    }
            //    else
            //    {
            //        var formatterErrorString = @"<b style=""color: #FF0000;"">FORMATTER ERROR (Transcluded inexistent page or this same page)</b>";
            //        sb.Insert(match.Index, formatterErrorString);
            //    }

            //    match = TransclusionRegex.Match(sb.ToString());
            //}

            return sb.ToString();
        }

        /// <summary>
        /// Builds the current namespace drop-down list.
        /// </summary>
        /// <returns>The drop-down list HTML markup.</returns>
        private static string BuildCurrentNamespaceDropDown() => string.Empty;

        private static string GetProfileLink(string username) => string.Empty;

        private static string GetLanguageLink(string username) => string.Empty;

        private static string GetLoginLink() => string.Empty;

        private static string GetLogoutLink() => string.Empty;

    }
}
