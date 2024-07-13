using System.Text.RegularExpressions;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace FlipLeaf.Markup.Markdown
{
    public class WikiLinkParser : InlineParser
    {
        public WikiLinkParser()
        {
            OpeningCharacters = new[] { '[' };
        }

        public string Extension { get; set; } = string.Empty;

        public bool IncludeTrailingCharacters { get; set; } = false;

        public char WhiteSpaceUrlChar { get; set; } = '_';

        public override bool Match(InlineProcessor processor, ref StringSlice slice)
        {
            char c;
            //var c = slice.CurrentChar;
            processor.GetSourcePosition(slice.Start, out var line, out var column);

            // consume first '['
            var c2 = slice.NextChar();
            if (c2 != '[')
            {
                return false;
            }

            var start = slice.Start + 1;
            var labelStart = start;

            var hasEscape = false;
            SourceSpan? labelSpan = null;
            string? label = null;
            SourceSpan? urlSpan = null;
            string? url = null;
            var success = false;
            var hasSeparator = false;
            while (true)
            {
                c = slice.NextChar();

                if (c == '\0' || c == '\r' || c == '\n')
                {
                    break;
                }

                if (hasEscape)
                {
                    // ignore escaped characters
                    hasEscape = false;
                }
                else
                {
                    if (c == '\\')
                    {
                        hasEscape = true;
                    }
                    else if (c == '|')
                    {
                        var span = new SourceSpan(start, slice.Start - 1);
                        if (span.Length != 0)
                        {
                            urlSpan = span;
                            url = slice.Text.Substring(span.Start, span.Length);
                        }

                        hasSeparator = true;
                        labelStart = slice.Start + 1;
                    }
                    else if (c == ']')
                    {
                        c2 = slice.PeekChar(1);
                        if (c2 == ']')
                        {
                            // end of link
                            success = true;
                            c = slice.NextChar();

                            var span = new SourceSpan(labelStart, slice.Start - 2);
                            var trailingCharsLen = 0;
                            var linkEnd = -1;
                            if (!hasSeparator && IncludeTrailingCharacters)
                            {
                                c2 = slice.PeekChar(1);
                                if (char.IsLetter(c2) || c2 == '\'')
                                {
                                    linkEnd = slice.Start - 1;
                                    c = slice.NextChar();
                                    while (char.IsLetter(c) || c == '\'')
                                    {
                                        trailingCharsLen++;
                                        c = slice.NextChar();
                                    }
                                }
                            }

                            slice.NextChar();

                            if (linkEnd != -1)
                            {
                                url = slice.Text.Substring(labelStart, linkEnd - 2);
                                urlSpan = span;

                                label = slice.Text.Substring(labelStart, linkEnd - 2)
                                    + slice.Text.Substring(linkEnd + 2, trailingCharsLen);
                            }

                            if (span.Length != 0)
                            {
                                labelSpan = span;
                                if (label == null)
                                {
                                    label = slice.Text.Substring(span.Start, span.Length);
                                }
                            }
                            break;

                        }
                    }
                }
            }

            if (success)
            {
                if (url == null)
                {
                    if (label == null)
                    {
                        return false;
                    }

                    // occurs when no separator were used
                    // copy label as url
                    url = label;
                    urlSpan = labelSpan;

                    // keep only the page name
                    var lastSegment = label.LastIndexOf('/');
                    if (lastSegment != -1)
                    {
                        label = label.Substring(lastSegment + 1);
                    }

                    // remove any hash
                    var hash = label.LastIndexOf('#');
                    if (hash != -1)
                    {
                        label = label.Substring(0, hash);
                    }
                }

                if (label == null)
                {
                    label = url;

                    // keep only the page name
                    var lastSegment = label.LastIndexOf('/');
                    if (lastSegment != -1)
                    {
                        label = label.Substring(lastSegment + 1);
                    }

                    // remove any hash
                    var hash = label.LastIndexOf('#');
                    if (hash != -1)
                    {
                        label = label.Substring(0, hash);
                    }

                    labelSpan = urlSpan;
                }

                if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                    && !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    // adapt relative url
                    url = Regex.Replace(url, "[ ]", $"{WhiteSpaceUrlChar}");
                    if (!string.IsNullOrEmpty(Extension))
                    {
                        url += Extension;
                    }
                }

                var link = new LinkInline()
                {
                    Column = column,
                    Line = line,
                    LabelSpan = labelSpan,
                    Label = label,
                    Url = url,
                    UrlSpan = urlSpan,
                    IsClosed = true,
                    //IsShortcut = false,
                    IsImage = false,
                };

                link.AppendChild(new LiteralInline(label));

                processor.Inline = link;
            }

            return success;
        }
    }
}
