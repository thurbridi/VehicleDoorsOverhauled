using System;
using System.Collections.Generic;
using MSCLoader;
using UnityEngine;

namespace VehicleDoorsOverhauled
{
  public abstract class VehiclePatcher
  {
    protected virtual float DefaultPlayerInteractionTorque => 50f;
    protected virtual float DefaultDoorCheckBreakTorque => 75f;
    protected virtual float DefaultAngularVelocityToCloseDoor => 2.2f;
    protected virtual float DefaultStaticFrictionTorque => 5f;
    protected virtual float DefaultDynamicFrictionTorque => 3f;

    protected float PlayerInteractionTorque => playerInteractionTorqueSlider.GetValue();
    protected float DoorCheckBreakTorque => doorCheckBreakTorqueSlider.GetValue();
    protected float AngularVelocityToCloseDoor => angularVelocityToCloseDoorSlider.GetValue();
    protected float StaticFrictionTorque => staticFrictionTorqueSlider.GetValue();
    protected float DynamicFrictionTorque => dynamicFrictionTorqueSlider.GetValue();

    public readonly string VehicleName;
    protected readonly Func<Transform> FindVehicle;
    private SettingsHeader settingsHeader;
    private SettingsCheckBox shouldPatchCheckBox;
    private SettingsSlider playerInteractionTorqueSlider;
    private SettingsSlider doorCheckBreakTorqueSlider;
    private SettingsSlider angularVelocityToCloseDoorSlider;
    private SettingsSlider staticFrictionTorqueSlider;
    private SettingsSlider dynamicFrictionTorqueSlider;

    private readonly List<Action> configUpdaters = new List<Action>();

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

      Action onChanged = UpdateDoorConfigs;
      playerInteractionTorqueSlider = Settings.AddSlider($"playerInteractionTorque{VehicleName}", "Player Interaction Torque", 10f, 300f, DefaultPlayerInteractionTorque, onChanged, 1);
      doorCheckBreakTorqueSlider = Settings.AddSlider($"doorCheckBreakTorque{VehicleName}", "Door Check Break Torque", 0f, 300f, DefaultDoorCheckBreakTorque, onChanged, 1);
      angularVelocityToCloseDoorSlider = Settings.AddSlider($"angularVelocityToCloseDoor{VehicleName}", "Angular Velocity to Close Door", 0.5f, 10f, DefaultAngularVelocityToCloseDoor, onChanged, 2);
      staticFrictionTorqueSlider = Settings.AddSlider($"staticFrictionTorque{VehicleName}", "Static Friction Torque", 0f, 50f, DefaultStaticFrictionTorque, onChanged, 1);
      dynamicFrictionTorqueSlider = Settings.AddSlider($"dynamicFrictionTorque{VehicleName}", "Dynamic Friction Torque", 0f, 50f, DefaultDynamicFrictionTorque, onChanged, 1);
      Settings.AddButton("Reset to defaults", ResetToDefaults, SettingsButton.ButtonIcon.Reset);

      OnCreateSettings();
    }

    private void UpdateDoorConfigs()
    {
      foreach (Action updater in configUpdaters)
        updater();
    }

    private void ResetToDefaults()
    {
      playerInteractionTorqueSlider.SetValue(DefaultPlayerInteractionTorque);
      doorCheckBreakTorqueSlider.SetValue(DefaultDoorCheckBreakTorque);
      angularVelocityToCloseDoorSlider.SetValue(DefaultAngularVelocityToCloseDoor);
      staticFrictionTorqueSlider.SetValue(DefaultStaticFrictionTorque);
      dynamicFrictionTorqueSlider.SetValue(DefaultDynamicFrictionTorque);
      UpdateDoorConfigs();
    }

    protected void RegisterConfigUpdater(VehicleDoor.Config config, DoorSide side)
    {
      bool isLeft = side == DoorSide.Left;
      configUpdaters.Add(() =>
      {
        config.playerOpenTorque = isLeft ? PlayerInteractionTorque : -PlayerInteractionTorque;
        config.playerCloseTorque = isLeft ? -PlayerInteractionTorque : PlayerInteractionTorque;
        config.doorCheckBreakTorque = DoorCheckBreakTorque;
        config.staticFrictionTorque = StaticFrictionTorque;
        config.dynamicFrictionTorque = DynamicFrictionTorque;
      });
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
      VehicleDoor.Config config = new VehicleDoor.Config()
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
      RegisterConfigUpdater(config, DoorSide.Left);
      return config;
    }

    protected VehicleDoor.Config CreateRightDoorConfig(
      GameObject door,
      Rigidbody vehicleRigidbody,
      float nearClosedAngle = 265f,
      float doorCheckAngle = 190f)
    {
      VehicleDoor.Config config = new VehicleDoor.Config()
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
      RegisterConfigUpdater(config, DoorSide.Right);
      return config;
    }
  }
}