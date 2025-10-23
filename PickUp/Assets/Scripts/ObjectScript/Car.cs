using UnityEngine;

public class Car : MonoBehaviour
{
    private CarSO carSO;
    
    public static Car Create(Vector3 worldPosition, CarSO carSO)
    {
        Transform carTransform = Instantiate(carSO.GetPrefab().transform, worldPosition, Quaternion.identity);

        Car car = carTransform.GetComponent<Car>();
        car.carSO = carSO;
        return car;
    }

    public CarSO GetCarSO()
    {
        return carSO;
    }

    public void SelfDestroy() //hoac la delay 1 frame roi moi update sideVisual who know hoac la destyroy immediate
    {
        Destroy(this.gameObject);
    }
    
}
