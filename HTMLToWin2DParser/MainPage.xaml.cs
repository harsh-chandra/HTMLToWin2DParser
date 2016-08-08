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
        private List<CanvasTextLayout> _textLayouts = new List<CanvasTextLayout>();
        private List<ParseItem> _breaks;
        private int _indexOffset;
        
        public MainPage()
        {
         //   _sampleHTMLString = "<html> <ol> <li> ABCDE <B> bold </B> </li> </ol> </html>";
            _sampleHTMLString = "<html> hello <br> <b> text </b> <h1> italics </h1> </html>";
            this.InitializeComponent();

            xCanvas.Draw += XCanvasOnDraw;

        }

        private void XCanvasOnDraw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            if (_tl == null)
                return;

            var verticalOffset = (float) 0.0;
            using (var ds = args.DrawingSession)
            {
                foreach (var textLayout in _textLayouts)
                {
                    ds.DrawTextLayout(textLayout, 0, verticalOffset, Colors.Black);
                    verticalOffset += (float) textLayout.DrawBounds.Height;
                }
                
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

            _breaks = (_parsedItems.Where(s => s.Tag == "br" || s.Tag == "p" || s.Tag == "ol" || s.Tag == "ul" || s.Tag == "li")).ToList();

            var text = HtmlRemoval.StripTagsRegex(_sampleHTMLString);
            var substring = text.Substring(0, _breaks.First().StartIndex);
            _indexOffset = _breaks.First().StartIndex;
            Debug.WriteLine("Offset = " + _indexOffset);
            _breaks.RemoveAt(0);

            _tl = new CanvasTextLayout(xCanvas, substring, new CanvasTextFormat(), 300, 300);
            _textLayouts.Add(_tl);

            foreach (var parsedItem in _parsedItems)
            {
                Debug.WriteLine(parsedItem.Tag + "\t"+ parsedItem.Text + "\t" + parsedItem.StartIndex + "\t" + parsedItem.Length);

                if (parsedItem.Tag == "b")
                {
                    _tl.SetFontWeight(parsedItem.StartIndex - _indexOffset, parsedItem.Length, FontWeights.ExtraBold);
                }

                if  (parsedItem.Tag == "br")
                {
                    
                    var followingText = text.Substring(parsedItem.StartIndex);
                    if (_breaks.Count > 0)
                    {
                        substring = followingText.Substring(0, _breaks.First().StartIndex);
                        _indexOffset += _breaks.First().StartIndex;
                        Debug.WriteLine("Offset = " + _indexOffset);
                        _breaks.RemoveAt(0);
                    }
                    else
                    {
                        substring = followingText;
                    }
                    
                    _tl = new CanvasTextLayout(xCanvas, substring, new CanvasTextFormat(), 300, 1000);
                    _textLayouts.Add(_tl);

                }

                if (parsedItem.Tag == "p")
                {
                    _textLayouts.Add(_tl);
                    var followingText = text.Substring(parsedItem.StartIndex);
                    _tl = new CanvasTextLayout(xCanvas, followingText, new CanvasTextFormat(), 300, 1000);

                    float space = (float) 4.0;
                    _tl.SetCharacterSpacing(0, parsedItem.Length, space, 0, 0);
                }

                if (parsedItem.Tag == "u")
                {
                    _tl.SetUnderline(parsedItem.StartIndex - _indexOffset, parsedItem.Length, true);
                }

                if (parsedItem.Tag == "i")
                {
                    _tl.SetFontStyle(parsedItem.StartIndex - _indexOffset, parsedItem.Length, FontStyle.Italic);
                }

                if (parsedItem.Tag == "strike")
                {
                    _tl.SetStrikethrough(parsedItem.StartIndex - _indexOffset, parsedItem.Length, true);
                }

                if (parsedItem.Tag == "h1")
                {
                    _tl.SetFontSize(parsedItem.StartIndex - _indexOffset, parsedItem.Length, 36);
                }
                if (parsedItem.Tag == "h2")
                {
                    _tl.SetFontSize(parsedItem.StartIndex - _indexOffset, parsedItem.Length, 28);
                }
                if (parsedItem.Tag == "h3")
                {
                    _tl.SetFontSize(parsedItem.StartIndex - _indexOffset, parsedItem.Length, 22);
                }
                if (parsedItem.Tag == "h4")
                {
                    _tl.SetFontSize(parsedItem.StartIndex - _indexOffset, parsedItem.Length, 18);
                }
                if (parsedItem.Tag == "h5")
                {
                    _tl.SetFontSize(parsedItem.StartIndex - _indexOffset, parsedItem.Length, 13);
                }
                if (parsedItem.Tag == "h6")
                {
                    _tl.SetFontSize(parsedItem.StartIndex - _indexOffset, parsedItem.Length, 11);
                }
                
                if (parsedItem.Tag == "ol")
                {
                    
                }
                
                if (parsedItem.Tag == "ul")
                {
                    
                }

                if (parsedItem.Tag == "li")
                {
                    AddLineBreak(parsedItem);
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

        public void AddLineBreak(ParseItem item)
        {
            item.Text = item.Text + "\n";
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
