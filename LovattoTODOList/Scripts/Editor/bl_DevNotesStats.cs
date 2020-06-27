using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class bl_DevNotesStats
{
    private List<TaskCount> CompletedTaks = new List<TaskCount>();
    private int MaxCatCount = 0;

    public void DrawStats(bl_DevNotesInfo notes)
    {
        bl_DevNoteList.DrawWindowArea();
        GUILayout.BeginVertical("box");
        GUILayout.Label(string.Format("Task completed: <size=20>{0}</size>", notes.HistoryNotes.Count));

        CountTasks(notes);
        GUILayout.BeginVertical("box");
        int barSize = Screen.width - 105;
        for (int i = 0; i < CompletedTaks.Count; i++)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(CompletedTaks[i].Category.Name + "s",EditorStyles.miniLabel, GUILayout.Width(80));
            GUI.color = CompletedTaks[i].Category.Color;
            int bz = Mathf.RoundToInt((float)CompletedTaks[i].Completed / (float)MaxCatCount * (float)barSize);
            GUILayout.Box(CompletedTaks[i].Completed.ToString(),EditorStyles.helpBox, GUILayout.Width(bz),GUILayout.Height(16));
            GUI.color = Color.white;
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();

        GUILayout.FlexibleSpace();
        GUILayout.EndVertical();
        GUILayout.EndArea();
       
    }

    void CountTasks(bl_DevNotesInfo notes)
    {
        CompletedTaks.Clear();
        MaxCatCount = 0;

        for (int i = 0; i < notes.HistoryNotes.Count; i++)
        {
            if (CompletedTaks.Exists(x => x.CatID == notes.HistoryNotes[i].CategoryID))
            {
                TaskCount tc = CompletedTaks.Find(x => x.CatID == notes.HistoryNotes[i].CategoryID);
                tc.Completed++;
                if(tc.Completed > MaxCatCount) { MaxCatCount = tc.Completed; }
            }
            else
            {
                TaskCount tc = new TaskCount();
                tc.CatID = notes.HistoryNotes[i].CategoryID;
                tc.Completed = 1;
                tc.Build(notes);
                CompletedTaks.Add(tc);
            }
        }
        MaxCatCount += 5;
    }

    [System.Serializable]
    public class TaskCount
    {
        public int CatID;
        public bl_DevNotesInfo.Categorys Category;
        public int Completed;

        public void Build(bl_DevNotesInfo notes)
        {
            Category = notes.AllCategorys[CatID];
        }
    }
}

public class NoteListTexBox : EditorWindow
{
    public string str = "";
    public static void Show(string message)
    {
        NoteListTexBox window = GetWindow<NoteListTexBox>();
        window.str = message;
    }

    private void OnGUI()
    {
        EditorGUILayout.TextArea(str,GUILayout.ExpandHeight(true));
    }
}