﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Util.IO;
using DataManagement;

namespace Util.IO
{
    /// <summary>
    /// static class for monitoring files on disk
    /// </summary>
    public static class FileWatcher
    {
        /// <summary>
        /// Is true when an indexing proccess is running
        /// </summary>
        private static bool IsIndexing { get; set; } = false;

        private static RuntimeData Data;

        /// <summary>
        /// checks wether the filetype is on the whitelist
        /// </summary>
        /// <param name="path">path to file</param>
        /// <returns>true, if file is allowed and supported</returns>
        public static bool checkForValidFile(string path)
        {
            if (String.IsNullOrWhiteSpace(path) || !File.Exists(path))
                return false;

            string format = path.Substring(path.LastIndexOf('.') + 1);

            foreach (var testFormat in Data.Persistent.SupportedFormats)
                if (testFormat == format)
                    return true;

            return false;
        }

        private static DataManagement.FileData getAllFileInfo(FileSystemEventArgs e)
        {
            return getAllFileInfo(e.FullPath, e.Name);
        }

        /// <summary>
        /// Copies all matching FileData object from source to target
        /// </summary>
        /// <param name="filter">filter to apply</param>
        /// <param name="source">source of FileData objects</param>
        /// <returns></returns>
        public static ObservableCollection<DataManagement.FileData> filterList(string filter,
            ObservableCollection<DataManagement.FileData> source)
        {
            if (IsIndexing)
            {
                SnackbarManager.SnackbarMessage("An indexing process is running", SnackbarManager.SnackbarAction.None);
                return new ObservableCollection<DataManagement.FileData>(source);
            }

            if (!string.IsNullOrEmpty(filter))
            {
                ObservableCollection<DataManagement.FileData> target = new ObservableCollection<DataManagement.FileData>();
                //make it once to lower, instead for any iteration
                filter = filter.ToLower();

                foreach (DataManagement.FileData file in source)
                {
                    //add all files matching
                    if (checkForLowerMatch(file, filter))
                        target.Add(file);
                }

                return target;
            }
            else
            {
                //reset filter if empty
                //make deep copy
                return new ObservableCollection<DataManagement.FileData>(source);
            }
        }

