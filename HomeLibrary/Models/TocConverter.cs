using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

public static class TocConverter
{
  /// <summary>
  /// HTML из редактора → XML оглавления.
  /// </summary>
  public static string HtmlToXml(string html)
  {
    if (string.IsNullOrWhiteSpace(html))
      return "<toc />";

    try
    {
      var doc = new XDocument(new XElement("toc"));
      var root = doc.Root!;

      // Извлекаем заголовки h1–h3 по порядку появления
      var matches = Regex.Matches(html,
          @"<h([1-3])[^>]*>(.*?)</h\1>",
          RegexOptions.IgnoreCase | RegexOptions.Singleline);

      foreach (Match m in matches)
      {
        var level = m.Groups[1].Value;
        var title = StripTags(m.Groups[2].Value).Trim();
        if (!string.IsNullOrEmpty(title))
        {
          root.Add(new XElement("item",
              new XAttribute("title", title),
              new XAttribute("level", level)));
        }
      }

      // Если заголовков нет — извлекаем из li
      if (!root.HasElements)
      {
        var liMatches = Regex.Matches(html,
            @"<li[^>]*>(.*?)</li>",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        foreach (Match li in liMatches)
        {
          var text = StripTags(li.Groups[1].Value).Trim();
          if (!string.IsNullOrEmpty(text))
          {
            root.Add(new XElement("item",
                new XAttribute("title", text),
                new XAttribute("level", "1")));
          }
        }
      }

      return doc.ToString();
    }
    catch
    {
      return "<toc />";
    }
  }

  /// <summary>
  /// XML оглавления → HTML для редактора.
  /// </summary>
  public static string XmlToHtml(string xml)
  {
    if (string.IsNullOrWhiteSpace(xml))
      return string.Empty;

    try
    {
      var doc = XDocument.Parse(xml);
      var sb = new StringBuilder();

      foreach (var item in doc.Root!.Elements("item"))
      {
        var title = item.Attribute("title")?.Value ?? "";
        var level = item.Attribute("level")?.Value ?? "1";

        var tag = level switch
        {
          "1" => "h1",
          "2" => "h2",
          "3" => "h3",
          _ => "h2"
        };

        sb.AppendLine($"<{tag}>{System.Security.SecurityElement.Escape(title)}</{tag}>");
      }

      return sb.ToString();
    }
    catch
    {
      return string.Empty;
    }
  }

  private static string StripTags(string input)
  {
    return Regex.Replace(input, "<.*?>", string.Empty);
  }
}
