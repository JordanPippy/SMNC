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
    private List<GameObject> abilitiesButtons = new List<GameObject>();
    private int index = 0;

    void Start()
    {
        if (!isLocalPlayer)
            return;
        GameManager.Instance.pauseMenu.SetActive(true);
        GameObject.Find("AbilityMenu").SetActive(true);
        canvasUI = GameManager.Instance.pauseMenu.GetComponent<Canvas>();
        canvasUI.enabled = true;
        abilityMenuParent = GameObject.Find("AbilityMenu");
        parentTransform = abilityMenuParent.GetComponent<RectTransform>();

        LoadAbilities();

        abilityMenuParent.SetActive(false);
        canvasUI.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer)
            return;
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            abilityMenuParent.SetActive(false);
            canvasUI.enabled = !canvasUI.enabled;
            GetComponent<MouseLook>().enabled = !canvasUI.enabled;
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
        if (abilityMenuParent == null)
        {
            abilityMenuParent = GameManager.Instance.pauseMenu.transform.Find("AbilityMenu").gameObject;
        }
        abilityMenuParent.SetActive(!abilityMenuParent.activeSelf);
    }
}