        /// <summary>
        /// applies a filter to all properties of file (except path)
        /// </summary>
        /// <param name="file">File to search for a hit in, case does not matter</param>
        /// <param name="filterLower">Filter, must be provided in lower case</param>
        /// <returns>true, if the filter got a hit in any property</returns>
        private static bool checkForLowerMatch(DataManagement.FileData file, string filterLower)
        {
            //filter for all known attributes (ignore case)
            if (file.Name.ToLower().Contains(filterLower) || file.Author.ToLower().Contains(filterLower)
                                                          || file.Album.ToLower().Contains(filterLower) ||
                                                          file.Genre.ToLower().Contains(filterLower))
            {
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// get all important information from a given file
        /// </summary>
        /// <param name="FullPath">Path to file, no validation</param>
        /// <returns>Data.FileData object, describing the file</returns>
        public static DataManagement.FileData getAllFileInfo(string FullPath)
        {           
                return getAllFileInfo(FullPath, Path.GetFileName(FullPath));          
        }

        private static DataManagement.FileData getAllFileInfo(string FullPath, string Name)
        {
            DataManagement.FileData file = new DataManagement.FileData()
            {
                //name is set based on file name
                Name = Name.Remove(Name.LastIndexOf('.')),
                Path = FullPath,
            };

            TagLib.File f;
            try
            {
                f = TagLib.File.Create(FullPath);
            }
            catch
            {
                return file;
            }

            //only set, when TagLib could load file
            var performers = f.Tag.Performers;
            if (performers.Length > 0)
                file.Author = performers[0];

            //only set if genres are existing
            var genres = f.Tag.Genres;
            if (genres.Length > 0)
                file.Genre = genres[0];

            var album = f.Tag.Album;
            if (album != null)
                file.Album = album;

            var length = f.Properties.Duration;
            if (length != null)
                file.Length = length;

            f.Dispose();
            return file;
        }

        /// <summary>
        /// updates list of files, deletes old list, runs in background thread
        /// </summary>
        /// <param name="sources">all directories to search</param>
        /// <param name="allowDuplicates">if true, doesn't test if file is already in list</param>
        public static void indexCleanFiles(ObservableCollection<string> sources, bool allowDuplicates = false)
        {
            Data.Files.Clear();
            indexFiles(sources, allowDuplicates);
        }

        /// <summary>
        /// updates list of files, keeps old list, runs in background thread
        /// </summary>
        /// <param name="sources">all directories to search</param>
        /// <param name="allowDuplicates">if true, doesn't test if file is already in list</param>
        public static void indexFiles(ObservableCollection<string> sources, bool allowDuplicates = false)
        {
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += new DoWorkEventHandler(indexFilesWorker);
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_Complete);

            var bwTuple = Tuple.Create(sources, allowDuplicates);
            bw.RunWorkerAsync(bwTuple);
        }

        private static void indexFilesWorker(object sender, DoWorkEventArgs e)
        {
            //retry every 100ms
            while (IsIndexing)
            {
                //await System.Threading.Tasks.Task.Delay(100);
                System.Threading.Thread.Sleep(100);
            }

            if (e.Argument is Tuple<ObservableCollection<string>, bool> workingTuple)
            {
                IsIndexing = true;
                indexFilesThread(new List<string>(workingTuple.Item1), workingTuple.Item2);
            }
        }

        private static void bw_Complete(object sender, RunWorkerCompletedEventArgs e)
        {
            IsIndexing = false;
        }

        private static void indexFilesThread(List<string> sources, bool allowDuplicates = false)
        {
            foreach (var dir in sources)
            {
                indexFolder(dir, allowDuplicates);
            }

            //sort by name
            Data.Files = new ObservableCollection<DataManagement.FileData>(Data.Files.OrderBy(o => o.Name));
        }

        /// <summary>
        /// recursive function for searching indexing a folder
        /// </summary>
        /// <param name="path">path to directory</param>
        /// <param name="allowDuplicates">if true, doesn't test if file is already in list</param>
        private static void indexFolder(string path, bool allowDuplicates)
        {
            string[] files = Directory.GetFiles(path);
            string[] subDirs = Directory.GetDirectories(path);

            foreach (var singleFile in files)
            {
                indexFile(singleFile, allowDuplicates);
            }

            foreach (var singleDir in subDirs)
            {
                indexFolder(singleDir, allowDuplicates);
            }
        }

        /// <summary>
        /// add file to list, if it's a supported format
        /// </summary>
        /// <param name="path">path to file</param>
        /// <param name="allowDuplicates">if true, doesn't test if file is already in list</param>
        private static void indexFile(string path, bool allowDuplicates)
        {
            if (!allowDuplicates)
            {
                //check for existing files, to avoid duplicates
                foreach (var singleFile in Data.Files)
                {
                    if (singleFile.Path == path) return;
                }
            }

            if (checkForValidFile(path))
            {
                //path needs to be valid
                Data.Files.Add(getAllFileInfo(path));
            }
        }

        /// <summary>
        /// inits one watcher for each path
        /// </summary>
        /// <param name="sources">collection of paths to directories to watch</param>
        /// <param name="dataRef">Ref to <see cref="RuntimeData"/> object</param>
        public static void StartMonitor(ObservableCollection<string> sources, RuntimeData dataRef)
        {
            Data = dataRef;

            foreach (var path in sources)
                initWatcher(path);
        }

        /// <summary>
        /// if a new sources was added, this adds a fileWatcher for that directory. <see cref="indexFiles(ObservableCollection{string}, bool)"/> should also be called
        /// </summary>
        ///
        /// <param name="source">path to directory</param>
        public static void addWatcher(string source)
        {
            initWatcher(source);
        }

        private static void initWatcher(string source)
        {
            FileSystemWatcher fsW = new FileSystemWatcher()
            {
                Path = source,
                IncludeSubdirectories = true,
                EnableRaisingEvents = true,
            };

            fsW.Changed += FileSystemWatcher_Changed;
            fsW.Created += FileSystemWatcher_Created;
            fsW.Renamed += FileSystemWatcher_Renamed;
            fsW.Deleted += FileSystemWatcher_Deleted;
        }

        #region events

        private static void FileSystemWatcher_Created(object sender, FileSystemEventArgs e)
        {
            if (checkForValidFile(e.FullPath))
            {
                DataManagement.FileData nFile = getAllFileInfo(e);

                Data.Files.Add(nFile);
            }
        }

        private static void FileSystemWatcher_Renamed(object sender, FileSystemEventArgs e)
        {
            string oldPath = ((RenamedEventArgs) e).OldFullPath;

            for (int i = 0; i < Data.Files.Count; i++)
            {
                if (Data.Files[i].Path == oldPath)
                {
                    Data.Files[i] = getAllFileInfo(e);
                    break;
                }
            }
        }

        private static void FileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            for (int i = 0; i < Data.Files.Count; i++)
            {
                if (Data.Files[i].Path == e.FullPath)
                {
                    Data.Files[i] = getAllFileInfo(e);
                    break;
                }
            }
        }

        private static void FileSystemWatcher_Deleted(object sender, FileSystemEventArgs e)
        {
            for (int i = 0; i < Data.Files.Count; i++)
            {
                if (Data.Files[i].Path == e.FullPath)
                {
                    Data.Files.RemoveAt(i);
                    break;
                }
            }
        }

        #endregion events
    }
}