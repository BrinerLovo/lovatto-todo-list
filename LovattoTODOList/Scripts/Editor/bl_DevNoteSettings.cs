using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;

[Serializable]
public class bl_DevNoteSettings
{
    public int CurrentList = 0;
    public List<ListInfo> allList = new List<ListInfo>() { new ListInfo() { Name = "Default", Path = ""} };

    public static bl_DevNoteSettings Init()
    {
        string path = GetDevNoteScriptPath();
        string settingsPath = $"{path}/Lists/settings.txt";
        if (File.Exists(settingsPath))
        {
            string st = File.ReadAllText(settingsPath);
            if (!string.IsNullOrEmpty(st))
            {
                return JsonUtility.FromJson<bl_DevNoteSettings>(st);
            }
            else return new bl_DevNoteSettings();
        }
        else
        {
            Directory.CreateDirectory($"{path}/Lists/");
            using (File.CreateText(settingsPath))
            {

            }
            return new bl_DevNoteSettings();
        }
    }

    public bl_DevNotesInfo GetNoteList()
    {
        string path = allList[CurrentList].GetPath();
        bl_DevNotesInfo list;
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            list = JsonUtility.FromJson<bl_DevNotesInfo>(json);
        }
        else
        {
            using (StreamWriter sw = File.CreateText(path)) { }
            list = new bl_DevNotesInfo();
            AssetDatabase.Refresh();
        }
        return list;
    }

    public void SaveSettings()
    {
        string path = GetDevNoteScriptPath();
        string settingsPath = $"{path}/Lists/settings.txt";
        string json = JsonUtility.ToJson(this);
        if (File.Exists(settingsPath))
        {
            using (StreamWriter sr = File.CreateText(settingsPath))
            {
                sr.Write(json);
            }
        }
        else
        {
            StreamWriter writer = new StreamWriter(settingsPath, false);
            writer.WriteLine(json);
            writer.Close();
        }
    }

    public void AddList()
    {
        string path = EditorUtility.OpenFilePanel("Select List Data", "Assets/", "txt");
        if (string.IsNullOrEmpty(path)) return;

        string name = Path.GetFileName(path).Replace(".txt", "");
        allList.Add(new ListInfo()
        {
            Name = name,
            Path = path
        });
        SaveSettings();
    }

    public void SaveList(bl_DevNotesInfo data)
    {
        string json = JsonUtility.ToJson(data);
        string path = allList[CurrentList].GetPath();
        if (File.Exists(path))
        {
            StreamWriter writer = new StreamWriter(path, false);
            writer.WriteLine(json);
            writer.Close();
        }
        else
        {
            using (StreamWriter sw = File.CreateText(path))
            {
                sw.Write(json);
            }
        }
        AssetDatabase.Refresh();
    }

    static string GetDevNoteScriptPath()
    {
        string[] res = System.IO.Directory.GetFiles(Application.dataPath, "bl_DevNoteList.cs", SearchOption.AllDirectories);
        string path = res[0].Replace("bl_DevNoteList.cs", "").Replace("\\", "/");
        return path;
    }

    [Serializable]
    public class ListInfo
    {
        public string Name;
        public string Path;

        public string GetPath()
        {
            string path = Path;
            if (string.IsNullOrEmpty(path))
            {
                path = $"{bl_DevNoteSettings.GetDevNoteScriptPath()}/Lists/{Name}.txt";
                Path = path;
            }
            return path;
        }
    }
}