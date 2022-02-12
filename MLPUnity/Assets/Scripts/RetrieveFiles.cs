using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class RetrieveFiles : MonoBehaviour
{
    public static Dictionary<string, List<FileInfo>>  GetDictionaryGenresFiles(string pathFolder)
    {
        var filesFromGenres = new Dictionary<string, List<FileInfo>>();
        var dataPath = Application.dataPath;

        var path = Path.Combine(dataPath, pathFolder);

        if (!Directory.Exists(path))
        {
            Debug.LogError("Invalid path");
            return null;
        }
        var dir = Directory.GetDirectories(path);
        foreach (var d in dir)
        {
            var folder = Path.Combine(path, d);
            var info = new DirectoryInfo(folder);
            var fileInfo = info.GetFiles().Where(name => !name.FullName.EndsWith(".meta"));
            var genre = new DirectoryInfo(d).Name;
            var files = fileInfo.ToList();
            filesFromGenres.Add(genre, files);
        }

        return filesFromGenres;
    }
}