using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field)]
public class ButtonAttribute : PropertyAttribute
{
    public string Text { get; set; }
    public string Method { get; set; }

    public ButtonAttribute(string text, string method)
    {
        Text = text;
        Method = method;
    }
}
