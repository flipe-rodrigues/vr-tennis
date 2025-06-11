using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class UIManager : Singleton<UIManager> 
{
    // Static fields to store subject information
    public static string subjectName = "name";
    public static string subjectAge = "age";
    public static string subjectSex = "sex";
    public static string subjectHandedness = "handedness";
    public static string subjectTennisExp = "tennis-exp";
    public static string subjectVRExp = "vr-exp";

    // Public fields
    public TMP_InputField nameInput;
    public TMP_InputField ageInput;
    public TMP_Dropdown sexDropdown;
    public TMP_Dropdown handednessDropdown;
    public TMP_Dropdown tennisDropdown;
    public TMP_Dropdown VRDropdown;

    // Read only fields
    [SerializeField, ReadOnly]
    private bool _nameIsFilled;
    [SerializeField, ReadOnly] 
    private bool _ageIsFilled;
    [SerializeField, ReadOnly] 
    private bool _sexIsSelected;
    [SerializeField, ReadOnly] 
    private bool _handednessIsSelected;
    [SerializeField, ReadOnly] 
    private bool _tennisExpIsSelected;
    [SerializeField, ReadOnly] 
    private bool _VRExpIsSelected;

    private void Update ()
    {
	    if (_nameIsFilled && _ageIsFilled && _sexIsSelected && _handednessIsSelected && _tennisExpIsSelected && _VRExpIsSelected)
        {
            subjectName = nameInput.text;
            subjectAge = ageInput.text;
            subjectSex = sexDropdown.options[sexDropdown.value].text;
            subjectHandedness = handednessDropdown.options[handednessDropdown.value].text;
            subjectTennisExp = tennisDropdown.options[tennisDropdown.value].text;
            subjectVRExp = VRDropdown.options[VRDropdown.value].text;

            SceneManager.LoadScene("Main");
        }
	}

    public void UpdateCode()
    {
        _nameIsFilled = !string.IsNullOrEmpty(nameInput.text);
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
        _tennisExpIsSelected = tennisDropdown.value > 0;
    }

    public void UpdateVRExp()
    {
        _VRExpIsSelected = VRDropdown.value > 0;
    }
}
