using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSlotDragDropItem : UIDragDropItem
{
    public bool IsPressed { get { return mPressed; } }
    public bool IsDragging { get { return mDragging; } }

    public void OnDragStartByWidget()
    {
        //if (mGrid && mGrid.animateSmoothly)
        //    mGrid.animateSmoothly = false;

        base.OnDragStart();
    }

    protected override void OnDragStart()
    {
        //if(mGrid && mGrid.animateSmoothly)
        //    mGrid.animateSmoothly = false;
        //
        //base.OnDragStart();
    }

    public void DragDropRelease()
    {

    }

    protected override void OnDragDropRelease(GameObject surface)
    {
        var currentComponent = gameObject?.GetComponent<PlayerSlotComponent>();
        var otherComponent = surface?.GetComponent<PlayerSlotComponent>();

        // We should ignore any other gameobject that does not have the PlayerSlotComponent component, since we just want to change the position between them.
        // Or if other gameobject is same of this current index.
        if (otherComponent == null || currentComponent.SlotIndex == otherComponent.SlotIndex)
        {
            base.OnDragDropRelease(surface);
            return;
        }
        else
        {
            //if (mGrid && !mGrid.animateSmoothly)
            //    mGrid.animateSmoothly = true;

            var uiRoot = GameObject.Find("UI Root");
            var uiManager = uiRoot?.GetComponent<UIManager>();
            var inRoomWindow = uiManager?.FindInstance(WindowType.ROOM) as RoomWindow;
            inRoomWindow?.ChangeSlotPosReq(currentComponent.SlotIndex, otherComponent.SlotIndex);
        }
    }

    internal void DragDrop(GameObject surface)
    {
        var currentComponent = gameObject?.GetComponent<PlayerSlotComponent>();
        var otherComponent = surface?.GetComponent<PlayerSlotComponent>();

        //if (mGrid && !mGrid.animateSmoothly)
        //    mGrid.animateSmoothly = true;

        var tmpSibling = gameObject.transform.GetSiblingIndex();
        gameObject.transform.SetSiblingIndex(surface.transform.GetSiblingIndex());
        surface.transform.SetSiblingIndex(tmpSibling);

        // Clear the reference to the scroll view since it might be in another scroll view now
        var drags = GetComponentsInChildren<UIDragScrollView>();
        foreach (var d in drags) d.scrollView = null;

        // Re-enable the collider
        if (mButton != null) mButton.isEnabled = true;
        else if (mCollider != null) mCollider.enabled = true;
        else if (mCollider2D != null) mCollider2D.enabled = true;

        // Is there a droppable container?
        UIDragDropContainer container = surface ? NGUITools.FindInParents<UIDragDropContainer>(surface) : null;

        if (container != null)
        {
            // Container found -- parent this object to the container
            mTrans.parent = (container.reparentTarget != null) ? container.reparentTarget : container.transform;

            Vector3 pos = mTrans.localPosition;
            pos.z = 0f;
            mTrans.localPosition = pos;
        }
        else
        {
            // No valid container under the mouse -- revert the item's parent
            mTrans.parent = mParent;
        }

        // Update the grid and table references
        mParent = mTrans.parent;
        mGrid = NGUITools.FindInParents<UIGrid>(mParent);
        mTable = NGUITools.FindInParents<UITable>(mParent);

        // Re-enable the drag scroll view script
        if (mDragScrollView != null) Invoke("EnableDragScrollView", 0.001f);

        // Notify the widgets that the parent has changed
        NGUITools.MarkParentAsChanged(gameObject);

        if (mTable != null) mTable.repositionNow = true;
        if (mGrid != null) mGrid.repositionNow = true;

        // We're now done
        OnDragDropEnd();
    }
}
