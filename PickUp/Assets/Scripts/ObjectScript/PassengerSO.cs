using UnityEngine;

[CreateAssetMenu(fileName = "PassengerSO", menuName = "ScriptableObject/PassengerSO")]
public class PassengerSO : ScriptableObject
{
    [SerializeField] private string objectName;
    [SerializeField] private EColor color;
    [SerializeField] private GameObject prefab;

    public GameObject GetPrefab()
    {
        return prefab;
    }

}
