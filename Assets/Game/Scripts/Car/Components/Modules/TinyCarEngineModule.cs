using UnityEngine;

namespace DavidJalbert
{
    [DisallowMultipleComponent]
    public class TinyCarEngineModule : TinyCarModule
    {
        public override int ExecutionOrder
        {
            get { return 30; }
        }

        public override void CarFixedUpdate(float deltaTime)
        {
            if (!State.OnGround || State.SurfaceParameters == null) return;

            Vector3 velocity = State.WorkingVelocity;
            TinyCarSurfaceParameters surface = State.SurfaceParameters;

            if (State.InputMotor == 0f || Mathf.Sign(State.InputMotor) != Mathf.Sign(State.ForwardVelocity))
            {
                velocity -= State.CrossForward * (Mathf.Sign(State.ForwardVelocity) * Mathf.Min(
                    Mathf.Abs(State.ForwardVelocity),
                    Car.ForwardFriction * State.ScaleAdjustment * deltaTime * surface.forwardFrictionMultiplier));
            }

            velocity -= State.CrossRight * (Mathf.Sign(State.RightVelocity) * Mathf.Min(
                Mathf.Abs(State.RightVelocity),
                Car.LateralFriction * State.ScaleAdjustment * deltaTime * surface.lateralFrictionMultiplier));

            float slopeMultiplier = Mathf.Max(0f, Mathf.Sign(State.InputMotor) * State.SlopeDelta * Car.SlopeFriction * State.ScaleAdjustment + 1f);
            float accelerationForce = Car.GetAccelerationForce(
                surface.accelerationMultiplier * State.BoostMultiplier * slopeMultiplier,
                surface.speedMultiplier * State.BoostMultiplier * slopeMultiplier);

            velocity += State.CrossForward * (State.InputMotor * accelerationForce * deltaTime);
            State.WorkingVelocity = velocity;
        }
    }
}
