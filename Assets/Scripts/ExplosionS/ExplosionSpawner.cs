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
        //subscribe the click event
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

    /// <summary>
    /// Spawns an explosion on click where the mouse is hovering
    /// </summary>
    private void OnClick()
    {
        //get position hit by the mouse click
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            //Instantiates the explosion object
            GameObject explosion = Instantiate(explosionPrefab, hit.point, Quaternion.identity);
            //Starts growing the explosion
            StartCoroutine(ExpandExplosion(explosion));
            //Grows the shockwave and then destroys it once it reaches its max size
            StartCoroutine(ExpandAndDestroy(explosion.transform.GetChild(1)));
        }
    }

    /// <summary>
    /// When an explosion is created it grows rapidly at first, then it slows down
    /// significantly, this is meant to represent an initial explosion followed by
    /// a slowly propagating fire, the fire will grow indefinitely.
    /// </summary>
    /// <param name="explosion">The Explosion GameObject spawned</param>
    /// <returns>This is a Coroutine</returns>
    IEnumerator ExpandExplosion(GameObject explosion)
    {
        float radius = 0f;

        //expand forever
        while (true)
        {
            //checks if current radius has passed the explosion phase and should
            //be considered a fire, fires expand much slower
            float speed = radius < explosionSlowRadius ? explosionInitialSpeed : explosionSlowSpeed;
            
            //grows the radius based on expansion speed and how much time has passed
            radius += speed * Time.deltaTime;
            
            float diameter = radius * 2f;
            
            //applies the scale transformation
            explosion.transform.localScale = Vector3.one * diameter;


            yield return null;
        }

    }

    /// <summary>
    /// When an explosion is created a shockwave is also released growing even
    /// more rapidly, but dying out after a while, this Coroutine grows the size
    /// of the shockwave and then destroys it once it reaches its maximum size
    /// </summary>
    /// <param name="shockwave">The transform of the shockwave to grow and destroy</param>
    /// <returns>This is a Coroutine</returns>
    IEnumerator ExpandAndDestroy (Transform shockwave)
    {
        float radius = 0f;
        float elapsed = 0f;

        //expand shockwave for a set duration
        while (elapsed < shockWaveDuration)
        {
            //increment time passed
            elapsed += Time.deltaTime;
            //increment radius based on a constant and time passed
            radius += shockWaveExpansionSpeedMult * Time.deltaTime;
            float diameter = radius * 2f;

            //apply the scale transformation
            if (shockwave != null)
                //the shockwave only grows in x and z so it remains flat
                shockwave.localScale = new Vector3(diameter, 0.1f, diameter);

            yield return null;
        }

        //once it has reached it's maximum size, destroy it.
        if(shockwave != null)
            Destroy(shockwave.gameObject);
    }
}
