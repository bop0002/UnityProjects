using UnityEngine;
using CodeMonkey.Utils;
using System;
using Unity.VisualScripting;
using static GridSystem;
using System.Collections.Generic;
using System.Collections;
using static UnityEngine.Rendering.DebugUI.Table;
public class Grid<TGridObject>
{
    private bool debug = false;
    private int width;
    private int height;
    private float cellSize;
    private float totalWidth;
    private TGridObject[,] gridArray;
    private LinkedList<TGridObject> gridObjectLinkedList;
    private TextMesh[,] textMeshArray;
    private Vector3 origin;
    public event EventHandler<OnGridColumnVisualChangedEventArgs> OnGridColumnVisualChanged;
    public class OnGridColumnVisualChangedEventArgs
    {
        public int column;
        public OnGridColumnVisualChangedEventArgs(int column)
        { this.column = column; }

    }
    public event EventHandler<OnGridRowVisualChangedEventArgs> OnGridRowVisualChanged;  //hoi can co the gop vao voi GridValueChange ko?
    public class OnGridRowVisualChangedEventArgs
    {
        public int row;
        public OnGridRowVisualChangedEventArgs(int row)
        { this.row = row; }

    }
    private event EventHandler<OnGridValueChangedEventArgs> OnGridValueChanged;
    public class OnGridValueChangedEventArgs
    {
        public int x;
        public int z;
        public OnGridValueChangedEventArgs(int x, int z)
        {
            this.x = x; this.z = z;
        }
    }


    public Grid(int width, int height, float cellSize, Vector3 origin,Func<Grid<TGridObject>,int,int,TGridObject> createGridObject)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.origin = origin;
        this.totalWidth = width * cellSize;
        gridArray = new TGridObject[width, height];
        gridObjectLinkedList = new LinkedList<TGridObject>();
        // Init array 
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                gridArray[x, z] = createGridObject(this, x, z);
            }
        }

        if (debug) DebugShow();
    }



    private void DebugShow()
    {
        textMeshArray = new TextMesh[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                textMeshArray[x, z] = UtilsClass.CreateWorldText(gridArray[x, z].ToString(), null, GetWorldPosition(x, z) + new Vector3(cellSize, 0, cellSize) * 0.5f, 5, Color.white, TextAnchor.MiddleCenter);
                Debug.DrawLine(GetWorldPosition(x, z), GetWorldPosition(x + 1, z), Color.white, 100f);
                Debug.DrawLine(GetWorldPosition(x, z), GetWorldPosition(x, z + 1), Color.white, 100f);
            }
        }
        Debug.DrawLine(GetWorldPosition(0, height), GetWorldPosition(width, height), Color.white, 100f);
        Debug.DrawLine(GetWorldPosition(width, 0), GetWorldPosition(width, height), Color.white, 100f);
        OnGridValueChanged += (object sender, OnGridValueChangedEventArgs eventArgs) => {
            textMeshArray[eventArgs.x, eventArgs.z].text = gridArray[eventArgs.x, eventArgs.z].ToString();
            //Debug.Log($"Grid value changed at {eventArgs.x},{eventArgs.z}");

        };
    }
    public float GetCellSize()
    {
        return this.cellSize;

    }
    public int GetGridHeight()
     {
        return this.height;
    }
    public int GetGridWidth()
    {
        return this.width;
    }
    public float GetTotalWidth()
    {
        return this.totalWidth;
    }
    //shift cac thu co the refactor thanh linkedlist va queue stack cac thu
    public void ShiftColumnUp(int column,int shiftCount)
    {
        for(int z = 0; z < height;z++)
        {
            gridObjectLinkedList.AddFirst(gridArray[column,z]);
        }
        for (int i = 0; i < shiftCount; i++)
        {
            TGridObject temp = gridObjectLinkedList.First.Value;
            gridObjectLinkedList.RemoveFirst();
            gridObjectLinkedList.AddLast(temp);
        }
        for (int z = 0; z < height; z++)
        {
            gridArray[column,z] = gridObjectLinkedList.Last.Value;
            gridObjectLinkedList.RemoveLast();
        }
        OnGridColumnVisualChanged?.Invoke(this, new OnGridColumnVisualChangedEventArgs(column));
    }
    //public void ShiftColumnUp(int column)
    //{
    //    TGridObject tmp = gridArray[column, 0];
    //    for (int row = 0; row < height - 1; row++)
    //    {
    //        gridArray[column, row] = gridArray[column, row + 1];
    //    }
    //    gridArray[column, height - 1] = tmp;
    //}

    //public void ShiftColumnDown(int column)
    //{
    //    TGridObject tmp = gridArray[column, height - 1];
    //    for (int row = height - 1; row > 0; row--)
    //    {
    //        gridArray[column, row] = gridArray[column, row - 1];
    //    }
    //    gridArray[column, 0] = tmp;
    //}
    public void ShiftRowRight(int row)
    {
        TGridObject tmp = gridArray[width - 1, row];
        for (int column = width - 1; column > 0; column--)
        {
            gridArray[column, row] = gridArray[column - 1, row];
        }
        gridArray[0, row] = tmp;
        OnGridRowVisualChanged?.Invoke(this, new OnGridRowVisualChangedEventArgs(row));
    }
    public void ShiftRowLeft(int row)
    {
        TGridObject tmp = gridArray[0,row];
        for(int column = 0; column < width-1; column++)
        {
            gridArray[column, row] = gridArray[column+1, row];
        }
        gridArray[width - 1,row] = tmp;
        OnGridRowVisualChanged?.Invoke(this, new OnGridRowVisualChangedEventArgs(row));
    }


    public void TriggerGridObjectChanged(int x,int z)
    {
        OnGridValueChanged?.Invoke(this, new OnGridValueChangedEventArgs(x,z));
    }

    public void SetGridObject(Vector3 worldposition, TGridObject gridObject)
    {
        int x, z;
        GetXZ(worldposition, out x, out z);
        SetGridObject(x, z, gridObject);
    }
    public void SetGridObject(int x, int z, TGridObject gridObject)
    {
        if (x >= 0 && x < width && z >= 0 && z < height) {
            gridArray[x, z] = gridObject;
            OnGridValueChanged?.Invoke(this,new OnGridValueChangedEventArgs(x,z));
        }
    }
    public TGridObject GetGridObject(Vector3 worldposition)
    {
        int x, z;
        GetXZ(worldposition,out x,out z);
        return GetGridObject(x,z);
    }
    public TGridObject GetGridObject(int x,int z)
    {
        if (x >= 0 && x < width && z >= 0 && z < height)
        {
            return gridArray[x, z];
        }
        else
        {
            return default(TGridObject);
        }
    }
    public Vector3 GetWorldPosition(int x,int z ){
        return new Vector3(x,0,z) * cellSize + origin;
    }
    public void GetXZ(Vector3 worldPosition, out int x, out int z)
    {
        x = Mathf.FloorToInt((worldPosition - origin ).x / cellSize);
        z = Mathf.FloorToInt((worldPosition - origin).z / cellSize);   
    }
}
