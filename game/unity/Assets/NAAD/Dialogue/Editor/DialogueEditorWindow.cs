using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;
using NAAD.Dialogue;

namespace NAAD.Dialogue.Editor
{
    /// <summary>
    /// Editor window for managing dialogue assets with form-based editing.
    /// Provides list view and detail view for dialogue authoring.
    /// </summary>
    public class DialogueEditorWindow : EditorWindow
    {
        private const string WindowTitle = "Dialogue Editor";
        private const string MenuItemPath = "Window/NAAD/Dialogue Editor";

        private ListView dialogueListView;
        private VisualElement detailPanel;
        private Label statusLabel;
        private Button createButton;
        private Button deleteButton;
        private Button refreshButton;
        private Button bulkExportButton;
        private Button bulkImportButton;

        private List<DialogueGraph> dialogues = new List<DialogueGraph>();
        private DialogueGraph selectedDialogue;
        private List<DialogueGraph> selectedDialogues = new List<DialogueGraph>();

        private string searchQuery = "";
        private string selectedNpcFilter = "All";

        [MenuItem(MenuItemPath)]
        public static void ShowWindow()
        {
            var window = GetWindow<DialogueEditorWindow>();
            window.titleContent = new GUIContent(WindowTitle);
            window.minSize = new Vector2(800, 600);
        }

        private void OnEnable()
        {
            LoadDialogues();
            BuildUI();
        }

        private void OnFocus()
        {
            LoadDialogues();
            RefreshDialogueList();
        }

        private void BuildUI()
        {
            var root = rootVisualElement;

            // Import UXML (if exists, otherwise build programmatically)
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                "Assets/NAAD/Dialogue/Editor/DialogueEditorWindow.uxml"
            );

            if (visualTree != null)
            {
                visualTree.CloneTree(root);
            }
            else
            {
                BuildUIProgrammatically(root);
            }

            // Find elements
            dialogueListView = root.Q<ListView>("dialogueListView");
            detailPanel = root.Q<VisualElement>("detailPanel");
            statusLabel = root.Q<Label>("statusLabel");
            createButton = root.Q<Button>("createButton");
            deleteButton = root.Q<Button>("deleteButton");
            refreshButton = root.Q<Button>("refreshButton");

            // Setup dialogue list view
            SetupDialogueListView();

            // Setup buttons
            if (createButton != null)
            {
                createButton.clicked += OnCreateDialogue;
            }

            if (deleteButton != null)
            {
                deleteButton.clicked += OnDeleteDialogue;
                deleteButton.SetEnabled(false);
            }

            if (refreshButton != null)
            {
                refreshButton.clicked += OnRefresh;
            }

            if (bulkExportButton != null)
            {
                bulkExportButton.clicked += OnBulkExport;
            }

            if (bulkImportButton != null)
            {
                bulkImportButton.clicked += OnBulkImport;
            }

            // Setup search
            var searchField = root.Q<TextField>("searchField");
            if (searchField != null)
            {
                searchField.RegisterValueChangedCallback(evt =>
                {
                    searchQuery = evt.newValue;
                    RefreshDialogueList();
                });
            }

            // Setup NPC filter
            var npcFilter = root.Q<DropdownField>("npcFilter");
            if (npcFilter != null)
            {
                var npcOptions = new List<string> { "All" };
                npcOptions.AddRange(GetUniqueNpcIds());
                npcFilter.choices = npcOptions;
                npcFilter.value = "All";
                npcFilter.RegisterValueChangedCallback(evt =>
                {
                    selectedNpcFilter = evt.newValue;
                    RefreshDialogueList();
                });
            }

            // Initial selection
            UpdateDetailPanel();
        }

