using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;
using Utils;

public partial class CanvasParticleSystem
{
  protected override void OnValidate()
  {
    if (Application.isPlaying) return;

    base.OnValidate();

    useLegacyMeshGeneration = true;
    raycastTarget = false;

    var pfxRenderer = GetComponent<ParticleSystemRenderer>();
    pfxRenderer.enabled = false;
    
    Init();
  }
  

  [CustomEditor(typeof(CanvasParticleSystem))]
  public class CanvasParticleSystemEditor : Editor
  {
    public override void OnInspectorGUI()
    {
      CanvasParticleSystem system = (CanvasParticleSystem) target;
      system.material = (Material)EditorGUILayout.ObjectField("Material", system.material, typeof(Material), false);
      Texture inputTexture = (Texture)EditorGUILayout.ObjectField("Texture", system.texture, typeof(Texture), false);
      
      if (ReferenceEquals(inputTexture, system.texture) == false)
      {
        system.texture = inputTexture;
        system.UpdateMaterial();
      }

#if CANVAS_PFX_JOBS
      EditorGUI.BeginDisabledGroup(false);
#else
      GUIStyle style = GUI.skin.GetStyle("HelpBox");
      style.richText = true;

      EditorGUILayout.TextArea("Unity Jobs is are not enabled: Install the <b>Jobs</b> and <b>Burst</b> packages in the Package Manager, " +
                                "\nthen apply the compilation symbol to the Player Settings.", style);

      if (GUILayout.Button("Add \"CANVAS_PFX_JOBS\" symbol to Player Settings"))
      {
        Debug.Log("AddDefineSymbols CANVAS_PFX_JOBS");
        DefineSymbolsUtils.AddDefineSymbols(new[] {"CANVAS_PFX_JOBS"});
      }
      
      EditorGUI.BeginDisabledGroup(true);
#endif
      
      system.useJobs = EditorGUILayout.Toggle("Convert to mesh using Jobs", system.useJobs);

      EditorGUI.EndDisabledGroup();
      
    }
  }
}

#endif
