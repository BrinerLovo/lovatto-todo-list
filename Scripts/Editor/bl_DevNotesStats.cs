using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Globalization;

public class bl_DevNotesStats
{
    private List<TaskCount> CompletedTaks = new List<TaskCount>();
    private int MaxCatCount = 0;
    private bool dateStatsFetchs = false;
    private int todayJobs, weekJobs, monthJobs = 0;
    private GUIStyle centerLabelStyle;
    int lastCount = -1;
    private AnimationCurve dateGraph = AnimationCurve.Linear(0, 0, 30, 30);
    private Dictionary<int, int> jobsInMonthPerDay;
    private string today, midMonth, starMonth = "";

    /// <summary>
    /// 
    /// </summary>
    /// <param name="notes"></param>
    public void DrawStats(bl_DevNotesInfo notes)
    {
        bl_DevNoteList.DrawWindowArea();
        GUILayout.BeginVertical("box");
        GUILayout.Label(string.Format("Task completed: <size=20>{0}</size>", notes.HistoryNotes.Count));

        CountTasks(notes);
        if (!dateStatsFetchs || lastCount != notes.HistoryNotes.Count)
        {
            FetchDateState(notes);
            BuildGraph();
        }

        GUILayout.BeginVertical("box");
        int barSize = Screen.width - 105;
        for (int i = 0; i < CompletedTaks.Count; i++)
        {
            GUILayout.BeginHorizontal();        
            Rect r = GUILayoutUtility.GetRect(70, EditorGUIUtility.singleLineHeight);
            DrawWhiteBox(r, Color.black);
            EditorGUI.LabelField(r, CompletedTaks[i].Category.Name + "s", centerLabelStyle);

            Color c = CompletedTaks[i].Category.Color;
            c.a = 0.4f;
            float bz = ((float)CompletedTaks[i].Completed / (float)MaxCatCount) * (float)barSize;
            r = GUILayoutUtility.GetRect(bz, EditorGUIUtility.singleLineHeight);
            DrawWhiteBox(r, c);
            r.width = 75;
            r.x += 5;
            GUI.Label(r, CompletedTaks[i].Completed.ToString());
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(4);
        }
        GUILayout.EndVertical();

        DrawDateStats();
        DrawDateGraph();

        GUILayout.FlexibleSpace();
        GUILayout.EndVertical();
        GUILayout.EndArea();
        lastCount = notes.HistoryNotes.Count;
    }

    /// <summary>
    /// 
    /// </summary>
    void DrawDateStats()
    {
        if(centerLabelStyle == null)
        {
            centerLabelStyle = new GUIStyle(EditorStyles.label);
            centerLabelStyle.richText = true;
            centerLabelStyle.alignment = TextAnchor.MiddleCenter;
            centerLabelStyle.normal.textColor = Color.white;
        }
        EditorGUILayout.BeginHorizontal("box");
        {
            GUILayout.FlexibleSpace();
            DrawInfoBox("Today", todayJobs.ToString());
            DrawInfoBox("Week", weekJobs.ToString());
            DrawInfoBox("Month", monthJobs.ToString());
            GUILayout.FlexibleSpace();
        }
        EditorGUILayout.EndHorizontal();
    }

    void DrawInfoBox(string title, string stat)
    {
        EditorGUILayout.BeginVertical("box", GUILayout.MaxWidth(50), GUILayout.Height(50));
        {
            EditorGUILayout.LabelField($"<size=8><b>{title.ToUpper()}</b></size>", centerLabelStyle, GUILayout.MaxWidth(50));
            GUILayout.Space(4);
            EditorGUILayout.LabelField($"<size=20>{stat}</size>", centerLabelStyle, GUILayout.MaxWidth(50));
        }
        EditorGUILayout.EndVertical();
    }

