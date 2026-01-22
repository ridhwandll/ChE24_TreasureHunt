using UnityEngine;
using UnityEngine.UIElements;

public enum MenuType
{
    DefuseMenu,
    BombLoadMenu,
    ProgressMenu,
    TeamNameMenu,
    RiddlerClueMenu
}

public class MenuManager
{
    private VisualElement _defuseMenu;
    private VisualElement _bombLoadMenu;
    private VisualElement _progressMenu;
    private VisualElement _teamNameMenu;
    private VisualElement _riddlerClueMenu;
    private VisualElement _BOMBPanel;

    public void Initialize(UIDocument document)
    {
        _defuseMenu = document.rootVisualElement.Q<VisualElement>("DefuseMenu");
        _bombLoadMenu = document.rootVisualElement.Q<VisualElement>("BombLoadMenu");
        _progressMenu = document.rootVisualElement.Q<VisualElement>("ProgressMenu");
        _teamNameMenu = document.rootVisualElement.Q<VisualElement>("TeamNameMenu");
        _riddlerClueMenu = document.rootVisualElement.Q<VisualElement>("RiddlerClueMenu"); 
        _BOMBPanel = document.rootVisualElement.Q<VisualElement>("BOMBS");
    }
    
    public void Show(MenuType menuType)
    {
        AudioManager.StopPlayingClue();
        VisualElement menu = GetMenu(menuType);

        // Hide All
        _defuseMenu.style.display = DisplayStyle.None;
        _bombLoadMenu.style.display = DisplayStyle.None;
        _progressMenu.style.display = DisplayStyle.None;
        _teamNameMenu.style.display = DisplayStyle.None;
        _riddlerClueMenu.style.display = DisplayStyle.None;
        _BOMBPanel.style.display = DisplayStyle.None;

        menu.style.display = DisplayStyle.Flex;
        menu.style.opacity = 0f;
        menu.style.scale = Vector3.one * 0.95f;

        menu.experimental.animation
            .Start(0f, 1f, 369, (VisualElement m, float value) =>
            {
                menu.style.opacity = value;
                menu.style.scale = Vector3.one * Mathf.Lerp(0.95f, 1f, value);
            });
        
        if (menuType != MenuType.TeamNameMenu)
            _BOMBPanel.style.display = DisplayStyle.Flex;
    }
    
    private VisualElement GetMenu(MenuType menuType)
    {
        switch (menuType)
        {
            case MenuType.ProgressMenu:
                return _progressMenu;
            case MenuType.TeamNameMenu:
                return _teamNameMenu;
            case MenuType.BombLoadMenu:
                return _bombLoadMenu;
            case MenuType.DefuseMenu:
                return _defuseMenu;
            case MenuType.RiddlerClueMenu:
                return _riddlerClueMenu;
        }
        return null;
    }
}
