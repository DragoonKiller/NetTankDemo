button = this:GetComponent(typeof(UnityUI.Button))

onClick = function()
    if App.isEditor 
    then  EditorApp.isPlaying = false
    else App.Quit(0)
    end
end

onStart = function()
    button.onClick:AddListener(onClick)
end

onDestroy = function()
    button.onClick:RemoveListener(onClick)
end
