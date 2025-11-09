using UnityEngine;

namespace HallControll.SO
{
    [CreateAssetMenu(fileName="DialogueBundle", menuName="Dialogue/Bundle")]
    public class DialogueBundle : ScriptableObject
    {
        public string bundleName;
        public int connectedFileID;

        public bool isBanned = false;
    }
}