button = this:GetComponent(typeof(UnityUI.Button))

onClick = function()
    pvpPanel:SetActive(false)
    mainPanel:SetActive(true)
end

onStart = function()
    button.onClick:AddListener(onClick)
end

onDestroy = function()
    button.onClick:RemoveListener(onClick)
end
