using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using SeCoucherMoinsBeteRssFeed.Services;
using WilderMinds.RssSyndication;

namespace SeCoucherMoinsBeteRssFeed.Controllers
{
    public class RssFeedController : Controller
    {
        private readonly FeedLoader _feedLoader;

        public RssFeedController(FeedLoader feedLoader)
        {
            _feedLoader = feedLoader;
        }

        [Route("/feed")]
        public ActionResult GetFeed()
        {
            var anecdotes = _feedLoader.Get();
            return Ok(anecdotes);
        }

        [Route("/feed.xml")]
        public ActionResult GetFeedXml()
        {
            var anecdotes = _feedLoader.Get();
            var feed = new Feed()
            {
                Title = "Se Coucher Moins Bete",
                Description = "Se Coucher Moins Bete",
                Link = new Uri("https://secouchermoinsbete.fr"),
                Copyright = "",
            };

            foreach (var item in anecdotes)
            {
                var item1 = new Item()
                {
                    Title = item.Title,
                    Body = item.Content,
                    Link = new Uri(item.Link),
                    Permalink = item.Link,
                    PublishDate = item.Date.Date,
                    Author = new Author() {Name = "John Dee", Email = "foo@foo.com"}
                };

                feed.Items.Add(item1);
            }

            var rss = feed.Serialize(new SerializeOption() {Encoding = Encoding.UTF8});

            return Content(rss, "application/rss+xml; charset=UTF-8", Encoding.UTF8);
        }
    }
}
