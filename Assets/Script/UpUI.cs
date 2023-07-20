using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UpUI : UICtrl
{
    private void Awake()
    {
        base.Awake();
        uiDict["UpUI"] = this;
    }
    
    void Start()
    {
        ChangeText("Gold", "Gold: ");
        elemDict["EndText"].SetActive(false);
        elemDict["CountDown"].SetActive(false);

        AddButtonListener("SceneSkill/SkillButton0", () => ButtonFunction(0));
    }

    public void ShowEnd(bool isWin)
    {
        elemDict["EndText"].SetActive(true);
        var str = (isWin == true ? "Win" : "Lose");
        ChangeText("EndText", str);
    }

    public void ShowCountDown(int time, bool isShow)
    {
        elemDict["CountDown"].SetActive(isShow);
        if(isShow == true)
            ChangeText("CountDown", time.ToString());
    }

    void ButtonFunction(int index)
    {
        Debug.Log(index);
        if(index == 0)
        {
            StartCoroutine(ClearAllEnemy());
            StartCoroutine(StartCoolDown());
            return;
        }
    }

    public IEnumerator StartCoolDown()
    {
        var image = elemDict["SceneSkill/SkillButton0/SkillImage"].GetComponent<Image>();

        var delay = new WaitForSeconds(0.1f);

        yield break;
    }

    public IEnumerator ClearAllEnemy()
    {
        var delay = new WaitForSeconds(1f);
        yield return delay;

        float shakeTime = 2.0f;
        float shakeAmount = 2.0f;
        float shakeSpeed = 3.0f;
        float elapsedTime = 0;
        Vector3 originalPos = Camera.main.transform.position;

        while (elapsedTime < shakeTime)
        {
            yield return null;

            var randomPoint = originalPos + Random.insideUnitSphere * shakeAmount;
            //randomPoint.z = Camera.main.transform.position.z;
            Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, randomPoint, Time.deltaTime * shakeSpeed);
            elapsedTime += Time.deltaTime;    
        }

        GameManager.instance.ClearAllEnemy();

        while (Camera.main.transform.position != originalPos)
        {
            yield return null;
            Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, originalPos, Time.deltaTime * shakeSpeed * 2);
        }

        
    }
}
