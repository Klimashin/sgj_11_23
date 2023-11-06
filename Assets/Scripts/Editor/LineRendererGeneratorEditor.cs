using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LineRendererGenerator))]
public class LineRendererGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        var lineRendererGenerator = target as LineRendererGenerator;
        if (lineRendererGenerator == null)
        {
            return;
        }

        if (lineRendererGenerator.LineRenderer != null && lineRendererGenerator.PathCreator != null)
        {
            if (GUILayout.Button("Update line"))
            {
                var vertexPath = lineRendererGenerator.PathCreator.path;
                lineRendererGenerator.LineRenderer.positionCount = vertexPath.NumPoints;
                for (int i = 0; i < vertexPath.NumPoints; i++)
                {
                    lineRendererGenerator.LineRenderer.SetPosition(i, vertexPath.GetPoint(i));
                }
                
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
