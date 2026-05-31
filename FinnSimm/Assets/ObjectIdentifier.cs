using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class ObjectIdentifier : MonoBehaviour
{
    [Header("Materials")]
    public Material red;
    public Material yellow;
    public Material blue;

    private const int _dangerAmount = 100;
    private const int _evaluatingAmount = 200;
    private const int _safeAmount = 300;

    public int _counter = 0;
    private Renderer _objectRenderer;

    private void Awake()
    {
        // Cache the renderer for better performance
        _objectRenderer = GetComponent<Renderer>();
        
        if (_objectRenderer == null)
        {
            Debug.LogError("Renderer component not found on this GameObject!", this);
        }
    }

    private void Update()
    {

        _objectRenderer.material = _counter switch
        {
            <= _evaluatingAmount when _objectRenderer => red,
            > _evaluatingAmount and <= _safeAmount when _objectRenderer => yellow,
            > _safeAmount when _objectRenderer => blue,
            _ => _objectRenderer.material
        };
        
        _counter = math.min(_counter, _safeAmount + 40); // Prevent counter from going too high

        if (_counter < -5)
        {
            Destroy(this.GameObject());
        }
    }

    public void IncreaseCounter(int amount)
    {
        _counter += amount;
    }
}