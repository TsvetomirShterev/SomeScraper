namespace SomeScraper;

using HtmlAgilityPack;
using ScrapySharp.Network;
using ScrapySharp.Extensions;
using CsvHelper;
using System.Globalization;

public class Program
{
    private static readonly ScrapingBrowser scrapingBrowser = new();

    private static readonly string url = "https://www.emag.bg/laptopi/c?ref=bc";

    public static void Main()
    {
        Console.WriteLine("Enter Search Term");
        var searchTerm = Console.ReadLine();

        var mainLinks = GetMainPageLinks(url);

        var listDetails = GetPageDetails(mainLinks, searchTerm);

        ExportToCsv(listDetails, searchTerm);
    }

    private static List<string> GetMainPageLinks(string url)
    {
        var homepageLinks = new List<string>();

        var html = GetHtml(url);

        var links = html.CssSelect("a");

        var notNullLinks = links.Where(x => x.Attributes["href"] != null).ToList();

        foreach (var link in notNullLinks)
        {
            if (link.Attributes["href"].Value.Contains("pd"))
            {
                homepageLinks.Add(link.Attributes["href"].Value);
            }
        }

        return homepageLinks;
    }

    private static List<PageDetails> GetPageDetails(List<string> urls, string searchTerm)
    {
        var pageDetailsCollection = new List<PageDetails>();

        foreach (var url in urls)
        {
            var htmlNode = GetHtml(url);

            var pageTitle = htmlNode.OwnerDocument.DocumentNode.SelectSingleNode("//html/head/title").InnerText;

            if (pageTitle.ToLower().Contains(searchTerm.ToLower()))
            {
                var pageDetails = new PageDetails()
                {
                    Title = pageTitle,
                    Url = url,
                };

                pageDetailsCollection.Add(pageDetails);
            }
        }

        return pageDetailsCollection;
    }

    private static void ExportToCsv(List<PageDetails> pageDetails, string searchTerm)
    {
        using var writer = new StreamWriter($@"/Users/tshterev/OneDrive - ENDAVA/Desktop/{searchTerm}_{DateTime.Now.ToFileTime()}.csv");
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        csv.WriteRecords(pageDetails);
    }

    private static HtmlNode GetHtml(string url)
    {
        var uri = new Uri(url);

        var webPage = scrapingBrowser.NavigateToPage(uri);

        return webPage.Html;
    }
}