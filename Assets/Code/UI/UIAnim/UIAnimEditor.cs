namespace Editor.Inspector
{
    using Core.Collections;
    using UIAnims;
    using UI.UIAnim;
    using Editor.Prefabs;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UI;
    using Extensions;

    [CustomEditor(typeof(UIAnim), editorForChildClasses: true)]
    public class UIAnimEditor : UnityEditor.Editor
    {
        public static AnimationCurve CopiedAnimCurve;

        public static UIAnim PreviewUIAnim;

        public static void FixupAllUIAnims()
        {
            BatchPrefabChangeApplier.DoForAllPrefabs("Fixing up UIAnims", path =>
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

                var anims = inst.GetComponentsInChildren<UIAnim>(true);

                foreach (var anim in anims)
                {
                    foreach (var item in anim.Item)
                    {
                        if (item != null)
                        {
                            item.Property = UIAnimProperty.None;

                            if (item.AnimEndColor != item.AnimStartColor)
                                item.Property |= UIAnimProperty.Color;

                            if (item.AnimEndRotation != item.AnimStartRotation)
                                item.Property |= UIAnimProperty.Rotation;

                            if (item.AnimEndPosition != item.AnimStartPosition)
                                item.Property |= UIAnimProperty.Position3D;

                            if (item.AnimEndSizeDelta != item.AnimStartSizeDelta)
                                item.Property |= UIAnimProperty.SizeDelta;

                            if (item.AnimEndScale != item.AnimStartScale)
                                item.Property |= UIAnimProperty.Scale;

                            if (!string.IsNullOrEmpty(item.UIEffectName))
                                item.Property |= UIAnimProperty.Effect;
                        }
                    }
                }

                if (anims.Length > 0)
                {
                    PrefabUtility.ReplacePrefab(inst, prefab, ReplacePrefabOptions.ConnectToPrefab);
                }

                DestroyImmediate(inst);
            });
        }

        public bool CleanUpNullAnimObjs()
        {
            UIAnim anim = (UIAnim)target;

            List<UIAnimItem> nonNullItems = new List<UIAnimItem>();
            bool foundNull = false;
            for (int i = 0; i < anim.Item.Length; i++)
            {
                if (anim.Item[i].AnimGameObject)
                {
                    nonNullItems.Add(anim.Item[i]);
                }
                else
                {
                    foundNull = true;
                }
            }

            if (foundNull)
            {
                anim.Item = nonNullItems.ToArray();
            }
            return foundNull;
        }

        public override void OnInspectorGUI()
        {
            UIAnim myTarget = (UIAnim)target;
            bool hasNullAnimObjs = false;

            myTarget.AnimName = EditorGUILayout.TextField("Name", myTarget.AnimName);
            myTarget.AnimDuration = EditorGUILayout.FloatField("Duration", myTarget.AnimDuration);
            myTarget.AutoPlay = EditorGUILayout.Toggle("Play On Enable", myTarget.AutoPlay);
            myTarget.LoopingAutoPlay = EditorGUILayout.Toggle("Play On Enable (looping)", myTarget.LoopingAutoPlay);

            if (myTarget.AnimDuration <= 0)
                myTarget.AnimDuration = 0.3f;

            if (myTarget.Item == null)
                myTarget.Item = EmptyArray<UIAnimItem>.Inst;

            if (ShowDetails == null)
                ShowDetails = new bool[myTarget.Item.Length];

            if (myTarget == PreviewUIAnim)
            {
                bool wasAuto = IsAutoPreviewing;
                IsAutoPreviewing = EditorGUILayout.Toggle("Auto Scrub", IsAutoPreviewing);

                if (IsAutoPreviewing)
                {
                    if (!wasAuto)
                    {
                        PreviewTime = EditorApplication.timeSinceStartup;
                    }
                    GUI.enabled = false;
                    EditorGUILayout.Slider(ScrubPosition, 0f, 1f);
                    GUI.enabled = true;
                    // force redraw
                    EditorUtility.SetDirty(target);
                }
                else
                {
                    ScrubPosition = EditorGUILayout.Slider(ScrubPosition, 0f, 1f);
                }

                if (GUILayout.Button("Exit Preview Mode"))
                {
                    ResetScrubPosition();
                    PreviewUIAnim = null;
                }
            }
            else if (PreviewUIAnim == null)
            {
                if (GUILayout.Button("Preview UI Animation"))
                {
                    PreviewUIAnim = myTarget;
                    StorePreviewState();
                    EditorApplication.update += UpdateScrubPosition;
                }

                for (int i = 0; i < myTarget.Item.Length; i++)
                {
                    UIAnimItem obj = myTarget.Item[i];
                    if (!obj.AnimGameObject)
                    {
                        hasNullAnimObjs = true;
                        continue;
                    }
                    EditorGUILayout.BeginHorizontal();

                    string name = obj.AnimGameObject.name;

                    var availableCategories = TopLevelCategories.Where(
                        x => x.Item2(obj.AnimGameObject)).Select(x => x.Item1).ToArray();

                    var items = availableCategories.Where(x => (x & obj.Property) != 0);

                    int count = items.Count();
                    if (count == 1)
                    {
                        name += "." + GetMaskName(obj.Property, items.First());
                    }
                    else if (count > 1 && count <= 3)
                    {
                        name += ".{";
                        bool first = true;
                        foreach (var item in items)
                        {
                            if (!first)
                                name += ", ";
                            first = false;
                            name += GetMaskName(obj.Property, item);
                        }
                        name += "}";
                    }
                    else if (count > 3)
                    {
                        name += ".{Multiple}";
                    }

                    ShowDetails[i] = EditorGUILayout.Foldout(ShowDetails[i], name);

                    obj.AnimCurve = EditorGUILayout.CurveField(obj.AnimCurve, Color.green, new Rect(0, 0, 1, 1), GUILayout.Width(EditorGUIUtility.currentViewWidth * 0.3f - 40));

                    if (GUILayout.Button("C", GUILayout.Width(20)))
                    {
                        CopiedAnimCurve = new AnimationCurve(obj.AnimCurve.keys);
                    }

                    EditorGUI.BeginDisabledGroup(CopiedAnimCurve == null);
                    if (GUILayout.Button("P", GUILayout.Width(20)))
                    {
                        obj.AnimCurve = new AnimationCurve(CopiedAnimCurve.keys);
                    }
                    EditorGUI.EndDisabledGroup();

                    EditorGUILayout.EndHorizontal();

                    if (ShowDetails[i])
                    {
                        DrawVector3Property("Position", ref obj.Property, ref obj.AnimStartPosition, ref obj.AnimEndPosition, UIAnimProperty.PositionX, UIAnimProperty.PositionY, obj.AnimGameObject.transform is RectTransform ? UIAnimProperty.None : UIAnimProperty.PositionZ, availableCategories);

                        DrawVector2Property("Size Delta", ref obj.Property, ref obj.AnimStartSizeDelta, ref obj.AnimEndSizeDelta, UIAnimProperty.SizeDeltaX, UIAnimProperty.SizeDeltaY, availableCategories);

                        DrawVector3Property("Rotation", ref obj.Property, ref obj.AnimStartRotation, ref obj.AnimEndRotation, UIAnimProperty.RotationX, UIAnimProperty.RotationY, UIAnimProperty.RotationZ, availableCategories);

                        if ((obj.Property & UIAnimProperty.Rotation) != 0)
                        {
                            EditorGUI.indentLevel++;
                            obj.UseRelativeRotation = EditorGUILayout.Toggle("Use Relative Rotation", obj.UseRelativeRotation);
                            EditorGUI.indentLevel--;
                        }

                        DrawVector3Property("Scale", ref obj.Property, ref obj.AnimStartScale, ref obj.AnimEndScale, UIAnimProperty.ScaleX, UIAnimProperty.ScaleY, UIAnimProperty.ScaleZ, availableCategories);

                        if (obj.AnimGameObject.GetComponent<Graphic>() != null)
                        {
                            DrawColorProperty("Color", ref obj.Property, ref obj.AnimStartColor, ref obj.AnimEndColor, UIAnimProperty.ColorRed, UIAnimProperty.ColorGreen, UIAnimProperty.ColorBlue, UIAnimProperty.Alpha, availableCategories);
                        }

                        if (obj.AnimGameObject.GetComponent<CanvasGroup>() != null)
                        {
                            DrawColorProperty("Alpha", ref obj.Property, ref obj.AnimStartColor, ref obj.AnimEndColor, UIAnimProperty.None, UIAnimProperty.None, UIAnimProperty.None, UIAnimProperty.Alpha, availableCategories);
                        }

                        DrawPropertyToggle(ref obj.Property, UIAnimProperty.Effect);
                        if ((obj.Property & UIAnimProperty.Effect) != 0)
                        {
                            EditorGUI.indentLevel++;

                            obj.UIEffectName = EditorGUILayout.TextField("PFX Trigger", obj.UIEffectName);

                            float visualEffectTrigger = obj.UIEffectTrigger * myTarget.AnimDuration;
                            // won't play if the trigger time is set to 0!
                            visualEffectTrigger = EditorGUILayout.Slider(visualEffectTrigger, 0.01f, myTarget.AnimDuration);
                            obj.UIEffectTrigger = visualEffectTrigger / myTarget.AnimDuration;

                            EditorGUI.indentLevel--;
                        }

                        if (GUILayout.Button("Remove"))
                        {
                            var arrayCopy = new UIAnimItem[myTarget.Item.Length - 1];
                            System.Array.Copy(myTarget.Item, arrayCopy, i);
                            System.Array.Copy(myTarget.Item, i + 1, arrayCopy, i, arrayCopy.Length - i);

                            var showArrayCopy = new bool[myTarget.Item.Length - 1];
                            System.Array.Copy(ShowDetails, showArrayCopy, i);
                            System.Array.Copy(ShowDetails, i + 1, showArrayCopy, i, showArrayCopy.Length - i);
                            ShowDetails = showArrayCopy;

                            myTarget.Item = arrayCopy;

                            i--;
                        }
                    }
                }

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("<Drag & Drop to Add>");
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                AddedObject = (GameObject)EditorGUILayout.ObjectField(AddedObject, typeof(GameObject), true);
                EditorGUILayout.EndHorizontal();

                if (hasNullAnimObjs || AddedObject)
                {
                    SyncAnimItems(myTarget);
                }
            }
            else
            {
                EditorGUILayout.LabelField("<Locked during UI Animation Preview>");
            }
        }

        private const int VectorLabelWidth = 12;

        private static readonly Tuple<UIAnimProperty, System.Func<GameObject, bool>>[] TopLevelCategories = new Tuple<UIAnimProperty, System.Func<GameObject, bool>>[]
                                {
        Tuple.Create<UIAnimProperty, System.Func<GameObject, bool>>(UIAnimProperty.Color, obj => obj.GetComponent<Graphic>()),
        Tuple.Create<UIAnimProperty, System.Func<GameObject, bool>>(UIAnimProperty.Alpha, obj => obj.GetComponent<CanvasGroup>()),
        Tuple.Create<UIAnimProperty, System.Func<GameObject, bool>>(UIAnimProperty.Position3D, obj => !(obj.transform is RectTransform)),
        Tuple.Create<UIAnimProperty, System.Func<GameObject, bool>>(UIAnimProperty.Position2D, obj => (obj.transform is RectTransform)),
        Tuple.Create<UIAnimProperty, System.Func<GameObject, bool>>(UIAnimProperty.Scale, obj => true),
        Tuple.Create<UIAnimProperty, System.Func<GameObject, bool>>(UIAnimProperty.Rotation, obj => true),
        Tuple.Create<UIAnimProperty, System.Func<GameObject, bool>>(UIAnimProperty.SizeDelta, obj => (obj.transform is RectTransform)),
        Tuple.Create<UIAnimProperty, System.Func<GameObject, bool>>(UIAnimProperty.Effect, obj => true),
        Tuple.Create<UIAnimProperty, System.Func<GameObject, bool>>(UIAnimProperty.Shader, obj => obj.GetComponent<Graphic>() || obj.GetComponent<Renderer>())
        };

        private static bool IsAutoPreviewing;
        private static float lastScrubPosition;
        private static UIAnimItem[] PreviewOriginalState;
        private static double PreviewTime;
        private static float ScrubPosition;
        private GameObject AddedObject = null;
        private bool[] ShowDetails;

        private static void ApplyStoredPreviewState()
        {
            if (PreviewOriginalState == null)
            {
                return;
            }

            for (int i = 0; i < PreviewOriginalState.Length; i++)
            {
                if (!PreviewOriginalState[i].AnimGameObject)
                    continue;

                RectTransform rtx = PreviewOriginalState[i].AnimRectTransform;

                UIAnimItem original = PreviewOriginalState[i];

                if (rtx)
                {
                    rtx.anchoredPosition = original.AnimStartPosition;
                    rtx.offsetMin = original.InitialOffsetMin;
                    rtx.offsetMax = original.InitialOffsetMax;
                }
                else
                {
                    original.AnimTransform.localPosition = original.AnimStartPosition;
                }
                original.AnimTransform.localEulerAngles = original.AnimStartRotation;
                original.AnimTransform.localScale = original.AnimStartScale;

                if (original.AnimGraphic)
                {
                    original.AnimGraphic.color = original.AnimStartColor;
                }
                if (original.AnimCanvasGroup)
                {
                    original.AnimCanvasGroup.alpha = original.AnimStartColor.a;
                }
            }

            PreviewOriginalState = null;
        }

        private static void ResetScrubPosition()
        {
            IsAutoPreviewing = false;
            ScrubPosition = 0;
            ApplyStoredPreviewState();

            UIAnim anim = PreviewUIAnim;
            if (anim)
                anim.CleanUpPFX();

            EditorApplication.update -= UpdateScrubPosition;
        }

        private static void StorePreviewState()
        {
            UIAnim myTarget = PreviewUIAnim;
            PreviewOriginalState = new UIAnimItem[myTarget.Item.Length];
            for (int i = 0; i < myTarget.Item.Length; i++)
            {
                UIAnimItem newItem = new UIAnimItem();
                newItem.AnimGameObject = myTarget.Item[i].AnimGameObject;
                newItem.AnimTransform = newItem.AnimGameObject.transform;
                RectTransform rtx = newItem.AnimTransform as RectTransform;
                if (rtx)
                {
                    newItem.AnimRectTransform = rtx;

                    newItem.InitialOffsetMin = rtx.offsetMin;
                    newItem.InitialOffsetMax = rtx.offsetMax;

                    newItem.AnimStartPosition = rtx.anchoredPosition;
                }
                else
                {
                    newItem.AnimStartPosition = newItem.AnimTransform.localPosition;
                }

                newItem.AnimStartRotation = newItem.AnimTransform.localEulerAngles;
                newItem.AnimStartScale = newItem.AnimTransform.localScale;

                Graphic newGraphic = newItem.AnimGameObject.GetComponent<Graphic>();
                if (newGraphic)
                {
                    newItem.AnimGraphic = newGraphic;
                    newItem.AnimStartColor = newGraphic.color;
                }
                CanvasGroup newCanvasGroup = newItem.AnimGameObject.GetComponent<CanvasGroup>();
                if (newCanvasGroup)
                {
                    newItem.AnimCanvasGroup = newCanvasGroup;
                    newItem.AnimStartColor = new Color(0, 0, 0, newCanvasGroup.alpha);
                }

                PreviewOriginalState[i] = newItem;
            }
        }

        private static void UpdateScrubPosition()
        {
            if (!PreviewUIAnim)
            {
                ResetScrubPosition();
                return;
            }

            if (IsAutoPreviewing)
            {
                if (EditorApplication.timeSinceStartup != PreviewTime)
                {
                    var delta = (EditorApplication.timeSinceStartup - PreviewTime) / PreviewUIAnim.AnimDuration;
                    ScrubPosition += Mathf.Clamp01((float)delta);
                    if (ScrubPosition >= 1)
                    {
                        ScrubPosition--;
                    }
                    PreviewTime = EditorApplication.timeSinceStartup;
                }
            }
            PreviewUIAnim.ForceLerp(ScrubPosition);
        }

        private void DrawColorProperty(string title, ref UIAnimProperty mask, ref Color start, ref Color end, UIAnimProperty red, UIAnimProperty green, UIAnimProperty blue, UIAnimProperty alpha, IEnumerable<UIAnimProperty> availableProps)
        {
            UIAnimProperty colorMask = (red | green | blue | alpha);
            if (!availableProps.Contains(colorMask))
                return;

            DrawPropertyToggle(ref mask, colorMask);
            EditorGUI.indentLevel++;
            if ((mask & colorMask) != 0)
            {
                DrawPropertyToggle(ref mask, red);
                DrawPropertyToggle(ref mask, green);
                DrawPropertyToggle(ref mask, blue);
                if (colorMask != alpha)
                    DrawPropertyToggle(ref mask, alpha);

                if ((mask & red) != 0
                    || (mask & green) != 0
                    || (mask & blue) != 0)
                {
                    start = EditorGUILayout.ColorField("Start " + title, start);
                    end = EditorGUILayout.ColorField("End " + title, end);
                }
                else if ((mask & alpha) != 0)
                {
                    start.a = EditorGUILayout.Slider("Start " + title, start.a, 0f, 1f);
                    end.a = EditorGUILayout.Slider("End " + title, end.a, 0f, 1f);
                }
            }
            EditorGUI.indentLevel--;
        }

        private void DrawPropertyToggle(ref UIAnimProperty mask, UIAnimProperty value)
        {
            if (value == UIAnimProperty.None)
                return;

            bool wasEnabled = (value & mask) != 0;
            bool enabled = EditorGUILayout.Toggle(value.ToString(), wasEnabled);

            if (enabled && !wasEnabled)
            {
                mask |= value;
            }
            else if (!enabled && wasEnabled)
            {
                mask &= ~value;
            }
        }

        private void DrawVector2Property(string title, ref UIAnimProperty mask, ref Vector2 start, ref Vector2 end, UIAnimProperty x, UIAnimProperty y, IEnumerable<UIAnimProperty> availableProps)
        {
            Vector3 start3 = start, end3 = end;
            DrawVector3Property(title, ref mask, ref start3, ref end3, x, y, UIAnimProperty.None, availableProps);
            start = start3;
            end = end3;
        }

        private void DrawVector3Property(string title, ref UIAnimProperty mask, ref Vector3 start, ref Vector3 end, UIAnimProperty x, UIAnimProperty y, UIAnimProperty z, IEnumerable<UIAnimProperty> availableProps)
        {
            UIAnimProperty vectorMask = (x | y | z);
            if (!availableProps.Contains(vectorMask))
                return;

            DrawPropertyToggle(ref mask, vectorMask);
            EditorGUI.indentLevel++;
            if ((mask & vectorMask) != 0)
            {
                DrawPropertyToggle(ref mask, x);
                DrawPropertyToggle(ref mask, y);
                DrawPropertyToggle(ref mask, z);

                var controlRect = EditorGUILayout.GetControlRect(false);
                EditorGUI.LabelField(controlRect, "Start " + title);

                EditorGUI.indentLevel--;

                controlRect.x += EditorGUIUtility.labelWidth;
                controlRect.width -= EditorGUIUtility.labelWidth;

                EditorGUIUtility.labelWidth = VectorLabelWidth;

                controlRect.width /= 3;

                if ((mask & x) != 0)
                {
                    start.x = EditorGUI.FloatField(controlRect, "X", start.x);
                }
                controlRect.x += controlRect.width;
                if ((mask & y) != 0)
                {
                    start.y = EditorGUI.FloatField(controlRect, "Y", start.y);
                }
                controlRect.x += controlRect.width;
                if ((mask & z) != 0)
                {
                    start.z = EditorGUI.FloatField(controlRect, "Z", start.z);
                }

                EditorGUIUtility.labelWidth = 0;
                EditorGUI.indentLevel++;

                controlRect = EditorGUILayout.GetControlRect(false);
                EditorGUI.LabelField(controlRect, "End " + title);

                controlRect.x += EditorGUIUtility.labelWidth;
                controlRect.width -= EditorGUIUtility.labelWidth;

                EditorGUI.indentLevel--;
                EditorGUIUtility.labelWidth = VectorLabelWidth;

                controlRect.width /= 3;

                if ((mask & x) != 0)
                {
                    end.x = EditorGUI.FloatField(controlRect, "X", end.x);
                }
                controlRect.x += controlRect.width;
                if ((mask & y) != 0)
                {
                    end.y = EditorGUI.FloatField(controlRect, "Y", end.y);
                }
                controlRect.x += controlRect.width;
                if ((mask & z) != 0)
                {
                    end.z = EditorGUI.FloatField(controlRect, "Z", end.z);
                }

                EditorGUIUtility.labelWidth = 0;
                EditorGUI.indentLevel++;
            }
            EditorGUI.indentLevel--;
        }

        private string GetMaskName(UIAnimProperty mask, UIAnimProperty value)
        {
            UIAnimProperty maskedValue = (mask & value);

            if (maskedValue == value)
                return value.ToString();

            int bits = 0;
            int firstShift = -1;
            for (int i = 0; i < 32; i++)
            {
                bits += (((int)value >> i) & 1);
                if (bits > 0 && firstShift < 0)
                    firstShift = i;
            }

            string nameBase = value.ToString() + ".";

            int shiftedMaskedValue = (int)maskedValue >> firstShift;
            if (bits == 4)
            {
                if ((shiftedMaskedValue & 1) != 0)
                    nameBase += "r";
                if ((shiftedMaskedValue & 2) != 0)
                    nameBase += "g";
                if ((shiftedMaskedValue & 4) != 0)
                    nameBase += "b";
                if ((shiftedMaskedValue & 8) != 0)
                    nameBase += "a";
            }
            else
            {
                if ((shiftedMaskedValue & 1) != 0)
                    nameBase += "x";
                if ((shiftedMaskedValue & 2) != 0)
                    nameBase += "y";
                if ((shiftedMaskedValue & 4) != 0)
                    nameBase += "z";
            }
            return nameBase;
        }

        private void OnDisable()
        {
            if (target == PreviewUIAnim)
            {
                IsAutoPreviewing = false;
            }
        }

        private void SyncAnimItems(UIAnim anim)
        {
            bool isDirty = CleanUpNullAnimObjs();

            if (anim.Item == null && AddedObject == null)
            {
                anim.Item = EmptyArray<UIAnimItem>.Inst;
            }
            else if (AddedObject != null)
            {
                UIAnimItem newItem = new UIAnimItem();
                newItem.AnimCurve = new AnimationCurve();
                newItem.AnimGameObject = AddedObject;
                newItem.AnimTransform = AddedObject.transform;
                newItem.AnimRectTransform = newItem.AnimTransform as RectTransform;
                if (newItem.AnimRectTransform)
                {
                    newItem.AnimStartPosition = newItem.AnimRectTransform.anchoredPosition;
                    newItem.AnimEndPosition = newItem.AnimRectTransform.anchoredPosition;
                    newItem.AnimStartSizeDelta = newItem.AnimRectTransform.sizeDelta;
                    newItem.AnimEndSizeDelta = newItem.AnimRectTransform.sizeDelta;
                    newItem.InitialPosition = newItem.AnimRectTransform.anchoredPosition;
                    newItem.InitialOffsetMin = newItem.AnimRectTransform.offsetMin;
                    newItem.InitialOffsetMax = newItem.AnimRectTransform.offsetMax;

                    if (newItem.AnimRectTransform.anchorMin.x != newItem.AnimRectTransform.anchorMax.x
                        || newItem.AnimRectTransform.anchorMin.y != newItem.AnimRectTransform.anchorMax.y)
                    {
                        newItem.UseRelativeSize = true;
                    }
                }
                else
                {
                    newItem.AnimStartPosition = newItem.AnimTransform.localPosition;
                    newItem.AnimEndPosition = newItem.AnimTransform.localPosition;
                }

                newItem.AnimStartRotation = newItem.AnimTransform.localEulerAngles;
                newItem.AnimEndRotation = newItem.AnimTransform.localEulerAngles;
                newItem.AnimStartScale = newItem.AnimTransform.localScale;
                newItem.AnimEndScale = newItem.AnimTransform.localScale;

                Graphic newGraphic = AddedObject.GetComponent<Graphic>();
                if (newGraphic)
                {
                    newItem.AnimGraphic = newGraphic;
                    newItem.AnimStartColor = newGraphic.color;
                    newItem.AnimEndColor = newGraphic.color;
                }
                else
                {
                    CanvasGroup canvasGroup = AddedObject.GetComponent<CanvasGroup>();
                    if (canvasGroup)
                    {
                        newItem.AnimCanvasGroup = canvasGroup;
                        newItem.AnimStartColor = new Color(1, 1, 1, canvasGroup.alpha);
                        newItem.AnimEndColor = newItem.AnimStartColor;
                    }
                }

                if (anim.Item == null)
                {
                    anim.Item = new UIAnimItem[1];
                    anim.Item[0] = newItem;
                    ShowDetails = new bool[1];
                }
                else
                {
                    UIAnimItem[] newArray = new UIAnimItem[anim.Item.Length + 1];
                    anim.Item.CopyTo(newArray, 0);
                    newArray[newArray.Length - 1] = newItem;
                    anim.Item = newArray;
                }

                AddedObject = null;
            }

            if (ShowDetails == null)
            {
                ShowDetails = new bool[anim.Item.Length];
            }
            else if (ShowDetails.Length != anim.Item.Length)
            {
                bool[] newShow = new bool[anim.Item.Length];
                for (int i = 0; i < newShow.Length; i++)
                {
                    if (i < ShowDetails.Length)
                    {
                        newShow[i] = ShowDetails[i];
                    }
                }
                ShowDetails = newShow;
            }

            if (isDirty)
            {
                string desc = "UIAnim has NULL objects that were cleaned up. Please APPLY these changes in the inspector!\n AnimName = "
                    + anim.AnimName
                    + "\n gameObject.name = "
                    + anim.gameObject.name;

                EditorUtility.DisplayDialog("UIAnim Nulls", desc, "Ok");
            }
        }
    }
}