        private void BuildUIProgrammatically(VisualElement root)
        {
            // Two-panel layout
            var twoColumn = new TwoPaneSplitView(0, 250, TwoPaneSplitViewOrientation.Horizontal);
            root.Add(twoColumn);

            // Left panel: List view
            var leftPanel = new VisualElement();
            leftPanel.name = "leftPanel";
            twoColumn.Add(leftPanel);

            // Toolbar
            var toolbar = new Toolbar();
            leftPanel.Add(toolbar);

            // Search field
            var searchField = new TextField("Search");
            searchField.name = "searchField";
            toolbar.Add(searchField);

            // NPC filter
            var npcFilter = new DropdownField("NPC Filter");
            npcFilter.name = "npcFilter";
            toolbar.Add(npcFilter);

            // Refresh button
            refreshButton = new Button(OnRefresh) { text = "Refresh" };
            toolbar.Add(refreshButton);

            // Create button
            createButton = new Button(OnCreateDialogue) { text = "Create" };
            toolbar.Add(createButton);

            // Delete button
            deleteButton = new Button(OnDeleteDialogue) { text = "Delete" };
            deleteButton.SetEnabled(false);
            toolbar.Add(deleteButton);

            // Bulk export button
            bulkExportButton = new Button(OnBulkExport) { text = "Export JSON" };
            toolbar.Add(bulkExportButton);

            // Bulk import button
            bulkImportButton = new Button(OnBulkImport) { text = "Import JSON" };
            toolbar.Add(bulkImportButton);

            // Dialogue list view
            dialogueListView = new ListView();
            dialogueListView.name = "dialogueListView";
            dialogueListView.style.flexGrow = 1;
            dialogueListView.makeItem = MakeDialogueItem;
            dialogueListView.bindItem = BindDialogueItem;
            dialogueListView.onSelectionChange += OnDialogueSelected;
            leftPanel.Add(dialogueListView);

            // Status label
            statusLabel = new Label();
            statusLabel.name = "statusLabel";
            statusLabel.style.fontSize = 10;
            statusLabel.style.color = Color.gray;
            leftPanel.Add(statusLabel);

            // Right panel: Detail view
            detailPanel = new VisualElement();
            detailPanel.name = "detailPanel";
            detailPanel.style.flexGrow = 1;
            twoColumn.Add(detailPanel);
        }

        private VisualElement MakeDialogueItem()
        {
            var item = new VisualElement();
            item.style.flexDirection = FlexDirection.Row;
            item.style.padding = 4;
            item.style.marginBottom = 2;

            var nameLabel = new Label();
            nameLabel.name = "nameLabel";
            nameLabel.style.flexGrow = 1;
            item.Add(nameLabel);

            var idLabel = new Label();
            idLabel.name = "idLabel";
            idLabel.style.unityTextAlign = TextAnchor.MiddleRight;
            idLabel.style.fontSize = 10;
            idLabel.style.color = Color.gray;
            item.Add(idLabel);

            return item;
        }

        private void BindDialogueItem(VisualElement item, int index)
        {
            var dialogue = GetFilteredDialogues()[index];
            var nameLabel = item.Q<Label>("nameLabel");
            var idLabel = item.Q<Label>("idLabel");

            if (nameLabel != null)
            {
                nameLabel.text = dialogue.graphName ?? "Unnamed";
            }

            if (idLabel != null)
            {
                idLabel.text = dialogue.graphId ?? "No ID";
            }
        }

        private void SetupDialogueListView()
        {
            if (dialogueListView == null) return;

            dialogueListView.itemsSource = GetFilteredDialogues();
            dialogueListView.style.flexGrow = 1;
            dialogueListView.selectionType = SelectionType.Single;
        }

