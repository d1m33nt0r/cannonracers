using UnityEngine;

namespace DavidJalbert
{
    [DisallowMultipleComponent]
    public class TinyCarSteeringModule : TinyCarModule
    {
        public override int ExecutionOrder
        {
            get { return 20; }
        }

        public override void CarFixedUpdate(float deltaTime)
        {
            if (State.Body == null || State.SurfaceParameters == null) return;

            float steering = Car.MaxSteering * State.InputSteering;
            float steeringForce =
                (State.OnGround ? 1f : Car.SteeringMultiplierInAir) *
                (State.ForwardVelocity < 0f ? -1f : 1f) *
                Car.SteeringBySpeed.Evaluate(Car.getForwardVelocityDelta()) *
                State.SurfaceParameters.steeringMultiplier;

            State.Body.MoveRotation(Quaternion.Euler(
                0f,
                transform.rotation.eulerAngles.y + steering * deltaTime * steeringForce,
                0f));
        }
    }
}
