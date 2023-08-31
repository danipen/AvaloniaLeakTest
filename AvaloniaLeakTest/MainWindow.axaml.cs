using Avalonia.Controls;

using System;

namespace AvaloniaLeakTest
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            mWindowMemLeakTest.Click += (sender, args) =>
            {
                MemoryLeakTest.Arrange(
                    () => new Window(),
                    out WeakReference<Window> reference);

                // Act
                MemoryLeakTest.Act(reference);

                // Assert
                bool isMemoryLeaked = MemoryLeakTest.IsMemoryLeaked(reference);

                mTextBlock.Text = isMemoryLeaked ? "Window test leaked memory!" : "Window test passed OK";
            };

            mPanelMemLeakTest.Click += (sender, args) =>
            {
                MemoryLeakTest.Arrange(
                    () => new Panel(),
                    out WeakReference<Panel> reference);

                // Act
                MemoryLeakTest.Act(reference);

                // Assert
                bool isMemoryLeaked = MemoryLeakTest.IsMemoryLeaked(reference);

                mTextBlock.Text = isMemoryLeaked ? "Panel test leaked memory!" : "Panel test passed OK";
            };
        }
    }
}