button = this:GetComponent(typeof(UnityUI.Button))

onClick = function()
    pvpPanel:SetActive(true)
    mainPanel:SetActive(false)
end

onStart = function()
    button.onClick:AddListener(onClick)
end

onDestroy = function()
    button.onClick:RemoveListener(onClick)
end
