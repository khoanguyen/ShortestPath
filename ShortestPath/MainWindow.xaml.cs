using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using ShortestPath.ViewModel;
using Microsoft.Win32;

namespace ShortestPath
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Loading files progress dialog
        /// </summary>
        private LoadDialog _loadDialog;

        /// <summary>
        /// Window's view-model
        /// </summary>
        private MainViewModel _viewModel;

        /// <summary>
        /// Constructor
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            
            // Initialize the view-model
            _viewModel = new MainViewModel(this);
            this.DataContext = _viewModel;
                       
        }

        /// <summary>
        /// Browse Files button hanlder
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _browseFiles_Click(object sender, RoutedEventArgs e)
        {
            // Open OpenFiles dialog for browsing .xml files
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "*.xml|*xml";

            // Load chosen files
            if (openFileDialog.ShowDialog(this) == true) LoadFiles(openFileDialog.FileNames);
        }

        /// <summary>
        /// Load From Working Dir button handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _loadFromWorkingDir_Click(object sender, RoutedEventArgs e)
        {
            // Get all .xml files from working directory
            var files = Directory.EnumerateFiles(".", "*.xml", SearchOption.TopDirectoryOnly);

            // Load all files
            LoadFiles(files);
        }

        /// <summary>
        /// Window Loaded event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _loadDialog = new LoadDialog() { Owner = this }; 
        }

        /// <summary>
        /// Load givens file and find shortest paths
        /// </summary>
        /// <param name="files"></param>
        private void LoadFiles(IEnumerable<string> files)
        {
            // Invoke LoadFilesAsync method of View-Model
            // with callbacks for show/hide _loadingDialog
            _viewModel.LoadFilesAsync(files,
                new Action<object>(param => this._loadDialog.ShowDialog()),
                new Action<object>(param => this._loadDialog.Hide()));
        }
    }
}
