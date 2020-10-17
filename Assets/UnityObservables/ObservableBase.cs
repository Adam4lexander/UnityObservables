using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using System.Linq;
using System.Reflection;
#endif

namespace UnityObservables {

    public abstract class ObservableBase {
        public abstract event Action OnChanged;
        public abstract void OnValidate();
        protected abstract string ValuePropName { get; }
        protected abstract void OnBeginGui();

#if UNITY_EDITOR
        [CustomPropertyDrawer(typeof(ObservableBase), true)]
        public class ObservableDrawer : PropertyDrawer {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

                var obs = fieldInfo.GetValue(GetParent(property)) as ObservableBase;

                obs.OnBeginGui();

                var val = property.FindPropertyRelative(obs.ValuePropName);

                EditorGUI.BeginChangeCheck();

                EditorGUI.PropertyField(position, val, label, true);

                if (EditorGUI.EndChangeCheck()) {
                    property.serializedObject.ApplyModifiedProperties();
                    obs.OnValidate();
                }
            }

            public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
                var obs = fieldInfo.GetValue(GetParent(property)) as ObservableBase;
                SerializedProperty val = property.FindPropertyRelative(obs.ValuePropName);
                return EditorGUI.GetPropertyHeight(val, label, true);
            }

            public static object GetParent(SerializedProperty prop) {
                var path = prop.propertyPath.Replace(".Array.data[", "[");
                object obj = prop.serializedObject.targetObject;
                var elements = path.Split('.');
                foreach (var element in elements.Take(elements.Length - 1)) {
                    if (element.Contains("[")) {
                        var elementName = element.Substring(0, element.IndexOf("["));
                        var index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                        obj = GetValue(obj, elementName, index);
                    } else {
                        obj = GetValue(obj, element);
                    }
                }
                return obj;
            }

            static object GetValue(object source, string name) {
                if (source == null)
                    return null;
                var type = source.GetType();
                var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (f == null) {
                    var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    if (p == null)
                        return null;
                    return p.GetValue(source, null);
                }
                return f.GetValue(source);
            }

            static object GetValue(object source, string name, int index) {
                var enumerable = GetValue(source, name) as IEnumerable;
                var enm = enumerable.GetEnumerator();
                while (index-- >= 0)
                    enm.MoveNext();
                return enm.Current;
            }
        }
    }
#endif
}