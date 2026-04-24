using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TerrainUtils;
using UnityEngine.UI;
public class Abilitiies : MonoBehaviour
{
    private PlayerInput playerInput;
    private PlayerScript playerScript;
    private PlayerStats playerStats;
    private QiController qiController;

    private Vector3 aimDir;
    private Quaternion aimRot;

    #region Ability-1
    [Header("Ability 1")]
    public Sprite abilityDefault;
    public Sprite abilityEarth;
    public Sprite abilityRiver;
    public Sprite abilityBush;

    public Image abilityImage1;
    public Image abilityImage1Greyed;
    public TextMeshProUGUI abilityText1;
    public InputAction ability1Action;
    public float ability1Cooldown = 5f;

    public Canvas ability1Canvas;
    public Image ability1Skillshot;
    public Image ability1EmpoweredSkillshot;

    public GameObject ability1Hitbox;
    public float ability1HitboxDuration;
    public GameObject ability1ProjectilePrefab;

    bool isEmpowered = false;
    bool isAbility1OnCooldown = false;
    float currentCooldown1;
    #endregion
    #region Ability-2
    [Header("Ability 2")]
    // Canvas and Setup
    public Image abilityImage2Greyed;
    public TextMeshProUGUI abilityText2;
    public InputAction ability2Action;


    public Canvas ability2Canvas;
    public Image ability2RangeIndicator;
    public Image ability2JumpIndicator;


    public Canvas ability2TargetCanvas;
    public Image ability2TargetIndicator;


    bool isAbility2OnCooldown = false;
    float currentCooldown2;

    // Ability 2 Values
    public float ability2Cooldown = 5f;
    public float maxAbility2Range = 5f;
    public float ability2TargetRange = 10f;

    public float maxHopDistance = 5f;
    public float hopHeight = 2f;
    public float hopSpeed = 1f;
    #endregion
    #region Ability-3
    [Header("Ability 3")]
    public Image abilityImage3Greyed;
    public TextMeshProUGUI abilityText3;
    public InputAction ability3Action;
    public float ability3Cooldown = 5f;

    public Canvas ability3Canvas;
    public Image ability3RangeIndicator;
    public Image ability3DashIndicator;

    public float maxAbility3Range = 5f;
    public float dashDuration = 0.5f;
    private bool isDashing = false;

    bool isAbility3OnCooldown = false;
    float currentCooldown3;
    #endregion
    #region Ultimate-Ability
    [Header("Ultimate Ability")]
    public Image ultimateAbilityImageGreyed;
    public TextMeshProUGUI ultimateAbilityText;
    public InputAction ultimateAbilityAction;
    public float ultimateAbilityCooldown = 5f;

    public Canvas ultimateAbilityCanvas;
    public Image ultimateAbilitySkillshot;

    bool isUltimateAbilityOnCooldown = false;
    float currentUltimateCooldown;
    #endregion

    private ElementType currentElement = ElementType.None;

    [SerializeField] private LayerMask terrainMask;
    [SerializeField] private LayerMask groundMask;

    public GameObject target;

    public Coroutine currentCoroutine;
    
    private Vector3 position;
    private RaycastHit hit;
    private Ray ray;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        playerScript = GetComponent<PlayerScript>();
        playerStats = GetComponent<PlayerStats>();
        qiController = GetComponent<QiController>();

        ability1Action = playerInput.actions["Ability 1"];
        ability2Action = playerInput.actions["Ability 2"];
        ability3Action = playerInput.actions["Ability 3"];
        ultimateAbilityAction = playerInput.actions["Ultimate"];

