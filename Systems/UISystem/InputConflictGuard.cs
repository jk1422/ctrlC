using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

internal sealed class InputConflictGuard : IDisposable
{
    private readonly List<InputAction> _conflicts = new();
    private InputAction _cBtn;

    public void Initialize()
    {
        _conflicts.Clear();
        foreach (var action in InputSystem.ListEnabledActions())
        {
            foreach (var binding in action.bindings)
            {
                if (binding.effectivePath.Contains("<Keyboard>/c") &&
                    action.name != "Open Mod Binding" &&
                    action.name != "cbtn")
                {
                    _conflicts.Add(action);
                    break;
                }
            }
        }

        _cBtn = new InputAction("cbtn", InputActionType.Button);
        _cBtn.AddBinding("<Keyboard>/C");
        _cBtn.Enable();
    }

    public void Tick(bool toolsActive)
    {
        if (!toolsActive || _cBtn == null) return;

        if (_cBtn.WasPerformedThisFrame()) Disable();
        else if (_cBtn.WasReleasedThisFrame()) Enable();
    }

    private void Disable() { foreach (var a in _conflicts) if (a.enabled) a.Disable(); }
    private void Enable() { foreach (var a in _conflicts) if (!a.enabled) a.Enable(); }

    public void Dispose()
    {
        _cBtn?.Disable();
        _cBtn?.Dispose();
        _cBtn = null;
    }
}
