using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NMeCab;

namespace RubyistHotlinksReports.Core.Models
{
    /// <summary>
    /// 発言モデル
    /// </summary>
    public class Talk
    {
        /// <summary>
        /// 発言者
        /// </summary>
        public string User { get; set; }
        /// <summary>
        /// 発言
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 発言を単語に分解した情報
        /// </summary>
        public IList<Word> Words { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public void ParseMessage(MeCabTagger meCabTagger)
        {
            Words = new List<Word>();
            var node = meCabTagger.ParseToNode(Message);
            while (node != null)
            {
                if (node.CharType > 0)
                {
                    string word = node.Surface;
                    var data = new List<string>(node.Feature.Split(','));

                    Words.Add(new Word
                    {
                        Surface = word,
                        Elements = data
                    });
                }
                node = node.Next;
            }
        }
    }
}
