﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[ExecuteInEditMode]
public class SkillController : MonoBehaviour
{
    public GameObject skillPanel;
    public List<Discipline> allDisciplines;
    public List<Skill> defaultSkills;
    public GameObject FirstSkillNodeGrid, SecondSkillNodeGrid, ThirdSkillNodeGrid;
    public Button disciplineButton;
    public Text commentText;
    public Text skillPointsLeftText;

    private int[] CharacterDiciplinesLevels;
    private int characterUnspentSkillPoints;
    bool fadingAnimation = false;

	// Use this for initialization
	void Start ()
    {
        skillPanel.SetActive(false);
	}
	
	// Update is called once per frame
	void Update ()
    {
        if(fadingAnimation)
        {
            if(skillPanel.GetComponent<Image>().color.a < 0.5)
            {
                Color color = skillPanel.GetComponent<Image>().color;
                color.a += 0.02f;
                skillPanel.GetComponent<Image>().color = color;
            }
            else
            {
                fadingAnimation = false;
            }
        }

        if (skillPanel.activeInHierarchy && Input.GetKeyDown(KeyCode.Escape))
        {
            //FindObjectOfType<PlayerController>().canMove = true;
            FindObjectOfType<PhoneController>().canUse = true;
            skillPanel.SetActive(false);

            FindObjectOfType<PlayerController>().GetComponent<StudentData>().discipLevels = CharacterDiciplinesLevels; // Сделать проверку на bool player/npc skills
        }
    }

    public void ShowSkillTree(int[] upgradeLevels) // потом можно будет передать bool касательно данных игрока/npc
    {
        this.gameObject.transform.SetAsLastSibling(); //Сделать панель активной для EventSystem

        fadingAnimation = true;
        //FindObjectOfType<PlayerController>().canMove = false;
        StartCoroutine(FindObjectOfType<PhoneController>().HidePhone());
        FindObjectOfType<PhoneController>().canUse = false;

        characterUnspentSkillPoints = FindObjectOfType<PlayerController>().GetComponent<StudentData>().unspentSkillPoints; //Сделать зависимость от player/npc
        skillPointsLeftText.text = "Непотраченных очков навыков: " + characterUnspentSkillPoints;

        CharacterDiciplinesLevels = upgradeLevels;
        skillPanel.SetActive(true);
        RefreshButtons();
    }
    
    private void RefreshButtons()
    {
        ClearGrid(FirstSkillNodeGrid);
        ClearGrid(SecondSkillNodeGrid);
        ClearGrid(ThirdSkillNodeGrid);

        for (int i = 0; i < allDisciplines.Count; i++)
        {
            Button newButton = (Button)Instantiate(disciplineButton);

            Text[] buttonText = newButton.GetComponentsInChildren<Text>();
            buttonText[0].text = allDisciplines[i].discName;
            buttonText[1].text = CharacterDiciplinesLevels[i] + "/" + allDisciplines[i].disciplineSkills.Count;
            int index = i;

            bool upgradeAvaible = true;
            int sumPointsUsed = 0;
            foreach(int discNum in allDisciplines[i].discNeeded)
            {
                if(CharacterDiciplinesLevels[discNum] == 0)
                {
                    upgradeAvaible = false;
                    break;
                }
                else
                {
                    sumPointsUsed += CharacterDiciplinesLevels[discNum];
                }
            }

            if (sumPointsUsed < allDisciplines[i].sumPointsNeeded && allDisciplines[i].discNeeded.Length != 0)
                upgradeAvaible = false;

            if(upgradeAvaible && CharacterDiciplinesLevels[i] < allDisciplines[i].maxLevel && 
                characterUnspentSkillPoints >= allDisciplines[i].cost) //Навык можно прокачать
            {
                var colors = newButton.GetComponent<Button>().colors;
                colors.normalColor = Color.white;
                colors.highlightedColor = Color.yellow;
                colors.pressedColor = Color.cyan;
                newButton.GetComponent<Button>().colors = colors;

                newButton.GetComponent<Button>().onClick.AddListener(delegate { DisciplineButtonOnClick(index); });
            }
            else if (upgradeAvaible && CharacterDiciplinesLevels[i] < allDisciplines[i].maxLevel && 
                characterUnspentSkillPoints < allDisciplines[i].cost) //Навык можно прокачать но не хватает очков
            {
                var colors = newButton.GetComponent<Button>().colors;
                colors.normalColor = Color.white;
                colors.highlightedColor = Color.gray;
                colors.pressedColor = Color.red;
                newButton.GetComponent<Button>().colors = colors;
            }
            else if (upgradeAvaible && CharacterDiciplinesLevels[i] >= allDisciplines[i].maxLevel) //Навык прокачан до конца
            {
                var colors = newButton.GetComponent<Button>().colors;
                colors.normalColor = Color.green;
                colors.highlightedColor = Color.green;
                colors.pressedColor = Color.green;
                newButton.GetComponent<Button>().colors = colors;
            }
            else //Навык нельзя прокачать
            {
                var colors = newButton.GetComponent<Button>().colors;
                colors.normalColor = Color.gray;
                colors.highlightedColor = Color.gray;
                colors.pressedColor = Color.gray;
                newButton.GetComponent<Button>().colors = colors;
            }

            EventTrigger buttonTrigger = newButton.GetComponent<EventTrigger>();
            EventTrigger.Entry moveOn = new EventTrigger.Entry();
            moveOn.eventID = EventTriggerType.PointerEnter;
            moveOn.callback = new EventTrigger.TriggerEvent();
            moveOn.callback.AddListener(delegate { ShowComment(index); });
            buttonTrigger.triggers.Add(moveOn);

            EventTrigger.Entry moveOut = new EventTrigger.Entry();
            moveOut.eventID = EventTriggerType.PointerExit;
            moveOut.callback = new EventTrigger.TriggerEvent(); ;
            moveOut.callback.AddListener(delegate { ClearComment(); });
            buttonTrigger.triggers.Add(moveOut);

            switch (allDisciplines[i].treeLevel)
            {
                case 1:
                    newButton.transform.SetParent(FirstSkillNodeGrid.transform);
                    break;
                case 2:
                    newButton.transform.SetParent(SecondSkillNodeGrid.transform);
                    break;
                case 3:
                    newButton.transform.SetParent(ThirdSkillNodeGrid.transform);
                    break;
            }

        }

        setButtonsFadeDuration(0.2f);
        //Мне приходиться делать этот бред потому-что из-за fadeDuration кнопка некоторое время после создания 
        //остаеться черной, что приводит к перемигиванию, когда ее цвет обновиться. В итоге надо сначала поставить fadeDuration = 0,
        //поменять цвет кнопок, поместить их на экран и затем сменить их fadeDuration на нормальный
    }
    
