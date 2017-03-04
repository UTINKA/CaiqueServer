using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MusicSearch
{
    public class SongRequestLocal
    {
        public static string MusicDir = Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%") + "\\Music\\";
        internal struct SmallFileInfo
        {
            internal string Name;
            internal string Dir;
            internal string Path;
            internal string Extension;
        }

        private static SmallFileInfo[] MusicFiles = new SmallFileInfo[0];
        private static DateTime LastEdit = new DateTime(0);

        internal static SmallFileInfo[] GetFiles()
        {
            try
            {
                var DirInfo = new DirectoryInfo(MusicDir);
                if (DirInfo.LastAccessTime > LastEdit)
                {
                    LastEdit = DirInfo.LastAccessTime;
                    //Console.WriteLine($"Reloading local music library, last detected change at {DirInfo.LastAccessTime.ToLongTimeString()}");
                    MusicFiles = AddFiles(new List<SmallFileInfo>(), DirInfo).OrderBy(x => x.Name).ToArray();
                }
            }
            catch //(Exception ex)
            {
                //Console.WriteLine(ex);
            }

            return MusicFiles;
        }

        private static string[] Exts = new[]
        {
            "FLAC", "WAV", "MP3", "M4A", "ALAC", "OGG", "APE"
        };

        private static List<SmallFileInfo> AddFiles(List<SmallFileInfo> List, DirectoryInfo Dir)
        {
            foreach (var File in Dir.GetFiles())
            {
                var Extention = File.Extension.Substring(1).ToUpper();

                if (Exts.Contains(Extention) && !File.Attributes.HasFlag(FileAttributes.System))
                {
                    List.Add(new SmallFileInfo
                    {
                        Name = File.Name.Substring(0, File.Name.Length - File.Extension.Length),
                        Dir = Dir.FullName,
                        Path = File.FullName,
                        Extension = Extention
                    });
                }
            }

            foreach (var SubDir in Dir.GetDirectories())
            {
                AddFiles(List, SubDir);
            }

            return List;
        }
    }
}
