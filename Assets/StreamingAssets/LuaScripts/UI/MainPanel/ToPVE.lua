loadLua('../UIUtils')

button = this:GetComponent(typeof(UnityUI.Button))

onClick = function()
    loadSceneWithEnv('PVE')
end

onStart = function()
    button.onClick:AddListener(onClick)
end

onDestroy = function()
    button.onClick:RemoveListener(onClick)
end