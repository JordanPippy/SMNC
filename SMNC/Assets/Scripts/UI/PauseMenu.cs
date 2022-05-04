using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class PauseMenu : NetworkBehaviour
{
    // Start is called before the first frame update

    public Canvas canvasUI;
    public GameObject buttonPrefab;
    private GameObject abilityMenuParent;
    private RectTransform parentTransform;
    private bool abilitiesOpen = false;
    private List<GameObject> abilitiesButtons = new List<GameObject>();
    private int index = 0;

    void Start()
    {
        if (!isLocalPlayer)
            return;
        canvasUI = GameObject.Find("PauseMenu").GetComponent<Canvas>();
        canvasUI.enabled = true;
        abilityMenuParent = GameObject.Find("AbilityMenu");
        parentTransform = abilityMenuParent.GetComponent<RectTransform>();
        LoadAbilities();
        canvasUI.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer)
            return;
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Should open Pause Menu");
            Debug.Log(canvasUI);
            canvasUI.enabled = !canvasUI.enabled;
        }
    }

    void LoadAbilities()
    {
        for (int i = 0; i < GameManager.Instance.abilities.Count; i++)
        {
            GameObject button = Instantiate(buttonPrefab, canvasUI.transform, false) as GameObject;

            button.GetComponent<RectTransform>().SetParent(parentTransform, false);

            button.GetComponentInChildren<Text>().text = GameManager.Instance.abilities[i].title;

            //Completely necessary line of code. Nope, I don't know either.
            string title = GameManager.Instance.abilities[i].title;
            //End completely necessary line of code. What a ride.

            button.GetComponent<Button>().onClick.AddListener(delegate {AddAbilityListener(title);});

            abilitiesButtons.Add(button);

        }
    }


    [Command]
    void RequestAbilitySwap(int index, string name)
    {
        AbilitySwapRpc(index, name);
    }

    [ClientRpc]
    void AbilitySwapRpc(int index, string name)
    {
        GetComponent<Player>().abilities[index] = GameManager.Instance.GetAbility(name);
    }

    void AddAbilityListener(string name)
    {
        RequestAbilitySwap(index, name);
        index++;
        if (index == 2)
            index = 0;
    }

    public void OpenAbilities()
    {
        abilitiesOpen = !abilitiesOpen;     
        abilityMenuParent.SetActive(abilitiesOpen);
    }
}
