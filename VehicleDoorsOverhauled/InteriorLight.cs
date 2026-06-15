using UnityEngine;
using MSCLoader;
using HutongGames.PlayMaker;
using System;


namespace VehicleDoorsOverhauled
{
  public class InteriorLight : MonoBehaviour
  {
    public enum SwitchPosition
    {
      ON,
      DOORS,
      OFF,
    }

    public SwitchPosition[] availablePositions;
    public int idxCurrentPosition = 0, openedDoors = 0;
    public Collider interactionCollider;
    public GameObject lightObject;
    public string interactionLabel;
    public bool IsLightOn { get; private set; }
    private FsmBool guiUse;
    private FsmString guiInteraction;
    private bool isColliderHit = false, wasColliderHit = false, isInitialized = false;
    private Action onSwitch;

    public void NextSwitchPosition()
    {
      idxCurrentPosition = (idxCurrentPosition + 1) % availablePositions.Length;
      UpdateLight();
      onSwitch?.Invoke();
    }

    public SwitchPosition GetSwitchPosition()
    {
      return availablePositions[idxCurrentPosition];
    }

    public void OnDoorOpened()
    {
      openedDoors++;
      UpdateLight();
    }

    public void OnDoorClosed()
    {
      openedDoors = Math.Max(openedDoors - 1, 0);
      UpdateLight();
    }

    private void UpdateLight()
    {
      switch (GetSwitchPosition())
      {
        case SwitchPosition.ON:
          IsLightOn = true;
          break;
        case SwitchPosition.DOORS:
          IsLightOn = openedDoors > 0;
          break;
        case SwitchPosition.OFF:
          IsLightOn = false;
          break;
      }
      lightObject.SetActive(IsLightOn);
    }

    private string GetUseText()
    {
      return GetSwitchPosition() switch
      {
        SwitchPosition.ON => "ON",
        SwitchPosition.DOORS => "DOORS",
        SwitchPosition.OFF => "OFF",
        _ => "",
      };
    }

    public void Initialize(SwitchPosition[] availablePositions, GameObject lightObject, Action onSwitch = null, string interactionLabel = null)
    {
      if (availablePositions.Length == 0)
        throw new ArgumentException("availablePositions cannot be empty.");

      interactionCollider = gameObject.GetComponent<Collider>();
      guiUse = FsmVariables.GlobalVariables.GetFsmBool("GUIuse");
      guiInteraction = FsmVariables.GlobalVariables.GetFsmString("GUIinteraction");

      this.availablePositions = availablePositions;
      this.lightObject = lightObject;
      this.onSwitch = onSwitch;
      this.interactionLabel = interactionLabel;

      isInitialized = true;
      enabled = true;
    }

    void Awake()
    {
      if (!isInitialized)
        enabled = false;
    }

    void Update()
    {
      isColliderHit = UnifiedRaycast.GetHitInteraction(interactionCollider);

      if (isColliderHit)
      {
        guiUse.Value = true;
        guiInteraction.Value = interactionLabel ?? GetUseText();
        wasColliderHit = true;

        if (Input.GetMouseButtonUp(0))
        {
          NextSwitchPosition();
        }
      }
      else if (wasColliderHit)
      {
        guiUse.Value = false;
        guiInteraction.Value = "";
        wasColliderHit = false;
      }
    }
  }
}