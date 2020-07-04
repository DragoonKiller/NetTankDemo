loadLua('../UIUtils')

button = this:GetComponent(typeof(UnityUI.Button))

onClick = function()
    local name = nameInput.text
    local ip = ipInput.text
    local port = tonumber(portInput.text)
    loadSceneWithEnv('PVP', function()
        local pvpEnv = CS.PVPEnv.inst
        pvpEnv.targetIP = ip
        pvpEnv.targetPort = port
        pvpEnv.playerName = name
    end)
end

onStart = function()
    button.onClick:AddListener(onClick)
end

onDestroy = function()
    button.onClick:RemoveListener(onClick)
end
