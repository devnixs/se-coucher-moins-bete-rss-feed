using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace SeCoucherMoinsBeteRssFeed.Services
{
    public class FeedLoader
    {
        private Anecdote[] _anecdotes = new Anecdote[0];

        private readonly IHttpClientFactory _clientFactory;

        public FeedLoader(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        private const string BaseUrl = "https://secouchermoinsbete.fr";

        private string GetPageUrl(int page)
        {
            return $"{BaseUrl}/?page=" + page;
        }

        public async Task Load()
        {
            int pagesToParse = 20;
            var anecdotes = new List<Anecdote>();
            for (int i = 1; i <= pagesToParse; i++)
            {
                var thisPage = await ParsePage(i);
                anecdotes.AddRange(thisPage);
            }

            _anecdotes = anecdotes.ToArray();
        }

        private async Task<Anecdote[]> ParsePage(int id)
        {
            HtmlWeb web = new HtmlWeb();

            var htmlDoc = await web.LoadFromWebAsync(GetPageUrl(id));
            Console.WriteLine("Retrieving page " + id);

            var path = "//*[@class=\"anecdote-content-wrapper\"]/p/a";
            var nodes = htmlDoc.DocumentNode.SelectNodes(path);

            var anecdotes = new List<Anecdote>();

            foreach (var node in nodes)
            {
                var link = node.Attributes["href"];
                var anecdote = await ParseAnecdote(link.Value);
                anecdotes.Add(anecdote);
            }

            return anecdotes.ToArray();
        }

        private async Task<Anecdote> ParseAnecdote(string link)
        {
            Console.WriteLine("Retrieving anecdote " + link);
            HtmlWeb web = new HtmlWeb();
            var fullUrl = BaseUrl + link;
            var htmlDoc = await web.LoadFromWebAsync(fullUrl);

            var path1 = "//article/p[contains(@class, 'summary')]";
            var path2 = "//article/p[contains(@class, 'details')]";
            var imagePath = "//*[@id='sources-image-wrapper']/a/img";
            var titlePath = "//h1/a";
            var datePath = "//time[contains(@class, 'anecdote-publication-date')]";
            var summary = htmlDoc.DocumentNode.SelectSingleNode(path1);
            var details = htmlDoc.DocumentNode.SelectSingleNode(path2);
            var image = htmlDoc.DocumentNode.SelectSingleNode(imagePath);
            var title = htmlDoc.DocumentNode.SelectSingleNode(titlePath);
            var dateElement = htmlDoc.DocumentNode.SelectSingleNode(datePath);

            var dateString = dateElement.InnerText;
            var dateSplitted = dateString.Split("/");
            var date = new DateTimeOffset(int.Parse(dateSplitted[2]), int.Parse(dateSplitted[1]), int.Parse(dateSplitted[0]), 0, 0, 0, 0, TimeSpan.Zero);

            var anecdote = new Anecdote()
            {
                Content = summary.InnerText + "\n" + (details != null ? details.InnerText : ""),
                Title = title.InnerText,
                Date = date,
                ImageUrl = image?.Attributes["src"].Value,
                Link = fullUrl
            };
            return anecdote;
        }

        public Anecdote[] Get()
        {
            return _anecdotes;
        }
    }

    public class Anecdote
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string Link { get; set; }
        public DateTimeOffset Date { get; set; }
        public string ImageUrl { get; set; }
    }
}
