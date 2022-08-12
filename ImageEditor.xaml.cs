using Microsoft.Win32;
using System;
using System.Collections.Generic;
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

namespace ImageEditorProject
{

    /// <summary>
    /// Interaction logic for ImageEditor.xaml
    /// </summary>
    public partial class ImageEditor : UserControl
    {

        Point currentPoint = new Point();
        Item CurrItem;
        BitmapImage img;
        TextBox textBox = new TextBox();
        TextBox SetBox = new TextBox();
        private Point startPoint;
        private Rectangle rect;
        private Ellipse ellipse;
        int i = 1,flag = 0;
        public static Dictionary<string, UIElement> textlist = new Dictionary<string, UIElement> { };
        public static Dictionary<string, Point> pointlist = new Dictionary<string, Point> { };

        public ImageEditor(TextBox settext)
        {
            InitializeComponent();
            textlist = new Dictionary<string, UIElement> { };
            pointlist = new Dictionary<string, Point> { };
            i = 1;
            CurrItem = Item.None;
            SetBox = settext;
            LB_Message.Visibility = Visibility.Collapsed;
        }

        public enum Item
        {
            None, Brush, Pencil, Highlighter, Rectangle, Text, Ellipse, Move
        }

        //Canvas 點擊事件
        private void CanvasMouseDown(object sender, MouseButtonEventArgs e)
        {

            if (Back.Source == null) return;

            if (e.ButtonState == MouseButtonState.Pressed)
                currentPoint = e.GetPosition(Drawing);

            if (CurrItem == Item.Rectangle)
            {
                startPoint = e.GetPosition(Drawing);
                rect = new Rectangle
                {
                    Stroke = Brushes.Red,
                    StrokeThickness = 2
                };

                Canvas.SetLeft(rect, startPoint.X);
                Canvas.SetTop(rect, startPoint.Y);
                Drawing.Children.Add(rect);
            }
            else if (CurrItem == Item.Text)
            {
                startPoint = e.GetPosition(Drawing);

                textBox = new TextBox
                {
                    Name = "text" + i.ToString(),
                    Foreground = SetBox.Foreground,
                    FontFamily = SetBox.FontFamily,
                    FontSize = SetBox.FontSize,
                    FontWeight = SetBox.FontWeight,
                    FontStyle = SetBox.FontStyle,
                    TextDecorations = SetBox.TextDecorations,
                    BorderBrush = SetBox.BorderBrush,
                    Background = SetBox.Background,
                    BorderThickness = new Thickness(2)
                };

                textBox.GotFocus += Move_GotFocus;
                textlist.Add("text" + i.ToString(), textBox);
                pointlist.Add("text" + i.ToString(), startPoint);
                i += 1;

                Canvas.SetLeft(textBox, startPoint.X);
                Canvas.SetTop(textBox, startPoint.Y);
                Drawing.Children.Add(textBox);

            }
            else if (CurrItem == Item.Ellipse)
            {
                startPoint = e.GetPosition(Drawing);
                ellipse = new Ellipse
                {
                    Height = 5,
                    Width = 5,
                    Stroke = Brushes.Red,
                    StrokeThickness = 2
                };
                Canvas.SetLeft(ellipse, startPoint.X);
                Canvas.SetTop(ellipse, startPoint.Y);
                Drawing.Children.Add(ellipse);
            }

        }

