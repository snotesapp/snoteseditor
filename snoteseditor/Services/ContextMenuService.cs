using System;
using BlazorApp1.Shared;

namespace BlazorApp1.Services;

public class ContextMenuService
{
    private SNContextMenu currentContextMenu;

    public async Task Register(SNContextMenu contextMenu)
    {
        if (currentContextMenu != null && currentContextMenu != contextMenu)
        {
            await currentContextMenu.HideAsync();
        }
        await Task.Delay(10);
        currentContextMenu = contextMenu;
    }

    public void Unregister(SNContextMenu contextMenu)
    {
        if (currentContextMenu == contextMenu)
        {
            currentContextMenu = null;
        }
    }
}

