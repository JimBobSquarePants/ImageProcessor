namespace ImageProcessor.Web.Caching
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;
    using System.Runtime;
    using System.Runtime.Caching;
    using System.Runtime.Caching.Hosting;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;

    /// <summary>
    ///     Monitors directories and file paths and notifies the cache of changes to the monitored items. This class cannot be
    ///     inherited.
    /// </summary>
    public sealed class HostFileChangeMonitor2 : FileChangeMonitor
    {
        private const int MAX_CHAR_COUNT_OF_LONG_CONVERTED_TO_HEXADECIMAL_STRING = 16;

        private static IFileChangeNotificationSystem s_fcn;

        private readonly ReadOnlyCollection<string> _filePaths;

        private object _fcnState;

        private DateTimeOffset _lastModified;

        private string _uniqueId;

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:System.Runtime.Caching.HostFileChangeMonitor" /> class.
        /// </summary>
        /// <param name="filePaths">A list that contains one or more directory paths or file paths to monitor. </param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="filePaths" /> is null. </exception>
        /// <exception cref="T:System.ArgumentException">
        ///     <paramref name="filePaths" /> contains zero items. -or-A path in the
        ///     <paramref name="filePaths" /> list is null or an empty string.
        /// </exception>
        public HostFileChangeMonitor2(IList<string> filePaths)
        {
            if (filePaths == null)
            {
                throw new ArgumentNullException("filePaths");
            }

            if (filePaths.Count == 0)
            {
                throw new ArgumentException("filePaths");
            }

            _filePaths = SanitizeFilePathsList(filePaths);
            InitFCN();
            InitDisposableMembers();
        }

        private HostFileChangeMonitor2()
        {
        }

        /// <summary>
        ///     Gets the collection of directories and file paths that was passed to the
        ///     <see cref="M:System.Runtime.Caching.HostFileChangeMonitor.#ctor(System.Collections.Generic.IList{System.String})" />
        ///     constructor.
        /// </summary>
        /// <returns>
        ///     A collection of directories and file paths.
        /// </returns>
        public override ReadOnlyCollection<string> FilePaths
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return _filePaths;
            }
        }

        /// <summary>
        ///     Gets a read-only value that indicates the last write time of a monitored file or path.
        /// </summary>
        /// <returns>
        ///     The last write time of a monitored file or path.
        /// </returns>
        public override DateTimeOffset LastModified
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return _lastModified;
            }
        }

        /// <summary>
        ///     Gets an identifier for the <see cref="T:System.Runtime.Caching.HostFileChangeMonitor" /> instance that is based on
        ///     the set of monitored directories and file paths.
        /// </summary>
        /// <returns>
        ///     An identifier for the change monitor.
        /// </returns>
        public override string UniqueId
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return _uniqueId;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposing || s_fcn == null
                || (_filePaths == null || _fcnState == null))
            {
                return;
            }

            if (_filePaths.Count > 1)
            {
                var hashtable = _fcnState as Hashtable;
                foreach (var filePath in _filePaths)
                {
                    if (filePath != null)
                    {
                        var state = hashtable[filePath];
                        if (state != null)
                        {
                            s_fcn.StopMonitoring(filePath, state);
                        }
                    }
                }
            }
            else
            {
                var filePath = _filePaths[0];
                if (filePath == null || _fcnState == null)
                {
                    return;
                }

                s_fcn.StopMonitoring(filePath, _fcnState);
            }
        }

        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        private static void InitFCN()
        {
            if (s_fcn != null)
            {
                return;
            }

            IFileChangeNotificationSystem notificationSystem = null;
            //var host = ObjectCache.Host;
            //if (host != null)
            //{
            //    notificationSystem = host.GetService(typeof(IFileChangeNotificationSystem)) as IFileChangeNotificationSystem;
            //}

            if (notificationSystem == null)
            {
                notificationSystem = new FileChangeNotificationSystem();
            }

            Interlocked.CompareExchange(ref s_fcn, notificationSystem, null);
        }

        [SecuritySafeCritical]
        private static ReadOnlyCollection<string> SanitizeFilePathsList(IList<string> filePaths)
        {
            var list = new List<string>(filePaths.Count);
            foreach (var path in filePaths)
            {
                if (string.IsNullOrEmpty(path))
                {
                    throw new ArgumentException("filePaths");
                }

                new FileIOPermission(FileIOPermissionAccess.PathDiscovery, path).Demand();
                list.Add(path);
            }

            return list.AsReadOnly();
        }

        private void InitDisposableMembers()
        {
            var flag = true;
            try
            {
                string str1;
                if (_filePaths.Count == 1)
                {
                    var filePath = _filePaths[0];
                    DateTimeOffset lastWriteTime;
                    long fileSize;
                    s_fcn.StartMonitoring(
                        filePath, 
                        OnChanged, 
                        out _fcnState, 
                        out lastWriteTime, 
                        out fileSize);
                    str1 = filePath + lastWriteTime.UtcDateTime.Ticks.ToString("X", CultureInfo.InvariantCulture)
                           + fileSize.ToString("X", CultureInfo.InvariantCulture);
                    _lastModified = lastWriteTime;
                }
                else
                {
                    var capacity = 0;
                    foreach (var str2 in _filePaths)
                    {
                        capacity += str2.Length + 32;
                    }

                    var hashtable = new Hashtable(_filePaths.Count);
                    _fcnState = hashtable;
                    var stringBuilder = new StringBuilder(capacity);
                    foreach (var filePath in _filePaths)
                    {
                        if (!hashtable.Contains(filePath))
                        {
                            object state;
                            DateTimeOffset lastWriteTime;
                            long fileSize;
                            s_fcn.StartMonitoring(
                                filePath, 
                                OnChanged, 
                                out state, 
                                out lastWriteTime, 
                                out fileSize);
                            hashtable[filePath] = state;
                            stringBuilder.Append(filePath);
                            stringBuilder.Append(
                                lastWriteTime.UtcDateTime.Ticks.ToString("X", CultureInfo.InvariantCulture));
                            stringBuilder.Append(fileSize.ToString("X", CultureInfo.InvariantCulture));
                            if (lastWriteTime > _lastModified)
                            {
                                _lastModified = lastWriteTime;
                            }
                        }
                    }

                    str1 = stringBuilder.ToString();
                }

                _uniqueId = str1;
                flag = false;
            }
            finally
            {
                InitializationComplete();
                if (flag)
                {
                    base.Dispose();
                }
            }
        }
    }

    internal sealed class FileChangeNotificationSystem : IFileChangeNotificationSystem
    {
        private Hashtable _dirMonitors;

        private object _lock;

        internal FileChangeNotificationSystem()
        {
            _dirMonitors = Hashtable.Synchronized(new Hashtable(StringComparer.OrdinalIgnoreCase));
            _lock = new object();
        }

        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        void IFileChangeNotificationSystem.StartMonitoring(
            string filePath, 
            OnChangedCallback onChangedCallback, 
            out object state, 
            out DateTimeOffset lastWriteTime, 
            out long fileSize)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException("filePath");
            }

            if (onChangedCallback == null)
            {
                throw new ArgumentNullException("onChangedCallback");
            }

            var fileInfo = new FileInfo(filePath);
            var directoryName = Path.GetDirectoryName(filePath);
            var directoryMonitor = _dirMonitors[directoryName] as DirectoryMonitor;
            if (directoryMonitor == null)
            {
                lock (_lock)
                {
                    directoryMonitor = _dirMonitors[directoryName] as DirectoryMonitor;
                    if (directoryMonitor == null)
                    {
                        directoryMonitor = new DirectoryMonitor();
                        directoryMonitor.Fsw = new FileSystemWatcher(directoryName);
                        directoryMonitor.Fsw.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName
                                                            | NotifyFilters.Size | NotifyFilters.LastWrite
                                                            | NotifyFilters.CreationTime | NotifyFilters.Security;
                        directoryMonitor.Fsw.EnableRaisingEvents = true;
                    }

                    _dirMonitors[directoryName] = directoryMonitor;
                }
            }

            var changeEventTarget = new FileChangeEventTarget(
                fileInfo.Name, 
                onChangedCallback);
            lock (directoryMonitor)
            {
                directoryMonitor.Fsw.Changed += changeEventTarget.ChangedHandler;
                directoryMonitor.Fsw.Created += changeEventTarget.ChangedHandler;
                directoryMonitor.Fsw.Deleted += changeEventTarget.ChangedHandler;
                directoryMonitor.Fsw.Error += changeEventTarget.ErrorHandler;
                directoryMonitor.Fsw.Renamed += changeEventTarget.RenamedHandler;
            }

            state = changeEventTarget;
            lastWriteTime = File.GetLastWriteTime(filePath);
            //lastWriteTime = (DateTimeOffset)fileInfo.LastWriteTimeUtc;
            fileSize = fileInfo.Exists ? fileInfo.Length : -1L;
        }

        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        void IFileChangeNotificationSystem.StopMonitoring(string filePath, object state)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException("filePath");
            }

            if (state == null)
            {
                throw new ArgumentNullException("state");
            }

            var changeEventTarget = state as FileChangeEventTarget;
            if (changeEventTarget == null)
            {
                throw new ArgumentException("state");
            }

            var directoryMonitor =
                _dirMonitors[Path.GetDirectoryName(filePath)] as DirectoryMonitor;
            if (directoryMonitor == null)
            {
                return;
            }

            lock (directoryMonitor)
            {
                directoryMonitor.Fsw.Changed -= changeEventTarget.ChangedHandler;
                directoryMonitor.Fsw.Created -= changeEventTarget.ChangedHandler;
                directoryMonitor.Fsw.Deleted -= changeEventTarget.ChangedHandler;
                directoryMonitor.Fsw.Error -= changeEventTarget.ErrorHandler;
                directoryMonitor.Fsw.Renamed -= changeEventTarget.RenamedHandler;
            }
        }

        internal class DirectoryMonitor
        {
            internal FileSystemWatcher Fsw;
        }

        internal class FileChangeEventTarget
        {
            private FileSystemEventHandler _changedHandler;

            private ErrorEventHandler _errorHandler;

            private string _fileName;

            private OnChangedCallback _onChangedCallback;

            private RenamedEventHandler _renamedHandler;

            internal FileChangeEventTarget(string fileName, OnChangedCallback onChangedCallback)
            {
                _fileName = fileName;
                _onChangedCallback = onChangedCallback;
                _changedHandler = OnChanged;
                _errorHandler = OnError;
                _renamedHandler = OnRenamed;
            }

            internal FileSystemEventHandler ChangedHandler
            {
                get
                {
                    return _changedHandler;
                }
            }

            internal ErrorEventHandler ErrorHandler
            {
                get
                {
                    return _errorHandler;
                }
            }

            internal RenamedEventHandler RenamedHandler
            {
                get
                {
                    return _renamedHandler;
                }
            }

            private static bool EqualsIgnoreCase(string s1, string s2)
            {
                if (string.IsNullOrEmpty(s1) && string.IsNullOrEmpty(s2))
                {
                    return true;
                }

                if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2) || s2.Length != s1.Length)
                {
                    return false;
                }

                return 0 == string.Compare(s1, 0, s2, 0, s2.Length, StringComparison.OrdinalIgnoreCase);
            }

            private void OnChanged(object sender, FileSystemEventArgs e)
            {
                if (!EqualsIgnoreCase(_fileName, e.Name))
                {
                    return;
                }

                _onChangedCallback(null);
            }

            private void OnError(object sender, ErrorEventArgs e)
            {
                _onChangedCallback(null);
            }

            private void OnRenamed(object sender, RenamedEventArgs e)
            {
                if (!EqualsIgnoreCase(_fileName, e.Name)
                    && !EqualsIgnoreCase(_fileName, e.OldName))
                {
                    return;
                }

                _onChangedCallback(null);
            }
        }
    }
}