        ability1Canvas.gameObject.SetActive(false);
        ability1Hitbox.SetActive(false);
        ability2Canvas.gameObject.SetActive(false);
        ability2TargetCanvas.gameObject.SetActive(false);
        ability3Canvas.gameObject.SetActive(false);
        ultimateAbilityCanvas.gameObject.SetActive(false);
    }
    void Start()
    {
        abilityImage1Greyed.fillAmount = 0;
        abilityImage2Greyed.fillAmount = 0;
        abilityImage3Greyed.fillAmount = 0;
        ultimateAbilityImageGreyed.fillAmount = 0;

        abilityText1.text = string.Empty;
        abilityText2.text = string.Empty;
        abilityText3.text = string.Empty;
        ultimateAbilityText.text = string.Empty;

        var setAbility1Hitbox = ability1Hitbox.GetComponent<SingleTickDamage>();
        setAbility1Hitbox.playerStats = playerStats;
        setAbility1Hitbox.Initialize();

    }

    // Update is called once per frame
    void Update()
    {
        ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        StartCoroutine(Ability1Input());
        Ability2Input();
        Ability3Input();
        UltimateAbilityInput();

        AbilityCooldown(ref currentCooldown1, ability1Cooldown, ref isAbility1OnCooldown, abilityImage1Greyed, abilityText1);
        AbilityCooldown(ref currentCooldown2, ability2Cooldown, ref isAbility2OnCooldown, abilityImage2Greyed, abilityText2);
        AbilityCooldown(ref currentCooldown3, ability3Cooldown, ref isAbility3OnCooldown, abilityImage3Greyed, abilityText3);
        AbilityCooldown(ref currentUltimateCooldown, ultimateAbilityCooldown, ref isUltimateAbilityOnCooldown, ultimateAbilityImageGreyed, ultimateAbilityText);

        Ability1Canvas();
        Ability2Canvas();
        Ability3Canvas();
        UltimateAbilityCanvas();
    }

    #region Ability-1-Methods
    void Ability1Canvas()
    {
        if (ability1Canvas.enabled)
        {
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                position = new Vector3(hit.point.x, hit.point.y, hit.point.z);
                
                GetAimDirection(position);
            }

            Quaternion ab1Canvas = Quaternion.LookRotation(position - transform.position);
            ab1Canvas.eulerAngles = new Vector3(0, ab1Canvas.eulerAngles.y, ab1Canvas.eulerAngles.z);

            ability1Canvas.transform.rotation = Quaternion.Lerp(ab1Canvas, ability1Canvas.transform.rotation.normalized, 0);
        }
    }

    IEnumerator Ability1Input()
    {
        int i = 1;

        if (ability1Action.WasPressedThisFrame())
        {
            // Activate ability 1
            SetActive(i);
            SetInactive(i);
            Cursor.visible = true;

            //Debug.Log("Ability 1 Previewed!");
        }            
        if (ability1Action.WasReleasedThisFrame())
        {
            SetInactive(0); // Deactivate all ability canvases when the button is released

            if (!isAbility1OnCooldown && ability1Canvas.enabled)
            {
                //Debug.Log("Ability 1 Activated!");
                if (currentElement == ElementType.None)
                {
                    yield return StartCoroutine(playerScript.WaitTillLanded());
                    playerScript.FaceAimDirection(aimRot);
                    ability1Hitbox.SetActive(true);
                    yield return new WaitForSeconds(ability1HitboxDuration);
                    ability1Hitbox.SetActive(false);

                }
                else
                {
                    //Debug.Log("Ability 1 was empowered with the element: " + currentElement);

                    yield return currentCoroutine = StartCoroutine(playerScript.WaitTillLanded());
                    playerScript.FaceAimDirection(aimRot);
                    Instantiate(ability1ProjectilePrefab, transform.position + Vector3.forward, aimRot).GetComponent<EdgeOfIxtalProjectile>().Initialize(currentElement, playerStats.attackDamage, playerStats.targetLayer);

                    currentCoroutine = null;
                    currentElement = ElementType.None;
                    isEmpowered = false;
                    abilityImage1.sprite = abilityDefault;
                }
                isAbility1OnCooldown = true;
                currentCooldown1 = ability1Cooldown;
            }
        }
        yield break;
    }
    #endregion
    #region Ability-2-Methods
    void Ability2Canvas()
    {
        if (ability2Canvas.enabled)
        {
            //int layerMask = ~LayerMask.GetMask("Player");

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundMask))
            {
                if (hit.collider.gameObject != this.gameObject)
                {
                    position = hit.point;
                }
            }
            // Jump Indication
            Quaternion ab2Canvas = Quaternion.LookRotation(position - transform.position);
            ab2Canvas.eulerAngles = new Vector3(0, ab2Canvas.eulerAngles.y, ab2Canvas.eulerAngles.z);

            ability2Canvas.transform.rotation = Quaternion.Lerp(ab2Canvas, ability2Canvas.transform.rotation.normalized, 0);

            // Target Indication
            Vector3 flatDir = hit.point - transform.position;       // Flatten the direction vector to ignore vertical differences
            flatDir.y = 0; // remove vertical influence
            flatDir.Normalize();

            float distance = Vector3.Distance(hit.point, transform.position);
            distance = Mathf.Min(distance, maxAbility2Range);

            var newHitPos = transform.position + flatDir * distance;
            ability2TargetCanvas.transform.position = newHitPos;

        }
    }
    void Ability2Input()
    {
        int i = 2;

        if (ability2Action.WasPressedThisFrame())
        {
            SetActive(i);
            SetInactive(i);
            Cursor.visible = true;

            //Debug.Log("Ability 2 Previewed!");
        }
        if (ability2Action.WasReleasedThisFrame())
        {
            SetInactive(0);

            if (!isAbility2OnCooldown && ability2Canvas.enabled)
            {
                //Debug.Log("Ability 2 Activated!");

                Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

                if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundMask))
                {
                    Vector3 cursorWorldPos = hit.point;

                    Vector3 aimDir = cursorWorldPos - transform.position;
                    aimDir.y = 0f;        // flatten to horizontal
                    aimDir.Normalize();   // normalize for consistent distance
                    if (GetTerrain() != null)
                    {
                        //Debug.Log("Terrain element targeted: " + GetTerrain().elementType);
                        TryHop(aimDir);
                        isEmpowered = true;
                        isAbility2OnCooldown = true;
                        currentCooldown2 = ability2Cooldown;
                        currentCooldown1 = 0; // Reset cooldown of ability 1 if it was on cooldown, since ability 2 is used
                    }
                    else
                    {
                        Debug.Log("No terrain element targeted. Cannot perform action");
                    }

                }
            }
        }
    }
    void TryHop(Vector3 aimDir)
    {
        //Debug.Log("Attempting to hop in direction: " + aimDir);
        var hopDistance = maxHopDistance;
        var minHopDistance = .1f;

        Vector3 start = transform.position;

        while (hopDistance > minHopDistance)
        {
            Vector3 endpoint = start + aimDir * hopDistance;

            if (IsValidPoint(endpoint))
            {
                //Debug.Log("Valid hop point found at distance: " + hopDistance);
                StartCoroutine(HopArc(start, endpoint));
                return;
            }

            hopDistance -= 0.1f;
            //Debug.Log("Hop distance " + hopDistance + " is not valid, trying shorter distance...");
        }

    }
    IEnumerator HopArc(Vector3 start, Vector3 end)
    {
        qiController.agent.enabled = false;

        //Debug.Log("Hopping from " + start + " to " + end);
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * hopSpeed;

            // Horizontal movement
            Vector3 pos = Vector3.Lerp(start, end, t);

            // Vertical arc
            float height = Mathf.Sin(t * Mathf.PI) * hopHeight;
            pos.y += height;

            transform.position = pos;

            yield return null;
        }
        qiController.agent.enabled = true;
    }
    TerrainElementVolume GetTerrain()
    {
        // Direct hit from cursor
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, terrainMask))
        {
            if (hit.collider.TryGetComponent(out TerrainElementVolume directVolume))
            {
                //Debug.Log("Direct hit on terrain element: " + directVolume.elementType);
                currentElement = directVolume.elementType;
                Debug.Log(currentElement);
                GetProfileForElement(currentElement);
                return directVolume;
            }
        }

        // No direct hit > find closest volume within radius
        if (Physics.Raycast(ray, out RaycastHit groundHit, Mathf.Infinity, groundMask))
        {
            Vector3 cursorWorldPos = groundHit.point;

            Collider[] hits = Physics.OverlapSphere(cursorWorldPos, ability2TargetRange, terrainMask);

            TerrainElementVolume closest = null;
            float closestDist = float.MaxValue;

            foreach (var col in hits)
            {
                if (col.TryGetComponent(out TerrainElementVolume volume))
                {
                    float dist = Vector3.Distance(cursorWorldPos, col.ClosestPoint(cursorWorldPos));

                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = volume;
                    }
                }
            }
            //Debug.Log("Closest terrain element: " + (closest != null ? closest.elementType.ToString() : "None") + " at distance: " + closestDist);
            if (closest != null)
            {
                currentElement = closest.elementType;
                Debug.Log(currentElement);
                GetProfileForElement(currentElement);
                return closest;
            }
        }
        //Debug.Log("No terrain element found within range.");
        return null;

    }
    void GetProfileForElement(ElementType element)
    {

        switch (element)
        {
            case ElementType.Earth:
                abilityImage1.sprite = abilityEarth;
                break;
            case ElementType.River:
                abilityImage1.sprite = abilityRiver;
                break;
            case ElementType.Brush:
                abilityImage1.sprite = abilityBush;
                break;
            default:
                break;
        }
    }
    void OnDrawGizmosSelected() // Visualize the search radius for terrain elements when the object is selected in the editor
    {
        var capsule = GetComponent<CapsuleCollider>();

        Vector3 feet = capsule.bounds.center - new Vector3(0, capsule.bounds.extents.y, 0);


        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(feet, ability2TargetRange);
    }
    #endregion
    #region Ability-3-Methods
    // NOTE: Usually The indicator for the dash grows or shrinks depending on the distance to the target, but for simplicity, it will just be a fixed size indicator that shows the direction of the dash.
    void Ability3Canvas()
    {
        if (ability3DashIndicator.enabled)
        {
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                position = new Vector3(hit.point.x, hit.point.y, hit.point.z);
            }

            Quaternion ab3Canvas = Quaternion.LookRotation(position - transform.position);
            ab3Canvas.eulerAngles = new Vector3(0, ab3Canvas.eulerAngles.y, ab3Canvas.eulerAngles.z);

            ability3Canvas.transform.rotation = Quaternion.Lerp(ab3Canvas, ability3Canvas.transform.rotation.normalized, 0);
        }
    }
    void Ability3Input()
    {
        int i = 3;

        if (ability3Action.IsPressed())
        {
            SetActive(i);
            SetInactive(i);
            Cursor.visible = true;

            Debug.Log("Ability 3 Previewed!");

            if (!isAbility3OnCooldown && Physics.Raycast(ray, out hit, Mathf.Infinity, playerStats.targetLayer))
                target = hit.rigidbody.gameObject;
  
        }
        if (ability3Action.WasReleasedThisFrame())
        {
            SetInactive(0);

            // Set target for PlayerScript as well
            // TODO: Finish implementing 3rd ability logic

            if (!isAbility3OnCooldown && target != null && ability3Canvas.enabled)
            {
                Debug.Log($"Target set to {target.name}");

                if (currentCoroutine != null)
                    StopCoroutine(currentCoroutine);

                currentCoroutine = StartCoroutine(DashCoroutine(target));

            }
        }
    }

    private IEnumerator DashCoroutine(GameObject target)
    {
        //var distance = Vector3.Distance(transform.position, target);
        //Debug.Log($"Distance between objects: {distance}");

        playerScript.SetTarget(target);

        yield return new WaitUntil(()=> Vector3.Distance(transform.position, target.transform.position) <= maxAbility3Range);
        //playerScript.agent.isStopped = true;

        Debug.Log($"Method triggered once player was {Vector3.Distance(transform.position, target.transform.position)} units away.");

        Debug.Log("Ability 3 Activated!");
        isAbility3OnCooldown = true;
        isDashing = true;
        currentCooldown3 = ability3Cooldown;

        var dashRot = GetAimDirection(target.transform.position);
        Vector3 dashDir = dashRot * Vector3.forward;
        Vector3 dashEnd = transform.position + dashDir * maxAbility3Range;

        float t = 0f;
        float duration = dashDuration;
        Vector3 start = transform.position;

        playerScript.agent.enabled = false;

        while (t < 1f)
        {
            transform.position = Vector3.Lerp(start, dashEnd, t);
            t += Time.deltaTime / duration;
            yield return null;
        }

        transform.position = dashEnd;
        playerScript.agent.enabled = true;
        playerScript.agent.Warp(transform.position);
        isDashing = false;

        yield return null;
    }
    #endregion
    #region Ultimate-Ability-Methods
    void UltimateAbilityCanvas()
    {
        if (ultimateAbilitySkillshot.enabled)
        {
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                position = new Vector3(hit.point.x, hit.point.y, hit.point.z);
            }

            Quaternion ultCanvas = Quaternion.LookRotation(position - transform.position);
            ultCanvas.eulerAngles = new Vector3(0, ultCanvas.eulerAngles.y, ultCanvas.eulerAngles.z);

            ultimateAbilityCanvas.transform.rotation = Quaternion.Lerp(ultCanvas, ultimateAbilityCanvas.transform.rotation.normalized, 0);
        }
    }
    void UltimateAbilityInput()
    {
        int i = 4;

        if (ultimateAbilityAction.WasPressedThisFrame())
        {
            // Activate ability 1
            SetActive(i);
            SetInactive(i);
            Cursor.visible = true;

            Debug.Log("Ultimate Ability Previewed!");
        }
        if (ultimateAbilityAction.WasReleasedThisFrame())
        {
            SetInactive(0); // Deactivate all ability canvases when the button is released

            if (!isUltimateAbilityOnCooldown && ultimateAbilityCanvas.enabled)
            {
                Debug.Log("Ultimate Ability Activated!");
                isUltimateAbilityOnCooldown = true;
                currentUltimateCooldown = ultimateAbilityCooldown;
            }
        }
    }
    #endregion

    void AbilityCooldown(ref float currentCooldown, float maxCooldown, ref bool isOnCooldown, Image abilityImage, TextMeshProUGUI abilityText)
    {
        if (isOnCooldown)
        {
            currentCooldown -= Time.deltaTime;
            abilityImage.fillAmount = (currentCooldown / maxCooldown);
            abilityText.text = Mathf.Ceil(currentCooldown).ToString();
            if (currentCooldown <= 0)
            {
                isOnCooldown = false;
                //abilityImage.fillAmount = 0;
                abilityText.text = string.Empty;
            }
        }
    }
    void SetInactive(int i)
    {
        if (i != 1)
        {
            ability1Canvas.gameObject.SetActive(false);
            ability1Skillshot.gameObject.SetActive(false);
            ability1EmpoweredSkillshot.gameObject.SetActive(false);
        }
        if (i != 2)
        {
            ability2Canvas.gameObject.SetActive(false);
            ability2TargetCanvas.gameObject.SetActive(false);
            ability2RangeIndicator.gameObject.SetActive(false);
            ability2JumpIndicator.gameObject.SetActive(false);
            ability2TargetIndicator.gameObject.SetActive(false);
        }
        if (i != 3)
        {
            ability3Canvas.gameObject.SetActive(false);
            ability3RangeIndicator.gameObject.SetActive(false);
            ability3DashIndicator.gameObject.SetActive(false);
        }
        if (i != 4)
        {
            ultimateAbilityCanvas.gameObject.SetActive(false);
            ultimateAbilitySkillshot.gameObject.SetActive(false);
        }
    }
    void SetActive(int i)
    {
        if (i == 1)
        {
            ability1Canvas.gameObject.SetActive(true);
            if (!isEmpowered)
                ability1Skillshot.gameObject.SetActive(true);
            else
                ability1EmpoweredSkillshot.gameObject.SetActive(true);
        }
        if (i == 2)
        {
            ability2Canvas.gameObject.SetActive(true);
            ability2TargetCanvas.gameObject.SetActive(true);
            ability2RangeIndicator.gameObject.SetActive(true);
            ability2JumpIndicator.gameObject.SetActive(true);
            ability2TargetIndicator.gameObject.SetActive(true);

        }
        if (i == 3)
        {
            ability3Canvas.gameObject.SetActive(true);
            ability3RangeIndicator.gameObject.SetActive(true);
            ability3DashIndicator.gameObject.SetActive(true);
        }
        if (i == 4)
        {
            ultimateAbilityCanvas.gameObject.SetActive(true);
            ultimateAbilitySkillshot.gameObject.SetActive(true);
        }
    }
    bool IsValidPoint(Vector3 point)
    {
        Vector3 origin = point + Vector3.up * 2f;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 5f, groundMask))
        {
            return true;
        }
        return false;
    }

    Quaternion GetAimDirection(Vector3 targetPos)
    {

        aimDir = targetPos - transform.position;
        aimDir.y = 0f;
        aimDir.Normalize();

        aimRot = Quaternion.LookRotation(aimDir, Vector3.up);
        return aimRot;
    }

    private void OnCollisionEnter(Collision other)
    { 
        if (!isDashing) { return; }

        if (other.gameObject.layer == playerStats.targetLayer && isDashing)
        {
            //Debug.Log("Collided with target: " + other.gameObject.name);

            if (other.gameObject.TryGetComponent<TargetDummy>(out TargetDummy targetStats))
            {
                //Debug.Log("Projectile hit player with current currentHealth: " + targetStats.currentHealth);

                targetStats.ChangeHealth(-playerStats.attackDamage);
                isDashing = false;
            }
        }
    }
}
