using UnityEngine;

namespace DavidJalbert
{
    [DisallowMultipleComponent]
    public class TinyCarBodyModule : TinyCarModule
    {
        public override int ExecutionOrder
        {
            get { return 0; }
        }

        protected override void OnRegistered()
        {
            State.Body = Car.body != null ? Car.body : Car.GetComponent<Rigidbody>();
            State.SphereCollider = Car.GetComponent<SphereCollider>();
            Car.body = State.Body;

            State.PhysicsMaterial = new PhysicsMaterial
            {
                bounciness = 0f,
                bounceCombine = PhysicsMaterialCombine.Minimum,
                staticFriction = 0f,
                dynamicFriction = 0f,
                frictionCombine = PhysicsMaterialCombine.Minimum
            };
        }

        public override void CarUpdate(float deltaTime)
        {
            if (State.Body == null || State.SphereCollider == null)
            {
                OnRegistered();
            }

            State.Body.hideFlags = HideFlags.NotEditable;
            State.SphereCollider.hideFlags = HideFlags.NotEditable;

            State.Body.mass = Car.BodyMass * (Car.AdjustToScale ? State.CubicScale : 1f);
            State.Body.linearDamping = 0f;
            State.Body.angularDamping = 0f;
            State.Body.constraints = RigidbodyConstraints.FreezeRotation;
            State.Body.useGravity = false;
            State.Body.isKinematic = false;
            State.Body.interpolation = RigidbodyInterpolation.Extrapolate;
            State.Body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            State.SphereCollider.radius = Car.ColliderRadius;
            State.SphereCollider.isTrigger = false;
            State.SphereCollider.material = State.PhysicsMaterial;
            State.PreviousPosition = transform.position;
        }

        public override void CarFixedUpdate(float deltaTime)
        {
            State.AverageScale = (transform.lossyScale.x + transform.lossyScale.y + transform.lossyScale.z) / 3f;
            State.ScaleAdjustment = Car.AdjustToScale ? State.AverageScale : 1f;
            State.InverseScaleAdjustment = State.ScaleAdjustment == 0f ? 0f : 1f / State.ScaleAdjustment;
            State.RealColliderRadius = State.SphereCollider.radius * State.AverageScale;
            State.CubicScale = Car.AdjustToScale ? Mathf.Pow(State.AverageScale, 3f) : 1f;
        }
    }
}
