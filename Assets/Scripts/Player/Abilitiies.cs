using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
public class Abilitiies : MonoBehaviour
{
    private PlayerInput playerInput;

    [Header("Ability 1")]
    public Image abilityImage1;
    public TextMeshProUGUI abilityText1;
    public InputAction ability1Action;
    public float ability1Cooldown = 5f;

    public Canvas ability1Canvas;
    public Image ability1Skillshot;

    [Header("Ability 2")]
    public Image abilityImage2;
    public TextMeshProUGUI abilityText2;
    public InputAction ability2Action;
    public float ability2Cooldown = 5f;

    public Canvas ability2Canvas;
    public Image ability2RangeIndicator;
    public Image ability2JumpIndicator;
    public float maxAbility2Range = 5f;

    public Canvas ability2TargetCanvas;
    public Image ability2TargetIndicator;

    [Header("Ability 3")]
    public Image abilityImage3;
    public TextMeshProUGUI abilityText3;
    public InputAction ability3Action;
    public float ability3Cooldown = 5f;

    public Canvas ability3Canvas;
    public Image ability3RangeIndicator;
    public Image ability3DashIndicator;
    public float maxAbility3Range = 5f;

    bool isAbility1OnCooldown = false;
    bool isAbility2OnCooldown = false;
    bool isAbility3OnCooldown = false;

    float currentCooldown1;
    float currentCooldown2;
    float currentCooldown3;

    private Vector3 position;
    private RaycastHit hit;
    private Ray ray;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        ability1Action = playerInput.actions["Ability 1"];
        ability2Action = playerInput.actions["Ability 2"];
        ability3Action = playerInput.actions["Ability 3"];

        ability1Canvas.gameObject.SetActive(false);
        ability2Canvas.gameObject.SetActive(false);
        ability2TargetCanvas.gameObject.SetActive(false);
        ability3Canvas.gameObject.SetActive(false);
    }
    void Start()
    {
        abilityImage1.fillAmount = 0;
        abilityImage2.fillAmount = 0;
        abilityImage3.fillAmount = 0;

        abilityText1.text = string.Empty;
        abilityText2.text = string.Empty;
        abilityText3.text = string.Empty;
    }

    // Update is called once per frame
    void Update()
    {
        ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        Ability1Input();
        Ability2Input();
        Ability3Input();

        AbilityCooldown(ref currentCooldown1, ability1Cooldown, ref isAbility1OnCooldown, abilityImage1, abilityText1);
        AbilityCooldown(ref currentCooldown2, ability2Cooldown, ref isAbility2OnCooldown, abilityImage2, abilityText2);
        AbilityCooldown(ref currentCooldown3, ability3Cooldown, ref isAbility3OnCooldown, abilityImage3, abilityText3);

        Ability1Canvas();
        Ability2Canvas();
        Ability3Canvas();
    }

    void Ability1Canvas()
    {
        if (ability1Skillshot.enabled)
        {
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                position = new Vector3(hit.point.x, hit.point.y, hit.point.z);
            }

            Quaternion ab1Canvas = Quaternion.LookRotation(position - transform.position);
            ab1Canvas.eulerAngles = new Vector3(0, ab1Canvas.eulerAngles.y, ab1Canvas.eulerAngles.z);

            ability1Canvas.transform.rotation = Quaternion.Lerp(ab1Canvas, ability1Canvas.transform.rotation.normalized, 0);
        }
    }

    void Ability1Input()
    {
        int i = 1;

        if (ability1Action.WasPressedThisFrame())
        {
            // Activate ability 1
            SetActive(i);
            SetInactive(i);
            Cursor.visible = true;

            Debug.Log("Ability 1 Previewed!");
        }            
        if (ability1Action.WasReleasedThisFrame())
        {
            SetInactive(0); // Deactivate all ability canvases when the button is released

            if (!isAbility1OnCooldown && ability1Canvas.enabled)
            {
                Debug.Log("Ability 1 Activated!");
                isAbility1OnCooldown = true;
                currentCooldown1 = ability1Cooldown;
            }
        }
    }

    void Ability2Canvas()
    {
        if (ability2Canvas.enabled)
        {
            int layerMask = ~LayerMask.GetMask("Player");

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
            {
                if (hit.collider.gameObject != this.gameObject) // Change later for element gathering
                {
                    position = hit.point;
                }
            }
            // Jump Indication
            Quaternion ab2Canvas = Quaternion.LookRotation(position - transform.position);
            ab2Canvas.eulerAngles = new Vector3(0, ab2Canvas.eulerAngles.y, ab2Canvas.eulerAngles.z);

            ability2Canvas.transform.rotation = Quaternion.Lerp(ab2Canvas, ability2Canvas.transform.rotation.normalized, 0);

            // Target Indication
            Vector3 flatDir = hit.point - transform.position;
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

            Debug.Log("Ability 2 Previewed!");
        }
        if (ability2Action.WasReleasedThisFrame())
        {
            SetInactive(0);

            if (!isAbility2OnCooldown && ability2Canvas.enabled)
            {
                Debug.Log("Ability 2 Activated!");
                isAbility2OnCooldown = true;
                currentCooldown2 = ability2Cooldown;
            }
        }
    }

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

        if (ability3Action.WasPressedThisFrame())
        {
            SetActive(i);
            SetInactive(i);
            Cursor.visible = true;

            Debug.Log("Ability 3 Previewed!");
        }
        if (ability3Action.WasReleasedThisFrame())
        {
            SetInactive(0);

            if (ability3Canvas.enabled && !isAbility3OnCooldown)
            {
                Debug.Log("Ability 3 Activated!");
                isAbility3OnCooldown = true;
                currentCooldown3 = ability3Cooldown;
            }
        }
    }

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

        }
    }
    void SetActive(int i)
    {
        if (i == 1)
        {
            ability1Canvas.gameObject.SetActive(true);
            ability1Skillshot.gameObject.SetActive(true);
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

        }
    }
}
