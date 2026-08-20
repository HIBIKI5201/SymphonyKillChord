using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Editor.Debugger
{
    /// <summary>
    ///     任意のオブジェクトをリフレクションで走査し、編集可能なUI Toolkit要素を組み立てるファクトリです。
    /// </summary>
    internal static class SaveDataFieldViewFactory
    {
        private const int MAX_DEPTH = 8;

        /// <summary>
        ///     対象オブジェクトのシリアライズ対象フィールドをすべて表示・編集するコンテナを作成します。
        /// </summary>
        /// <param name="target"> 表示対象のインスタンスです。 </param>
        /// <param name="onChanged"> 値が変更されたときに呼び出されるコールバックです。 </param>
        public static VisualElement CreateFieldsContainer(object target, Action onChanged)
        {
            return CreateFieldsContainer(target, onChanged, 0);
        }

        private static VisualElement CreateFieldsContainer(object target, Action onChanged, int depth)
        {
            VisualElement container = new();
            container.AddToClassList("kcd-fields");

            if (target == null)
            {
                container.Add(CreateReadonlyLabel("null"));
                return container;
            }

            if (depth > MAX_DEPTH)
            {
                container.Add(CreateReadonlyLabel("階層が深すぎるため省略しました。"));
                return container;
            }

            foreach (FieldInfo field in GetSerializableFields(target.GetType()))
            {
                container.Add(CreateFieldRow(target, field, onChanged, depth));
            }

            return container;
        }

        /// <summary>
        ///     Unityのシリアライズ規則（public、またはSerializeField付与のprivate/protected）に沿って
        ///     対象の型（基底型を含む）のインスタンスフィールドを列挙します。
        /// </summary>
        private static IEnumerable<FieldInfo> GetSerializableFields(Type type)
        {
            List<Type> typeChain = new();
            for (Type current = type; current != null && current != typeof(object); current = current.BaseType)
            {
                typeChain.Add(current);
            }
            typeChain.Reverse();

            HashSet<string> seenNames = new();

            foreach (Type current in typeChain)
            {
                FieldInfo[] fields = current.GetFields(
                    BindingFlags.Instance
                    | BindingFlags.Public
                    | BindingFlags.NonPublic
                    | BindingFlags.DeclaredOnly);

                foreach (FieldInfo field in fields)
                {
                    if (field.IsStatic || !seenNames.Add(field.Name))
                    {
                        continue;
                    }

                    bool isSerialized = field.IsPublic
                        ? field.GetCustomAttribute<NonSerializedAttribute>() == null
                        : field.GetCustomAttribute<SerializeField>() != null;

                    if (isSerialized)
                    {
                        yield return field;
                    }
                }
            }
        }

        private static VisualElement CreateFieldRow(object owner, FieldInfo field, Action onChanged, int depth)
        {
            VisualElement row = new();
            row.AddToClassList("kcd-field-row");

            Label label = new(ObjectNames.NicifyVariableName(field.Name));
            label.AddToClassList("kcd-field-row__label");
            row.Add(label);

            VisualElement valueElement = CreateValueControl(owner, field, onChanged, depth);
            valueElement.AddToClassList("kcd-field-row__value");
            row.Add(valueElement);

            return row;
        }

        private static VisualElement CreateValueControl(object owner, FieldInfo field, Action onChanged, int depth)
        {
            Type fieldType = field.FieldType;

            if (fieldType == typeof(int))
            {
                IntegerField control = new() { value = (int)field.GetValue(owner) };
                control.RegisterValueChangedCallback(evt =>
                {
                    field.SetValue(owner, evt.newValue);
                    onChanged?.Invoke();
                });
                return control;
            }

            if (fieldType == typeof(float))
            {
                FloatField control = new() { value = (float)field.GetValue(owner) };
                control.RegisterValueChangedCallback(evt =>
                {
                    field.SetValue(owner, evt.newValue);
                    onChanged?.Invoke();
                });
                return control;
            }

            if (fieldType == typeof(bool))
            {
                Toggle control = new() { value = (bool)field.GetValue(owner) };
                control.RegisterValueChangedCallback(evt =>
                {
                    field.SetValue(owner, evt.newValue);
                    onChanged?.Invoke();
                });
                return control;
            }

            if (fieldType == typeof(string))
            {
                TextField control = new() { value = (string)field.GetValue(owner) ?? string.Empty };
                control.RegisterValueChangedCallback(evt =>
                {
                    field.SetValue(owner, evt.newValue);
                    onChanged?.Invoke();
                });
                return control;
            }

            if (fieldType.IsEnum)
            {
                EnumField control = new((Enum)field.GetValue(owner));
                control.RegisterValueChangedCallback(evt =>
                {
                    field.SetValue(owner, evt.newValue);
                    onChanged?.Invoke();
                });
                return control;
            }

            if (typeof(UnityEngine.Object).IsAssignableFrom(fieldType))
            {
                return CreateReadonlyLabel("(このエディタでは編集非対応の参照型です)");
            }

            if (TryGetListElementType(fieldType, out Type elementType))
            {
                return CreateListField(owner, field, elementType, onChanged, depth);
            }

            if (fieldType.IsClass)
            {
                return CreateNestedObjectField(owner, field, onChanged, depth);
            }

            return CreateReadonlyLabel(field.GetValue(owner)?.ToString() ?? "null");
        }

        private static VisualElement CreateNestedObjectField(object owner, FieldInfo field, Action onChanged, int depth)
        {
            object value = field.GetValue(owner);
            if (value == null)
            {
                value = CreateDefaultInstance(field.FieldType);
                field.SetValue(owner, value);
            }

            Foldout foldout = new() { text = ObjectNames.NicifyVariableName(field.Name), value = false };
            foldout.AddToClassList("kcd-nested-foldout");
            foldout.Add(CreateFieldsContainer(value, onChanged, depth + 1));
            return foldout;
        }

        private static bool TryGetListElementType(Type fieldType, out Type elementType)
        {
            if (fieldType.IsArray)
            {
                elementType = fieldType.GetElementType();
                return true;
            }

            if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>))
            {
                elementType = fieldType.GetGenericArguments()[0];
                return true;
            }

            elementType = null;
            return false;
        }

        private static VisualElement CreateListField(
            object owner,
            FieldInfo field,
            Type elementType,
            Action onChanged,
            int depth)
        {
            Foldout foldout = new() { value = false };
            foldout.AddToClassList("kcd-nested-foldout");

            void Rebuild()
            {
                object currentValue = field.GetValue(owner);
                IList list = currentValue as IList;
                int count = list?.Count ?? 0;
                foldout.text = $"{ObjectNames.NicifyVariableName(field.Name)} ({count})";

                foldout.Clear();

                VisualElement body = new();
                body.AddToClassList("kcd-list-body");

                if (list != null)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        int index = i;

                        VisualElement itemRow = new();
                        itemRow.AddToClassList("kcd-list-item-row");

                        VisualElement itemValueElement = CreateListItemControl(
                            list,
                            index,
                            elementType,
                            onChanged,
                            depth);
                        itemValueElement.AddToClassList("kcd-list-item-row__value");
                        itemRow.Add(itemValueElement);

                        Button removeButton = new(() =>
                        {
                            RemoveListItem(owner, field, elementType, index);
                            onChanged?.Invoke();
                            Rebuild();
                        })
                        { text = "削除" };
                        removeButton.AddToClassList("kcd-button");
                        removeButton.AddToClassList("kcd-list-item-row__remove");
                        itemRow.Add(removeButton);

                        body.Add(itemRow);
                    }
                }

                Button addButton = new(() =>
                {
                    AddListItem(owner, field, elementType);
                    onChanged?.Invoke();
                    Rebuild();
                })
                { text = "追加" };
                addButton.AddToClassList("kcd-button");
                addButton.AddToClassList("kcd-list__add-button");
                body.Add(addButton);

                foldout.Add(body);
            }

            Rebuild();
            return foldout;
        }

        private static VisualElement CreateListItemControl(
            IList list,
            int index,
            Type elementType,
            Action onChanged,
            int depth)
        {
            if (elementType == typeof(int))
            {
                IntegerField control = new() { value = (int)list[index] };
                control.RegisterValueChangedCallback(evt =>
                {
                    list[index] = evt.newValue;
                    onChanged?.Invoke();
                });
                return control;
            }

            if (elementType == typeof(float))
            {
                FloatField control = new() { value = (float)list[index] };
                control.RegisterValueChangedCallback(evt =>
                {
                    list[index] = evt.newValue;
                    onChanged?.Invoke();
                });
                return control;
            }

            if (elementType == typeof(bool))
            {
                Toggle control = new() { value = (bool)list[index] };
                control.RegisterValueChangedCallback(evt =>
                {
                    list[index] = evt.newValue;
                    onChanged?.Invoke();
                });
                return control;
            }

            if (elementType == typeof(string))
            {
                TextField control = new() { value = (string)list[index] ?? string.Empty };
                control.RegisterValueChangedCallback(evt =>
                {
                    list[index] = evt.newValue;
                    onChanged?.Invoke();
                });
                return control;
            }

            if (elementType.IsEnum)
            {
                EnumField control = new((Enum)list[index]);
                control.RegisterValueChangedCallback(evt =>
                {
                    list[index] = evt.newValue;
                    onChanged?.Invoke();
                });
                return control;
            }

            if (elementType.IsClass && elementType != typeof(string))
            {
                object element = list[index];
                if (element == null)
                {
                    element = CreateDefaultInstance(elementType);
                    list[index] = element;
                }

                Foldout foldout = new() { text = $"[{index}]", value = false };
                foldout.AddToClassList("kcd-nested-foldout");
                foldout.Add(CreateFieldsContainer(element, onChanged, depth + 1));
                return foldout;
            }

            return CreateReadonlyLabel(list[index]?.ToString() ?? "null");
        }

        private static void AddListItem(object owner, FieldInfo field, Type elementType)
        {
            object newElement = CreateDefaultInstance(elementType);

            if (field.FieldType.IsArray)
            {
                Array currentArray = (Array)field.GetValue(owner);
                int oldLength = currentArray?.Length ?? 0;
                Array newArray = Array.CreateInstance(elementType, oldLength + 1);
                if (currentArray != null)
                {
                    Array.Copy(currentArray, newArray, oldLength);
                }
                newArray.SetValue(newElement, oldLength);
                field.SetValue(owner, newArray);
                return;
            }

            IList list = (IList)field.GetValue(owner);
            if (list == null)
            {
                list = (IList)Activator.CreateInstance(field.FieldType);
                field.SetValue(owner, list);
            }
            list.Add(newElement);
        }

        private static void RemoveListItem(object owner, FieldInfo field, Type elementType, int index)
        {
            if (field.FieldType.IsArray)
            {
                Array currentArray = (Array)field.GetValue(owner);
                if (currentArray == null || index < 0 || index >= currentArray.Length)
                {
                    return;
                }

                Array newArray = Array.CreateInstance(elementType, currentArray.Length - 1);
                int newIndex = 0;
                for (int i = 0; i < currentArray.Length; i++)
                {
                    if (i == index)
                    {
                        continue;
                    }
                    newArray.SetValue(currentArray.GetValue(i), newIndex++);
                }
                field.SetValue(owner, newArray);
                return;
            }

            IList list = (IList)field.GetValue(owner);
            if (list == null || index < 0 || index >= list.Count)
            {
                return;
            }
            list.RemoveAt(index);
        }

        /// <summary>
        ///     引数無しコンストラクタを持たない型（例: 独自コンストラクタしか持たないクラス）も含めて
        ///     既定インスタンスを生成します。
        /// </summary>
        private static object CreateDefaultInstance(Type type)
        {
            if (type == typeof(string))
            {
                return string.Empty;
            }

            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }

            ConstructorInfo parameterlessConstructor = type.GetConstructor(Type.EmptyTypes);
            if (parameterlessConstructor != null)
            {
                return parameterlessConstructor.Invoke(null);
            }

            return FormatterServices.GetUninitializedObject(type);
        }

        private static Label CreateReadonlyLabel(string text)
        {
            Label label = new(text);
            label.AddToClassList("kcd-field-row__readonly");
            return label;
        }
    }
}
