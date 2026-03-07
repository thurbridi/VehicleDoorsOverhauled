using System;
using MSCLoader;
using UnityEngine;

namespace VehicleDoorsOverhauled
{
  public abstract class VehiclePatcher
  {
    protected virtual float PlayerInteractionTorque => 50f;
    protected virtual float DoorCheckBreakTorque => 75f;
    protected virtual float AngularVelocityToCloseDoor => 2.2f;
    protected virtual float StaticFrictionTorque => 5f;
    protected virtual float DynamicFrictionTorque => 3f;
    private readonly string VehicleName;
    protected readonly Func<Transform> FindVehicle;
    private SettingsHeader settingsHeader;
    private SettingsCheckBox shouldPatchCheckBox;

    public VehiclePatcher(string vehicleName, Func<Transform> vehicleResolver)
    {
      VehicleName = vehicleName;
      FindVehicle = () =>
      {
        Transform vehicle = vehicleResolver();
        if (!vehicle)
        {
          ModConsole.LogError($"[VehicleDoorsOverhauled] Could not find vehicle {VehicleName} using provided resolver.");
        }
        return vehicle;
      };
    }

    public bool IsEnabled => shouldPatchCheckBox.GetValue();

    public abstract void Patch();

    public void CreateSettings()
    {
      settingsHeader = Settings.AddHeader($"{VehicleName}", collapsedByDefault: true);
      shouldPatchCheckBox = Settings.AddCheckBox(settingID: $"shouldPatch{VehicleName}", name: $"Patch {VehicleName}", value: true);
      OnCreateSettings();
    }

    public void HideSettings()
    {
      settingsHeader.SetVisibility(false);
    }

    protected virtual void OnCreateSettings() { }

    protected virtual void OnDoorOpened(Transform door) { }

    protected virtual void OnDoorClosed(Transform door) { }

    protected VehicleDoor.Config CreateLeftDoorConfig(
      GameObject door,
      Rigidbody vehicleRigidbody,
      float nearClosedAngle = 275f,
      float doorCheckAngle = 350f)
    {
      return new VehicleDoor.Config()
      {
        playerOpenTorque = PlayerInteractionTorque,
        playerCloseTorque = -PlayerInteractionTorque,
        doorCheckBreakTorque = DoorCheckBreakTorque,
        staticFrictionTorque = StaticFrictionTorque,
        dynamicFrictionTorque = DynamicFrictionTorque,
        door = door,
        openHingeLimits = new JointLimits() { min = 0.5f, max = 80f },
        closedHingeLimits = new JointLimits() { min = 0f, max = 0f },
        vehicleRigidbody = vehicleRigidbody,
        onDoorOpened = () => OnDoorOpened(door.transform),
        onDoorClosed = () => OnDoorClosed(door.transform),
        isDoorNearClosedPredicate = (doorAngle) => doorAngle <= nearClosedAngle,
        isPastDoorcheckAnglePredicate = (doorAngle) => doorAngle > doorCheckAngle,
        isDoorFastEnoughToClosePredicate = (doorAngularVelocity) => doorAngularVelocity <= -AngularVelocityToCloseDoor,
        hingeAxis = VehicleDoor.Axis.Z,
        angularVelocityAxis = VehicleDoor.Axis.Y,
        doorAngleAxis = VehicleDoor.Axis.Y,
      };
    }

    protected VehicleDoor.Config CreateRightDoorConfig(
      GameObject door,
      Rigidbody vehicleRigidbody,
      float nearClosedAngle = 265f,
      float doorCheckAngle = 190f)
    {
      return new VehicleDoor.Config()
      {
        playerOpenTorque = -PlayerInteractionTorque,
        playerCloseTorque = PlayerInteractionTorque,
        doorCheckBreakTorque = DoorCheckBreakTorque,
        staticFrictionTorque = StaticFrictionTorque,
        dynamicFrictionTorque = DynamicFrictionTorque,
        door = door,
        openHingeLimits = new JointLimits() { min = -80f, max = -0.5f },
        closedHingeLimits = new JointLimits() { min = 0f, max = 0f },
        vehicleRigidbody = vehicleRigidbody,
        onDoorOpened = () => OnDoorOpened(door.transform),
        onDoorClosed = () => OnDoorClosed(door.transform),
        isDoorNearClosedPredicate = (doorAngle) => doorAngle >= nearClosedAngle,
        isPastDoorcheckAnglePredicate = (doorAngle) => doorAngle < doorCheckAngle,
        isDoorFastEnoughToClosePredicate = (doorAngularVelocity) => doorAngularVelocity >= AngularVelocityToCloseDoor,
        hingeAxis = VehicleDoor.Axis.Z,
        angularVelocityAxis = VehicleDoor.Axis.Y,
        doorAngleAxis = VehicleDoor.Axis.Y,
      };
    }
  }
}