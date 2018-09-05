using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GraphX.Controls;
using GraphX.PCL.Common.Enums;
using GraphX.PCL.Logic.Algorithms.LayoutAlgorithms;
using NMeCab;
using RubyistHotlinksReports.Core.Models;
using RubyistHotlinksReports.Core.Services;
using RubyistHotlinksReports.WordGraph.Models;
using RubyistHotlinksReports.WordGraph.ViewModels;

namespace RubyistHotlinksReports.WordGraph
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly AnalyzeService analyzeService = new AnalyzeService();

        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<WordViewModel> WordViewModels = new ObservableCollection<WordViewModel>();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            //Customize Zoombox a bit
            //Set minimap (overview) window to be visible by default
            ZoomControl.SetViewFinderVisibility(zoomControl, Visibility.Visible);
            //Set Fill zooming strategy so whole graph will be always visible
            zoomControl.ZoomToFill();
            this.Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await Setup();
        }

        private async Task Setup()
        {

            await analyzeService.CacheAllTalk();

            foreach (var userName in analyzeService.AllTalkDictionary.Keys)
            {
                ListBoxUser.Items.Add(userName);
            }

            var totalWord = new List<Word>();

            foreach (var item in analyzeService.AllTalkDictionary)
            {
                totalWord.AddRange(item.Value);
            }

            var enumerable = totalWord.GroupBy(w => w.Base).Select(x => new
            {
                RealName = x.Key,
                Count = x.Count()
            })
                .Where(x => x.Count > 1)
                .OrderByDescending(x => x.Count);


            foreach (var item in enumerable)
            {
                WordViewModels.Add(new WordViewModel { Base = item.RealName, Count = item.Count });
            }

            ListBoxWord.ItemsSource = WordViewModels;
        }


        private void SetupGraphArea()
        {
            //Lets create logic core and filled data graph with edges and vertices
            var logicCore = new WordGXLogicCore
            { Graph = SetupGraph(), DefaultLayoutAlgorithm = LayoutAlgorithmTypeEnum.KK };
            //This property sets layout algorithm that will be used to calculate vertices positions
            //Different algorithms uses different values and some of them uses edge Weight property.
            //Now we can set parameters for selected algorithm using AlgorithmFactory property. This property provides methods for
            //creating all available algorithms and algo parameters.
            logicCore.DefaultLayoutAlgorithmParams =
                logicCore.AlgorithmFactory.CreateLayoutParameters(LayoutAlgorithmTypeEnum.KK);
            //Unfortunately to change algo parameters you need to specify params type which is different for every algorithm.
            ((KKLayoutParameters)logicCore.DefaultLayoutAlgorithmParams).MaxIterations = 100;

            //This property sets vertex overlap removal algorithm.
            //Such algorithms help to arrange vertices in the layout so no one overlaps each other.
            logicCore.DefaultOverlapRemovalAlgorithm = OverlapRemovalAlgorithmTypeEnum.FSA;
            //Default parameters are created automaticaly when new default algorithm is set and previous params were NULL
            logicCore.DefaultOverlapRemovalAlgorithmParams.HorizontalGap = 50;
            logicCore.DefaultOverlapRemovalAlgorithmParams.VerticalGap = 50;

            //This property sets edge routing algorithm that is used to build route paths according to algorithm logic.
            //For ex., SimpleER algorithm will try to set edge paths around vertices so no edge will intersect any vertex.
            //Bundling algorithm will try to tie different edges that follows same direction to a single channel making complex graphs more appealing.
            logicCore.DefaultEdgeRoutingAlgorithm = EdgeRoutingAlgorithmTypeEnum.SimpleER;

            //This property sets async algorithms computation so methods like: Area.RelayoutGraph() and Area.GenerateGraph()
            //will run async with the UI thread. Completion of the specified methods can be catched by corresponding events:
            //Area.RelayoutFinished and Area.GenerateGraphFinished.
            logicCore.AsyncAlgorithmCompute = false;

            //Finally assign logic core to GraphArea object
            Area.LogicCore = logicCore;
        }

        private WordGraphModel SetupGraph()
        {
            //Lets make new data graph instance
            var dataGraph = new WordGraphModel();
            IDictionary<string, WordDataVertex> vertexDictionary = new Dictionary<string, WordDataVertex>();
            IDictionary<string, WordDataVertex> userVertexDictionary = new Dictionary<string, WordDataVertex>();
            var selectedUsers = GetSelectedUsers().ToList();
            var selectedWords = GetSelectedWords().ToList();

            IDictionary<string, IList<Word>> selectedTalkDictionary = new Dictionary<string, IList<Word>>();
            foreach (string selectedUser in selectedUsers)
            {
                selectedTalkDictionary.Add(selectedUser,
                    analyzeService.AllTalkDictionary[selectedUser].Where(w => selectedWords.Select(s => s.Base).Contains(w.Base))
                        .ToList());
            }

            foreach (var item in selectedTalkDictionary)
            {
                var userVertex = new WordDataVertex(item.Key);
                userVertexDictionary.Add(item.Key, userVertex);
                dataGraph.AddVertex(userVertex);
            }

            var words = new List<Word>();
            foreach (var item in selectedTalkDictionary)
            {
                words.AddRange(item.Value);
            }

            var topWords = words.GroupBy(w => w.Base).Select(x => new
            {
                RealName = x.Key,
                Count = x.Count()
            })
                .Where(x => x.Count > 1)
                .OrderByDescending(x => x.Count);
            foreach (var word in WordViewModels.Where(vm => topWords.Select(tw => tw.RealName).Contains(vm.Base)))
            {
                if (vertexDictionary.ContainsKey(word.Base))
                {
                    continue;
                }

                var wordVertex = new WordDataVertex($"{word.Base}[{word.Count}]");
                vertexDictionary.Add(word.Base, wordVertex);
                dataGraph.AddVertex(wordVertex);
            }


            foreach (var item in userVertexDictionary)
            {
                foreach (var wordVertex in vertexDictionary)
                {
                    var count = selectedTalkDictionary[item.Key].Count(v => v.Base == wordVertex.Key);
                    if (count == 0)
                    {
                        continue;
                    }

                    var dataEdge = new WordDataEdge(item.Value, wordVertex.Value, count)
                    {
                        Text = count.ToString()
                    };
                    dataGraph.AddEdge(dataEdge);
                }
            }

            return dataGraph;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetSelectedUsers()
        {
            if (ListBoxUser.SelectedItems.Count == 0)
            {
                foreach (var selectedItem in ListBoxUser.Items)
                {
                    yield return selectedItem.ToString();
                }
            }

            foreach (var selectedItem in ListBoxUser.SelectedItems)
            {
                yield return selectedItem.ToString();
            }
        }

        public IEnumerable<WordViewModel> GetSelectedWords()
        {
            if (ListBoxWord.SelectedItems.Count == 0)
            {
                foreach (var word in WordViewModels)
                {
                    yield return word;
                }
            }

            foreach (var selectedItem in ListBoxWord.SelectedItems)
            {
                var viewModel = selectedItem as WordViewModel;
                if (viewModel == null)
                {
                    continue;
                }

                yield return viewModel;
            }
        }

        private void ButtonRedrawOnClick(object sender, RoutedEventArgs e)
        {
            SetupGraphArea();
            Area.GenerateGraph(true, true);
            Area.SetEdgesDashStyle(EdgeDashStyle.Dash);
            Area.ShowAllEdgesArrows(false);
            Area.ShowAllEdgesLabels(true);
            zoomControl.ZoomToFill();
        }
    }
}
