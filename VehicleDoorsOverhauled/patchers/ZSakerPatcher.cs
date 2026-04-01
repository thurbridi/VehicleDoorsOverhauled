using System;
using UnityEngine;

namespace VehicleDoorsOverhauled
{
  public class ZSakerPatcher : VehiclePatcher
  {
    private Transform doors;
    private Rigidbody vehicleRigidbody;
    private Saker.Car.InteriorLight interiorLightBehaviour;
    private Saker.AmbientVolume ambientVolumeBehaviour;
    private const string audioGroup = "CarFoley";
    private const string audioClipOpen = "open_door1";
    private const string audioClipClose = "close_door1";

    public ZSakerPatcher(string vehicleName, Func<Transform> vehicleResolver) : base(vehicleName, vehicleResolver) {}

    public override void Patch()
    {
      Transform vehicle = FindVehicle();
      vehicleRigidbody = vehicle.GetComponent<Rigidbody>();
      doors = vehicle.Find("Doors");
      interiorLightBehaviour = vehicle.Find("LOD/Interior Light/Interact").GetComponent<Saker.Car.InteriorLight>();
      ambientVolumeBehaviour = vehicle.Find("LOD/Interior Light/Interact").GetComponent<Saker.AmbientVolume>();
      PatchFLDoor();
      PatchFRDoor();
      PatchRLDoor();
      PatchRRDoor();
    }

    protected override void OnDoorOpened(Transform door)
    {
      MasterAudio.PlaySound3DAndForget(sType: audioGroup, sourceTrans: door, variationName: audioClipOpen);
      interiorLightBehaviour.DoorOpened();
      ambientVolumeBehaviour.AddOpenedObject();
    }
    protected override void OnDoorClosed(Transform door)
    {
      MasterAudio.PlaySound3DAndForget(sType: audioGroup, sourceTrans: door, variationName: audioClipClose);
      interiorLightBehaviour.DoorClosed();
      ambientVolumeBehaviour.RemoveOpenedObject();
    }

    private void PatchDoor(string doorName, bool isLeft)
    {
      Transform door = doors.Find(doorName);
      Transform handle = door.Find("Handle");

      Saker.Car.Door doorBehaviour = door.GetComponent<Saker.Car.Door>();
      if (!doorBehaviour) doorBehaviour.enabled = false;

      VehicleDoor vehicleDoor = handle.gameObject.AddComponent<VehicleDoor>();
      vehicleDoor.Initialize(isLeft
        ? CreateSakerLeftDoorConfig(door.gameObject)
        : CreateSakerRightDoorConfig(door.gameObject));
    }

    private VehicleDoor.Config CreateSakerLeftDoorConfig(GameObject door)
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
        isDoorNearClosedPredicate = (doorAngle) => doorAngle <= 10f,
        isPastDoorcheckAnglePredicate = (doorAngle) => doorAngle > 79f,
        isDoorFastEnoughToClosePredicate = (doorAngularVelocity) => doorAngularVelocity <= -AngularVelocityToCloseDoor,
        hingeAxis = VehicleDoor.Axis.Y,
        angularVelocityAxis = VehicleDoor.Axis.Y,
        doorAngleAxis = VehicleDoor.Axis.Y,
      };
      RegisterConfigUpdater(config, DoorSide.Left);
      return config;
    }

    private VehicleDoor.Config CreateSakerRightDoorConfig(GameObject door)
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
        isDoorNearClosedPredicate = (doorAngle) => doorAngle >= 350f,
        isPastDoorcheckAnglePredicate = (doorAngle) => doorAngle < 281f,
        isDoorFastEnoughToClosePredicate = (doorAngularVelocity) => doorAngularVelocity >= AngularVelocityToCloseDoor,
        hingeAxis = VehicleDoor.Axis.Y,
        angularVelocityAxis = VehicleDoor.Axis.Y,
        doorAngleAxis = VehicleDoor.Axis.Y,
      };
      RegisterConfigUpdater(config, DoorSide.Right);
      return config;
    }

    private void PatchFLDoor() => PatchDoor("DoorFL", isLeft: true);
    private void PatchFRDoor() => PatchDoor("DoorFR", isLeft: false);
    private void PatchRLDoor() => PatchDoor("DoorRL", isLeft: true);
    private void PatchRRDoor() => PatchDoor("DoorRR", isLeft: false);
  }
}
