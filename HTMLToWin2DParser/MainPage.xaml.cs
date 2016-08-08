using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using HtmlAgilityPack;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace HTMLToWin2DParser
{

    public struct ParseItem
    {
        public string Tag;
        public string Text;
        public int StartIndex;
        public int Length;

    }
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private string _sampleHTMLString;
        private List<ParseItem> _parsedItems = new List<ParseItem>();
        private CanvasTextLayout _tl;
        
        public MainPage()
        {
         //   _sampleHTMLString = "<html> <ol> <li> ABCDE <B> bold </B> </li> </ol> </html>";
            _sampleHTMLString = "<p>  hello <b> text </b> </p> <h1> <i> italics </i> </h1>";
            this.InitializeComponent();

            xCanvas.Draw += XCanvasOnDraw;

        }

        private void XCanvasOnDraw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            if (_tl == null)
                return;
            using (var ds = args.DrawingSession)
            {
                ds.DrawTextLayout(_tl, 0,0, Colors.Black);
            }
        }

        public HtmlDocument GetHTML(string url)
        {
            HtmlWeb web = new HtmlWeb();
            HtmlDocument document = web.LoadFromWebAsync(url).Result;
            return document;
        }

        private void UIElement_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            GetHTML(URLTextBox.Text);
        }

        private void ParseSampleButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            var result = GetHTMLFromSample();
            RecursiveParsing(result.DocumentNode.ChildNodes,0);

            var text = HtmlRemoval.StripTagsRegex(_sampleHTMLString);

            _tl = new CanvasTextLayout(xCanvas, text, new CanvasTextFormat(), 300, 300);
            

            foreach (var parsedItem in _parsedItems)
            {
                Debug.WriteLine(parsedItem.Tag + "\t"+ parsedItem.Text + "\t" + parsedItem.StartIndex + "\t" + parsedItem.Length);

                if (parsedItem.Tag == "b")
                {
                    _tl.SetFontWeight(parsedItem.StartIndex, parsedItem.Length, FontWeights.ExtraBold);
                }

                if  (parsedItem.Tag == "br")
                {
                    
                    //Microsoft.Graphics.Canvas.Text.CanvasClusterProperties.Newline;
                    
                }

                if (parsedItem.Tag == "p")
                {
                    //float space = Single.Parse(xCanvas.ActualWidth.ToString());
                    //float space = (float) 10.0;
                    //_tl.SetCharacterSpacing(parsedItem.StartIndex, parsedItem.Length, space, space, 0);
                }

                if (parsedItem.Tag == "u")
                {
                    _tl.SetUnderline(parsedItem.StartIndex, parsedItem.Length, true);
                }

                if (parsedItem.Tag == "i")
                {
                    _tl.SetFontStyle(parsedItem.StartIndex, parsedItem.Length, FontStyle.Italic);
                }

                if (parsedItem.Tag == "strike")
                {
                    _tl.SetStrikethrough(parsedItem.StartIndex, parsedItem.Length, true);
                }

                if (parsedItem.Tag == "h1")
                {
                    _tl.SetFontSize(parsedItem.StartIndex, parsedItem.Length, 36);
                }
                if (parsedItem.Tag == "h2")
                {
                    _tl.SetFontSize(parsedItem.StartIndex, parsedItem.Length, 28);
                }
                if (parsedItem.Tag == "h3")
                {
                    _tl.SetFontSize(parsedItem.StartIndex, parsedItem.Length, 22);
                }
                if (parsedItem.Tag == "h4")
                {
                    _tl.SetFontSize(parsedItem.StartIndex, parsedItem.Length, 18);
                }
                if (parsedItem.Tag == "h5")
                {
                    _tl.SetFontSize(parsedItem.StartIndex, parsedItem.Length, 13);
                }
                if (parsedItem.Tag == "h6")
                {
                    _tl.SetFontSize(parsedItem.StartIndex, parsedItem.Length, 11);
                }
                
                if (parsedItem.Tag == "ol")
                {
                    
                }
                
                if (parsedItem.Tag == "ul")
                {
                    
                }

                if (parsedItem.Tag == "li")
                {
                    
                }
            }


            xCanvas.Invalidate();
        }

        public HtmlDocument GetHTMLFromSample()
        {
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(_sampleHTMLString);
            return document;
        }

        public void RecursiveParsing(IEnumerable<HtmlNode> children, int currentIndex)
        {
            var characterIndex = currentIndex;
            foreach (HtmlNode node in children)
            {
                if (node.HasChildNodes)
                {
                    RecursiveParsing(node.ChildNodes, currentIndex);
                }

                var innerString = HtmlRemoval.StripTagsRegex(node.InnerHtml);
                //Debug.WriteLine(node.Name + " " + innerString);
                var item = new ParseItem();
                item.Text = innerString;
                item.Length = innerString.Length;
                item.StartIndex = characterIndex;
                item.Tag = node.Name;
                _parsedItems.Add(item);

                characterIndex += item.Length;
            }
        }
    }
}
