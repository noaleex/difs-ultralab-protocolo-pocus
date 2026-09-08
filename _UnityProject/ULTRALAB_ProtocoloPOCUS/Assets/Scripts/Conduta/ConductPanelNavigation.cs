using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class ConductPanelNavigation : MonoBehaviour
{
    [Header("Campos na ordem do TAB")]
    [SerializeField] private Selectable[] fields;

    [Header("Input System")]
    [SerializeField] private InputActionReference nextFieldAction;

    private void OnEnable()
    {
        if (nextFieldAction != null)
        {
            nextFieldAction.action.performed += OnNextField;
            nextFieldAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (nextFieldAction != null)
        {
            nextFieldAction.action.performed -= OnNextField;
            nextFieldAction.action.Disable();
        }
    }


    // =========================================================
    // TAB
    // =========================================================

    private void OnNextField(InputAction.CallbackContext context)
    {
        bool shiftPressed =
            Keyboard.current != null &&
            (
                Keyboard.current.leftShiftKey.isPressed ||
                Keyboard.current.rightShiftKey.isPressed
            );

        if (shiftPressed)
        {
            SelectPreviousField();
        }
        else
        {
            SelectNextField();
        }
    }


    // =========================================================
    // PRÓXIMO CAMPO
    // =========================================================

    private void SelectNextField()
    {
        if (fields == null || fields.Length == 0)
            return;

        int currentIndex = GetCurrentFieldIndex();

        int startIndex = currentIndex + 1;

        if (currentIndex == -1)
            startIndex = 0;

        for (int i = 0; i < fields.Length; i++)
        {
            int index =
                (startIndex + i) % fields.Length;

            if (IsFieldValid(index))
            {
                SelectField(index);
                return;
            }
        }
    }


    // =========================================================
    // CAMPO ANTERIOR
    // =========================================================

    private void SelectPreviousField()
    {
        if (fields == null || fields.Length == 0)
            return;

        int currentIndex = GetCurrentFieldIndex();

        int startIndex = currentIndex - 1;

        if (currentIndex == -1)
            startIndex = fields.Length - 1;

        for (int i = 0; i < fields.Length; i++)
        {
            int index =
                (startIndex - i + fields.Length)
                % fields.Length;

            if (IsFieldValid(index))
            {
                SelectField(index);
                return;
            }
        }
    }


    // =========================================================
    // VERIFICAR CAMPO
    // =========================================================

    private bool IsFieldValid(int index)
    {
        if (index < 0 ||
            index >= fields.Length)
        {
            return false;
        }

        if (fields[index] == null)
            return false;

        if (!fields[index].gameObject.activeInHierarchy)
            return false;

        if (!fields[index].interactable)
            return false;

        return true;
    }


    // =========================================================
    // SELECIONAR CAMPO
    // =========================================================

    private void SelectField(int index)
    {
        if (!IsFieldValid(index))
            return;

        Selectable field = fields[index];

        EventSystem.current.SetSelectedGameObject(
            field.gameObject
        );

        TMP_InputField inputField =
            field as TMP_InputField;

        if (inputField != null)
        {
            inputField.Select();
            inputField.ActivateInputField();

            inputField.caretPosition =
                inputField.text.Length;

            return;
        }

        TMP_Dropdown dropdown =
            field as TMP_Dropdown;

        if (dropdown != null)
        {
            dropdown.Select();
        }
    }


    // =========================================================
    // CAMPO ATUAL
    // =========================================================

    private int GetCurrentFieldIndex()
    {
        if (EventSystem.current == null)
            return -1;

        GameObject selected =
            EventSystem.current.currentSelectedGameObject;

        if (selected == null)
            return -1;

        for (int i = 0; i < fields.Length; i++)
        {
            if (fields[i] == null)
                continue;

            if (selected == fields[i].gameObject)
                return i;

            if (selected.transform.IsChildOf(
                fields[i].transform))
            {
                return i;
            }
        }

        return -1;
    }


    // =========================================================
    // RESETAR
    // =========================================================

    public void ResetNavigation()
    {
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }


    // =========================================================
    // SELECIONAR PRIMEIRO CAMPO
    // =========================================================

    public void SelectFirstField()
    {
        if (fields == null ||
            fields.Length == 0)
        {
            return;
        }

        for (int i = 0; i < fields.Length; i++)
        {
            if (IsFieldValid(i))
            {
                SelectField(i);
                return;
            }
        }
    }
}