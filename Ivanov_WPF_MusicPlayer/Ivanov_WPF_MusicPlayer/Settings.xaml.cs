using System.Windows;

namespace Ivanov_WPF_MusicPlayer
{
    public delegate void VisualizerDelegate(string name);

    public partial class Settings : Window
    {
        public event VisualizerDelegate PerformVisualizer;

        public Settings()
        {
            InitializeComponent();
            visualizerCB.Items.Add("Bar");
            visualizerCB.Items.Add("PeakBar");
            visualizerCB.Items.Add("Lines");
            visualizerCB.Items.Add("Wave");
            visualizerCB.Items.Add("Dot");
            visualizerCB.Items.Add("Bean");
            visualizerCB.Items.Add("Eclipse");
            visualizerCB.Items.Add("WaveForm");
            visualizerCB.Items.Add("SpectrumText");
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            PerformVisualizer?.Invoke(visualizerCB.Text);
            this.Close();
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