    private void DisciplineButtonOnClick(int i)
    {
        this.GetComponent<AudioSource>().Play();
        CharacterDiciplinesLevels[i]++;
        characterUnspentSkillPoints -= allDisciplines[i].cost;
        RefreshButtons();
        skillPointsLeftText.text = "Непотраченных очков навыков: " + characterUnspentSkillPoints;
    }

    private void ShowComment(int discIndex)
    {
       commentText.text = allDisciplines[discIndex].discDescription;
    }

    private void ClearComment()
    {
        commentText.text = string.Empty;
    }

    private void setButtonsFadeDuration(float dur)
    {
        foreach (Transform child in FirstSkillNodeGrid.transform)
        {
            var colors = child.gameObject.GetComponent<Button>().colors;
            colors.fadeDuration = dur;
            child.gameObject.GetComponent<Button>().colors = colors;
        }
        foreach (Transform child in SecondSkillNodeGrid.transform)
        {
            var colors = child.gameObject.GetComponent<Button>().colors;
            colors.fadeDuration = dur;
            child.gameObject.GetComponent<Button>().colors = colors;
        }
        foreach (Transform child in SecondSkillNodeGrid.transform)
        {
            var colors = child.gameObject.GetComponent<Button>().colors;
            colors.fadeDuration = dur;
            child.gameObject.GetComponent<Button>().colors = colors;
        }
    }

    private void ClearGrid(GameObject grid)
    {
        int childs = grid.transform.childCount;
        for (int i = childs - 1; i >= 0; i--)
        {
            GameObject.Destroy(grid.transform.GetChild(i).gameObject);
        }
    }
}


[System.Serializable]
public class Discipline
{
    public string discName;
    public enum subject { Math, Phys, Prog, Tech, Engi, No };
    public subject Subject;
    public int cost;
    public string discDescription;
    public int treeLevel;
    public int[] discNeeded;
    public int sumPointsNeeded;
    public Image icon;
    public List<Skill> disciplineSkills;
    public int maxLevel
    {
        get
        {
            return disciplineSkills.Count;
        }
    }
}

[System.Serializable]
public class Skill
{
    public string Name;
    public string comment;
    public enum SkillTarget { self, teacher};
    public SkillTarget skillTarger;
    public enum SkillType { empty, think, speak, act, passive };
    public SkillType skillType;
    public float bonus;
    public int effectCode;
}
