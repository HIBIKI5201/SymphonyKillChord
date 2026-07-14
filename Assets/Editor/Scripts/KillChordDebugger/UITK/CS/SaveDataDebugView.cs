using KillChord.Runtime.Utility.OutGame.Savedata;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Editor.Debugger
{
    /// <summary>
    ///     存在する全ての<see cref="SaveBase"/>派生クラスをリフレクションで検出し、
    ///     内容の表示・編集・保存を行うビューです。
    /// </summary>
    internal sealed class SaveDataDebugView
    {
        private const string UXML_PATH =
            "Assets/Editor/Scripts/KillChordDebugger/UITK/UXML/SaveDataDebugView.uxml";

        public SaveDataDebugView()
        {
            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UXML_PATH);
            if (visualTree == null)
            {
                Root = new Label($"UXMLが見つかりません: {UXML_PATH}");
                return;
            }

            Root = visualTree.CloneTree();

            _listContainer = Root.Q<VisualElement>("savedata-list");

            Button reloadAllButton = Root.Q<Button>("reload-all-button");
            Button saveAllButton = Root.Q<Button>("save-all-button");

            reloadAllButton?.RegisterCallback<ClickEvent>(_ => BuildSections());
            saveAllButton?.RegisterCallback<ClickEvent>(_ =>
            {
                foreach (SaveDataEntry entry in _entries)
                {
                    entry.Save();
                }
                BuildSections();
            });

            BuildSections();
        }

        /// <summary> このビューのルート要素です。 </summary>
        public VisualElement Root { get; }

        private void BuildSections()
        {
            if (_listContainer == null)
            {
                return;
            }

            _listContainer.Clear();
            _entries.Clear();

            List<Type> saveTypes = TypeCache.GetTypesDerivedFrom<SaveBase>().ToList();

            foreach (Type saveType in saveTypes)
            {
                if (saveType.IsAbstract || saveType.IsGenericTypeDefinition)
                {
                    continue;
                }

                SaveDataEntry entry = new(saveType);

                try
                {
                    entry.Load();
                }
                catch (Exception exception)
                {
                    Label errorLabel = new($"{saveType.Name} の読み込みに失敗しました: {exception.Message}");
                    errorLabel.AddToClassList("kcd-field-row__readonly");
                    _listContainer.Add(errorLabel);
                    continue;
                }

                _entries.Add(entry);
                _listContainer.Add(BuildSection(entry));
            }

            if (_entries.Count == 0)
            {
                Label emptyLabel = new("表示できるセーブデータクラスが見つかりませんでした。");
                emptyLabel.AddToClassList("kcd-field-row__readonly");
                _listContainer.Add(emptyLabel);
            }
        }

        private VisualElement BuildSection(SaveDataEntry entry)
        {
            VisualElement section = new();
            section.AddToClassList("kcd-section");

            Foldout foldout = new() { text = entry.SaveType.Name, value = true };
            foldout.AddToClassList("kcd-section__foldout");
            section.Add(foldout);

            Label pathLabel = new(entry.FilePath + (entry.FileExists ? string.Empty : "（未作成）"));
            pathLabel.AddToClassList("kcd-section__path");
            foldout.Add(pathLabel);

            VisualElement toolbar = new();
            toolbar.AddToClassList("kcd-section__toolbar");

            Label statusLabel = new();
            statusLabel.AddToClassList("kcd-status-label");

            void RefreshStatus()
            {
                statusLabel.text = entry.IsDirty ? "未保存の変更があります" : "保存済み";
                statusLabel.EnableInClassList("kcd-status-label--dirty", entry.IsDirty);
            }
            RefreshStatus();

            VisualElement fieldsContainer = new();
            fieldsContainer.AddToClassList("kcd-fields");

            void RebuildFields()
            {
                fieldsContainer.Clear();
                fieldsContainer.Add(SaveDataFieldViewFactory.CreateFieldsContainer(entry.Instance, () =>
                {
                    entry.MarkDirty();
                    RefreshStatus();
                }));
            }
            RebuildFields();

            Button reloadButton = new(() =>
            {
                entry.Load();
                RebuildFields();
                RefreshStatus();
            })
            { text = "再読み込み" };
            reloadButton.AddToClassList("kcd-button");

            Button saveButton = new(() =>
            {
                entry.Save();
                RefreshStatus();
            })
            { text = "保存" };
            saveButton.AddToClassList("kcd-button");
            saveButton.AddToClassList("kcd-button--primary");

            toolbar.Add(reloadButton);
            toolbar.Add(saveButton);
            toolbar.Add(statusLabel);

            foldout.Add(toolbar);
            foldout.Add(fieldsContainer);

            return section;
        }

        private readonly VisualElement _listContainer;
        private readonly List<SaveDataEntry> _entries = new();

        /// <summary>
        ///     1つのセーブデータクラスに対する、読み込み済みインスタンスと保存状態を保持します。
        /// </summary>
        private sealed class SaveDataEntry
        {
            public SaveDataEntry(Type saveType)
            {
                SaveType = saveType;
                FilePath = Path.Combine(Application.persistentDataPath, $"{saveType.Name}.json");
            }

            /// <summary> このエントリが表す<see cref="SaveBase"/>派生型です。 </summary>
            public Type SaveType { get; }

            /// <summary> セーブファイルのパスです。 </summary>
            public string FilePath { get; }

            /// <summary> 読み込み済みのインスタンスです。 </summary>
            public object Instance { get; private set; }

            /// <summary> ファイルが存在するかどうかです。 </summary>
            public bool FileExists => File.Exists(FilePath);

            /// <summary> 未保存の変更があるかどうかです。 </summary>
            public bool IsDirty { get; private set; }

            /// <summary>
            ///     未保存の変更があることを記録します。
            /// </summary>
            public void MarkDirty()
            {
                IsDirty = true;
            }

            /// <summary>
            ///     ディスクからセーブデータを読み込みます。ファイルが無い場合は既定値のインスタンスにします。
            /// </summary>
            public void Load()
            {
                Instance = Activator.CreateInstance(SaveType);

                if (File.Exists(FilePath))
                {
                    string json = File.ReadAllText(FilePath);
                    JsonUtility.FromJsonOverwrite(json, Instance);
                }

                IsDirty = false;
            }

            /// <summary>
            ///     現在のインスタンスの内容をディスクへ保存します。
            /// </summary>
            public void Save()
            {
                string json = JsonUtility.ToJson(Instance, true);

                string directory = Path.GetDirectoryName(FilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string tempPath = FilePath + ".tmp";
                File.WriteAllText(tempPath, json);

                if (File.Exists(FilePath))
                {
                    File.Delete(FilePath);
                }
                File.Move(tempPath, FilePath);

                IsDirty = false;

                Debug.Log($"[{nameof(SaveDataDebugView)}] 保存しました。Path: {FilePath}");
            }
        }
    }
}
