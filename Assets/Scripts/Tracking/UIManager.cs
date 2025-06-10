using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class UIManager : Singleton<UIManager> 
{
    // Static fields to store subject information
    public static string subjectCode = "code";
    public static string subjectAge = "age";
    public static string subjectSex = "sex";
    public static string subjectHandedness = "handedness";
    public static string subjectTennisExp = "tennisxp";
    public static string subjectVRExp = "vrexp";

    // Public fields
    public TMP_InputField codeInput;
    public TMP_InputField ageInput;
    public TMP_Dropdown sexDropdown;
    public TMP_Dropdown handednessDropdown;
    public TMP_Dropdown TennisDropdown;
    public TMP_Dropdown VRDropdown;

    // Read only fields
    [SerializeField, ReadOnly]
    private bool _codeIsFilled;
    [SerializeField, ReadOnly] 
    private bool _ageIsFilled;
    [SerializeField, ReadOnly] 
    private bool _sexIsSelected;
    [SerializeField, ReadOnly] 
    private bool _handednessIsSelected;
    [SerializeField, ReadOnly] 
    private bool _tennisExpIsSelected;
    [SerializeField, ReadOnly] 
    private bool _vrExpIsSelected;

    private void Update ()
    {
	    if (_codeIsFilled && _ageIsFilled && _sexIsSelected && _handednessIsSelected && _tennisExpIsSelected && _vrExpIsSelected)
        {
            subjectCode = codeInput.text;
            subjectAge = ageInput.text;
            subjectTennisExp = TennisDropdown.options[TennisDropdown.value].text;
            subjectVRExp = VRDropdown.options[VRDropdown.value].text;
            subjectSex = sexDropdown.options[sexDropdown.value].text;
            subjectHandedness = handednessDropdown.options[handednessDropdown.value].text;
            SceneManager.LoadScene("Main");
        }
	}

    public void UpdateCode()
    {
        _codeIsFilled = !string.IsNullOrEmpty(codeInput.text);
    }

    public void UpdateAge()
    {
        _ageIsFilled = !string.IsNullOrEmpty(ageInput.text);
    }

    public void UpdateSex()
    {
        _sexIsSelected = sexDropdown.value > 0;
    }
    public void UpdateHandedness()
    {
        _handednessIsSelected = handednessDropdown.value > 0;
    }

    public void UpdateTennisExp()
    {
        _tennisExpIsSelected = TennisDropdown.value > 0;
    }

    public void UpdateVRExp()
    {
        _vrExpIsSelected = VRDropdown.value > 0;
    }
}
