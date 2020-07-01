loadSceneWithEnv = function (sceneName, callback)
    local loadTaskA = SceneManager.LoadSceneAsync('Env', Scene.LoadSceneMode.Additive)
    loadTaskA:completed('+', function (_)
        loadTaskA.allowSceneActivation = false
        local unloadTask = SceneManager.UnloadSceneAsync('Open')
        unloadTask:completed('+', function (_)
            local loadTaskB = SceneManager.LoadSceneAsync(sceneName, Scene.LoadSceneMode.Additive)
            loadTaskB:completed('+', function (_) 
                if callback ~= nil then
                    callback()
                end
                loadTaskA.allowSceneActivation = true
            end)
        end)
    end)
end
