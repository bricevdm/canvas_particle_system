#if UNITY_EDITOR

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEditor;

namespace Utils
{
  public class DefineSymbolsUtils
  {
    [PublicAPI]
    public static void AddDefineSymbols(string[] symbols)
    {
      string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
      List<string> allDefines = definesString.Split(';').ToList();
      allDefines.AddRange(symbols.Except(allDefines));
      PlayerSettings.SetScriptingDefineSymbolsForGroup(
        EditorUserBuildSettings.selectedBuildTargetGroup,
        string.Join(";", allDefines.Distinct().ToArray()));
    }
  }
}

#endif