using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class RetrieveFiles : MonoBehaviour
{


    private static Texture2D Resize(Texture2D texture2D, int targetX, int targetY)
    {
        RenderTexture rt = new RenderTexture(targetX, targetY, 24);
        RenderTexture.active = rt;
        Graphics.Blit(texture2D, rt);
        Texture2D result = new Texture2D(targetX, targetY);
        result.ReadPixels(new Rect(0, 0, targetX, targetY), 0, 0);
        result.Apply();
        return result;
    }

    public static List<double> getPixelsFromImage(FileInfo file)
    {
        Texture2D tex = LoadPNG(file.FullName);
        tex = Resize(tex, 8, 8);
        var tabColor = tex.GetPixels(0);
        List<double> pixels = tabColor.Select(color => (double)color.grayscale).ToList();
        return pixels;
    }
    public static Dictionary<string, List<List<double>>>  GetDictionaryGenresFiles(string pathFolder)
    {
        var filesFromGenres = new Dictionary<string, List<List<double>>>();
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
            List<List<double>> images = new List<List<double>>();
            foreach (var file in files)
            {
                images.Add(getPixelsFromImage(file));
            }
                filesFromGenres.Add(genre, images);
        }

        return filesFromGenres;
    }

    public static Texture2D LoadPNG(string filePath)
    {

        Texture2D tex = null;
        byte[] fileData;

        if (File.Exists(filePath))
        {
            fileData = File.ReadAllBytes(filePath);
            tex = new Texture2D(2, 2);
            tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
        }
        return tex;
    }
}