        //Canvas 移動事件
        private void CanvasMouseMove(object sender, MouseEventArgs e)
        {
            if (Back.Source == null) return;

            if (CurrItem == Item.Move)
            {
                if (textBox != null)
                {

                    foreach (var point in pointlist)
                    {
                        if (((FrameworkElement)e.Source).Name == point.Key)
                        {
                            startPoint = point.Value;
                        }
                    }

                    if (flag == 1)
                    {
                        var pos = e.GetPosition(Drawing);
                        Canvas.SetLeft(textBox, pos.X);
                        Canvas.SetTop(textBox, pos.Y);
                    }
                }

            }

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (CurrItem == Item.Rectangle)
                {
                    if (e.LeftButton == MouseButtonState.Released || rect == null)
                        return;


                    var pos = e.GetPosition(Drawing);

                    var x = Math.Min(pos.X, startPoint.X);
                    var y = Math.Min(pos.Y, startPoint.Y);

                    var w = Math.Max(pos.X, startPoint.X) - x;
                    var h = Math.Max(pos.Y, startPoint.Y) - y;

                    rect.Width = w;
                    rect.Height = h;

                    Canvas.SetLeft(rect, x);
                    Canvas.SetTop(rect, y);
                }
                else if (CurrItem == Item.Ellipse)
                {
                    if (e.LeftButton == MouseButtonState.Released || ellipse == null)
                        return;

                    var pos = e.GetPosition(Drawing);

                    var x = Math.Min(pos.X, startPoint.X);
                    var y = Math.Min(pos.Y, startPoint.Y);

                    var w = Math.Max(pos.X, startPoint.X) - x;
                    var h = Math.Max(pos.Y, startPoint.Y) - y;

                    ellipse.Width = w;
                    ellipse.Height = h;

                    Canvas.SetLeft(ellipse, x);
                    Canvas.SetTop(ellipse, y);
                }
                else if (CurrItem == Item.Text)
                {
                    if (e.LeftButton == MouseButtonState.Released )
                        return;

                    foreach (var text in textlist)
                    {
                        if (((FrameworkElement)e.Source).Name == text.Key)
                        {
                            textBox = (TextBox)text.Value;
                        }
                    }

                    foreach (var point in pointlist)
                    {
                        if (((FrameworkElement)e.Source).Name == point.Key)
                        {
                            startPoint = point.Value;
                        }
                    }

                    var pos = e.GetPosition(Drawing);

                    var x = Math.Min(pos.X, startPoint.X);
                    var y = Math.Min(pos.Y, startPoint.Y);

                    var w = Math.Max(pos.X, startPoint.X) - x;
                    var h = Math.Max(pos.Y, startPoint.Y) - y;

                    textBox.Width = w;
                    textBox.Height = h;
                    Canvas.SetLeft(textBox, x);
                    Canvas.SetTop(textBox, y);
                }
                else
                {
                    Line line = new Line();
                    if (CurrItem == Item.Brush)
                    {
                        line.Stroke = Brushes.Red;
                        line.StrokeThickness = 4;
                    }
                    else if (CurrItem == Item.Pencil)
                    {
                        line.Stroke = SystemColors.WindowFrameBrush;
                    }
                    else if (CurrItem == Item.Highlighter)
                    {
                        line.Stroke = Brushes.Yellow;
                        line.StrokeThickness = 10;
                        line.Opacity = 0.2;
                    }
                    line.X1 = currentPoint.X;
                    line.Y1 = currentPoint.Y;
                    line.X2 = e.GetPosition(Drawing).X;
                    line.Y2 = e.GetPosition(Drawing).Y;
                    currentPoint = e.GetPosition(Drawing);
                    Drawing.Children.Add(line);
                }

            }
        }

