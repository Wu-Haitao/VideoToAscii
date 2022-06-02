using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Microsoft.Win32;
using OpenCvSharp;
using Window = System.Windows.Window;
using Size = OpenCvSharp.Size;
using System.Diagnostics;

namespace VideoToAscii
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Init();
        }

        private void Init()
        {
            Title = "VideoToAscii";
            ForegroundColor.ItemsSource = Enum.GetValues(typeof(ConsoleColor));
            ForegroundColor.SelectedIndex = ForegroundColor.Items.Count - 1;
            BackgroundColor.ItemsSource = Enum.GetValues(typeof(ConsoleColor));
            BackgroundColor.SelectedIndex = 0;
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Select file";
            dialog.Multiselect = false;

            if ((bool)dialog.ShowDialog())
            {
                FilePath.Content = dialog.FileName;
            }
        }
        private void ForegroundColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Console.ForegroundColor = (ConsoleColor)ForegroundColor.SelectedIndex;
        }
        private void BackgroundColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Console.BackgroundColor = (ConsoleColor)BackgroundColor.SelectedIndex;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            string filePath, space, ascii;
            int interval;
            float speed, scaleX, scaleY;
            try
            {
                filePath = FilePath.Content.ToString();
                interval = int.Parse(Interval.Text);
                speed = float.Parse(Speed.Text);
                scaleX = float.Parse(ScaleX.Text);
                scaleY = float.Parse(ScaleY.Text);
                ascii = Ascii.Text;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Parameter error!");
                return;
            }

            ConvertVideoToAscii(filePath, interval, speed, scaleX, scaleY, ascii);
        }


        Mat image;
        VideoCapture vc;

        private void ConvertVideoToAscii(string filePath, int interval, float speed, float scaleX, float scaleY, string ascii)
        {
           
            if (vc != null) vc.Release();
            if (image != null) image.Release();
            vc = new VideoCapture(filePath);
            image = new Mat();

            Console.Clear();
            int count = 0;
            double fps = vc.Get(VideoCaptureProperties.Fps);
            string line = "";

            while (vc.IsOpened())
            {
                if (!vc.Read(image)) break; //If no more frames then break the loop
                var sw = Stopwatch.StartNew();
                
                count++;
                if (count >= interval)
                {
                    count = 0;

                    Cv2.Resize(image, image, new Size(vc.FrameWidth / scaleX, vc.FrameHeight / scaleY));
                    Cv2.CvtColor(image, image, ColorConversionCodes.RGB2GRAY);

                    Console.SetCursorPosition(0, 0);
                    if ((image.Width + 1 <= Console.LargestWindowWidth) && (image.Height + 1 <= Console.LargestWindowHeight))
                    {
                        Console.SetWindowSize(image.Width + 1, image.Height + 1);
                        Console.SetBufferSize(Console.WindowWidth, Console.WindowHeight);
                    }

                    for (int i = 0; i < image.Height; i++)
                    {
                        line = "";
                        for (int j = 0; j < image.Width; j++)
                        {
                            int pixel = (int)image.At<byte>(i, j);
                            line += ascii[pixel * ascii.Length / 256];
                        }
                        Console.WriteLine(line);
                    }

                    float usage = (float)(sw.ElapsedMilliseconds / (1000 / fps / speed * interval));
                    line = "";
                    for (int i = 0; i < image.Width; i++) line += (i < image.Width * usage) ? '-' : ' ';
                    Console.Write(line);
                }
                sw.Stop();
                Cv2.WaitKey((int)(1000 / fps / speed) - (int)sw.ElapsedMilliseconds);
            }

            vc.Release();
            image.Release();
        }
    }
}
