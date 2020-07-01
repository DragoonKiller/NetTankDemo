loadLua('../UIUtils')

button = this:GetComponent(typeof(UnityUI.Button))

onClick = function()
    loadSceneWithEnv('PVP', function()
        local pvpEnv = Unity.Component.FindObjectOfType(typeof(CS.PVPEnv))
        pvpEnv.targetIP = ipInput.text
        pvpEnv.targetPort = tonumber(portInput.text)
    end)
end

onStart = function()
    button.onClick:AddListener(onClick)
end

onDestroy = function()
    button.onClick:RemoveListener(onClick)
end
