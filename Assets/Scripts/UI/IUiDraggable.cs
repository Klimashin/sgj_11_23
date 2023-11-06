using UnityEngine.EventSystems;

public interface IUiDraggable : IBeginDragHandler, IEndDragHandler, IDragHandler
{
        
}
    
public interface IUiDrop : IDropHandler
{
        
}