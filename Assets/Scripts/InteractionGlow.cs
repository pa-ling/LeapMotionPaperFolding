using Leap.Unity;
using Leap.Unity.Interaction;
using UnityEngine;

[RequireComponent(typeof(InteractionBehaviour))]
public class InteractionGlow : MonoBehaviour
{

    [Tooltip("If enabled, the object will lerp to its hoverColor when a hand is nearby.")]
    public bool useHover = false;

    [Tooltip("If enabled, the object will use its primaryHoverColor when the primary hover of an InteractionHand.")]
    public bool usePrimaryHover = true;

    [Header("InteractionBehaviour Colors")]
    public Color defaultColor = Color.white;
    public Color suspendedColor = Color.red;
    public Color hoverColor = Color.white;
    public Color primaryHoverColor = Color.Lerp(Color.blue, Color.white, 0.8F);
    public Color graspColor = Color.Lerp(Color.blue, Color.white, 0.5F);
    public Color multiGraspColor = Color.Lerp(Color.blue, Color.white, 0.2F);

    private Material _material;

    private InteractionBehaviour _intObj;

    void Start()
    {
        _intObj = GetComponent<InteractionBehaviour>();

        Renderer renderer = GetComponent<Renderer>();
        if (renderer == null)
        {
            renderer = GetComponentInChildren<Renderer>();
        }
        if (renderer != null)
        {
            _material = renderer.material;
        }
    }

    void Update()
    {
        if (_material != null)
        {
            // The target color for the Interaction object will be determined by various simple state checks.
            Color targetColor = defaultColor;

            // "Primary hover" is a special kind of hover state that an InteractionBehaviour can
            // only have if an InteractionHand's thumb, index, or middle finger is closer to it
            // than any other interaction object.
            if (_intObj.isPrimaryHovered && usePrimaryHover)
            {
                targetColor = primaryHoverColor;
            }
            else if (_intObj.isHovered && useHover)
            {
                // Of course, any number of objects can be hovered by any number of InteractionHands.
                // InteractionBehaviour provides an API for accessing various interaction-related
                // state information such as the closest hand that is hovering nearby, if the object
                // is hovered at all.
                float glow = _intObj.closestHoveringControllerDistance.Map(0F, 0.2F, 1F, 0.0F);
                targetColor = Color.Lerp(defaultColor, hoverColor, glow);
            }

            if (_intObj.isGrasped)
            {
                if (1 < _intObj.graspingHands.Count)
                {
                    targetColor = multiGraspColor;
                } else
                {
                    targetColor = graspColor;
                }
            }

            if (_intObj.isSuspended)
            {
                // If the object is held by only one hand and that holding hand stops tracking, the
                // object is "suspended." InteractionBehaviour provides suspension callbacks if you'd
                // like the object to, for example, disappear, when the object is suspended.
                // Alternatively you can check "isSuspended" at any time.
                targetColor = suspendedColor;
            }

            // Lerp actual material color to the target color.
            _material.color = Color.Lerp(_material.color, targetColor, 30F * Time.deltaTime);
        }
    }

}
