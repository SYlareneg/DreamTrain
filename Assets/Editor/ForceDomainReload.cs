using UnityEditor;

public static class ForceDomainReload
{
    // Unity 에디터 상단 메뉴에 "Tools/Force Domain Reload" 메뉴 항목을 추가합니다.
    [MenuItem("Tools/Force Domain Reload #r")]
    public static void DoDomainReload()
    {
     

        UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();

        UnityEngine.Debug.Log("Manual Domain Reload Requested via Tools menu.");
    }
}