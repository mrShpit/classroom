using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class SkillController : MonoBehaviour
{
    public List<Discipline> allDisciplines;
    public GameObject SkillTreePanel;

    private int[] UpgradeLevels;

	// Use this for initialization
	void Start ()
    {
        SkillTreePanel.SetActive(false);
	}
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    public void ShowSkillTree(int[] upgradeLevels) 
    {
        UpgradeLevels = upgradeLevels;
        SkillTreePanel.SetActive(true);


    }


}

[System.Serializable]
public class Discipline
{
    public string discName;
    public string discDescription;
    public int treeLevel;
    public Image Icon;
    public List<Skill> discSkills;

}

[System.Serializable]
public class Skill
{
    //??
}

