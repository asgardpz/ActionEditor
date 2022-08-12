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
using System.Threading;
using PdfiumViewer;

namespace ImageEditorProject
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        //定義一個集合，存儲客戶端信息
        public static Dictionary<string, ImageEditor> clientConnectionItems = new Dictionary<string, ImageEditor> { };
        public static Dictionary<string, TextBox> textlist = new Dictionary<string, TextBox> { };
        public static Dictionary<string, MemoryStream> memorylist = new Dictionary<string, MemoryStream> { };
        public static Dictionary<string, Image> imagelist = new Dictionary<string, Image> { };
        public int i = 1;
        public ImageSource ImgSource { get; set; }
        public string[] oldfile;
        public TextBox textset;
        public System.Windows.Forms.FontDialog fontDialog = new System.Windows.Forms.FontDialog
        {
            ShowColor = true
        };
        public System.Windows.Forms.ColorDialog FrameDialog = new System.Windows.Forms.ColorDialog
        {
            AllowFullOpen = true,
            FullOpen = true,
            ShowHelp = true
        };
        public System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog
        {
            AllowFullOpen = true,
            FullOpen = true,
            ShowHelp = true
        };
        public MainWindow()
        {
            InitializeComponent();
            Reset();
        }

        //選擇項目
        private void CanvasList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var Station in clientConnectionItems)
            {
                if (Station.Key == ((FrameworkElement)CanvasList.SelectedItem).Name)
                {
                    container.Children.Clear();
                    Canvas.SetLeft(Station.Value, 0);
                    Canvas.SetTop(Station.Value, 0);
                    container.Children.Add(Station.Value);
                }
            }
        }

        //載入圖片
        private void Upload_Image(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog
            {
                Title = "選擇檔案",
                Filter = "Image files|*.jpg;*.jpeg|" +
                         "Office files|*.xls;*.ppt;*.doc;*.xlsx;*.pptx;*.docx|" +
                         "PDF files|*.pdf",
                Multiselect = true,
            };
            if (op.ShowDialog() == true)
            {
                Reset();
                CanvasList.Items.Clear();
                Get_path(op.FileNames);
            }
        }

        //拖曳載入圖片
        private void Drawing_DragEnter(object sender, DragEventArgs e)
        {
            try
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    Reset();
                    CanvasList.Items.Clear();
                    var pathlist = e.Data.GetData(DataFormats.FileDrop) as string[];
                    Get_path(pathlist);
                }
            }
            catch
            {

            }
            finally
            {

            }

        }

        //圖片清單
        public void Get_path(string[] pathlist)
        {
            try
            {
                foreach (var Station in pathlist)
                {
                    if (Station.Contains("jpg") == false && Station.Contains("jpeg") == false)
                    {

                        //ppt轉圖 office版
                        if (Station.Contains(".ppt") == true)
                        {
                            var app = new Microsoft.Office.Interop.PowerPoint.Application();
                            var ppt = app.Presentations.Open(Station, Microsoft.Office.Core.MsoTriState.msoFalse, Microsoft.Office.Core.MsoTriState.msoFalse, Microsoft.Office.Core.MsoTriState.msoFalse);
                            var index = 0;
                            foreach (Microsoft.Office.Interop.PowerPoint.Slide slid in ppt.Slides)
                            {
                                ++index;
                                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "ppt" + index.ToString() + ".jpg") != true)
                                {
                                    slid.Export(AppDomain.CurrentDomain.BaseDirectory + string.Format("ppt{0}.jpg", index.ToString()), "jpg", 610, 680);
                                }
                                Set_Cavnas(AppDomain.CurrentDomain.BaseDirectory + "ppt" + index.ToString() + ".jpg");
                                //刪除轉換後的圖
                                string[] files = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "ppt*.jpg");
                                foreach (var file in files)
                                {
                                    File.Delete(file);
                                }
                            }

                            //釋放資源
                            ppt.Close();
                            app.Quit();
                            GC.Collect();
                        }

                        //excel轉圖 office版
                        if (Station.Contains(".xls") == true)
                        {
                            //station--> excel file path
                            var app = new Microsoft.Office.Interop.Excel.Application();
                            var xls = app.Workbooks.Open(Station, false, true);
                            Microsoft.Office.Interop.Excel.Sheets sheets = xls.Worksheets;
                            for (int j = 1; j <= sheets.Count; j++)
                            {
                                Microsoft.Office.Interop.Excel.Worksheet sheet = sheets[j];
                                string startRange = "A1";
                                Microsoft.Office.Interop.Excel.Range endRange = sheet.Cells.SpecialCells(Microsoft.Office.Interop.Excel.XlCellType.xlCellTypeLastCell, Type.Missing);
                                Microsoft.Office.Interop.Excel.Range range = sheet.get_Range(startRange, endRange);
                                range.Rows.AutoFit();
                                range.Columns.AutoFit();
                                range.Copy();

                                BitmapSource image = Clipboard.GetImage();
                                FormatConvertedBitmap fcbitmap = new FormatConvertedBitmap(image, PixelFormats.Bgr32, null, 0);
                                using (var fileStream = new FileStream(AppDomain.CurrentDomain.BaseDirectory + j + ".jpg", FileMode.Create))
                                {
                                    PngBitmapEncoder encoder = new PngBitmapEncoder
                                    {
                                        Interlace = PngInterlaceOption.On
                                    };
                                    encoder.Frames.Add(BitmapFrame.Create(fcbitmap));
                                    encoder.Save(fileStream);
                                }
                                Set_Cavnas(AppDomain.CurrentDomain.BaseDirectory + j + ".jpg");
                                File.Delete(AppDomain.CurrentDomain.BaseDirectory + j + ".jpg");
                                Clipboard.Clear();
                            }
                            //釋放資源
                            app.DisplayAlerts = false;
                            xls.Close(false);
                            app.Quit();
                            GC.Collect();

                        }

                        //pdf轉圖 
                        if (Station.Contains(".pdf") == true)
                        {
                            //PdfiumViewer
                            PdfDocument pdfDoc;
                            pdfDoc = PdfDocument.Load(Station);
                            for (int count = 0; count < pdfDoc.PageCount; count++)
                            {
                                var image = pdfDoc.Render(count, 610, 680, 96, 96, false);
                                image.Save(AppDomain.CurrentDomain.BaseDirectory + "1.jpg");
                                Set_Cavnas(AppDomain.CurrentDomain.BaseDirectory + "1.jpg");
                            }
                            File.Delete(AppDomain.CurrentDomain.BaseDirectory + "1.jpg");
                            pdfDoc.Dispose();
                            GC.Collect();
                        }

                        //Word轉圖 office版轉pdf 
                        if (Station.Contains(".doc") == true)
                        {
                            var app = new Microsoft.Office.Interop.Word.Application();
                            var doc = app.Documents.Open(Station, false, true);
                            doc.ExportAsFixedFormat(AppDomain.CurrentDomain.BaseDirectory + "doc.pdf", Microsoft.Office.Interop.Word.WdExportFormat.wdExportFormatPDF);
                            //釋放資源
                            doc.Close(false, false, false);
                            app.Quit();
                            GC.Collect();

                            //PdfiumViewer
                            PdfDocument pdfDoc;
                            pdfDoc = PdfDocument.Load(AppDomain.CurrentDomain.BaseDirectory + "doc.pdf");
                            for (int count = 0; count < pdfDoc.PageCount; count++)
                            {
                                var image = pdfDoc.Render(count, 610, 680, 96, 96, false);
                                image.Save(AppDomain.CurrentDomain.BaseDirectory + "1.jpg");
                                Set_Cavnas(AppDomain.CurrentDomain.BaseDirectory + "1.jpg");
                            }
                            File.Delete(AppDomain.CurrentDomain.BaseDirectory + "1.jpg");
                            pdfDoc.Dispose();
                            File.Delete(AppDomain.CurrentDomain.BaseDirectory + "doc.pdf");
                            GC.Collect();
                        }
                    }
                    else
                    {
                        Set_Cavnas(Station);
                    };
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.ToString(), "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            finally
            {

            }


        }

        //儲存圖片
        private void Save_Image(object sender, RoutedEventArgs e)
        {
            if (clientConnectionItems.Count == 0)
            {
                MessageBox.Show(this, "空白不需匯出", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "請選擇匯出資料夾"
            };
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (string.IsNullOrEmpty(dialog.SelectedPath))
                {
                    MessageBox.Show(this, "資料夾路徑不能為空", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                foreach (var Station in clientConnectionItems)
                {
                    string path = Station.Key;
                    foreach (var text in textlist)
                    {
                        if (Station.Key == text.Key)
                        {
                            path = text.Value.Text;
                        }
                    }
                    Station.Value.ExportToPng(new Uri(dialog.SelectedPath + "\\" + path + ".jpg"));
                }
                MessageBox.Show(this, "已匯出", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        //清空按鈕
        private void Click_RemoveEdits(object sender, RoutedEventArgs e)
        {
            Reset();
        }

        //清空
        public void Reset()
        {
            //清空記憶體
            if (memorylist != null)
            {
                foreach (var memory in memorylist)
                {
                    memory.Value.Dispose();
                }
                memorylist = new Dictionary<string, MemoryStream> { };

                foreach (var image in imagelist)
                {
                    image.Value.Source = null;
                }
                imagelist = new Dictionary<string, Image> { };

            }
            GC.Collect();
            clientConnectionItems = new Dictionary<string, ImageEditor> { };
            textlist = new Dictionary<string, TextBox> { };
            CanvasList.Items.Clear();
            container.Children.Clear();
            GroupBox gb = new GroupBox
            {
                Name = "GB_Title",
                Header = "拖曳檔案至此",
                Height = 670,
                Width = 130,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(2),
                AllowDrop = true
            };
            CanvasList.Items.Add(gb);
            CanvasList.AllowDrop = true;
            i = 1;
        }

        //設定畫布
        public void Set_Cavnas(string file)
        {
            int flag = 0;
            //清空記憶體
            if (memorylist != null)
            {
                foreach (var memory in memorylist)
                {
                    if (memory.Key == "Work" + i.ToString())
                    {
                        flag += 1;
                    }
                }
            }
            if (flag > 0)
            {
                return;
            }

            TextBox textBox = new TextBox
            {
                Name = "Work" + i.ToString(),
                Text = "Work" + i.ToString(),
                Width = 61 * 2
            };

            BitmapImage img;

            byte[] imageData;

            using (var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                imageData = binaryReader.ReadBytes((int)fileStream.Length);
            }

            MemoryStream ms = new MemoryStream(imageData);
            memorylist.Add("Work" + i.ToString(), ms);
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = ms;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            img = bitmap;

            double width = img.Width / 610;
            double height = img.Height / 680;
            Image Back = new Image
            {
                Width = 61 * 2,
                Height = 68 * 2,
                Source = img,
                Name = "Work" + i.ToString()
            };
            //檔名
            CanvasList.Items.Add(textBox);
            textlist.Add("Work" + i.ToString(), textBox);

            //縮圖
            CanvasList.Items.Add(Back);
            imagelist.Add("Work" + i.ToString(), Back);

            ImageEditor rowItem = new ImageEditor(colorBtn);

            double divisor;
            if (width <= height)
            {
                divisor = height;
            }
            else
            {
                divisor = width;
            }

            rowItem.Drawing.Children.Clear();
            rowItem.Drawing.Width = 610;
            rowItem.Drawing.Height = 680;
            Back = new Image
            {
                Width = (int)img.Width / divisor,
                Height = (int)img.Height / divisor,
                Source = img
            };
            Canvas.SetLeft(Back, (int)(610 - Back.Width) / 2);
            Canvas.SetTop(Back, (int)(680 - Back.Height) / 2);
            rowItem.Back.Source = img;
            rowItem.Drawing.Children.Add(Back);

            container.Children.Clear();
            Canvas.SetLeft(rowItem, 0);
            Canvas.SetTop(rowItem, 0);
            container.Children.Add(rowItem);
            clientConnectionItems.Add("Work" + i.ToString(), rowItem);

            i += 1;

            foreach (var Station in clientConnectionItems)
            {
                if (Station.Key == "Work1")
                {
                    container.Children.Clear();
                    Canvas.SetLeft(Station.Value, 0);
                    Canvas.SetTop(Station.Value, 0);
                    container.Children.Add(Station.Value);
                }
            }

            foreach (var text in textlist)
            {
                if (text.Key == "Work1")
                {
                    text.Value.Focus();
                }
            }
        }


        //字型變更
        private void Btn_Font_Click(object sender, RoutedEventArgs e)
        {
            var result = fontDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string strColor = System.Drawing.ColorTranslator.ToHtml(fontDialog.Color);
                BrushConverter brushConverter = new BrushConverter();
                Brush brush = (Brush)brushConverter.ConvertFromString(strColor);
                colorBtn.Foreground = brush;
                colorBtn.FontFamily = new FontFamily(fontDialog.Font.Name);
                colorBtn.FontSize = fontDialog.Font.Size * 96.0 / 72.0;
                colorBtn.FontWeight = fontDialog.Font.Bold ? FontWeights.Bold : FontWeights.Regular;
                colorBtn.FontStyle = fontDialog.Font.Italic ? FontStyles.Italic : FontStyles.Normal;

                TextDecorationCollection tdc = new TextDecorationCollection();
                if (fontDialog.Font.Underline) tdc.Add(TextDecorations.Underline);
                if (fontDialog.Font.Strikeout) tdc.Add(TextDecorations.Strikethrough);
                colorBtn.TextDecorations = tdc;
            }

        }

        //框線色彩變更
        private void Btn_Frame_Click(object sender, RoutedEventArgs e)
        {
            var result = FrameDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string strColor = System.Drawing.ColorTranslator.ToHtml(FrameDialog.Color);
                BrushConverter brushConverter = new BrushConverter();
                Brush brush = (Brush)brushConverter.ConvertFromString(strColor);
                colorBtn.BorderBrush = brush;
            }

        }

        //背景色彩變更
        private void Btn_Color_Click(object sender, RoutedEventArgs e)
        {
            var result = colorDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string strColor = System.Drawing.ColorTranslator.ToHtml(colorDialog.Color);
                BrushConverter brushConverter = new BrushConverter();
                Brush brush = (Brush)brushConverter.ConvertFromString(strColor);
                colorBtn.Background = brush;
            }

        }
    }
}
