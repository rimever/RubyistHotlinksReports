using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