    void DrawDateGraph()
    {
        EditorGUILayout.BeginVertical("box");
        {
            EditorGUILayout.CurveField(dateGraph, Color.yellow, Rect.zero, GUILayout.ExpandWidth(true), GUILayout.Height(100));
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField($"<size=8>{starMonth}</size>", centerLabelStyle, GUILayout.Width(40));
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField($"<size=8>{midMonth}</size>", centerLabelStyle, GUILayout.Width(40));
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField($"<size=8>{today}</size>", centerLabelStyle, GUILayout.Width(40));
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();

        Rect r = GUILayoutUtility.GetLastRect();
        DrawLines(r);
    }

    public void DrawWhiteBox(Rect rect, Color color)
    {
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill);
        GUI.color = Color.white;
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

    void FetchDateState(bl_DevNotesInfo notes)
    {
        todayJobs = 0;
        weekJobs = 0;
        monthJobs = 0;
        jobsInMonthPerDay = new Dictionary<int, int>();
        today = $"{DateTime.Now.Day} {DateTime.Now.ToString("MMMM")}";
        DateTime lastMonth = DateTime.Now.AddDays(-30);
        starMonth = $"{lastMonth.Day} {lastMonth.ToString("MMMM")}";

        lastMonth = DateTime.Now.AddDays(-15);
        midMonth = $"{lastMonth.Day} {lastMonth.ToString("MMMM")}";

        for (int i = 0; i < notes.HistoryNotes.Count; i++)
        {
            var note = notes.HistoryNotes[i];
            if (string.IsNullOrEmpty(note.CompleteDate)) continue;

            DateTime date = bl_DevNotesUtils.ParseDate(note.CompleteDate);
            var ts = new TimeSpan(DateTime.Now.Ticks - date.Ticks);

            if (ts.TotalDays <= 30)
            {
                int day = 30 - Mathf.FloorToInt((float)ts.TotalDays);
                if(ts.TotalDays < 1)
                {
                    day = 30;
                }

                if (!jobsInMonthPerDay.ContainsKey(day))
                {
                    jobsInMonthPerDay.Add(day, 1);
                }
                else
                {
                    jobsInMonthPerDay[day]++;
                }
            }

            if(ts.TotalDays <= 1)
            {
                todayJobs++;
                weekJobs++;
                monthJobs++;
                continue;
            }
            if (ts.TotalDays <= 7)
            {
                weekJobs++;
                monthJobs++;
                continue;
            }
            if (ts.TotalDays <= 30)
            {
                monthJobs++;
                continue;
            }
        }
        dateStatsFetchs = true;
    }

    /// <summary>
    /// 
    /// </summary>
    void BuildGraph()
    {
        dateGraph = AnimationCurve.Linear(0, 0, 30, 0);

        for (int i = 0; i <= 30; i++)
        {
            int jobs = jobsInMonthPerDay.ContainsKey(i) ? jobsInMonthPerDay[i] : 0;
            if(i == 0 || i == 30)
            dateGraph.MoveKey(i, new Keyframe(i, jobs));
            else
            dateGraph.AddKey(new Keyframe(i, jobs));           
        }
    }

    private Material lineMaterial;
    private void DrawLines(Rect area)
    {
        area.x += 5;
        area.height -= 2;
        area.width -= 5;
        if(lineMaterial == null)
        {
            lineMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));
        }
        GUI.BeginClip(area);
        GL.PushMatrix();
        lineMaterial.SetPass(0);

        GL.Begin(GL.LINES);
        GL.Color(new Color(1,1,1,0.3f));
        float space = area.width / 30;
        float currentX = 0;
        for (int i = 0; i < 30; i++)
        {
            DrawLine(new Vector2(currentX, 5), new Vector2(currentX, 100));
            currentX += space;
        }
        GL.End();
        GL.PopMatrix();
        GUI.EndClip();
    }

    private void DrawLine(Vector2 p1, Vector2 p2)
    {
        GL.Vertex(p1);
        GL.Vertex(p2);
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

public static class bl_DevNotesUtils
{
    const int SECOND = 1;
    const int MINUTE = 60 * SECOND;
    const int HOUR = 60 * MINUTE;
    const int DAY = 24 * HOUR;
    const int MONTH = 30 * DAY;

    public static string GetRelativeTimeName(DateTime yourDate)
    {
        var ts = new TimeSpan(DateTime.Now.Ticks - yourDate.Ticks);
        double delta = Math.Abs(ts.TotalSeconds);

        if (delta < 1 * MINUTE)
            return ts.Seconds == 1 ? "one second ago" : ts.Seconds + " seconds ago";

        if (delta < 2 * MINUTE)
            return "a minute ago";

        if (delta < 45 * MINUTE)
            return ts.Minutes + " minutes ago";

        if (delta < 90 * MINUTE)
            return "an hour ago";

        if (delta < 24 * HOUR)
            return ts.Hours + " hours ago";

        if (delta < 48 * HOUR)
            return "yesterday";

        if (delta < 30 * DAY)
            return ts.Days + " days ago";

        if (delta < 12 * MONTH)
        {
            int months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
            return months <= 1 ? "one month ago" : months + " months ago";
        }
        else
        {
            int years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));
            return years <= 1 ? "one year ago" : years + " years ago";
        }
    }

    public static DateTime ParseDate(string dateStr)
    {
        var ci = new CultureInfo("en-US");
        return DateTime.Parse(dateStr, ci);
    }
}