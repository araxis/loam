using System.Text.RegularExpressions;

namespace Loam.Gallery;

internal static class GallerySourceCode
{
    private const string ComponentsSourceFile = "ComponentsView.cs";
    private static readonly Lazy<string?> ComponentsSource = new(LoadComponentsSource);
    private static readonly Dictionary<string, string> SnippetCache = new(StringComparer.Ordinal);

    public static string ForMethod(string methodName)
    {
        if (SnippetCache.TryGetValue(methodName, out var cached))
        {
            return cached;
        }

        var source = ComponentsSource.Value;
        if (source is null)
        {
            return $"{methodName}();";
        }

        var snippet = ExtractMethod(source, methodName);
        var code = string.IsNullOrWhiteSpace(snippet) ? $"{methodName}();" : snippet;
        SnippetCache[methodName] = code;
        return code;
    }

    private static string? LoadComponentsSource()
    {
        foreach (var start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
        {
            var directory = new DirectoryInfo(start);
            while (directory is not null)
            {
                foreach (var candidate in CandidatePaths(directory.FullName))
                {
                    if (File.Exists(candidate))
                    {
                        return File.ReadAllText(candidate);
                    }
                }

                directory = directory.Parent;
            }
        }

        return null;
    }

    private static IEnumerable<string> CandidatePaths(string root)
    {
        yield return Path.Combine(root, ComponentsSourceFile);
        yield return Path.Combine(root, "Source", ComponentsSourceFile);
        yield return Path.Combine(root, "samples", "Loam.Gallery", ComponentsSourceFile);
    }

    private static string? ExtractMethod(string source, string methodName)
    {
        var match = Regex.Match(
            source,
            $@"private\s+static\s+[^\r\n=;{{]+?\s+{Regex.Escape(methodName)}\s*\(");
        if (!match.Success)
        {
            return null;
        }

        var openBrace = source.IndexOf('{', match.Index + match.Length);
        if (openBrace < 0)
        {
            return null;
        }

        var closeBrace = FindMatchingBrace(source, openBrace);
        if (closeBrace < 0)
        {
            return null;
        }

        var start = source.LastIndexOf('\n', match.Index);
        start = start < 0 ? 0 : start + 1;
        return Normalize(source[start..(closeBrace + 1)]);
    }

    private static int FindMatchingBrace(string source, int openBrace)
    {
        var depth = 0;
        var inString = false;
        var inChar = false;
        var inLineComment = false;
        var inBlockComment = false;
        var verbatimString = false;

        for (var i = openBrace; i < source.Length; i++)
        {
            var current = source[i];
            var next = i + 1 < source.Length ? source[i + 1] : '\0';

            if (inLineComment)
            {
                if (current is '\r' or '\n')
                {
                    inLineComment = false;
                }

                continue;
            }

            if (inBlockComment)
            {
                if (current == '*' && next == '/')
                {
                    inBlockComment = false;
                    i++;
                }

                continue;
            }

            if (inString)
            {
                if (verbatimString && current == '"' && next == '"')
                {
                    i++;
                    continue;
                }

                if (current == '"' && (verbatimString || !IsEscaped(source, i)))
                {
                    inString = false;
                    verbatimString = false;
                }

                continue;
            }

            if (inChar)
            {
                if (current == '\'' && !IsEscaped(source, i))
                {
                    inChar = false;
                }

                continue;
            }

            if (current == '/' && next == '/')
            {
                inLineComment = true;
                i++;
                continue;
            }

            if (current == '/' && next == '*')
            {
                inBlockComment = true;
                i++;
                continue;
            }

            if (current == '"')
            {
                inString = true;
                verbatimString = i > 0 && source[i - 1] == '@';
                continue;
            }

            if (current == '\'')
            {
                inChar = true;
                continue;
            }

            if (current == '{')
            {
                depth++;
            }
            else if (current == '}')
            {
                depth--;
                if (depth == 0)
                {
                    return i;
                }
            }
        }

        return -1;
    }

    private static bool IsEscaped(string source, int index)
    {
        var slashCount = 0;
        for (var i = index - 1; i >= 0 && source[i] == '\\'; i--)
        {
            slashCount++;
        }

        return slashCount % 2 == 1;
    }

    private static string Normalize(string code)
    {
        var lines = code.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n').ToList();
        while (lines.Count > 0 && string.IsNullOrWhiteSpace(lines[0]))
        {
            lines.RemoveAt(0);
        }

        while (lines.Count > 0 && string.IsNullOrWhiteSpace(lines[^1]))
        {
            lines.RemoveAt(lines.Count - 1);
        }

        var indent = lines
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(LeadingSpaces)
            .DefaultIfEmpty(0)
            .Min();

        return string.Join(Environment.NewLine, lines.Select(line => line.Length >= indent ? line[indent..] : line));
    }

    private static int LeadingSpaces(string line)
    {
        var count = 0;
        while (count < line.Length && line[count] == ' ')
        {
            count++;
        }

        return count;
    }
}
