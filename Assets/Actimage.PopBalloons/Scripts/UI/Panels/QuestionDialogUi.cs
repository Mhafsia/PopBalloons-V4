using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PopBalloons.Data;

public class QuestionDialogUi : MonoBehaviour
{
    public static QuestionDialogUi Instance { get; private set; }
    
    
    private TextMeshProUGUI textMeshPro;
    private Button yesBtn;
    private Button noBtn;
    

    
    private void Awake()
    {
        Instance = this;
        textMeshPro = transform.Find("Text").GetComponent<TextMeshProUGUI>();
        yesBtn = transform.Find("YesBtn").GetComponent<Button>();
        noBtn = transform.Find("NoBtn").GetComponent<Button >();
        HideQuestion();
    }
    
    public void ShowQuestion(string questionText, Action yesAction, Action noAction)
    {
        gameObject.SetActive(true);

        yesBtn.onClick.AddListener((() =>
        {
            HideQuestion();
            yesAction();
        }));
        noBtn.onClick.AddListener((() =>
        {
            HideQuestion();
            noAction();
        }));
    }

    private void HideQuestion(){
        gameObject.SetActive(false);
    }
    
    public void RevealQuestion(){
        gameObject.SetActive(true);
    }


    public void deleteProfile(PlayerData data)
    {
        ProfilesManager.Instance.DeleteProfile(data.id);
    }
}
