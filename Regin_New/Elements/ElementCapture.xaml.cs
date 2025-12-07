using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Regin_New.Elements
{
    /// <summary>
    /// Логика взаимодействия для ElementCapture.xaml
    /// </summary>
    public partial class ElementCapture : UserControl
    {
        public CorrectCapture HandlerCorrectCapture;
        public delegate void CorrectCapture();
        string StrCapture = "";
        int ElementWidth = 280;
        int ElementHeight = 50;
        private readonly Random random = new Random();

        public ElementCapture()
        {
            InitializeComponent();
            CreateCapture();
        }

        public void CreateCapture()
        {
            InputCapture.Text = "";
            Capture.Children.Clear();
            StrCapture = "";
            CreateBackground();
            Background();
        }

        void CreateBackground()
        {
            for (int i = 0; i < 100; i++)
            {
                Capture.Children.Add(CreateLabel(
                    random.Next(0, 10).ToString(),
                    random.Next(10, 16),
                    100,
                    random.Next(0, ElementWidth - 20),
                    random.Next(0, ElementHeight - 20)
                ));
            }
        }

        void Background()
        {
            var codeBuilder = new StringBuilder();

            for (int i = 0; i < 4; i++)
            {
                int digit = random.Next(0, 10);
                codeBuilder.Append(digit);

                Capture.Children.Add(CreateLabel(
                    digit.ToString(),
                    30,
                    255,
                    ElementWidth / 2 - 60 + i * 30,
                    random.Next(-10, 10)
                ));
            }

            StrCapture = codeBuilder.ToString();
        }

        private Label CreateLabel(string content, int fontSize, byte opacity, int marginLeft, int marginTop)
        {
            return new Label()
            {
                Content = content,
                FontSize = fontSize,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(GetRandomColor(opacity)),
                Margin = new Thickness(marginLeft, marginTop, 0, 0)
            };
        }

        private Color GetRandomColor(byte opacity)
        {
            return Color.FromArgb(
                opacity,
                (byte)random.Next(0, 256),
                (byte)random.Next(0, 256),
                (byte)random.Next(0, 256)
            );
        }

        private bool IsCaptchaValid() => StrCapture == InputCapture.Text;

        private void EnterCapture(object sender, KeyEventArgs e)
        {
            if (InputCapture.Text.Length != 4) return;

            if (!IsCaptchaValid())
            {
                CreateCapture();
            }
            else
            {
                HandlerCorrectCapture?.Invoke();
            }
        }
    }
}
