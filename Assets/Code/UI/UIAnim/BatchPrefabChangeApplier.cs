namespace Editor.Prefabs
{
    using Core.Collections;
    using Extensions;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    public static class BatchPrefabChangeApplier
    {
        [MenuItem("CONTEXT/Component/Apply Change To All")]
        public static void ApplyChangeToAll(MenuCommand command)
        {
            var obj = command.context as Component;

            if (obj == null)
            {
                return;
            }

            var po = PrefabUtility.GetPrefabParent(PrefabUtility.FindPrefabRoot(obj.gameObject)) ?? obj.gameObject;

            var prefab = (GameObject)po;

            var children = prefab.GetComponentsInChildren(obj.GetType(), true);

            var match = children.FirstOrDefault(x => x.name == obj.name);

            if (match == null)
            {
                return;
            }

            List<PrefabChange> changes = new List<PrefabChange>();

            GameObject startingObject = match.gameObject;

            HashSet<Component> processed = new HashSet<Component>();

            Queue<Tuple<ComponentReference, Component, Component>> queue = new Queue<Tuple<ComponentReference, Component, Component>>();
            processed.Add(obj);

            queue.Enqueue(Tuple.Create(new ComponentReference()
            {
                RelativePathToGameObject = string.Empty,
                ComponentName = match.GetType().FullName,
                ComponentIndex = 0,
                ReferencedComponentFields = EmptyArray<string>.Inst
            }, match, obj));

            HashSet<string> transformPropertyWhitelist = new HashSet<string>
            {
                "localPosition",
                "localRotation",
                "localScale",
                "anchorMin",
                "anchorMax",
                "sizeDelta",
                "pivot"
            };

            while (queue.Count > 0)
            {
                var tuple = queue.Dequeue();

                Action<string, Type, object, object> processField = (fieldName, fieldType, prefabValue, newValue) =>
                {
                    if (typeof(UnityEngine.Object).IsAssignableFrom(fieldType))
                    {
                        GameObject go = null;
                        Component comp = null;
                        GameObject newGo = null;
                        Component newComp = null;

                        if (prefabValue is GameObject)
                        {
                            go = (GameObject)prefabValue;
                        }
                        if (prefabValue is Component)
                        {
                            comp = ((Component)prefabValue);

                            if (comp != null)
                            {
                                go = comp.gameObject;
                            }
                        }

                        if (newValue is GameObject)
                        {
                            newGo = (GameObject)newValue;
                        }
                        if (newValue is Component)
                        {
                            newComp = ((Component)newValue);

                            if (newComp != null)
                            {
                                newGo = newComp.gameObject;
                            }
                        }

                        if ((go != null && SamePrefab(tuple.Item2.gameObject, go)) || (newGo != null && SamePrefab(tuple.Item3.gameObject, newGo)))
                        {
                            if (newValue == null)
                            {
                                changes.Add(new PrefabGlobalChange()
                                {
                                    Component = tuple.Item1,
                                    FieldName = fieldName,
                                    Target = null
                                });
                            }
                            else
                            {
                                if (prefabValue == null || GetRelativePath(startingObject, newGo) != GetRelativePath(startingObject, go))
                                {
                                    changes.Add(new PrefabLocalChange()
                                    {
                                        Component = tuple.Item1,
                                        FieldName = fieldName,
                                        TargetComponent = new ComponentReference()
                                        {
                                            RelativePathToGameObject = GetRelativePath(startingObject, newGo),
                                            ComponentName = newComp == null ? newGo.GetType().FullName : newComp.GetType().FullName,
                                            ComponentIndex = 0 // TODO : Handle multiple components
                                        }
                                    });
                                }
                                else if (newGo.activeSelf != go.activeSelf)
                                {
                                    ComponentReference newRef = new ComponentReference()
                                    {
                                        RelativePathToGameObject = tuple.Item1.RelativePathToGameObject,
                                        ComponentName = tuple.Item1.ComponentName,
                                        ComponentIndex = tuple.Item1.ComponentIndex,
                                        ReferencedComponentFields = new string[tuple.Item1.ReferencedComponentFields.Length + 2]
                                    };

                                    Array.Copy(tuple.Item1.ReferencedComponentFields, newRef.ReferencedComponentFields, tuple.Item1.ReferencedComponentFields.Length);
                                    newRef.ReferencedComponentFields[newRef.ReferencedComponentFields.Length - 2] = fieldName;
                                    newRef.ReferencedComponentFields[newRef.ReferencedComponentFields.Length - 1] = "gameObject";

                                    changes.Add(new PrefabValueChange()
                                    {
                                        Component = newRef,
                                        FieldName = "activeSelf",
                                        JsonValue = newGo.activeSelf ? "true" : "false"
                                    });
                                }

                                if (newComp != null)
                                {
                                    if (!processed.Contains(newComp))
                                    {
                                        ComponentReference newRef = new ComponentReference()
                                        {
                                            RelativePathToGameObject = tuple.Item1.RelativePathToGameObject,
                                            ComponentName = tuple.Item1.ComponentName,
                                            ComponentIndex = tuple.Item1.ComponentIndex,
                                            ReferencedComponentFields = new string[tuple.Item1.ReferencedComponentFields.Length + 1]
                                        };

                                        Array.Copy(tuple.Item1.ReferencedComponentFields, newRef.ReferencedComponentFields, tuple.Item1.ReferencedComponentFields.Length);
                                        newRef.ReferencedComponentFields[newRef.ReferencedComponentFields.Length - 1] = fieldName;

                                        queue.Enqueue(Tuple.Create(newRef, comp, newComp));
                                        processed.Add(newComp);
                                    }
                                }
                            }
                        }
                        else
                        {
                            if ((prefabValue == null && newValue != null) || (prefabValue != null && !prefabValue.Equals(newValue)))
                            {
                                if ((newComp != null && string.IsNullOrEmpty(AssetDatabase.GetAssetPath(newComp))) ||
                                    (newGo != null && string.IsNullOrEmpty(AssetDatabase.GetAssetPath(newGo))))
                                {
                                    changes.Add(new PrefabLocalChange()
                                    {
                                        Component = tuple.Item1,
                                        FieldName = fieldName,
                                        TargetComponent = new ComponentReference()
                                        {
                                            RelativePathToGameObject = GetRelativePath(startingObject, newGo),
                                            ComponentName = newComp == null ? newGo.GetType().FullName : newComp.GetType().FullName,
                                            ComponentIndex = 0 // TODO : Handle multiple components
                                        }
                                    });

                                    if (newComp != null)
                                    {
                                        if (!processed.Contains(newComp))
                                        {
                                            ComponentReference newRef = new ComponentReference()
                                            {
                                                RelativePathToGameObject = tuple.Item1.RelativePathToGameObject,
                                                ComponentName = tuple.Item1.ComponentName,
                                                ComponentIndex = tuple.Item1.ComponentIndex,
                                                ReferencedComponentFields = new string[tuple.Item1.ReferencedComponentFields.Length + 1]
                                            };

                                            Array.Copy(tuple.Item1.ReferencedComponentFields, newRef.ReferencedComponentFields, tuple.Item1.ReferencedComponentFields.Length);
                                            newRef.ReferencedComponentFields[newRef.ReferencedComponentFields.Length - 1] = fieldName;

                                            queue.Enqueue(Tuple.Create(newRef, comp, newComp));
                                            processed.Add(newComp);
                                        }
                                    }
                                }
                                else
                                {
                                    changes.Add(new PrefabGlobalChange()
                                    {
                                        Component = tuple.Item1,
                                        FieldName = fieldName,
                                        Target = (UnityEngine.Object)newValue
                                    });
                                }
                            }
                        }
                    }
                    else
                    {
                        if ((prefabValue == null && newValue != null) || (prefabValue != null && !prefabValue.Equals(newValue)))
                        {
                            string json = null;
                            // unity's json serializer is garbage for primitives
                            if (newValue.GetType().IsPrimitive || newValue is string)
                            {
                                json = MiniJSON.Json.Serialize(newValue);
                            }
                            else if (newValue.GetType().IsEnum)
                            {
                                json = MiniJSON.Json.Serialize(newValue.ToString());
                            }
                            else
                            {
                                json = JsonUtility.ToJson(newValue);
                            }

                            changes.Add(new PrefabValueChange()
                            {
                                Component = tuple.Item1,
                                FieldName = fieldName,
                                JsonValue = json
                            });
                        }
                    }
                };

                var type = tuple.Item3.GetType();

                Dictionary<string, System.Reflection.FieldInfo> fields = type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).ToDictionary(f => f.Name, f => f);
                type = type.BaseType;

                // go through all parent classes to grab any private fields they might have.
                while (type != typeof(UnityEngine.Object) && type != typeof(System.Object) && type != null)
                {
                    var tmpFields = type.GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                    foreach (var field in tmpFields)
                    {
                        if (!fields.ContainsKey(field.Name))
                        {
                            fields[field.Name] = field;
                        }
                    }
                    type = type.BaseType;
                }

                foreach (var field in fields.Values)
                {
                    if (field.GetCustomAttributes(typeof(NonSerializedAttribute), true).Length > 0)
                        continue;
                    if (field.IsPrivate && field.GetCustomAttributes(typeof(SerializeField), true).Length == 0)
                        continue;

                    // Temporarily disable lists and generic types
                    if (field.FieldType.IsArray || field.FieldType.IsGenericType)
                        continue;

                    var prefabValue = tuple.Item2 != null ? field.GetValue(tuple.Item2) : (field.FieldType.IsValueType ? Activator.CreateInstance(field.FieldType) : null);
                    var newValue = field.GetValue(tuple.Item3);

                    processField(field.Name, field.FieldType, prefabValue, newValue);
                }

                var properties = tuple.Item3.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                foreach (var property in properties)
                {
                    if (property.GetGetMethod() == null && property.GetSetMethod() == null)
                        continue;

                    if (typeof(Transform).IsAssignableFrom(tuple.Item3.GetType()))
                    {
                        // only get properties we need from transform.
                        if (!transformPropertyWhitelist.Contains(property.Name))
                            continue;
                    }
                    else
                    {
                        // only want to serialize the enabled property
                        if (property.Name != "enabled")
                            continue;
                    }

                    var prefabValue = tuple.Item2 != null ? property.GetValue(tuple.Item2, null) : (property.PropertyType.IsValueType ? Activator.CreateInstance(property.PropertyType) : null);
                    var newValue = property.GetValue(tuple.Item3, null);

                    processField(property.Name, property.PropertyType, prefabValue, newValue);
                }
            }
            string changeList = string.Join("\n", changes.Select(x => x.ToString()).ToArray());

            UnityEngine.Debug.Log("The Following Changes have been made to the prefab: \n" + changeList);

            if (changes.Count == 0 || !EditorUtility.DisplayDialog("Prefab Changed", "The Following Changes have been made to the prefab: \n" + changeList, "OK"))
                return;

            bool promptForEachPrefab = EditorUtility.DisplayDialog("Apply To All?", "Do you want a prompt for each prefab?", "YES", "NO");

            DoForAllPrefabs("Applying Changes", path =>
            {
                var p = (GameObject)AssetDatabase.LoadAssetAtPath<GameObject>(path);

                var prefabComps = p.GetComponentsInChildren(obj.GetType(), true);

                if (prefabComps.Length == 0)
                {
                    return;
                }

                if (promptForEachPrefab && !EditorUtility.DisplayDialog("Update Prefab", "Update prefab " + path + "?", "OK"))
                {
                    return;
                }

                GameObject inst;
                try
                {
                    inst = (GameObject)PrefabUtility.InstantiatePrefab(p);
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError("Caught exception [" + e + "] instantiating the prefab: " + path);
                    return;
                }
                inst.name = inst.name.Replace("(Clone)", "");

                var instComps = inst.GetComponentsInChildren(obj.GetType(), true);

                foreach (var instComp in instComps)
                {
                    foreach (var change in changes)
                    {
                        change.ApplyChange(instComp.gameObject);
                    }
                }

                PrefabUtility.ReplacePrefab(inst, p, ReplacePrefabOptions.ConnectToPrefab);

                GameObject.DestroyImmediate(inst);
            });
        }

        public static void DoForAllAssets(string title, string typeName, string extension, Action<string> action, bool modifyAssets = true)
        {
            var paths = AssetDatabase.FindAssets(string.IsNullOrEmpty(typeName) ? "" : ("t:" + typeName)).Select(x => AssetDatabase.GUIDToAssetPath(x)).Where(p => p.EndsWith(extension)).ToArray();

            EditorUtility.DisplayProgressBar(title, "Processing...", 0);

            Stopwatch sw = Stopwatch.StartNew();

            if (modifyAssets)
            {
                AssetDatabase.StartAssetEditing();
            }
            for (int i = 0; i < paths.Length; i++)
            {
                var path = paths[i];

                try
                {
                    if (sw.ElapsedMilliseconds > 100)
                    {
                        sw = Stopwatch.StartNew();
                        EditorUtility.DisplayProgressBar(title, "Processing " + path, i / (float)paths.Length);
                    }
                    
                }
                catch (System.Exception ex)
                {
                    UnityEngine.Debug.LogError(ex);
                }
            }

            if (modifyAssets)
            {
                EditorUtility.DisplayProgressBar("Saving And Reimporting Assets", "This can take a little while", 0f);
                AssetDatabase.StopAssetEditing();
                AssetDatabase.Refresh(ImportAssetOptions.Default);
            }

            EditorUtility.ClearProgressBar();
        }

        public static void DoForAllMaterials(string title, Action<string> action, bool modifyAssets = true)
        {
            DoForAllAssets(title, "Material", ".mat", action, modifyAssets);
        }

        public static void DoForAllPrefabs(string title, Action<string> action, bool modifyAssets = true)
        {
            DoForAllAssets(title, "GameObject", ".prefab", action, modifyAssets);
        }

        public static void DoForAllProceduralMaterials(string title, Action<string> action, bool modifyAssets = true)
        {
            DoForAllAssets(title, "ProceduralMaterial", ".sbsar", action, modifyAssets);
        }

        public static void DoForAllScenes(string title, Action<string> action, bool modifyAssets = true)
        {
            DoForAllAssets(title, "Scene", ".unity", action, modifyAssets);
        }

        public static void DoForAllShaders(string title, Action<string> action, bool modifyAssets = true)
        {
            DoForAllAssets(title, "Shader", ".shader", action, modifyAssets);
        }

        [MenuItem("Assets/+MunkyFun/List Referenced Assets")]
        public static void FindReferencedAssets()
        {
            var selectedAssets = Selection.objects;

            HashSet<string> referencedGuids = new HashSet<string>();
            HashSet<string> visitedPaths = new HashSet<string>();

            foreach (var asset in selectedAssets)
            {
                var path = AssetDatabase.GetAssetPath(asset);

                BuildReferenceList(path, referencedGuids, visitedPaths);
            }

            UnityEngine.Debug.Log("All References\n==========================\n" +
                visitedPaths.Where(x => !x.EndsWith(".meta")).ToArray().JoinString("\n"));
        }

        [MenuItem("Assets/+MunkyFun/List Referencing Assets (Very Slow)")]
        public static void FindReferencingAssets()
        {
            var selectedAssets = Selection.objects;

            List<string> guids = new List<string>();
            Dictionary<string, string> guidToPaths = new Dictionary<string, string>();
            Dictionary<string, HashSet<string>> referencingFiles = new Dictionary<string, HashSet<string>>();

            foreach (var asset in selectedAssets)
            {
                var path = AssetDatabase.GetAssetPath(asset);
                var guid = "guid: " + AssetDatabase.AssetPathToGUID(path);
                guids.Add(guid);
                guidToPaths.Add(guid, path);
                referencingFiles[guid] = new HashSet<string>();
            }

            DoForAllAssets("Searching...", string.Empty, string.Empty, path =>
            {
                if (!System.IO.File.Exists(path))
                    return;

                string text = string.Empty;
                if (IsYaml(path))
                {
                    text = System.IO.File.ReadAllText(path);
                }

                string metaText = System.IO.File.ReadAllText(path + ".meta");

                for (int i = 0; i < guids.Count; i++)
                {
                    if (text.Contains(guids[i]) || metaText.Contains(guids[i]))
                    {
                        referencingFiles[guids[i]].Add(path);
                    }
                }
            }, false);

            UnityEngine.Debug.Log("All Referencing Assets\n==========================\n" +
                referencingFiles.Select(x => guidToPaths[x.Key] + "\n    " + x.Value.ToList().JoinString("\n    ")).ToArray().JoinString("\n"));
        }

        public static string GetFullPath(GameObject go)
        {
            return string.Join("/", GetFullSplitPath(go));
        }

        public static string GetRelativePath(GameObject startingObject, GameObject target)
        {
            if (startingObject == target)
                return ".";

            var pathA = GetFullSplitPath(startingObject);
            var pathB = GetFullSplitPath(target, startingObject);

            int i = 0;
            for (; i < pathA.Length && i < pathB.Length && pathA[i] == pathB[i]; i++) ;

            if (i == pathA.Length && i == pathB.Length)
                return ".";

            string partB = string.Join("/", pathB, i, pathB.Length - i);

            for (int j = i; j < pathA.Length; j++)
            {
                pathA[j] = "..";
            }

            string partA = pathA.Length == i ? "." : string.Join("/", pathA, i, pathA.Length - i);

            return partA + "/" + partB;
        }

        public static bool SamePrefab(GameObject a, GameObject b)
        {
            var aRoot = PrefabUtility.FindPrefabRoot(a);
            var bRoot = PrefabUtility.FindPrefabRoot(b);

            // if this is the prefab, GetPrefabParent will return null
            var aPrefab = PrefabUtility.GetPrefabParent(aRoot) ?? aRoot;
            var bPrefab = PrefabUtility.GetPrefabParent(bRoot) ?? bRoot;

            return aPrefab == bPrefab;
        }

        public class ComponentReference
        {
            public int ComponentIndex;
            public string ComponentName;

            // it might be easier to connect to our target by getting a field reference.
            public string[] ReferencedComponentFields = EmptyArray<string>.Inst;

            public string RelativePathToGameObject;

            public UnityEngine.Object Evaluate(GameObject startingObject, bool createifMissing)
            {
                var source = FindTransform(startingObject, RelativePathToGameObject, createifMissing);

                if (source == null)
                {
                    UnityEngine.Debug.LogError(string.Format("Could not locate object {0} From {1}", RelativePathToGameObject, startingObject));
                    return null;
                }

                var assembiles = AppDomain.CurrentDomain.GetAssemblies();

                Type componentType = assembiles.Select(x => x.GetType(ComponentName)).Where(x => x != null).FirstOrDefault();

                if (componentType == null)
                {
                    UnityEngine.Debug.LogError(string.Format("Could not find type {0}", ComponentName));
                    return null;
                }

                if (componentType == typeof(GameObject))
                {
                    return source.gameObject;
                }

                var components = source.GetComponents(componentType);

                if (components.Length <= ComponentIndex)
                {
                    if (createifMissing)
                    {
                        var newComponents = new UnityEngine.Component[ComponentIndex + 1];

                        Array.Copy(components, newComponents, components.Length);

                        for (int i = components.Length; i <= ComponentIndex; i++)
                        {
                            newComponents[i] = source.gameObject.AddComponent(componentType);
                        }

                        components = newComponents;
                    }
                    else
                    {
                        UnityEngine.Debug.LogError(string.Format("Could not locate Component {0} {1} On Object {2}", ComponentName, ComponentIndex, source));
                        return null;
                    }
                }
                UnityEngine.Object component = components[ComponentIndex];

                for (int i = 0; i < ReferencedComponentFields.Length; i++)
                {
                    var type = component.GetType();

                    var field = type.GetField(ReferencedComponentFields[i], System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                    type = type.BaseType;
                    // check if a base type has the field.
                    while (field == null && type != typeof(UnityEngine.Object) && type != typeof(System.Object) && type != null)
                    {
                        field = type.GetField(ReferencedComponentFields[i], System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        type = type.BaseType;
                    }

                    if (field == null)
                    {
                        var property = component.GetType().GetProperty(ReferencedComponentFields[i], System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                        if (property == null)
                        {
                            UnityEngine.Debug.LogError(string.Format("Could not find field {0} On Component {1}", ReferencedComponentFields[i], component.GetType()));
                            return null;
                        }

                        if (!typeof(UnityEngine.Object).IsAssignableFrom(property.PropertyType))
                        {
                            UnityEngine.Debug.LogError(string.Format("Field {0}.{1} Is not a UnityEngine.Object", component.GetType(), ReferencedComponentFields[i]));
                            return null;
                        }

                        var newComponent = (UnityEngine.Object)property.GetValue(component, null);

                        if (newComponent == null)
                        {
                            UnityEngine.Debug.LogError(string.Format("Field {0}.{1} Is null on {2}", component.GetType(), ReferencedComponentFields[i], component.name));
                            return null;
                        }

                        component = newComponent;
                    }
                    else
                    {
                        if (!typeof(UnityEngine.Object).IsAssignableFrom(field.FieldType))
                        {
                            UnityEngine.Debug.LogError(string.Format("Field {0}.{1} Is not a UnityEngine.Object", component.GetType(), ReferencedComponentFields[i]));
                            return null;
                        }

                        var newComponent = (UnityEngine.Object)field.GetValue(component);

                        if (newComponent == null)
                        {
                            UnityEngine.Debug.LogError(string.Format("Field {0}.{1} Is null on {2}", component.GetType(), ReferencedComponentFields[i], component.name));
                            return null;
                        }
                        component = newComponent;
                    }
                }

                return component;
            }

            public override string ToString()
            {
                if (ComponentName == "UnityEngine.GameObject")
                    return RelativePathToGameObject;

                string result = string.Format("{0}<{1}{2}>", RelativePathToGameObject, ComponentName, ComponentIndex == 0 ? string.Empty : "[" + ComponentIndex + "]");

                for (int i = 0; i < ReferencedComponentFields.Length; i++)
                {
                    result = "(" + result + ")." + ReferencedComponentFields[i];
                }

                return result;
            }

            protected Transform FindTransform(GameObject go, string path, bool createIfMissing)
            {
                Transform t = go.transform;
                string[] splitPath = RelativePathToGameObject.Split();

                return FindTransform(t, splitPath, 0, createIfMissing);
            }

            private Transform FindTransform(Transform t, string[] splitPath, int index, bool createIfMissing)
            {
                if (index == splitPath.Length)
                    return t;

                if (t == null)
                    return null;

                if (splitPath[index] == "." || splitPath[index] == "")
                {
                    // t = t;
                }
                else if (splitPath[index] == "..")
                {
                    t = t.parent;
                }
                else
                {
                    var newT = t.FindChild(splitPath[index]);

                    if (createIfMissing && newT == null)
                    {
                        GameObject go = new GameObject(splitPath[index]);
                        go.transform.SetParent(t, false);
                        newT = go.transform;
                    }

                    t = newT;
                }

                return FindTransform(t, splitPath, index + 1, createIfMissing);
            }
        }

        public abstract class PrefabChange
        {
            public ComponentReference Component;

            public string FieldName;

            public void ApplyChange(GameObject startingObject)
            {
                var component = Component.Evaluate(startingObject, true);

                if (component == null)
                {
                    return;
                }

                var componentType = component.GetType();

                if (componentType == typeof(GameObject))
                {
                    if (FieldName == "activeSelf")
                    {
                        ((GameObject)component).SetActive((bool)GetValue(startingObject, typeof(bool)));
                    }
                    return;
                }

                var field = componentType.GetField(FieldName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                var type = componentType.BaseType;
                // check if a base type has the field.
                while (field == null && type != typeof(UnityEngine.Object) && type != typeof(System.Object) && type != null)
                {
                    field = type.GetField(FieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    type = type.BaseType;
                }

                if (field == null)
                {
                    var property = componentType.GetProperty(FieldName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    if (property == null)
                    {
                        UnityEngine.Debug.LogError(string.Format("Could not find field {0} On Component {1}", FieldName, component));
                    }
                    else
                    {
                        property.SetValue(component, GetValue(startingObject, property.PropertyType), null);
                    }
                }
                else
                {
                    field.SetValue(component, GetValue(startingObject, field.FieldType));
                }
            }

            public override string ToString()
            {
                return string.Format("Change {0}.{1} To {2}", Component, FieldName, ValueString());
            }

            public abstract string ValueString();

            protected abstract object GetValue(GameObject startingObject, Type type);
        }

        public class PrefabGlobalChange : PrefabChange
        {
            public UnityEngine.Object Target;

            public override string ValueString()
            {
                return Target == null ? "null" : "(" + Target.GetType().Name + ")" + Target.name;
            }

            protected override object GetValue(GameObject startingObject, Type type)
            {
                if (Target == null)
                {
                    return null;
                }

                if (!type.IsAssignableFrom(Target.GetType()))
                {
                    UnityEngine.Debug.LogError(string.Format("Cannot assign type {0} to field type {1}", Target.GetType(), type));
                    return null;
                }

                return Target;
            }
        }

        public class PrefabLocalChange : PrefabChange
        {
            public ComponentReference TargetComponent;

            public override string ValueString()
            {
                return TargetComponent.ToString();
            }

            protected override object GetValue(GameObject startingObject, Type type)
            {
                var target = TargetComponent.Evaluate(startingObject, true);

                if (target == null)
                {
                    return null;
                }

                Type componentType = target.GetType();

                if (!type.IsAssignableFrom(componentType))
                {
                    UnityEngine.Debug.LogError(string.Format("Cannot assign type {0} to field type {1}", componentType, type));
                    return null;
                }

                return target;
            }
        }

        public class PrefabValueChange : PrefabChange
        {
            public string JsonValue;

            public override string ValueString()
            {
                return JsonValue;
            }

            protected override object GetValue(GameObject startingObject, Type type)
            {
                if (JsonValue == null)
                {
                    return null;
                }

                try
                {
                    // unity's serializer is terrible at primitives. https://docs.unity3d.com/ScriptReference/JsonUtility.ToJson.html
                    if (type.IsPrimitive || type == typeof(string))
                    {
                        var val = MiniJSON.Json.Deserialize(JsonValue);

                        if (val is long && type == typeof(int))
                            return (int)(long)val;
                        if (val is double && type == typeof(float))
                            return (float)(double)val;
                        return val;
                    }
                    if (type.IsEnum)
                        return Enum.Parse(type, (string)MiniJSON.Json.Deserialize(JsonValue));
                    return JsonUtility.FromJson(JsonValue, type);
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogException(ex);
                    return null;
                }
            }
        }

        private static void BuildReferenceList(string path, HashSet<string> guids, HashSet<string> visitedPaths)
        {
            visitedPaths.Add(path);

            if (!System.IO.File.Exists(path))
                return;

            HashSet<string> pathsToVisit = new HashSet<string>();

            bool shouldRead = IsYaml(path);

            if (shouldRead)
            {
                var text = System.IO.File.ReadAllText(path);
                const string searchString = "guid: ";

                int index = -1;

                while ((index = text.IndexOf(searchString, index + 1)) != -1)
                {
                    var guid = text.Substring(index + searchString.Length, 32);

                    string np = AssetDatabase.GUIDToAssetPath(guid);
                    if (!string.IsNullOrEmpty(np))
                    {
                        guids.Add(guid);
                        pathsToVisit.Add(np);
                    }
                }
            }

            if (!path.EndsWith(".meta"))
            {
                guids.Add(AssetDatabase.AssetPathToGUID(path));
                pathsToVisit.Add(path + ".meta");
            }

            foreach (var p in pathsToVisit)
            {
                if (!visitedPaths.Contains(p))
                {
                    BuildReferenceList(p, guids, visitedPaths);
                }
            }
        }

        private static string[] GetFullSplitPath(GameObject go, GameObject prefabObj = null)
        {
            var t = go.transform;

            List<string> path = new List<string>();

            // if we passed in prefabObj, it means that go might be a new game object that is nested
            // under our prefab.
            bool isChildOfPrefab = prefabObj == null;
            if (prefabObj == null)
                prefabObj = go;

            do
            {
                // check if we have walked past the head of our prefab.
                bool samePrefab = SamePrefab(t.gameObject, prefabObj);

                isChildOfPrefab |= samePrefab;

                if (!samePrefab && isChildOfPrefab)
                    break;

                path.Add(t.name);
                t = t.parent;
            } while (t != null);

            string[] result = new string[path.Count];

            // reverse it.
            for (int i = 0; i < path.Count; i++)
            {
                result[path.Count - i - 1] = path[i];
            }

            return result;
        }

        private static bool IsYaml(string path)
        {
            const string header = "%YAML 1.1";

            byte[] headerBuffer = new byte[header.Length];

            using (var fs = System.IO.File.OpenRead(path))
            {
                int l = fs.Read(headerBuffer, 0, headerBuffer.Length);

                if (l != headerBuffer.Length)
                {
                    return false;
                }
                else if (System.Text.Encoding.UTF8.GetString(headerBuffer) != header)
                {
                    return false;
                }
            }
            return true;
        }
    }
}