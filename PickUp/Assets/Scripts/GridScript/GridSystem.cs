using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR;
using System;
using Unity.Mathematics;
using System.IO;
public class GridSystem : MonoBehaviour
{
    [SerializeField] private List<CarSO> carSOList;
    private Dictionary<ECarType, CarSO> carSODict;
    [SerializeField] private List<PassengerSO> passengerSOList;
    private List<EColor> carTypeSOList;
    private List<RowVisualGroup> rowParentVisual;
    private CarSO carSO;
    private PassengerSO passengerSO;
    private Grid<CarObject> gridCar;
    private Grid<PassengerObject> gridPassenger;
    private int gridCarWidth;
    private int gridCarHeight;
    private int gridPassengerWidth;
    private int gridPassengerHeight;
    private float gridCarCellSize;
    private float gridPassengerCellSize;
    private Vector3 gridCarOrigin;
    private Vector3 gridPassengerOrigin;
    private float carGridTotalWidth;
    private CarSpawnData carSpawnData;
    public event Action<int> OnCenterGroupChange;
    
    private void Awake()
    {
        InitDict();
        Init();
    }
    //Init base
    private void InitDict()
    {
        carSODict = new Dictionary<ECarType, CarSO>();
        foreach (var so in carSOList)
        {
            carSODict[so.GetCarType()] = so;
        }
    }
    private void Init()
    {
        gridCarWidth = 5;
        gridCarHeight = 3;
        gridCarCellSize = 3f;
        carGridTotalWidth = gridCarCellSize * gridCarWidth;
        gridCarOrigin = new Vector3(-7, 0, -12.5f);

        gridPassengerWidth = 3;
        gridPassengerHeight = 6;
        gridPassengerCellSize = 2.65f ;
        gridPassengerOrigin = new Vector3(-4f, 0, -3f);


        gridCar = new Grid<CarObject>(gridCarWidth, gridCarHeight, gridCarCellSize, gridCarOrigin, (Grid<CarObject> g, int x, int z) => new CarObject(g, x, z));
        gridPassenger = new Grid<PassengerObject> (gridPassengerWidth,gridPassengerHeight ,gridPassengerCellSize,gridPassengerOrigin,(Grid<PassengerObject> g, int x,int z) => new PassengerObject(g, x,z));
        carSO = carSOList[0];
        passengerSO = passengerSOList[0];
        rowParentVisual = new List<RowVisualGroup>();
        

        gridCar.OnGridRowVisualChanged += Grid_OnGridRowVisualChanged;
        gridCar.OnGridColumnVisualChanged += GridCar_OnGridColumnVisualChanged;
        InitCarFile();
        InitPassenger();
    }
    private void GridCar_OnGridColumnVisualChanged(object sender, Grid<CarObject>.OnGridColumnVisualChangedEventArgs e) //co the refactor
    {
        if (e.column < 0 || e.column >= gridCarWidth)
        {
            Debug.Log("out of index column");
            return;
        }
        for (int i = 0; i < gridCarHeight; i++)
        {
            UpdateVisualRowAfterShift(i);
        }
    }
    private void InitCarFile()
    {
        string folderPath = Path.Combine(Application.streamingAssetsPath, "LevelPattern");
        string filePath = Path.Combine(folderPath, "level1.json");

        string json = File.ReadAllText(filePath);
        carSpawnData = JsonUtility.FromJson<CarSpawnData>(json);
        for (int z = 0; z < gridCarHeight; z++)
        {
            GameObject rowVisualParent = new GameObject($"Row_{z}");
            rowVisualParent.transform.SetParent(this.transform, false);
            GameObject centerVisualParent = new GameObject($"Row_{z}_Center");
            centerVisualParent.transform.SetParent(rowVisualParent.transform);
            for (int x = 0; x < gridCarWidth; x++)
            {
                string enumString = carSpawnData.columns[x].pattern[carSpawnData.columns[x].indexCount++];
                if (Enum.TryParse(enumString, out ECarType carType))
                {
                    if (carSODict.TryGetValue(carType, out CarSO carSO))
                    {
                        Debug.Log($"JSON content:\n{carSpawnData.columns[x].columnIndex}");
                        Car spawnnedCar = Car.Create(gridCar.GetWorldPosition(x, z), carSO);
                        spawnnedCar.transform.SetParent(centerVisualParent.transform, false);
                        CarObject gridObject = gridCar.GetGridObject(x, z);
                        gridObject.SetCarObject(spawnnedCar);
                    }
                    else
                    {
                        Debug.Log("Cant find carSO");
                    }
                }
                else
                {
                    Debug.Log("Cant parse enum");
                }
            }
            rowParentVisual.Add(new RowVisualGroup(z, centerVisualParent.transform, carGridTotalWidth, this));
        }

    }
    //private void InitCar()
    //{
    //    for (int z = 0; z < gridCarHeight; z++)
    //    {
    //        GameObject rowVisualParent = new GameObject($"Row_{z}");
    //        rowVisualParent.transform.SetParent( this.transform, false);
    //        GameObject centerVisualParent = new GameObject($"Row_{z}_Center");
    //        centerVisualParent.transform.SetParent( rowVisualParent.transform);
    //        for (int x = 0; x < gridCarWidth; x++)
    //        {
    //            carSO = carSOList[UnityEngine.Random.Range(0, carSOList.Count)];
    //            Car spawnnedCar = Car.Create(gridCar.GetWorldPosition(x, z), carSO);
    //            spawnnedCar.transform.SetParent( centerVisualParent.transform,false);
    //            CarObject gridObject = gridCar.GetGridObject(x, z);
    //            gridObject.SetCarObject(spawnnedCar);
    //        }
    //        rowParentVisual.Add(new RowVisualGroup(z, centerVisualParent.transform, carGridTotalWidth, this));
    //    }
    //}
    private void InitPassenger()
    {
        for (int z = 0; z < gridPassengerHeight; z++)
        {
            for (int x = 0; x < gridPassengerWidth; x++)
            {
                passengerSO = passengerSOList[UnityEngine.Random.Range(0, passengerSOList.Count)];
                Passenger passenger = Passenger.Create(gridPassenger.GetWorldPosition(x, z), passengerSO);
                PassengerObject gridObject = gridPassenger.GetGridObject(x, z);
                gridObject.SetPassengerObject(passenger);
            }
        }
    }
    //Visual manage
    public void TestSpawn(int x,int z)
    {
        CarObject gridObject = gridCar.GetGridObject(x, z);
        Car car = gridObject.GetCar();
        string enumString = carSpawnData.columns[x].pattern[carSpawnData.columns[x].indexCount++];
        if (Enum.TryParse(enumString, out ECarType carType))
        {
            if (carSODict.TryGetValue(carType, out CarSO carSO))
            {
                car = Car.Create(gridCar.GetWorldPosition(x, z), carSO);
                car.transform.SetParent(rowParentVisual[z].GetCenterGroup().transform, false);
                gridObject.SetCarObject(car);
            }
            else
            {
                Debug.Log("Cant find carSO");
            }
        }
        else
        {
            Debug.Log("Cant parse enum");
        }

    }
    private void Grid_OnGridRowVisualChanged(object sender, Grid<CarObject>.OnGridRowVisualChangedEventArgs e)
    {
        if (e.row < 0 || e.row >= gridCarHeight)
        {
            Debug.Log("out of index");
            return;
        }
        UpdateVisualRowAfterShift(e.row);

    }
    private void UpdateVisualRowAfterShift(int row)
    {
        ClearRowObjectVisual(row);
        for (int x = 0; x < gridCarWidth; x++)
        {
            CarObject gridObject = gridCar.GetGridObject(x, row);
            Car spawnedCar = Car.Create(gridCar.GetWorldPosition(x, row), gridObject.GetCar().GetCarSO());
            spawnedCar.transform.SetParent(rowParentVisual[row].GetCenterGroup(),false);
            gridObject.SetCarObject(spawnedCar);
        }
        OnCenterGroupChange?.Invoke(row);
    }

