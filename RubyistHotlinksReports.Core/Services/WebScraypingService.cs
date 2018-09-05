using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using RubyistHotlinksReports.Core.Models;

namespace RubyistHotlinksReports.Core.Services
{
    public class WebScraypingService
    {
        private static readonly string StartBaseUrl = "https://magazine.rubyist.net/articles/0001/0001-Hotlinks.html";

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// 個々のインタビューに連載一覧が記載されているので、第一回のインタビューから一覧をリストアップする。
        /// </remarks>
        /// <returns></returns>
        public async Task<IList<string>> ToListRubyistHotlinksUrl()
        {
            HttpClient httpClient = new HttpClient();
            var rawHtml = await httpClient.GetStringAsync(StartBaseUrl);
            return ParseHotlinksUrl(rawHtml).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rawHtml"></param>
        /// <returns></returns>
        private IEnumerable<string> ParseHotlinksUrl(string rawHtml)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(rawHtml);
            var articles = htmlDoc.DocumentNode.SelectNodes(@"//ul/li/p/a").Select(a => new
            {
                Url = a.Attributes["href"].Value.Trim(),
                Title = a.InnerText.Trim()
            });
            Uri startUri = new Uri(StartBaseUrl);
            yield return startUri.AbsoluteUri;
            foreach (var article in articles)
            {
                var uri = new Uri(startUri,article.Url);
                yield return uri.AbsoluteUri;
            }

        }
        /// <summary>
        /// ページの中から発言を抽出します。
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<IList<Talk>> ToListTalks(string url)
        {
            HttpClient httpClient = new HttpClient();
            var rawHtml = await httpClient.GetStringAsync(url);
            return ParseTalkHtml(rawHtml).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rawHtml"></param>
        /// <returns></returns>
        private IEnumerable<Talk> ParseTalkHtml(string rawHtml)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(rawHtml);
            foreach (var item in ParseTalkHtml2(htmlDoc))
            {
                yield return item;
            }

            if (htmlDoc.DocumentNode.SelectNodes(@"//dl/*") != null)
            {
                var dlChildren = htmlDoc.DocumentNode.SelectNodes(@"//dl/*").ToList();
                for (int i = 0; i < dlChildren.Count; i++)
                {
                    var nowNode = dlChildren[i];
                    if (nowNode.Name != "dt")
                    {
                        continue;
                    }

                    var messageHtmlNode = dlChildren.Skip(i).FirstOrDefault(node => node.Name == "dd");
                    if (messageHtmlNode == null)
                    {
                        continue;
                    }

                    var messageHtml = messageHtmlNode.InnerHtml;
                    messageHtml = CleaningSupportTag(messageHtmlNode, messageHtml);

                    yield return new Talk
                    {
                        User = nowNode.InnerText,
                        Message = messageHtml
                    };
                }
            }
        }

        private static string CleaningSupportTag(HtmlNode messageHtmlNode, string messageHtml)
        {
            var htmlNodeCollection = messageHtmlNode.SelectNodes(@"//sup");
            if (htmlNodeCollection != null)
            {
                foreach (var child in htmlNodeCollection)
                {
                    messageHtml = messageHtml.Replace(child.OuterHtml, string.Empty);
                }
            }

            htmlNodeCollection = messageHtmlNode.SelectNodes(@"//img");
            if (htmlNodeCollection != null)
            {
                foreach (var child in htmlNodeCollection)
                {
                    messageHtml = messageHtml.Replace(child.OuterHtml, string.Empty);
                }
            }

            htmlNodeCollection = messageHtmlNode.SelectNodes(@"//a");
            if (htmlNodeCollection != null)
            {
                foreach (var child in htmlNodeCollection)
                {
                    messageHtml = messageHtml.Replace(child.OuterHtml, child.InnerHtml);
                }
            }
            return messageHtml;
        }

        private IEnumerable<Talk> ParseTalkHtml2(HtmlDocument htmlDoc)
        {
            var dlChildren = htmlDoc.DocumentNode.SelectNodes(@"//p").ToList();
            foreach (var htmlNode in dlChildren)
            {
                string message = htmlNode.InnerHtml;
                var userNode = htmlNode.SelectSingleNode(@"strong");
                if (userNode == null)
                {
                    continue;
                }
                var user = userNode.InnerText;
                message = message.Replace(userNode.OuterHtml, string.Empty);
                message = CleaningSupportTag(htmlNode, message);
                yield return new Talk
                {
                    User = user,
                    Message = message
                };

            }
        }
    }
}
