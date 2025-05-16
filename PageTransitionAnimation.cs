using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace unreal_GUI
{
    public static class PageTransitionAnimation
    {
        public static void ApplyTransition(ContentControl contentControl, UIElement newContent)
        {
            var fadeOut = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                Duration = TimeSpan.FromSeconds(0.15)
            };

            var fadeIn = new DoubleAnimation
            {
                From = 0.0,
                To = 1.0,
                Duration = TimeSpan.FromSeconds(0.15)
            };

            fadeOut.Completed += (s, _) =>
            {
                contentControl.Content = newContent;
                contentControl.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            };

            contentControl.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }

        
    }
}