        private void LoadDialogues()
        {
            dialogues = AssetDatabase.FindAssets("t:DialogueGraph")
                .Select(guid => AssetDatabase.LoadAssetAtPath<DialogueGraph>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(d => d != null)
                .OrderBy(d => d.graphName)
                .ToList();

            UpdateStatusLabel();
        }

        private List<DialogueGraph> GetFilteredDialogues()
        {
            var filtered = dialogues;

            // Apply search filter
            if (!string.IsNullOrEmpty(searchQuery))
            {
                var query = searchQuery.ToLower();
                filtered = filtered.Where(d =>
                    (d.graphName?.ToLower().Contains(query) ?? false) ||
                    (d.graphId?.ToLower().Contains(query) ?? false)
                ).ToList();
            }

            // Apply NPC filter
            if (selectedNpcFilter != "All")
            {
                filtered = filtered.Where(d =>
                    d.nodes != null && d.nodes.Any(n =>
                        n.speakerId?.ToLower() == selectedNpcFilter.ToLower()
                    )
                ).ToList();
            }

            return filtered;
        }

        private void RefreshDialogueList()
        {
            if (dialogueListView != null)
            {
                dialogueListView.itemsSource = GetFilteredDialogues();
                dialogueListView.Rebuild();
            }
            UpdateStatusLabel();
        }

        private void UpdateStatusLabel()
        {
            if (statusLabel != null)
            {
                var filtered = GetFilteredDialogues();
                statusLabel.text = $"Showing {filtered.Count} of {dialogues.Count} dialogues";
            }
        }

        private void OnDialogueSelected(IEnumerable<object> selectedItems)
        {
            var selected = selectedItems.FirstOrDefault() as DialogueGraph;
            selectedDialogue = selected;

            if (deleteButton != null)
            {
                deleteButton.SetEnabled(selected != null);
            }

            UpdateDetailPanel();
        }

        private void UpdateDetailPanel()
        {
            if (detailPanel == null) return;

            detailPanel.Clear();

            if (selectedDialogue == null)
            {
                var emptyLabel = new Label("Select a dialogue to edit");
                emptyLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                emptyLabel.style.flexGrow = 1;
                detailPanel.Add(emptyLabel);
                return;
            }

            // Build detail view
            BuildDetailView();
        }

        private void BuildDetailView()
        {
            // Header
            var header = new Box();
            header.style.marginBottom = 10;
            detailPanel.Add(header);

            var titleLabel = new Label(selectedDialogue.graphName ?? "Unnamed");
            titleLabel.style.fontSize = 16;
            titleLabel.style.fontWeight = FontWeight.Bold;
            header.Add(titleLabel);

            var idLabel = new Label($"ID: {selectedDialogue.graphId ?? "No ID"}");
            idLabel.style.fontSize = 12;
            idLabel.style.color = Color.gray;
            header.Add(idLabel);

            // Properties
            var propertiesBox = new Box();
            propertiesBox.style.marginBottom = 10;
            detailPanel.Add(propertiesBox);

            var graphIdField = new TextField("Graph ID");
            graphIdField.value = selectedDialogue.graphId ?? "";
            graphIdField.RegisterValueChangedCallback(evt =>
            {
                selectedDialogue.graphId = evt.newValue;
                EditorUtility.SetDirty(selectedDialogue);
                RefreshDialogueList();
            });
            propertiesBox.Add(graphIdField);

            var graphNameField = new TextField("Graph Name");
            graphNameField.value = selectedDialogue.graphName ?? "";
            graphNameField.RegisterValueChangedCallback(evt =>
            {
                selectedDialogue.graphName = evt.newValue;
                EditorUtility.SetDirty(selectedDialogue);
                RefreshDialogueList();
            });
            propertiesBox.Add(graphNameField);

            // Node count
            var nodeCountLabel = new Label($"Nodes: {selectedDialogue.nodes?.Count ?? 0}");
            nodeCountLabel.style.fontSize = 12;
            nodeCountLabel.style.color = Color.gray;
            propertiesBox.Add(nodeCountLabel);

            // Save button
            var saveButton = new Button(() =>
            {
                AssetDatabase.SaveAssets();
                Debug.Log($"Saved dialogue: {selectedDialogue.graphName}");
            })
            {
                text = "Save"
            };
            propertiesBox.Add(saveButton);

            // Open in Inspector button
            var inspectButton = new Button(() =>
            {
                Selection.activeObject = selectedDialogue;
                EditorUtility.FocusProjectWindow();
            })
            {
                text = "Open in Inspector"
            };
            propertiesBox.Add(inspectButton);

            // Add DialogueDetailPanel for node editing
            var nodePanel = new VisualElement();
            nodePanel.name = "nodePanel";
            nodePanel.style.flexGrow = 1;
            detailPanel.Add(nodePanel);

            new DialogueDetailPanel(nodePanel, selectedDialogue);
        }

        private void OnCreateDialogue()
        {
            var path = EditorUtility.SaveFilePanelInProject(
                "Create Dialogue Graph",
                "NewDialogue",
                "asset",
                "Save dialogue graph"
            );

            if (string.IsNullOrEmpty(path)) return;

            var dialogue = ScriptableObject.CreateInstance<DialogueGraph>();
            dialogue.graphId = System.IO.Path.GetFileNameWithoutExtension(path).ToLower().Replace(" ", "_");
            dialogue.graphName = System.IO.Path.GetFileNameWithoutExtension(path);

            AssetDatabase.CreateAsset(dialogue, path);
            AssetDatabase.SaveAssets();

            LoadDialogues();
            RefreshDialogueList();

            // Select new dialogue
            dialogueListView.selectedIndex = dialogues.IndexOf(dialogue);
        }

        private void OnDeleteDialogue()
        {
            if (selectedDialogue == null) return;

            if (!EditorUtility.DisplayDialog(
                "Delete Dialogue",
                $"Are you sure you want to delete '{selectedDialogue.graphName}'?",
                "Delete",
                "Cancel"
            ))
            {
                return;
            }

            var path = AssetDatabase.GetAssetPath(selectedDialogue);
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.SaveAssets();

            selectedDialogue = null;
            LoadDialogues();
            RefreshDialogueList();
            UpdateDetailPanel();

            if (deleteButton != null)
            {
                deleteButton.SetEnabled(false);
            }
        }

        private void OnRefresh()
        {
            LoadDialogues();
            RefreshDialogueList();
        }

        private void OnBulkExport()
        {
            var path = EditorUtility.SaveFolderPanel("Export Dialogues", "", "");
            if (string.IsNullOrEmpty(path)) return;

            var exported = 0;
            foreach (var dialogue in dialogues)
            {
                var json = JsonUtility.ToJson(dialogue, true);
                var filename = $"{dialogue.graphId ?? dialogue.graphName ?? "dialogue"}.json";
                var filepath = System.IO.Path.Combine(path, filename);
                System.IO.File.WriteAllText(filepath, json);
                exported++;
            }

            EditorUtility.DisplayDialog("Export Complete", $"Exported {exported} dialogues to {path}", "OK");
        }

        private void OnBulkImport()
        {
            var path = EditorUtility.OpenFolderPanel("Import Dialogues", "", "");
            if (string.IsNullOrEmpty(path)) return;

            var files = System.IO.Directory.GetFiles(path, "*.json");
            if (files.Length == 0)
            {
                EditorUtility.DisplayDialog("Import Failed", "No JSON files found in selected folder", "OK");
                return;
            }

            var imported = 0;
            foreach (var file in files)
            {
                try
                {
                    var json = System.IO.File.ReadAllText(file);
                    var dialogue = ScriptableObject.CreateInstance<DialogueGraph>();
                    JsonUtility.FromJsonOverwrite(json, dialogue);

                    var savePath = $"Assets/NAAD/Dialogue/{System.IO.Path.GetFileNameWithoutExtension(file)}.asset";
                    AssetDatabase.CreateAsset(dialogue, savePath);
                    imported++;
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to import {file}: {e.Message}");
                }
            }

            AssetDatabase.SaveAssets();
            LoadDialogues();
            RefreshDialogueList();

            EditorUtility.DisplayDialog("Import Complete", $"Imported {imported} dialogues from {path}", "OK");
        }

        private List<string> GetUniqueNpcIds()
        {
            return dialogues
                .Where(d => d.nodes != null)
                .SelectMany(d => d.nodes.Select(n => n.speakerId))
                .Where(id => !string.IsNullOrEmpty(id))
                .Distinct()
                .OrderBy(id => id)
                .ToList();
        }
    }
}
