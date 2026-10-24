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
    /// Detail panel for editing dialogue nodes and responses.
    /// Extends the basic detail view with full node editing capabilities.
    /// </summary>
    public class DialogueDetailPanel
    {
        private VisualElement root;
        private DialogueGraph dialogue;
        private ListView nodeListView;
        private VisualElement nodeDetailPanel;
        private DialogueNodeData selectedNode;

        public DialogueDetailPanel(VisualElement parent, DialogueGraph dialogue)
        {
            this.dialogue = dialogue;
            root = parent;
            BuildUI();
        }

        private void BuildUI()
        {
            root.Clear();

            if (dialogue == null || dialogue.nodes == null || dialogue.nodes.Count == 0)
            {
                var emptyLabel = new Label("No nodes in this dialogue");
                emptyLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                emptyLabel.style.flexGrow = 1;
                root.Add(emptyLabel);
                return;
            }

            // Two-panel layout for nodes
            var twoColumn = new TwoPaneSplitView(0, 300, TwoPaneSplitViewOrientation.Horizontal);
            twoColumn.style.flexGrow = 1;
            root.Add(twoColumn);

            // Left panel: Node list
            var leftPanel = new VisualElement();
            leftPanel.name = "nodeListPanel";
            twoColumn.Add(leftPanel);

            // Node list header
            var header = new Toolbar();
            header.Add(new Label("Nodes"));
            leftPanel.Add(header);

            // Add node button
            var addNodeButton = new Button(OnAddNode) { text = "+ Add Node" };
            header.Add(addNodeButton);

            // Node list view
            nodeListView = new ListView();
            nodeListView.name = "nodeListView";
            nodeListView.style.flexGrow = 1;
            nodeListView.makeItem = MakeNodeItem;
            nodeListView.bindItem = BindNodeItem;
            nodeListView.onSelectionChange += OnNodeSelected;
            nodeListView.itemsSource = dialogue.nodes;
            leftPanel.Add(nodeListView);

            // Right panel: Node detail
            nodeDetailPanel = new VisualElement();
            nodeDetailPanel.name = "nodeDetailPanel";
            nodeDetailPanel.style.flexGrow = 1;
            twoColumn.Add(nodeDetailPanel);

            // Initial selection
            if (dialogue.nodes.Count > 0)
            {
                nodeListView.selectedIndex = 0;
            }
        }

        private VisualElement MakeNodeItem()
        {
            var item = new VisualElement();
            item.style.flexDirection = FlexDirection.Column;
            item.style.padding = 4;
            item.style.marginBottom = 2;

            var idLabel = new Label();
            idLabel.name = "idLabel";
            idLabel.style.fontSize = 10;
            idLabel.style.color = Color.gray;
            item.Add(idLabel);

            var speakerLabel = new Label();
            speakerLabel.name = "speakerLabel";
            speakerLabel.style.fontWeight = FontWeight.Bold;
            item.Add(speakerLabel);

            var textLabel = new Label();
            textLabel.name = "textLabel";
            textLabel.style.fontSize = 11;
            textLabel.style.whiteSpace = WhiteSpace.Normal;
            textLabel.style.wordWrap = WordWrap.Wrap;
            item.Add(textLabel);

            return item;
        }

        private void BindNodeItem(VisualElement item, int index)
        {
            var node = dialogue.nodes[index];
            var idLabel = item.Q<Label>("idLabel");
            var speakerLabel = item.Q<Label>("speakerLabel");
            var textLabel = item.Q<Label>("textLabel");

            if (idLabel != null)
            {
                idLabel.text = node.nodeId ?? "No ID";
            }

            if (speakerLabel != null)
            {
                speakerLabel.text = node.speakerId ?? "Unknown";
            }

            if (textLabel != null)
            {
                var text = node.text ?? "";
                textLabel.text = text.Length > 50 ? text.Substring(0, 50) + "..." : text;
            }
        }

        private void OnNodeSelected(IEnumerable<object> selectedItems)
        {
            selectedNode = selectedItems.FirstOrDefault() as DialogueNodeData;
            UpdateNodeDetailPanel();
        }

        private void UpdateNodeDetailPanel()
        {
            nodeDetailPanel.Clear();

            if (selectedNode == null)
            {
                var emptyLabel = new Label("Select a node to edit");
                emptyLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                emptyLabel.style.flexGrow = 1;
                nodeDetailPanel.Add(emptyLabel);
                return;
            }

            // Build node detail view
            var scroll = new ScrollView();
            scroll.style.flexGrow = 1;
            nodeDetailPanel.Add(scroll);

            var content = new VisualElement();
            scroll.Add(content);

            // Node ID
            var idField = new TextField("Node ID");
            idField.value = selectedNode.nodeId ?? "";
            idField.RegisterValueChangedCallback(evt =>
            {
                selectedNode.nodeId = evt.newValue;
                EditorUtility.SetDirty(dialogue);
                nodeListView.Rebuild();
            });
            content.Add(idField);

            // Speaker ID
            var speakerField = new TextField("Speaker ID");
            speakerField.value = selectedNode.speakerId ?? "";
            speakerField.RegisterValueChangedCallback(evt =>
            {
                selectedNode.speakerId = evt.newValue;
                EditorUtility.SetDirty(dialogue);
                nodeListView.Rebuild();
            });
            content.Add(speakerField);

            // Text
            var textField = new TextField("Text");
            textField.value = selectedNode.text ?? "";
            textField.multiline = true;
            textField.RegisterValueChangedCallback(evt =>
            {
                selectedNode.text = evt.newValue;
                EditorUtility.SetDirty(dialogue);
                nodeListView.Rebuild();
            });
            content.Add(textField);

            // Emotion
            var emotionField = new DropdownField("Emotion");
            emotionField.choices = new List<string> { "Neutral", "Happy", "Angry", "Sad", "Surprised", "Skeptical", "Flirty" };
            emotionField.value = selectedNode.emotion.ToString();
            emotionField.RegisterValueChangedCallback(evt =>
            {
                if (System.Enum.TryParse(evt.newValue, out AI.DialogueEmotion emotion))
                {
                    selectedNode.emotion = emotion;
                    EditorUtility.SetDirty(dialogue);
                }
            });
            content.Add(emotionField);

            // Next Node ID
            var nextNodeField = new TextField("Next Node ID");
            nextNodeField.value = selectedNode.nextNodeId ?? "";
            nextNodeField.RegisterValueChangedCallback(evt =>
            {
                selectedNode.nextNodeId = evt.newValue;
                EditorUtility.SetDirty(dialogue);
            });
            content.Add(nextNodeField);

            // Responses section
            var responsesHeader = new Label("Responses");
            responsesHeader.style.fontSize = 14;
            responsesHeader.style.fontWeight = FontWeight.Bold;
            responsesHeader.style.marginTop = 10;
            content.Add(responsesHeader);

            var responsesList = new ListView();
            responsesList.name = "responsesList";
            responsesList.style.height = 200;
            responsesList.makeItem = MakeResponseItem;
            responsesList.bindItem = BindResponseItem;
            responsesList.itemsSource = selectedNode.responses ?? new List<DialogueResponseData>();
            content.Add(responsesList);

            // Add response button
            var addResponseButton = new Button(OnAddResponse) { text = "+ Add Response" };
            content.Add(addResponseButton);

            // Delete node button
            var deleteNodeButton = new Button(OnDeleteNode)
            {
                text = "Delete Node",
                style = { backgroundColor = new Color(0.8f, 0.2f, 0.2f) }
            };
            deleteNodeButton.style.marginTop = 10;
            content.Add(deleteNodeButton);
        }

        private VisualElement MakeResponseItem()
        {
            var item = new VisualElement();
            item.style.flexDirection = FlexDirection.Column;
            item.style.padding = 4;

            var textLabel = new Label();
            textLabel.name = "textLabel";
            item.Add(textLabel);

            return item;
        }

        private void BindResponseItem(VisualElement item, int index)
        {
            var response = selectedNode.responses[index];
            var textLabel = item.Q<Label>("textLabel");

            if (textLabel != null)
            {
                var text = response.text ?? "";
                textLabel.text = text.Length > 40 ? text.Substring(0, 40) + "..." : text;
            }
        }

        private void OnAddNode()
        {
            var newNode = new DialogueNodeData
            {
                nodeId = $"node_{dialogue.nodes.Count + 1}",
                speakerId = "unknown",
                text = "New dialogue text",
                emotion = AI.DialogueEmotion.Neutral,
                responses = new List<DialogueResponseData>()
            };

            dialogue.nodes.Add(newNode);
            EditorUtility.SetDirty(dialogue);
            nodeListView.Rebuild();
            nodeListView.selectedIndex = dialogue.nodes.Count - 1;
        }

        private void OnDeleteNode()
        {
            if (selectedNode == null) return;

            if (!EditorUtility.DisplayDialog(
                "Delete Node",
                $"Are you sure you want to delete node '{selectedNode.nodeId}'?",
                "Delete",
                "Cancel"
            ))
            {
                return;
            }

            dialogue.nodes.Remove(selectedNode);
            selectedNode = null;
            EditorUtility.SetDirty(dialogue);
            nodeListView.Rebuild();
            UpdateNodeDetailPanel();
        }

        private void OnAddResponse()
        {
            if (selectedNode == null) return;

            if (selectedNode.responses == null)
            {
                selectedNode.responses = new List<DialogueResponseData>();
            }

            var newResponse = new DialogueResponseData
            {
                text = "New response",
                nextNodeId = "",
                conditions = new List<DialogueConditionData>()
            };

            selectedNode.responses.Add(newResponse);
            EditorUtility.SetDirty(dialogue);

            var responsesList = nodeDetailPanel.Q<ListView>("responsesList");
            if (responsesList != null)
            {
                responsesList.Rebuild();
            }
        }
    }
}
