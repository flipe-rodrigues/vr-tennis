using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(TruncatedExponentialDistribution))]
public class TruncatedExponentialDistributionPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // Label
        position = EditorGUI.PrefixLabel(position, label);

        // Draw min/mean/max as a foldout
        SerializedProperty minProp = property.FindPropertyRelative("min");
        SerializedProperty meanProp = property.FindPropertyRelative("mean");
        SerializedProperty maxProp = property.FindPropertyRelative("max");
        SerializedProperty pdfProp = property.FindPropertyRelative("_pdf");

        Rect fieldRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField(fieldRect, minProp);
        fieldRect.y += EditorGUIUtility.singleLineHeight + 2;
        EditorGUI.PropertyField(fieldRect, meanProp);
        fieldRect.y += EditorGUIUtility.singleLineHeight + 2;
        EditorGUI.PropertyField(fieldRect, maxProp);
        fieldRect.y += EditorGUIUtility.singleLineHeight + 2;

        // Draw PDF curve tightly framed
        if (pdfProp != null && pdfProp.animationCurveValue != null && pdfProp.animationCurveValue.length > 0)
        {
            AnimationCurve curve = pdfProp.animationCurveValue;
            float minX = curve.keys[0].time;
            float maxX = curve.keys[curve.length - 1].time;
            float minY = float.MaxValue, maxY = float.MinValue;
            foreach (var k in curve.keys)
            {
                if (k.value < minY) minY = k.value;
                if (k.value > maxY) maxY = k.value;
            }
            if (Mathf.Approximately(minY, maxY)) maxY = minY + 1f;

            Rect curveRect = new Rect(minX, minY, maxX - minX, maxY - minY);

            pdfProp.animationCurveValue = EditorGUI.CurveField(
                fieldRect,
                "PDF",
                curve,
                Color.green,
                curveRect
            );
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // space for 4 fields (min/mean/max/pdf) + spacing
        return (EditorGUIUtility.singleLineHeight + 2) * 4;
    }
}
