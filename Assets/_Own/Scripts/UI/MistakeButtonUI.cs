using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class MistakeButtonUI : MonoBehaviour
{
    [SerializeField] private TMP_Text numberText;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Image icon;
    [SerializeField] private Image backgroundImage;

    [SerializeField] private MistakeButtonSettings _settings;
    public MistakeButtonSettings settings => _settings;

    public event Action<MistakeButtonUI, MistakeButtonSettings> onClicked;

    void OnEnable()
    {
        Set(_settings);

        var button = GetComponent<Button>();
        button.onClick.AddListener(() => onClicked?.Invoke(this, settings));
    }

    void OnValidate()
    {
        Set(_settings);
    }

    public void Set(MistakeButtonSettings newSettings)
    {
        _settings = newSettings;
        
        numberText.text = settings.mistakeId.ToString();
        nameText.text = settings.name;
        // TODO
        //icon.image = //////settings.iconPath;

        backgroundImage.color = settings.color;
    }
}
