using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public class DialogueManager : MonoBehaviour
{
    public TextAsset dialogueCSV;
    public List<DialogueEntry> dialogueList = new List<DialogueEntry>();

    void Awake()
    {
        ParseCSV();
    }

    void ParseCSV()
    {
        dialogueList.Clear();
        string[] lines = dialogueCSV.text.Split(new string[] { "\r\n", "\n" }, System.StringSplitOptions.None);

        if (lines.Length <= 1)
        {
            Debug.LogError("CSV file is empty or has only a header row.");
            return;
        }

        int lastParsedId = -1;

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i];

            if (string.IsNullOrWhiteSpace(line) || string.IsNullOrWhiteSpace(line.Replace(",", "")))
                continue;

            string[] cells = Regex.Split(line, ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");

            if (cells.Length < 10)
            {
                Debug.LogWarning($"Skipping line {i + 1}: {line}");
                continue;
            }

            try
            {
                DialogueEntry entry = new DialogueEntry();

                if (!string.IsNullOrEmpty(cells[0]))
                {
                    entry.ID = int.Parse(cells[0]);
                    lastParsedId = entry.ID;
                }
                else
                {
                    entry.ID = lastParsedId;
                }

                entry.BoxLocation = cells[1].Trim();
                entry.Dialogue_KO = cells[2].Trim().Trim('"');
                entry.Dialogue_EN = cells[3].Trim().Trim('"');
                entry.Type = cells[4].Trim();
                entry.SFX = cells[5].Trim();
                entry.IdToGet = string.IsNullOrEmpty(cells[6]) ? 0 : int.Parse(cells[6]);
                entry.IdPoint = string.IsNullOrEmpty(cells[7]) ? 0 : int.Parse(cells[7]);
                entry.NextID = string.IsNullOrEmpty(cells[8]) ? 0 : int.Parse(cells[8]);
                entry.Function = cells[9].Trim();

                dialogueList.Add(entry);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error parsing line {i + 1}: {e.Message}");
            }
        }
    }

    public List<DialogueEntry> GetDialogueOptionsByID(int id)
    {
        return dialogueList.FindAll(d => d.ID == id);
    }
}