        //Canvas 放開
        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            rect = null;
            ellipse = null;
            textBox = null;
            flag = 0;
            if (CurrItem == Item.Move)
            {
                Btn_Move.Focus();
            }

        }

        //取得Move時的Focus
        private void Move_GotFocus(object sender, RoutedEventArgs e)
        {
            if (CurrItem == Item.Move)
            {
                foreach (var text in textlist)
                {
                    if (((FrameworkElement)e.Source).Name == text.Key)
                    {
                        textBox = (TextBox)text.Value;
                        flag = 1;
                    }
                }
            }
        }

        //載入圖片
        private void Upload_Image(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog
            {
                Title = "選擇圖片",
                Filter = "All supported graphics|*.jpg;*.jpeg;*.png",
                Multiselect = true
            };
            if (op.ShowDialog() == true)
            {
                img = new BitmapImage(new Uri(op.FileName));
                double width = img.Width / 610;
                double height = img.Height / 680;

                double divisor;
                if (width <= height)
                {
                    divisor = height;
                }
                else
                {
                    divisor = width;
                }

                Drawing.Children.Clear();
                Drawing.Width = 610;
                Drawing.Height = 680;
                Back = new Image
                {
                    Width = (int)img.Width / divisor,
                    Height = (int)img.Height / divisor,
                    Source = img
                };
                Canvas.SetLeft(Back, (int)610 - Back.Width);
                Canvas.SetTop(Back, (int)(680 - Back.Height) / 2);
                Drawing.Children.Add(Back);
            }
        }
        
        //儲存功能
        private void Save_Image(object sender, RoutedEventArgs e)
        {

            SaveFileDialog dlg = new SaveFileDialog
            {
                FileName = "Document",
                Filter = "JPeg Image|*.jpg|Bitmap Image|*.bmp|Gif Image|*.gif"
            };
            if (dlg.ShowDialog() == true)
            {
                ExportToPng(new Uri(dlg.FileName));
            }
        }

        //清空繪畫功能
        public void Click_None()
        {
            Btn_Pencil.BorderThickness = new Thickness(0.5);
            Btn_Pencil.BorderBrush = Brushes.Black;
            Btn_Brush.BorderThickness = new Thickness(0.5);
            Btn_Brush.BorderBrush = Brushes.Black;
            Btn_Ellipse.BorderThickness = new Thickness(0.5);
            Btn_Ellipse.BorderBrush = Brushes.Black;
            Btn_Highlight.BorderThickness = new Thickness(0.5);
            Btn_Highlight.BorderBrush = Brushes.Black;
            Btn_Rectangle.BorderThickness = new Thickness(0.5);
            Btn_Rectangle.BorderBrush = Brushes.Black;
            Btn_Text.BorderThickness = new Thickness(0.5);
            Btn_Text.BorderBrush = Brushes.Black;
            Btn_Move.BorderThickness = new Thickness(0.5);
            Btn_Move.BorderBrush = Brushes.Black;
            LB_Message.Content = "";
        }

        //鉛筆功能
        private void Click_Pencil(object sender, RoutedEventArgs e)
        {
            if (Back.Source == null) return;
            Click_None();
            Btn_Pencil.BorderThickness = new Thickness(3);
            Btn_Pencil.BorderBrush = Brushes.Red;
            CurrItem = Item.Pencil;
            LB_Message.Content = "可於下方畫線寫字";
        }

        //筆刷功能
        private void Click_Brush(object sender, RoutedEventArgs e)
        {
            if (Back.Source == null) return;
            Click_None();
            Btn_Brush.BorderThickness = new Thickness(3);
            Btn_Brush.BorderBrush = Brushes.Red;
            CurrItem = Item.Brush;
            LB_Message.Content = "可於下方註解";
        }

        //圓形功能
        private void Click_Ellipse(object sender, RoutedEventArgs e)
        {
            if (Back.Source == null) return;
            Click_None();
            Btn_Ellipse.BorderThickness = new Thickness(3);
            Btn_Ellipse.BorderBrush = Brushes.Red;
            CurrItem = Item.Ellipse;
            LB_Message.Content = "可於下方畫圓";
        }

        //螢光筆功能
        private void Click_Highlight(object sender, RoutedEventArgs e)
        {
            if (Back.Source == null) return;
            Click_None();
            Btn_Highlight.BorderThickness = new Thickness(3);
            Btn_Highlight.BorderBrush = Brushes.Red;
            CurrItem = Item.Highlighter;
            LB_Message.Content = "可於下方螢光筆註解";
        }

        //距形功能
        private void Click_Rectangle(object sender, RoutedEventArgs e)
        {
            if (Back.Source == null) return;
            Click_None();
            Btn_Rectangle.BorderThickness = new Thickness(3);
            Btn_Rectangle.BorderBrush = Brushes.Red;
            CurrItem = Item.Rectangle;
            LB_Message.Content = "可於下方畫距形";
        }

        //文字輸入功能
        private void Click_Text(object sender, RoutedEventArgs e)
        {
            if (Back.Source == null) return;
            Click_None();
            Btn_Text.BorderThickness = new Thickness(3);
            Btn_Text.BorderBrush = Brushes.Red;
            CurrItem = Item.Text;
            LB_Message.Content = "可於下方新增文字框";
        }

        //清空功能
        private void Click_RemoveEdits(object sender, RoutedEventArgs e)
        {
            Back.Source = null;
            CurrItem = Item.None;
            Click_None();
            Drawing.Children.Clear();
        }

        //移動功能
        private void Btn_Move_Click(object sender, RoutedEventArgs e)
        {
            if (Back.Source == null) return;
            Click_None();
            Btn_Move.BorderThickness = new Thickness(3);
            Btn_Move.BorderBrush = Brushes.Red;
            CurrItem = Item.Move;
            LB_Message.Content = "左鍵點擊移動文字框";
        }

        //匯出成圖片
        public void ExportToPng(Uri path)
        {
            if (path == null) return;
            Canvas surface = Drawing;
            Transform transform = surface.LayoutTransform;
            surface.LayoutTransform = null;
            Size size = new Size(surface.Width, surface.Height);
            surface.Measure(size);
            surface.Arrange(new Rect(size));
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap((int)size.Width,(int)size.Height,96d,96d,PixelFormats.Pbgra32);
            renderBitmap.Render(surface);
            using (FileStream outStream = new FileStream(path.LocalPath, FileMode.Create))
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
                encoder.Save(outStream);
            }
            surface.LayoutTransform = transform;
        }

    }
}
