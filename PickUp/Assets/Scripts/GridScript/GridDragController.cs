using UnityEngine;
using System.Collections.Generic;
using static UnityEngine.UI.Image;
using static GridSystem;

public class GridDragController : MonoBehaviour
{
    //legacy input co the refactor xau
    private GridSystem gridSystem;
    private Grid<GridSystem.CarObject> grid;
    private int startX, startZ;
    private float cellSize;
    private float dragThreshold;
    private Vector3 startMousePos;
    private Vector3 currentMousePos;
    private Vector3 endMousePos;
    private bool isDragging;
    private float totalWidth;
    private int maxShift;
    private int width;
    private int height;

    //De trong start vi ben kia init awake co the loi
    private void Start()
    {
        gridSystem = GetComponent<GridSystem>();
        grid = gridSystem.GetGrid();
        isDragging = false;
        cellSize = grid.GetCellSize();
        totalWidth = grid.GetTotalWidth();
        width = grid.GetGridWidth();
        height = grid.GetGridHeight();
        dragThreshold = 0.3f;
        maxShift = Mathf.RoundToInt(totalWidth / cellSize);
    }


    //mousepos la vector3 nen neu camera ma ko phai 90 xuong la kha nang co van de

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            startMousePos = Mouse3D.GetMouseWorldPosition();
            grid.GetXZ(Mouse3D.GetMouseWorldPosition(), out startX, out startZ);
            if(startX >=0 && startX<width && startZ<height && startZ>=0)
            {
                gridSystem.GetRowVisualGroup(startZ).RememberOriginalPositions();
                isDragging = true;
            }
        }

        if(isDragging)
        {
            currentMousePos = Mouse3D.GetMouseWorldPosition();
            float delta = currentMousePos.x - startMousePos.x;
            delta = Mathf.Clamp(delta, -totalWidth, totalWidth);
            gridSystem.GetRowVisualGroup(startZ).SetRowOffset(delta);
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
            endMousePos = Mouse3D.GetMouseWorldPosition();
            Vector3 dragVector = endMousePos - startMousePos;

            if (dragVector.magnitude > dragThreshold)
            {
                int shiftCount = 0;
                shiftCount = Mathf.RoundToInt(dragVector.x / cellSize);
                shiftCount = Mathf.Clamp(shiftCount,-maxShift,maxShift);
                if (shiftCount > 0)
                {
                    for (int i = 0; i < shiftCount; i++)
                    {
                        grid.ShiftRowRight(startZ);
                        Debug.Log($"Shift row {startZ} → {shiftCount} steps Right");
                    }
                }
                else if (shiftCount < 0)
                {
                    for (int i = 0; i < -shiftCount; i++)
                    {
                        grid.ShiftRowLeft(startZ);
                        Debug.Log($"Shift row {startZ} ← {-shiftCount} steps Left");
                    }
                }
            }
            gridSystem.GetRowVisualGroup(startZ).ResetPosition();
        }

        if (Input.GetMouseButtonDown(1))
        {
            startMousePos = Mouse3D.GetMouseWorldPosition();
            grid.GetXZ(Mouse3D.GetMouseWorldPosition(), out startX, out startZ);
            gridSystem.TestSpawn(startX, startZ);
            grid.ShiftColumnUp(startX, 1);
        }

            //if(Input.GetMouseButtonDown(1))
            //{
            //    startMousePos = Mouse3D.GetMouseWorldPosition();
            //    grid.GetXZ(Mouse3D.GetMouseWorldPosition(), out startX, out startZ);
            //    if (startX >= 0 && startX < width && startZ < height && startZ >= 0)
            //    {
            //        grid.ShiftColumnUp(startX, 1);
            //    }
            //}

    }
}
