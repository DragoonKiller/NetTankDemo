onStart = function()
    menu:SetActive(false)
end

onUpdate = function()
    if Unity.Input.GetKeyDown(Unity.KeyCode.Escape) then menu:SetActive(not menu.activeSelf) end
    local showState = menu.activeSelf
    CS.ExCursor.cursorLocked = not showState
    CS.PlayerControl.inst.enabled = not showState
    CS.CameraFollow.inst.enabled = not showState
    CS.CameraZoom.inst.enabled = not showState
end
