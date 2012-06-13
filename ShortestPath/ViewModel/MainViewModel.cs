using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Collections.ObjectModel;
using ShortestPath.Model;
using System.Windows.Input;
using System.Windows.Threading;
using RoadSystemLib;
using System.IO;

namespace ShortestPath.ViewModel
{
    /// <summary>
    /// Main Window view-model
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged
    {
        #region Properties, Events and Memebers
        /// <summary>
        /// PropertyChanged event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        private ObservableCollection<ResultItem> _resultItems = new ObservableCollection<ResultItem>();
        
        /// <summary>
        /// Dispatcher object which provide function to execute code on UI Thread
        /// </summary>
        private DispatcherObject _dispatcher;         

        /// <summary>
        /// Collection for storing Result from Shortest Path finder
        /// </summary>
        public ObservableCollection<ResultItem> ResultItems
        {
            get { return _resultItems; }
            set
            {
                if (_resultItems != value)
                {
                    _resultItems = value;

                    // Raise Property Changed event
                    RaisePropertyChanged(() => this.ResultItems);
                }
            }
        }
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dispatcher">Dispatcher object</param>
        public MainViewModel(DispatcherObject dispatcher)
        {            
            _dispatcher = dispatcher;
        }

        /// <summary>
        /// Asynchronously Load all Files, find shortest paths and put the result into ResultItems 
        /// </summary>
        /// <param name="files"></param>
        /// <param name="uiBeforeCallback"></param>
        /// <param name="uiAfterCallback"></param>
        /// <returns>IAsyncResult</returns>
        public IAsyncResult LoadFilesAsync( IEnumerable<string> files, 
            Action<object> uiBeforeCallback = null, 
            Action<object> uiAfterCallback = null)
        {
            ResultItems.Clear();
            // Create Delegate to LoadFiles function
            Func<IEnumerable<string>, Action<object>, Action<object>, IEnumerable<ResultItem>> loadFunc =
                new Func<IEnumerable<string>, Action<object>, Action<object>, IEnumerable<ResultItem>>(LoadFiles);

            // Asnchronously invoke the delegate
            return loadFunc.BeginInvoke(files, uiBeforeCallback, uiAfterCallback, new AsyncCallback(LoadFilesCallback), loadFunc);
        }

        /// <summary>
        /// Raise Property Changed evnet
        /// </summary>
        /// <param name="expression"></param>
        protected void RaisePropertyChanged(Expression<Func<object>> expression)
        {
            // Get Lambda Expression
            LambdaExpression lambdaExp = expression as LambdaExpression;

            // Retrieve MemberExpression and Raise PropertyChanged event with Memeber name
            if (lambdaExp != null)
            {
                var propertyName = string.Empty;

                MemberExpression memberExp = lambdaExp.Body as MemberExpression;
                if (memberExp != null) propertyName = memberExp.Member.Name;

                if (propertyName != string.Empty)
                {
                    PropertyChangedEventArgs e = new PropertyChangedEventArgs(propertyName);                    
                    if (PropertyChanged != null) PropertyChanged(this, e);
                }
            }
        }

        /// <summary>
        /// Async callback of Load file for populating the result into ResultItems collection
        /// </summary>
        /// <param name="ar"></param>
        private void LoadFilesCallback(IAsyncResult ar)
        {
            // Get LoadFiles result
            var result = ((Func<IEnumerable<string>, Action<object>, Action<object>, IEnumerable<ResultItem>>)ar.AsyncState)
                         .EndInvoke(ar);

            // Add ResultItems to the ResultItems collection on Dispatcher's Thread
            _dispatcher.Dispatcher.Invoke(new Action<IEnumerable<ResultItem>>(resultSet =>
            {
                foreach (var item in resultSet)
                {
                    this.ResultItems.Add(item);
                }
            }), new object[] { result });
        }

        /// <summary>
        /// Load all files
        /// Call uiBeforeCallback before loading files and uiAfterCallback after loading files
        /// </summary>
        /// <param name="files"></param>
        /// <param name="uiBeforeCallback"></param>
        /// <param name="uiAfterCallback"></param>
        /// <returns></returns>
        private IEnumerable<ResultItem> LoadFiles(IEnumerable<string> files,
            Action<object> uiBeforeCallback = null,
            Action<object> uiAfterCallback = null)
        {
            // Invoke UI before callback on Dispatcher's Thread
            if (uiBeforeCallback != null)
                _dispatcher.Dispatcher.BeginInvoke(uiBeforeCallback, new object[] { files });

            // Create shortest path finder
            IShortestPathFinder finder = new DijkstraShortestPathFinder();

            // Iterate through the files, process them and put the result into result collection
            List<ResultItem> result = new List<ResultItem>();
            if (files != null)
            {
                foreach (string file in files)
                {
                    result.Add(ProcessFile(file, finder));
                }
            }

            // Invoke UI after callback on Dispatcher's Thread
            if (uiAfterCallback != null)
                _dispatcher.Dispatcher.BeginInvoke(uiAfterCallback, new object[] { files });

            // Return the result for processing by the Async callback
            return result.AsEnumerable();
        }

        /// <summary>
        /// Process the files, load the RoadSystem then find the shortest path and put the result into ResultItem
        /// If there is any error then the ResultItem.HasError will be true and ResultItem.Result will be error message
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="finder"></param>
        /// <returns></returns>
        private ResultItem ProcessFile(string filePath, IShortestPathFinder finder)
        {
            ResultItem item = null;           
            string filename = Path.GetFileName(filePath);
            try
            {
                // Load RoadSystem 
                RoadSystem system = RoadSystem.LoadFromXml(filePath);

                // Find the shortest path
                var result = finder.FindShortestPath(system);

                // Create ResultItem from the result
                if (result.Count() == 0)
                {
                    item = new ResultItem
                    {
                        Filename = filename,
                        Result = "No path found"
                    };
                }
                else
                {
                    item = new ResultItem
                    {
                        Filename = filename,
                        Result = String.Join(", ", result.Select(node => node.ID.ToString()).ToArray())
                    };
                }
            }
            catch (LoadRoadSystemException ex)
            {
                // Create ResultItem from error
                item = new ResultItem
                {
                    Filename = filename,
                    HasError = true,
                    Result = ex.Message
                };
            }

            // Return ResultItem
            return item;
        }
    }
}
