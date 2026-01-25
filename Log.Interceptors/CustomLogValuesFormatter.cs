using System.Globalization;
using System.Text;

namespace Log.Interceptors;

public class CustomLogValuesFormatter
{
    private readonly string _format;
    private readonly List<string> _valueNames = new();

    public CustomLogValuesFormatter(string format)
    {
        _format = Parse(format);
    }

    // Converts "{Name}" style templates to index-based "{0}" for string.Format
    private string Parse(string template)
    {
        var sb = new StringBuilder();
        int scanIndex = 0;
        int endIndex = template.Length;

        while (scanIndex < endIndex)
        {
            int openBraceIndex = template.IndexOf('{', scanIndex);
            int closeBraceIndex = template.IndexOf('}', scanIndex);

            if (openBraceIndex == -1 || closeBraceIndex == -1 || closeBraceIndex < openBraceIndex)
            {
                sb.Append(template, scanIndex, endIndex - scanIndex);
                break;
            }

            sb.Append(template, scanIndex, openBraceIndex - scanIndex);

            // Extract the property name (e.g., "Name")
            string name = template.Substring(openBraceIndex + 1, closeBraceIndex - openBraceIndex - 1);
            _valueNames.Add(name);

            // Replace with index for string.Format
            sb.Append('{');
            sb.Append(_valueNames.Count - 1);
            sb.Append('}');

            scanIndex = closeBraceIndex + 1;
        }

        return sb.ToString();
    }

    public string Format(object?[]? values)
    {
        if (values == null) return _format;
        return string.Format(CultureInfo.InvariantCulture, _format, values);
    }

    public IEnumerable<KeyValuePair<string, object?>> GetValues(object?[] values)
    {
        for (int i = 0; i < _valueNames.Count; i++)
        {
            yield return new KeyValuePair<string, object?>(_valueNames[i], values[i]);
        }
    }
}