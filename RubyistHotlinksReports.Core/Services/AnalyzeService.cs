using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NMeCab;
using RubyistHotlinksReports.Core.Models;

namespace RubyistHotlinksReports.Core.Services
{
    /// <summary>
    /// 分析を行うサービスクラスです。
    /// </summary>
    public class AnalyzeService
    {
        public readonly IDictionary<string, IList<Word>> AllTalkDictionary = new Dictionary<string, IList<Word>>();

        private readonly WebScraypingService  _webScraypingService = new WebScraypingService();

        /// <summary>
        /// 文字列を単語に分解します。
        /// </summary>
        /// <param name="meCabTagger"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public IEnumerable<Word> ParseText(MeCabTagger meCabTagger, string message)
        {
            var node = meCabTagger.ParseToNode(message);
            while (node != null)
            {
                if (node.CharType > 0)
                {
                    string word = node.Surface;
                    var data = new List<string>(node.Feature.Split(','));

                    yield return new Word
                    {
                        Surface = word,
                        Elements = data
                    };
                }

                node = node.Next;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task CacheAllTalk()
        {
            var list = await _webScraypingService.ToListRubyistHotlinksUrl();
            var allTalks = new List<Talk>();
            foreach (var item in list.ToList())
            {
                allTalks.AddRange(await _webScraypingService.ToListTalks(item));
            }
            var mecabParam = new MeCabParam
            {
                DicDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                    @"..\..\..\RubyistHotlinksReports.Core\dic\ipadic")
            };
            var meCabTagger = MeCabTagger.Create(mecabParam);
            foreach (var talk in allTalks)
            {
                var words = ParseText(meCabTagger, talk.Message).ToList();
                foreach (var word in words)
                {
                    if (word.Pos != "名詞")
                    {
                        continue;
                    }

                    if (word.Base == "*")
                    {
                        continue;
                    }

                    if (word.Pos1 == "非自立")
                    {
                        continue;
                    }

                    if (!AllTalkDictionary.ContainsKey(talk.User))
                    {
                        AllTalkDictionary.Add(talk.User, new List<Word>());
                    }

                    AllTalkDictionary[talk.User].Add(word);
                }
            }
        }

    }
}
