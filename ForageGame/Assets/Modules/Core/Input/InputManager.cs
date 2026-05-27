using System;
using System.Collections.Generic;
using System.Data;
using TDK.ItemSystem.Inventory;
using TDK.PlayerSystem;
using Project.Menus;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public void OnMove(InputAction.CallbackContext context)
    {
        if (AppController.Instance.state != AppController.State.Gameplay)
            return;
        Player.Instance?.playerController.OnMove(context);
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (AppController.Instance.state != AppController.State.Gameplay)
            return;
        Player.Instance?.playerController.OnSprint(context);
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (AppController.Instance.state != AppController.State.Gameplay)
            return;
        Player.Instance?.playerController.OnAttack(context);
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (AppController.Instance.state != AppController.State.Gameplay)
            return;
        Player.Instance?.playerController?.OnJump(context);
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        if (AppController.Instance.state != AppController.State.Gameplay) return;
        Player.Instance?.playerInteract?.Interact();
    }

    // ------------ Pausing ------------

    public void Escape(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (AppController.Instance.state == AppController.State.Gameplay)
                GameplayController.Instance?.Escape();
            else if (AppController.Instance.state == AppController.State.MainMenu)
                MainMenuController.Instance?.Escape();
        }
    }

    // ------------ Inventory ------------

    public void Next(InputAction.CallbackContext context)
    {
        if (AppController.Instance.state != AppController.State.Gameplay)
            return;

        if (context.started)
            InventoryServices.Next();
    }

    public void Previous(InputAction.CallbackContext context)
    {
        if (AppController.Instance.state != AppController.State.Gameplay)
            return;

        if (context.started)
            InventoryServices.Previous();
    }

    public void Scroll(InputAction.CallbackContext context)
    {
        print("scroll");
        if (AppController.Instance.state != AppController.State.Gameplay)
            return;

        if (context.ReadValue<float>() > 0.01f)
            InventoryServices.Next();
        else if (context.ReadValue<float>() < -0.01f)
            InventoryServices.Previous();
    }

    public void DropItem(InputAction.CallbackContext context)
    {
        if (AppController.Instance.state != AppController.State.Gameplay)
            return;

        if (context.started)
            InventoryController.Instance?.TryDropItem();
    }

    public void UseItem(InputAction.CallbackContext context)
    {
        if (AppController.Instance.state != AppController.State.Gameplay)
            return;

        if (context.started)
            InventoryController.Instance?.TryUseItem();
    }

    public void SelectSlot(int index)
    {
        if (AppController.Instance.state != AppController.State.Gameplay)
            return;

        InventoryController.Instance?.SelectSlot(index);
    }

    // ------------ Recipes ------------

    public void ShowRecipes(InputAction.CallbackContext context)
    {
        if (AppController.Instance.state != AppController.State.Gameplay)
            return;

        if (context.started)
            RecipeBookController.Instance?.TriggerVisualization();
    }
}