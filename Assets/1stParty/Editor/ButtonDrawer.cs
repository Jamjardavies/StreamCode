using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

[CustomPropertyDrawer(typeof(ButtonAttribute))]
public class ButtonDrawer : PropertyDrawer
{
    private const float Spacer = 4;

    private MethodInfo m_method;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        Rect fieldRect = new Rect(position);
        Rect buttonPos = new Rect(position);

        if (m_method != null)
        {
            fieldRect.height *= 0.5f;
            buttonPos.height *= 0.5f;
        }

        EditorGUI.PropertyField(fieldRect, property, label);

        if (!(attribute is ButtonAttribute button))
        {
            return;
        }

        Object target = property.serializedObject.targetObject;

        if (m_method == null)
        {
            Type type = target.GetType();
            m_method = type.GetMethod(button.Method, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        }
        
        if (m_method == null)
        {
            return;
        }

        buttonPos.y += fieldRect.height + Spacer;
        
        if (GUI.Button(buttonPos, button.Text))
        {
            m_method.Invoke(target, null);
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = base.GetPropertyHeight(property, label);

        return m_method == null ? height : height * 2 + Spacer;
    }
}
