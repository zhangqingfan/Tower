using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UICtrl : MonoBehaviour
{
    public static UICtrl instance { get; private set; }
    public static Dictionary<string, UICtrl> uiDict = new Dictionary<string, UICtrl>();

    public Dictionary<string, GameObject> elemDict = new Dictionary<string, GameObject>();
    void LoadAllElems(GameObject root, string path)
    {
        foreach(Transform tf in root.transform)
        {
            var key = path + tf.gameObject.name;
            
            if (elemDict.ContainsKey(key) == true)
                continue;

            elemDict[key] = tf.gameObject;
            LoadAllElems(tf.gameObject, key + "/");
        }
    }

    public UICtrl GetUI(string key)
    {
        if (uiDict.ContainsKey(key) == false)
            return null;

        return uiDict[key];
    }

    public void Awake()
    {
        instance = this;
        LoadAllElems(this.gameObject, "");
    }

    public void AddButtonListener(string key, UnityAction onClick)
    {
        if (elemDict.ContainsKey(key) == false)
        {
            Debug.LogWarning("Can not find Button: " + key);
            return;
        }
            
        var button = elemDict[key].GetComponent<Button>();
        if (button == null)
        {
            Debug.LogWarning("Can not find Button: " + key);
            return;
        }

        button.onClick.AddListener(onClick);
    }

    public void AddSliderListener(string key, UnityAction<float> onValueChange)
    {
        if (elemDict.ContainsKey(key) == false)
        {
            Debug.LogWarning("Can not find Slider: " + key);
            return;
        }

        var slider = elemDict[key].GetComponent<Slider>();
        if (slider == null)
        {
            Debug.LogWarning("Can not find Slider: " + key);
            return;
        }

        slider.onValueChanged.AddListener(onValueChange);
    }

    public void ChangeText(string key, string words)
    {
        if (elemDict.ContainsKey(key) == false)
        {
            Debug.LogWarning("Can not find Text: " + key);
            return;
        }

        var text = elemDict[key].GetComponent<Text>();
        if (text == null)
        {
            Debug.LogWarning("Can not find Text: " + key);
            return;
        }

        text.text = words;
    }
}
