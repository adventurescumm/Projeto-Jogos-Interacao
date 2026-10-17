using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// O stack trace ObjectPreview.DrawPreview pertence ao Editor, não ao build Web.
// Limpa seleções temporárias ao sair do Play, sem alterar cenas ou esconder erros.
[InitializeOnLoad]
public static class InspectorPreviewRecovery
{
    static InspectorPreviewRecovery()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.ExitingPlayMode) return;
        UnityEngine.Object selected = Selection.activeObject;
        GameObject go = selected as GameObject;
        if (selected is Component component && component != null) go = component.gameObject;
        if (go != null && !EditorUtility.IsPersistent(go) && go.GetComponent<Image>() != null)
            ResetPreview();
    }

    [MenuItem("King of the Kill/Limpar seleção e prévia do Inspector")]
    public static void ResetPreview()
    {
        Selection.objects = Array.Empty<UnityEngine.Object>();
        EditorApplication.delayCall += Rebuild;
    }

    private static void Rebuild()
    {
        ActiveEditorTracker.sharedTracker.ForceRebuild();
        UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
    }
}
