using Regin_New.Classes;
using Regin_New.Pages;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Regin_New;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public static MainWindow mainWindow;
    public User UserLogIn = new User();

    public MainWindow()
    {
        InitializeComponent();
        mainWindow = this;
        OpenPage(new Login());
    }

    public void OpenPage(Page page)
    {
        AnimatePageTransition(page);
    }

    private void AnimatePageTransition(Page page)
    {
        AnimateOpacity(1, 0, 0.6, () =>
        {
            frame.Navigate(page);
            AnimateOpacity(0, 1, 1.2, null);
        });
    }

    private void AnimateOpacity(double from, double to, double seconds, AnimationCompletedCallback callback)
    {
        var animation = new DoubleAnimation
        {
            From = from,
            To = to,
            Duration = TimeSpan.FromSeconds(seconds)
        };

        if (callback != null)
        {
            animation.Completed += (s, e) => callback();
        }

        frame.BeginAnimation(Frame.OpacityProperty, animation);
    }

    private delegate void AnimationCompletedCallback();
}