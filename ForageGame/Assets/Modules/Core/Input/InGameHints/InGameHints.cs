using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using TDK.ItemSystem.Inventory;
using TDK.ItemSystem.Types;
using TDK.ItemSystem;
using TDK.PlayerSystem;
using NPC;

[RequireComponent(typeof(CanvasGroup))]
public class InGameHints : MonoBehaviour
{
    private HashSet<InGameHintElement> currentHints = new();
    private enum State { Activated, Deactivating, Deactivated, Activating }
    private State currentState = State.Activated;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    void Start() => HidePrompts(direct: true);

    #region Update

    private float idleTime = 0;
    private readonly float activationTime = 2;

    void Update()
    {
        if (Input.anyKey)
            HidePrompts();
        else if (idleTime < activationTime)
            idleTime += Time.deltaTime;
        else
            ShowPrompts();
    }

    #endregion

    #region Show & Hide Current Hints

    private Tween tween;
    private CanvasGroup canvasGroup;

    private void ShowPrompts(bool direct = false)
    {
        if (currentState == State.Activated || currentState == State.Activating) return;
        currentState = State.Activating;

        RefreshCurrentHints();
        RefreshVisuals();

        foreach (InGameHintElement hint in currentHints)
            hint.gameObject.SetActive(true);

        tween?.Kill();
        if (direct)
        {
            canvasGroup.alpha = 1;
            currentState = State.Activated;
        }
        else
        {
            tween = canvasGroup.DOFade(1, 1).SetEase(Ease.InOutQuad)
        .OnComplete(() => currentState = State.Activated);
        }

    }

    private void HidePrompts(bool direct = false)
    {
        idleTime = 0;
        if (currentState == State.Deactivated || currentState == State.Deactivating) return;
        currentState = State.Deactivating;

        tween?.Kill();
        if (direct)
        {
            canvasGroup.alpha = 0;
            foreach (Transform child in transform)
                child.gameObject.SetActive(false);
            currentState = State.Deactivated;
        }
        else
        {
            tween = canvasGroup.DOFade(0, 0.2f).SetEase(Ease.InOutQuad)
            .OnComplete(() =>
            {
                foreach (Transform child in transform)
                    child.gameObject.SetActive(false);
                currentState = State.Deactivated;
            });
        }
    }

    #endregion

    #region Refresh

    public void RefreshVisuals()
    {
        foreach (InGameHintElement hint in currentHints)
            hint.RefreshVisuals();
    }

    [Header("Built In Help Prompts")]
    [SerializeField] private InGameHintElement consumeHint;
    [SerializeField] private InGameHintElement pickupHint;
    [SerializeField] private InGameHintElement talkHint;
    [SerializeField] private InGameHintElement attackHint;
    [SerializeField] private InGameHintElement dashHint;
    [SerializeField] private InGameHintElement jumpHint;
    [SerializeField] private InGameHintElement flyHint;
    [SerializeField] private InGameHintElement recipeHint;


    public void RefreshCurrentHints()
    {
        currentHints = new();

        if (InventoryController.Instance.GetItemAtCurrent() is ConsumableItem)
            currentHints.Add(consumeHint);

        if (Player.Instance.playerInteract._currentFocus?.GetComponent<ItemController>())
            currentHints.Add(pickupHint);

        if (Player.Instance.playerInteract._currentFocus?.GetComponent<NpcLocation>())
            currentHints.Add(talkHint);

        if (!Player.Instance.playerData.hasUsedAttack)
            if (Player.Instance.playerData.attackUnlocked)
                currentHints.Add(attackHint);

        if (!Player.Instance.playerData.hasUsedSprint)
            if (Player.Instance.playerData.sprintUnlocked)
                currentHints.Add(dashHint);

        if (!Player.Instance.playerData.hasUsedJump)
            if (Player.Instance.playerData.wingLevel == 1)
                currentHints.Add(jumpHint);

        if (!Player.Instance.playerData.hasUsedFly)
            if (Player.Instance.playerData.wingLevel == 2)
                currentHints.Add(flyHint);

        if (!Player.Instance.playerData.hasOpenedRecipeBook)
            if (RecipeBookController.Instance.CollectedRecipes.Count > 0)
                currentHints.Add(recipeHint);
    }

    #endregion
}
