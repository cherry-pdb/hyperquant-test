using System.Text;

namespace TestHQ.Core.Helpers;

public class UrlBuilder
{
    private const char UrlDelimiter = '/';
    
    private readonly StringBuilder _url;
    private readonly List<string> _paths;
    private readonly Dictionary<string, string> _queryParams;

    public UrlBuilder(string baseUrl)
    {
        _url = new StringBuilder(baseUrl.TrimEnd(UrlDelimiter));
        _paths = new List<string>();
        _queryParams = new Dictionary<string, string>();
    }
    
    public UrlBuilder AddPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path can't be null or empty.", nameof(path));

        _paths.Add(path.Trim(UrlDelimiter));
        
        return this;
    }
    
    public UrlBuilder AddParameter(string paramName, string? paramValue)
    {
        if (string.IsNullOrWhiteSpace(paramName))
            throw new ArgumentException("Parameter name can't be null or empty.", nameof(paramName));

        if (!string.IsNullOrWhiteSpace(paramValue))
            _queryParams[paramName] = paramValue;

        return this;
    }
    
    public string Build()
    {
        if (_paths.Count != 0)
        {
            _url.Append(UrlDelimiter).Append(string.Join(UrlDelimiter, _paths));
        }

        if (_queryParams.Count != 0)
        {
            var queryString = string.Join("&",
                _queryParams.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
            _url.Append('?')
                .Append(queryString);
        }

        return _url.ToString();
    }
}