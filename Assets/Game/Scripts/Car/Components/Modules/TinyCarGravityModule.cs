using UnityEngine;

namespace DavidJalbert
{
    [DisallowMultipleComponent]
    public class TinyCarGravityModule : TinyCarModule
    {
        public override int ExecutionOrder
        {
            get { return 40; }
        }

        public override void CarFixedUpdate(float deltaTime)
        {
            State.GravityDirection = Vector3.zero;
            switch (Car.GravityMode)
            {
                case TinyCarController.GRAVITY_MODE.AlwaysDown:
                    State.GravityDirection = Vector3.down;
                    break;
                case TinyCarController.GRAVITY_MODE.TowardsGround:
                    State.GravityDirection = State.OnGround ? -State.CrossUp : Vector3.down;
                    break;
            }

            float adjustedMaxGravity = Car.MaxGravity * State.ScaleAdjustment;
            float adjustedGravityVelocity = Car.GravityVelocity * State.ScaleAdjustment;
            State.WorkingVelocity += State.GravityDirection *
                Mathf.Min(Mathf.Max(0f, adjustedMaxGravity + State.WorkingVelocity.y), adjustedGravityVelocity * deltaTime);
        }
    }
}
