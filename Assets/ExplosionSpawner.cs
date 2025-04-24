using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class ExplosionSpawner : MonoBehaviour
{
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private float explosionSlowRadius = 10f;
    [SerializeField] private float explosionInitialSpeed = 5f;
    [SerializeField] private float explosionSlowSpeed = 0.5f;
    [SerializeField] private float shockWaveDuration = 2f;
    [SerializeField] private float shockWaveExpansionSpeedMult = 0.5f;

    private SimControls controls;

    private void Awake()
    {
        controls = new SimControls();
        controls.Sim.Click.performed += ctx => OnClick();
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    private void OnClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            GameObject explosion = Instantiate(explosionPrefab, hit.point, Quaternion.identity);
            StartCoroutine(ExpandExplosion(explosion));
            StartCoroutine(ExpandAndDestroy(explosion.transform.GetChild(1)));
        }
    }

    IEnumerator ExpandExplosion(GameObject explosion)
    {
        float radius = 0f;

        while (true)
        {
            float speed = radius < explosionSlowRadius ? explosionInitialSpeed : explosionSlowSpeed;
            radius += speed * Time.deltaTime;
            
            float diameter = radius * 2f;

            explosion.transform.localScale = Vector3.one * diameter;


            yield return null;
        }

    }

    IEnumerator ExpandAndDestroy (Transform shockwave)
    {
        float radius = 0f;
        float elapsed = 0f;

        while (elapsed < shockWaveDuration)
        {
            elapsed += Time.deltaTime;
            radius += shockWaveExpansionSpeedMult * Time.deltaTime;
            float diameter = radius * 2f;

            if (shockwave != null)
                shockwave.localScale = new Vector3(diameter, 0.1f, diameter);

            yield return null;
        }

        if(shockwave != null)
            Destroy(shockwave.gameObject);
    }
}
