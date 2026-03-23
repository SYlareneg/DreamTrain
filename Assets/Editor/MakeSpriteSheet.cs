#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public class MakeSpriteSheet : EditorWindow
{
    [MenuItem("Tools/Make TMP Sprite Sheet")]
    public static void Pack()
    {
        // 텍스처가 있는 폴더 경로 (프로젝트 상황에 맞게 수정)
        string folderPath = "Assets/Resources/Relics";

        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folderPath });
        if (guids.Length == 0)
        {
            Debug.LogError("[에러] 해당 폴더에서 텍스처를 찾을 수 없습니다. 경로가 맞는지 확인해주세요.");
            return;
        }

        Texture2D[] textures = new Texture2D[guids.Length];

        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            TextureImporter ti = (TextureImporter)AssetImporter.GetAtPath(path);

            // 유니티 내부에서 이미지를 읽고 합치려면 Read/Write 권한이 필요합니다.
            if (ti != null && !ti.isReadable)
            {
                ti.isReadable = true;
                ti.SaveAndReimport();
            }
            textures[i] = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        // 아틀라스 생성 (최대 2048x2048 픽셀, 이미지 간 간격 2픽셀)
        Texture2D atlas = new Texture2D(2048, 2048);
        atlas.PackTextures(textures, 2, 2048);

        // PNG 파일로 Resources 폴더에 저장
        byte[] bytes = atlas.EncodeToPNG();
        string savePath = Application.dataPath + "/Resources/Relics/TMP_Sheet.png";
        File.WriteAllBytes(savePath, bytes);

        AssetDatabase.Refresh();
        Debug.Log($"<color=#3CE74A>스프라이트 시트 생성 완료!</color> 총 {guids.Length}개의 이미지를 성공적으로 합쳤습니다.\n위치: Resources 폴더의 Relics_TMP_Sheet.png");
    }
}
#endif