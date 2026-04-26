using UnityEngine;
using System.Collections.Generic;
using System.Text;
using TMPro;

public class WH_CipherP2 : MonoBehaviour
{
    public TextMeshProUGUI mappingTableDisplay;

    public void UpdateMappingDisplay(Dictionary<char, char> map)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("<b><size=130%><color=#00FFFF>[ 키보드 해킹 데이터 ]</color></size></b>\n");

        int count = 0;
        // 알파벳 순서대로 정렬하여 출력
        var sortedMap = new SortedDictionary<char, char>(map);

        foreach (var pair in sortedMap)
        {
            float posValue = (count % 3) * 33f;
            // 입력값(Key)은 흰색, 변환값(Value)은 강조색(황금색)으로 표시
            sb.Append($"<pos={posValue}%>{pair.Key} <color=#555555>→</color> <color=#FFD700><b>{pair.Value}</b></color>");

            count++;
            if (count % 3 == 0) sb.AppendLine();
        }

        mappingTableDisplay.text = sb.ToString();
    }
}