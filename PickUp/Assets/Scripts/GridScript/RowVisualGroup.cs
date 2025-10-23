using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.Rendering.DebugUI.Table;
public class RowVisualGroup

{
    private int rowIndex;
    private Transform leftGroup;
    private Transform centerGroup;
    private Transform rightGroup;
    private GridSystem gridSystem;
    private float totalWidth;
    private Vector3 offset;

    private Vector3 originalCenterPos;


    public RowVisualGroup(int rowIndex,Transform centerGroup,float totalWidth,GridSystem gridSystem)
    {
        this.rowIndex = rowIndex;
        this.centerGroup = centerGroup;
        this.totalWidth =  totalWidth;
        this.offset = new Vector3(totalWidth, 0,0);
        this.gridSystem = gridSystem;
        gridSystem.OnCenterGroupChange += GridSystem_OnCenterGroupChange;


        InitSideVisual(rowIndex);

    }

    private void InitSideVisual(int rowIndex)
    {
        if (leftGroup != null && rightGroup != null)
        {
            GameObject.DestroyImmediate(leftGroup.gameObject);
            GameObject.DestroyImmediate(rightGroup.gameObject);
        }
        leftGroup = GameObject.Instantiate(centerGroup, centerGroup.position - offset, Quaternion.identity, centerGroup.parent);
        leftGroup.name = $"Row{rowIndex}_Left";
        rightGroup = GameObject.Instantiate(centerGroup, centerGroup.position + offset, Quaternion.identity, centerGroup.parent);
        rightGroup.name = $"Row{rowIndex}_Right";
    }

    public void RememberOriginalPositions()
    {
        originalCenterPos = centerGroup.localPosition;
    }
    public void ResetPosition()
    {
        centerGroup.localPosition = originalCenterPos;
        leftGroup.localPosition = originalCenterPos - offset;
        rightGroup.localPosition = originalCenterPos + offset;
    }
    public void SetRowOffset(float delta)
    {
        centerGroup.localPosition = originalCenterPos + new Vector3(delta, 0, 0);
        leftGroup.localPosition = originalCenterPos - offset + new Vector3(delta, 0, 0);
        rightGroup.localPosition = originalCenterPos + offset + new Vector3(delta, 0, 0);

    }

    private void GridSystem_OnCenterGroupChange(int rowIndex)
    {
        if (this.rowIndex == rowIndex)
            gridSystem.StartCoroutine(DelayedClone());
    }

    private IEnumerator DelayedClone()
    {
        yield return null;
        InitSideVisual(rowIndex);
    }

    public Transform GetCenterGroup()
    {
        return centerGroup;
    }



    
}
