using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UpUI : UICtrl
{
    private void Awake()
    {
        base.Awake();
        uiDict["UpUI"] = this;

        ChangeText("Gold", "Gold: ");
        elemDict["EndText"].SetActive(false);
        elemDict["CountDown"].SetActive(false);
        elemDict["FreezeTime"].SetActive(false);
        elemDict["Restart"].SetActive(false);

        AddButtonListener("SceneSkill/SkillButton0", () => ButtonFunction(0));
        AddButtonListener("SceneSkill/SkillButton1", () => ButtonFunction(1));
        AddButtonListener("SceneSkill/SkillButton2", () => ButtonFunction(2));
        AddButtonListener("Restart", RestartGame);
    }

    public void ShowEnd(bool isWin)
    {
        elemDict["EndText"].SetActive(true);
        var str = (isWin == true ? "Win" : "Lose");
        ChangeText("EndText", str);

        elemDict["Restart"].SetActive(true);
    }

    public void ShowCountDown(int time, bool isShow)
    {
        elemDict["CountDown"].SetActive(isShow);
        if(isShow == true)
            ChangeText("CountDown", time.ToString());
    }

    void ButtonFunction(int index)
    {
        if (GameManager.instance.enemyDict.Count == 0)
            return;

        if (GameManager.instance.IsAllTowerDestory() == true)
            return;

        if (index == 0)
        {
            var image = elemDict["SceneSkill/SkillButton0/SkillImage"].GetComponent<Image>();
            if (image.fillAmount < 1.0f)
                return;

            StartCoroutine(ClearAllEnemy());
            StartCoroutine(StartCoolDown(image, 3f));
            return;
        }

        if(index == 1)
        {
            var image = elemDict["SceneSkill/SkillButton1/SkillImage"].GetComponent<Image>();
            if (image.fillAmount < 1.0f)
                return;

            StartCoroutine(ChangeEnemySpeed());
            StartCoroutine(StartCoolDown(image, 3f));
            return;
        }

        if(index == 2)
        {
            var image = elemDict["SceneSkill/SkillButton2/SkillImage"].GetComponent<Image>();
            if (image.fillAmount < 1.0f)
                return;

            GameManager.instance.RepairAllTowers();
            StartCoroutine(StartCoolDown(image, 3f));
            return;
        }
    }

    void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public IEnumerator ChangeEnemySpeed()
    {
        GameManager.instance.ChangeAllEnemySpeed(0.5f);

        elemDict["FreezeTime"].SetActive(true);
        
        for (int countDownTime = 3; countDownTime >= 0; countDownTime--)
        {
            ChangeText("FreezeTime", "Freezing: " + countDownTime);
            yield return new WaitForSeconds(1f);
        }
        
        elemDict["FreezeTime"].SetActive(false);
        
        GameManager.instance.ChangeAllEnemySpeed(1f);
    }

    public IEnumerator StartCoolDown(Image image, float cdTime)
    {
        image.fillAmount = 0;

        var deltaTime = 0.05f;
        var delay = new WaitForSeconds(deltaTime);
        var loopTime = Mathf.Ceil(cdTime / deltaTime) ;

        for (float i = 0; i <= loopTime; i++)
        {
            image.fillAmount = i / loopTime;
            yield return delay;
        }
    }

    public IEnumerator ClearAllEnemy()
    {
        //var delay = new WaitForSeconds(1f);
        //yield return delay;

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
