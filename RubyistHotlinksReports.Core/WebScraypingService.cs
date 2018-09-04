using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using RubyistHotlinksReports.Core.Models;

namespace RubyistHotlinksReports.Core
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
        public async Task<IEnumerable<string>> EnumerableRubyistHotlinksUrl()
        {
            HttpClient httpClient = new HttpClient();
            var rawHtml = await httpClient.GetStringAsync(StartBaseUrl);
            return ParseHotlinksUrl(rawHtml);
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
        public async Task<IEnumerable<Talk>> EnumerableTalks(string url)
        {
            HttpClient httpClient = new HttpClient();
            var rawHtml = await httpClient.GetStringAsync(url);
            return ParseTalkHtml(rawHtml);
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
            var dlChildren = htmlDoc.DocumentNode.SelectNodes(@"//dl/*").ToList();
            for (int i = 0; i < dlChildren.Count; i+= 2)
            {
                var messageHtmlNode = dlChildren[i + 1];
                string messageHtml = messageHtmlNode.InnerHtml;
                foreach (var child in messageHtmlNode.SelectNodes(@"//sup"))
                {
                    messageHtml = messageHtml.Replace(child.OuterHtml, string.Empty);
                }
                foreach (var child in messageHtmlNode.SelectNodes(@"//img"))
                {
                    messageHtml = messageHtml.Replace(child.OuterHtml, string.Empty);
                }
                foreach (var child in messageHtmlNode.SelectNodes(@"//a"))
                {
                    messageHtml = messageHtml.Replace(child.OuterHtml, child.InnerHtml);
                }
                yield return new Talk()
                {
                    User = dlChildren[i].InnerText,
                    Message = messageHtml
                };
            }
        }
    }
}
