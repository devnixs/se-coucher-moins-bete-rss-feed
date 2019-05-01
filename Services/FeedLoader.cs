using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.Extensions.Hosting;
using SharpRaven;
using SharpRaven.Data;

namespace SeCoucherMoinsBeteRssFeed.Services
{
    public class FeedLoader
    {
        private Anecdote[] _anecdotes = new Anecdote[0];

        private readonly IHttpClientFactory _clientFactory;
        private readonly IHostingEnvironment _environment;
        private readonly RavenClient _ravenClient;

        public FeedLoader(IHttpClientFactory clientFactory, IHostingEnvironment environment, RavenClient ravenClient)
        {
            _clientFactory = clientFactory;
            _environment = environment;
            _ravenClient = ravenClient;
        }

        private const string BaseUrl = "https://secouchermoinsbete.fr";

        private string GetPageUrl(int page)
        {
            return $"{BaseUrl}/?page=" + page;
        }

        public async Task Load()
        {
            int pagesToParse = _environment.IsDevelopment() ? 1 : 10;
            var anecdotes = new List<Anecdote>();
            for (int i = 1; i <= pagesToParse; i++)
            {
                try
                {
                    var thisPage = await ParsePage(i);
                    anecdotes.AddRange(thisPage);
                }
                catch (Exception e)
                {
                    _ravenClient.Capture(new SentryEvent(e));
                }
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
                try
                {
                    var anecdote = await ParseAnecdote(link.Value);
                    anecdotes.Add(anecdote);
                }
                catch (Exception e)
                {
                    _ravenClient.Capture(new SentryEvent(e));
                }
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
            var titlePath = "//*[@id='anecdote-item']/header/h1/a";
            var datePath = "//time[contains(@class, 'anecdote-publication-date')]";
            var summary = htmlDoc.DocumentNode.SelectSingleNode(path1);
            var details = htmlDoc.DocumentNode.SelectSingleNode(path2);
            var image = htmlDoc.DocumentNode.SelectSingleNode(imagePath);
            var title = htmlDoc.DocumentNode.SelectSingleNode(titlePath);
            var dateElement = htmlDoc.DocumentNode.SelectSingleNode(datePath);

            var dateString = dateElement.InnerText;
            var dateSplitted = dateString.Split("/");
            var date = new DateTimeOffset(int.Parse(dateSplitted[2]), int.Parse(dateSplitted[1]), int.Parse(dateSplitted[0]), 0, 0, 0, 0, TimeSpan.Zero);

            var imageUrl = image?.Attributes["src"].Value;
            long imageLength = 0;
            if (imageUrl != null)
            {
                var client = _clientFactory.CreateClient();
                var imageContent = await client.GetByteArrayAsync(imageUrl);
                imageLength = imageContent.Length;
            }

            var anecdote = new Anecdote()
            {
                Content = summary.InnerText + "\n" + (details != null ? details.InnerText : ""),
                Title = title.InnerText,
                Date = date,
                ImageUrl = imageUrl,
                ImageLength = imageLength,
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
        public long ImageLength { get; set; }
    }
}
