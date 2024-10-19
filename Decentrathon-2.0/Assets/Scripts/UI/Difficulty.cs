using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

public class DifficultyChooser : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown dropdown;
    public int selectedDifficulty = 0;

    public int GetDropdownValue()
    {
        int pickedEntryIndex = dropdown.value;
        Debug.Log(pickedEntryIndex);
        return pickedEntryIndex;
    }

    public void OnDifficultyChanged()
    {
        selectedDifficulty = GetDropdownValue();
        switch (selectedDifficulty)
        {
            case 1:
                selectedDifficulty = 1;
                break;
            case 2:
                selectedDifficulty = 2;
                break;
            case 3:
                selectedDifficulty = 3;
                break;
            default:
                Debug.LogError("Invalid difficulty index: " + selectedDifficulty);
                break;
        }
    }
}