using GraphX.PCL.Logic.Models;
using QuickGraph;

namespace RubyistHotlinksReports.WordGraph.Models
{
    /// <summary>
    /// Logics core object which contains all algorithms and logic settings
    /// </summary>
    public class WordGXLogicCore : GXLogicCore<WordDataVertex, WordDataEdge,
        BidirectionalGraph<WordDataVertex, WordDataEdge>>
    {
    }
}