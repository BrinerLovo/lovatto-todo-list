using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Linq;
using System.Globalization;

public class bl_DevNoteList : EditorWindow
{
    public bl_DevNotesInfo Notes;
    private bool showAddBox = false;
    public bl_DevNotesStats Stats = new bl_DevNotesStats();
    private string AddComment = "";
    private string AddNote = "";
    private int AddCat = 0;
    private int ShowCat = 0;
    private int OpenCommentID = -1;
    private int windowID = 0;
    private Vector2 noteScroll;
    private Vector2 historyScroll;
    private bool showSort = false;
    public Texture[] icons = new Texture[5];
    private Texture2D whiteBox;
    private bool reorderableMode = true;
    public ReorderableList notesList;
    public SerializedObject serializedObject;
    public bl_DevNoteSettings settings;
    private int lastListID = 0;
    private static GUIStyle miniLabelStyle = null;
    private CultureInfo cultureInfo = new CultureInfo("en-US");

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        if(settings == null)
        settings = bl_DevNoteSettings.Init();
        LoadList();

        lastListID = settings.CurrentList;
        if (notesList == null)
        {
            serializedObject = new SerializedObject(this);
            var notes = serializedObject.FindProperty("Notes").FindPropertyRelative("Notes");
            notesList = new ReorderableList(serializedObject, notes, true, false, true, true);
            notesList.onAddCallback += (list) => { showAddBox = true; };
            notesList.onRemoveCallback += (list) =>
            {
                if (EditorUtility.DisplayDialog("Confirm Action", "Do you want to remove this note?", "Yes","Cancel"))
                {
                    Notes.Notes.RemoveAt(notesList.index);
                    SaveNotes();
                }
            };
            notesList.onReorderCallback += (list) => { SaveNotes(); };
            notesList.drawElementCallback += DrawReorderableNotesContent;
            notesList.headerHeight = 8;
            notesList.elementHeightCallback += CalculateElementHeight;
            notesList.elementHeight = EditorGUIUtility.singleLineHeight;
            notesList.showDefaultBackground = true;
        }
        titleContent = new GUIContent("Notes");
        minSize = new Vector2(435, 250);
        icons[0] = GetUnityIcon("d_clear");
        icons[1] = GetUnityIcon("ol plus");
        icons[2] = GetUnityIcon("d_Profiler.PrevFrame");
        icons[3] = GetUnityIcon("align_vertically_center_active");
        icons[4] = GetUnityIcon("HorizontalSplit");
        whiteBox = Texture2D.whiteTexture;
    }

    public void LoadList(bool forced = false)
    {
        if (Notes == null || forced)
            Notes = settings.GetNoteList();
    }

    public Texture GetUnityIcon(string name)
    {
        return EditorGUIUtility.IconContent(name).image;
    }
    /// <summary>
    /// 
    /// </summary>
    private void OnGUI()
    {
        if (miniLabelStyle == null)
        {
            GUI.skin.label.richText = true;
            EditorStyles.label.richText = true;
            miniLabelStyle = new GUIStyle(EditorStyles.wordWrappedMiniLabel);
            miniLabelStyle.richText = true;
            miniLabelStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f, 1);
            miniLabelStyle.padding.top = -1;
        }
        DrawHeader();
        if (windowID == 0)
        {
            if (!reorderableMode)
                DrawNotes();
            else ReorderableNotes();
        }
        else if (windowID == 1)
        {
            DrawHistory();
        }
        else if (windowID == 2)
        {
            Stats.DrawStats(Notes);
        }
        DrawFoot();
    }

    /// <summary>
    /// 
    /// </summary>
    void DrawHeader()
    {
        EditorGUI.BeginChangeCheck();
        GUILayout.BeginHorizontal(EditorStyles.toolbarButton);
        if (windowID > 0)
        {
            if (GUILayout.Button(new GUIContent(icons[2]), EditorStyles.toolbarButton, GUILayout.Width(20)))
            {
                windowID = 0;
            }
            if (windowID == 1)
            {
                if (GUILayout.Button("P", EditorStyles.toolbarButton, GUILayout.Width(20)))
                {
                    PrintRawHistory();
                }
            }
        }
        else
        {
            GUILayout.Label("", GUILayout.Width(20));
        }
        GUILayout.Space(91);
        GUILayout.FlexibleSpace();
        GUILayout.Label("DEV NOTES", EditorStyles.toolbarButton);
        GUILayout.FlexibleSpace();
        settings.CurrentList = EditorGUILayout.Popup(settings.CurrentList, settings.allList.Select(x => x.Name).ToArray(), EditorStyles.toolbarPopup, GUILayout.Width(75));
        if (GUILayout.Button("+", EditorStyles.toolbarButton, GUILayout.Width(16)))
        {
            settings.AddList();
        }
        if (windowID == 1)
        {
            if (GUILayout.Button(new GUIContent(icons[4]), EditorStyles.label, GUILayout.Width(20)))
            {
                AddHistorySeparator();
            }
        }
        if (GUILayout.Button(EditorGUIUtility.IconContent("d_CloudConnect"), EditorStyles.toolbarButton, GUILayout.Width(25)))
        {
            SaveNotes();
        }
        GUILayout.EndHorizontal();
        if (EditorGUI.EndChangeCheck())
        {
            if(lastListID != settings.CurrentList)
            {
                settings.SaveSettings();
                LoadList(true);
                lastListID = settings.CurrentList;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void DrawNotes()
    {
        DrawWindowArea();
        if (Notes != null)
        {
            noteScroll = GUILayout.BeginScrollView(noteScroll, "box");
            bool bg = false;
            for (int i = 0; i < Notes.Notes.Count; i++)
            {
                if (showSort)
                {
                    if (Notes.Notes[i].CategoryID != (ShowCat - 1)) continue;
                }
                Rect r = EditorGUILayout.BeginHorizontal();
                Rect boxr = r;
                if (bg)
                {
                    r.height += 2;
                    GUI.Box(r, GUIContent.none);
                }
                GUI.color = Notes.AllCategorys[Notes.Notes[i].CategoryID].Color;
                boxr.width = 3;
                GUI.DrawTexture(boxr, whiteBox, ScaleMode.StretchToFill);
                GUI.color = Color.white;
                GUILayout.Space(3);
                Notes.Notes[i].CategoryID = EditorGUILayout.Popup(Notes.Notes[i].CategoryID, CategoryString.ToArray(), EditorStyles.toolbarPopup, GUILayout.Width(60));
                Notes.Notes[i].Note = GUILayout.TextField(Notes.Notes[i].Note,EditorStyles.wordWrappedLabel);
                if (GUILayout.Button("✉", EditorStyles.wordWrappedLabel, GUILayout.Width(16)))
                {
                    OpenCommentID = i;
                }
                if (i > 0)
                {
                    if (GUILayout.Button("↑", EditorStyles.wordWrappedLabel, GUILayout.Width(16)))
                    {
                        MoveField(i, true);
                    }
                }
                else { GUILayout.Box("", EditorStyles.wordWrappedLabel, GUILayout.Width(16)); }
                if (i < Notes.Notes.Count - 1)
                {
                    if (GUILayout.Button("↓", EditorStyles.wordWrappedLabel, GUILayout.Width(16)))
                    {
                        MoveField(i, false);
                    }
                }
                else { GUILayout.Box("", EditorStyles.wordWrappedLabel, GUILayout.Width(16)); }
                if (GUILayout.Button("✓", EditorStyles.wordWrappedLabel, GUILayout.Width(16)))
                {
                    OnCompleteJob(i);
                }
                EditorGUILayout.EndHorizontal();
                if(OpenCommentID != -1 && OpenCommentID == i)
                {
                    GUILayout.BeginVertical("box");
                    Notes.Notes[i].Comment = GUILayout.TextField(Notes.Notes[i].Comment,EditorStyles.helpBox, GUILayout.Height(50));
                    GUILayout.BeginHorizontal("box");
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("✓", EditorStyles.wordWrappedLabel, GUILayout.Width(16)))
                    {
                        if (!string.IsNullOrEmpty(Notes.Notes[i].Comment))
                        { SaveNotes(); }
                        OpenCommentID = -1;
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.EndVertical();
                }
                GUILayout.Space(1);
                bg = !bg;
            }
             GUILayout.EndScrollView();
        }

        if (!showAddBox)
        {
            GUILayout.BeginVertical("box");
            if (GUILayout.Button(new GUIContent(" ADD", icons[1]), EditorStyles.toolbarButton))
            {
                showAddBox = true;
            }
            GUILayout.EndHorizontal();
        }
        else
        {
            GUILayout.BeginVertical("box");
            AddCat = EditorGUILayout.Popup(AddCat, CategoryString.ToArray(), GUILayout.Width(60));
            AddNote = GUILayout.TextArea(AddNote, GUILayout.Height(22));
            AddComment = GUILayout.TextArea(AddComment, GUILayout.Height(50));
            GUILayout.EndVertical();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent(" ADD", icons[1]), EditorStyles.toolbarButton))
            {
                AddNewJob();
            }
            if (GUILayout.Button("Cancel", EditorStyles.toolbarButton, GUILayout.Width(100)))
            {
                AddComment = string.Empty;
                showAddBox = false;
                AddNote = string.Empty;
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndArea();
    }

    /// <summary>
    /// 
    /// </summary>
    void ReorderableNotes()
    {
        if (serializedObject == null) return;

        EditorGUI.BeginChangeCheck();
        serializedObject.Update();
        noteScroll = GUILayout.BeginScrollView(noteScroll);
        notesList.DoLayoutList();
        GUILayout.EndScrollView();

        if (showAddBox)
        {
            DrawAddBox();
        }
        if (!EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
        }
        GUILayout.Space(32);
    }

    void DrawAddBox()
    {
        if(Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
        {
            AddNewJob();
        }
        GUILayout.BeginVertical("box");
        AddCat = EditorGUILayout.Popup(AddCat, CategoryString.ToArray(), GUILayout.Width(60));
        AddNote = GUILayout.TextArea(AddNote, GUILayout.Height(22));
        AddComment = GUILayout.TextArea(AddComment, GUILayout.Height(50));
        GUILayout.EndVertical();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button(new GUIContent(" ADD", icons[1]), EditorStyles.toolbarButton))
        {
            AddNewJob();
        }
        if (GUILayout.Button("Cancel", EditorStyles.toolbarButton, GUILayout.Width(100)))
        {
            AddComment = string.Empty;
            showAddBox = false;
            AddNote = string.Empty;
        }
        GUILayout.EndHorizontal();
    }

    void DrawReorderableNotesContent(Rect rect, int index, bool isActive, bool isFocused)
    {
        var element = notesList.serializedProperty.GetArrayElementAtIndex(index);
        var comment = element.FindPropertyRelative("Comment");
        var category = element.FindPropertyRelative("CategoryID");
        bool altStyle = (index % 2) == 0;
        if (altStyle)
        {
            DrawWhiteBox(rect, new Color(0, 0, 0, 0.3f));
        }
        Rect r = rect;
        r.width = 3;
        r.x -= 3;
        Color categoryColor = Notes.AllCategorys[category.intValue].Color;
        DrawWhiteBox(r, categoryColor);

        r = rect;
        r.x += 5;
        r.width -= 50;
        r.height = EditorGUIUtility.singleLineHeight;
        var note = element.FindPropertyRelative("Note");
        if (OpenCommentID != index)
            GUI.Label(r, note.stringValue);
        else
        {
            var sourceNote = Notes.Notes[index];
            sourceNote.Note = EditorGUI.TextField(r, note.stringValue);

            r.y += EditorGUIUtility.singleLineHeight + 2;
            r.height = 40;
            r.width -= 20;
            sourceNote.Comment = EditorGUI.TextArea(r, comment.stringValue);

            r = rect;
            r.y += EditorGUIUtility.singleLineHeight + 2;
            r.height = EditorGUIUtility.singleLineHeight;
            r.x += r.width - 64;
            r.width = 60;

            sourceNote.CategoryID = EditorGUI.Popup(r, category.intValue, CategoryString.ToArray(), EditorStyles.toolbarPopup);
            r.y += EditorGUIUtility.singleLineHeight + 4;
            if (GUI.Button(r, "Done", EditorStyles.toolbarButton))
            {
                serializedObject.ApplyModifiedProperties();
                SaveNotes();
                OpenCommentID = -1;
            }
        }

        r = rect;
        r.x += r.width - 20;
        r.width = 16;
        r.height = EditorGUIUtility.singleLineHeight;
        if (GUI.Button(r, "✓", EditorStyles.wordWrappedLabel))
        {
            OnCompleteJob(index);
        }
        r.x -= 20;
        if (GUI.Button(r, "✉", EditorStyles.wordWrappedLabel))
        {
            OpenCommentID = index;
        }
    }

    private float CalculateElementHeight(int index)
    {
        if (OpenCommentID != index)
            return EditorGUIUtility.singleLineHeight;
        else
        {
            return EditorGUIUtility.singleLineHeight + 50;
        }
    }

    public void DrawWhiteBox(Rect rect, Color color)
    {
        GUI.color = color;
        GUI.DrawTexture(rect, whiteBox, ScaleMode.StretchToFill);
        GUI.color = Color.white;
    }

    /// <summary>
    /// 
    /// </summary>
    void DrawHistory()
    {
        if (Notes == null) return;

        DrawWindowArea();
        historyScroll = GUILayout.BeginScrollView(historyScroll, "box");
        bool bg = false;
        for (int i = Notes.HistoryNotes.Count - 1; i >= 0; i--)
        {
            bl_DevNotesInfo.Info note = Notes.HistoryNotes[i];
            Rect r = EditorGUILayout.BeginHorizontal();
            Rect sr = r;
            if (bg)
            {
                DrawWhiteBox(r, new Color(0, 0, 0, 0.2f));
            }
            if (note.noteType == bl_DevNotesInfo.NoteType.Note)
            {
                sr.width = 3;
                Color c = Notes.AllCategorys[note.CategoryID].Color;
                DrawWhiteBox(sr, c);
                GUILayout.Space(4);
             
                sr = GUILayoutUtility.GetRect(60,EditorGUIUtility.singleLineHeight);
                DrawWhiteBox(sr, new Color(0,0,0,0.3f));
                EditorGUI.LabelField(sr, Notes.AllCategorys[note.CategoryID].Name);

                EditorGUILayout.LabelField(note.Note, miniLabelStyle);
                GUILayout.FlexibleSpace();
                if (!string.IsNullOrEmpty(note.CompleteDate))
                {
                    var cDate = DateTime.Parse(note.CompleteDate, cultureInfo);
                    string timeRelative = bl_DevNotesUtils.GetRelativeTimeName(cDate);
                    EditorGUILayout.LabelField($"<size=8><i>{timeRelative}</i></size>", miniLabelStyle);
                }
            }
            else if (note.noteType == bl_DevNotesInfo.NoteType.Separator)
            {
                Rect lr = GUILayoutUtility.GetRect(position.width - 60, 15);
                lr.height = 1;
                lr.y += 9;
                DrawWhiteBox(lr, new Color(1, 1, 1, 0.5f));
            }

            if (GUILayout.Button(new GUIContent(icons[0]), EditorStyles.label, GUILayout.Width(20)))
            {
                Notes.HistoryNotes.RemoveAt(i);
                SaveNotes();
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(2);
            bg = !bg;
        }
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    /// <summary>
    /// 
    /// </summary>
    void DrawFoot()
    {
        GUILayout.BeginArea(new Rect(0, Screen.height - 50, Screen.width, 25));
        GUILayout.BeginHorizontal("box");
        GUILayout.Label("Show: ", GUILayout.Width(50));
        List<string> cl = new List<string>();
        cl.Add("All");
        cl.AddRange(CategoryString.ToArray());
        ShowCat = EditorGUILayout.Popup(ShowCat, cl.ToArray(), EditorStyles.toolbarPopup, GUILayout.Width(70));
        showSort = ShowCat != 0;
        if (GUILayout.Button("History",EditorStyles.toolbarButton, GUILayout.Width(75)))
        {
            windowID = 1;
        }
        if (GUILayout.Button("Stats", EditorStyles.toolbarButton, GUILayout.Width(75)))
        {
            windowID = 2;
        }
        GUILayout.FlexibleSpace();
        if (GUILayout.Button(new GUIContent(icons[3]), EditorStyles.wordWrappedLabel, GUILayout.Width(22)))
        {
            bl_DevNotesCategorys cat = EditorWindow.GetWindow<bl_DevNotesCategorys>();
            cat.SetNotes(Notes, this);
        }
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }

    void MoveField(int index, bool up)
    {
        if (up)
        {
            bl_DevNotesInfo.Info info = new bl_DevNotesInfo.Info();
            info.CategoryID = Notes.Notes[index].CategoryID;
            info.Note = Notes.Notes[index].Note;
            Notes.Notes.RemoveAt(index);
            Notes.Notes.Insert(index - 1, info);
        }
        else
        {
            bl_DevNotesInfo.Info info = new bl_DevNotesInfo.Info();
            info.CategoryID = Notes.Notes[index].CategoryID;
            info.Note = Notes.Notes[index].Note;
            Notes.Notes.RemoveAt(index);
            Notes.Notes.Insert(index + 1, info);
        }
        SaveNotes();
    }

    /// <summary>
    /// 
    /// </summary>
    void AddNewJob()
    {
        if(Notes == null)
        {
            Notes = new bl_DevNotesInfo();
        }
        bl_DevNotesInfo.Info info = new bl_DevNotesInfo.Info();
        info.CategoryID = AddCat;
        info.Note = AddNote;
        info.Comment = AddComment;
        info.noteType = bl_DevNotesInfo.NoteType.Note;
        info.CreateDate = GetTodayDateAsString();
        Notes.Notes.Add(info);
        SaveNotes();
        showAddBox = false;
        AddComment = string.Empty;
        AddNote = string.Empty;
        Repaint();
    }

    void OnCompleteJob(int index)
    {
        Notes.Notes[index].CompleteDate = GetTodayDateAsString();
        Notes.HistoryNotes.Add(Notes.Notes[index]);
        Notes.Notes.RemoveAt(index);
        SaveNotes();
    }

    void AddHistorySeparator()
    {
        bl_DevNotesInfo.Info info = new bl_DevNotesInfo.Info();
        info.CategoryID = 0;
        info.Note = "--";
        info.noteType = bl_DevNotesInfo.NoteType.Separator;
        Notes.HistoryNotes.Add(info);
        SaveNotes();
    }

    void PrintRawHistory()
    {
        string[] list = Notes.HistoryNotes.Select(x => x.Note).ToArray();
        string str = "";
        for (int i = list.Length - 1; i > 0; i--)
        {
            str += $"{list[i]}\n";
        }
        NoteListTexBox.Show(str);
    }

    /// <summary>
    /// 
    /// </summary>
    void SaveNotes()
    {
        settings.SaveList(Notes);
    }

    private List<string> CategoryString
    {
        get
        {
            List<string> list = new List<string>();
            for (int i = 0; i < Notes.AllCategorys.Count; i++)
            {
                list.Add(Notes.AllCategorys[i].Name);
            }
            return list;
        }
    }

    public static void DrawWindowArea()
    {
        GUILayout.BeginArea(new Rect(0, 32, Screen.width, Screen.height - 86));
    }

    public static string GetTodayDateAsString()
    {
        var en = new CultureInfo("en-US");
        return DateTime.Now.ToString(en);
    }

    [MenuItem("Lovatto/Note List")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(bl_DevNoteList));
    }
}