    private void ClearRowObjectVisual(int row)
    {
        for (int x = 0; x < gridCarWidth; x++)
        {
            CarObject gridObject = gridCar.GetGridObject(x, row);
            if (gridObject.GetCar() != null)
            {
                gridObject.GetCar().SelfDestroy();
            }
        }
    }
    // GetSet
    public RowVisualGroup GetRowVisualGroup(int row)
    {
        return rowParentVisual[row];
    }

    public Grid<CarObject> GetGrid()
    {
        return this.gridCar;
    }
    //GridObjet
    public class PassengerObject
    {
        private Grid<PassengerObject> grid;
        private int x;
        private int z;
        Passenger passenger;

        public void SetPassengerObject(Passenger placedPassenger)
        {
            this.passenger = placedPassenger;
            grid.TriggerGridObjectChanged(x, z);
        }

        public Passenger GetPassenger()
        {
            return this.passenger;
        }

        public void ClearObject()
        {
            passenger = null;
            grid.TriggerGridObjectChanged(x, z);
        }

        public bool CanBuild()
        {
            return this.passenger == null;
        }

        public PassengerObject(Grid<PassengerObject> grid, int x, int z)
        {
            this.grid = grid;
            this.x = x;
            this.z = z;
        }
        override
        public string ToString()
        {
            return x + "," + z;
        }
    }
    public class CarObject
    {
        private Grid<CarObject> grid;
        private int x;
        private int z;
        Car car;

        public void SetCarObject(Car placeCar)
        {
            this.car = placeCar;
            grid.TriggerGridObjectChanged(x, z);
        }

        public Car GetCar()
        {
            return this.car;
        }

        public void ClearObject()
        {
            car = null;
            grid.TriggerGridObjectChanged(x, z);
        }

        public bool CanBuild()
        {
            return this.car == null;
        }

        public CarObject(Grid<CarObject> grid, int x, int z)
        {
            this.grid = grid;
            this.x = x;
            this.z = z;
        }
        override
        public string ToString()
        {
            return x + "," + z;
        }
    }
}
