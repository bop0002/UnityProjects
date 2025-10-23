using UnityEngine;

public class Passenger : MonoBehaviour
{
    private PassengerSO passengerSO;
    public static Passenger Create(Vector3 worldPosition, PassengerSO passengerSO)
    {
        Transform passengerTransform = Instantiate(passengerSO.GetPrefab().transform, worldPosition, Quaternion.identity);

        Passenger passenger = passengerTransform.GetComponent<Passenger>();
        passenger.passengerSO = passengerSO;
        return passenger;
    }
}
