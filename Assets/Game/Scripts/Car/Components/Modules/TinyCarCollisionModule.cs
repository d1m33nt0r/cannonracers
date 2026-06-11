using UnityEngine;

namespace DavidJalbert
{
    [DisallowMultipleComponent]
    public class TinyCarCollisionModule : TinyCarModule
    {
        public override int ExecutionOrder
        {
            get { return 50; }
        }

        public override void CarFixedUpdate(float deltaTime)
        {
            TinyCarSurfaceParameters surface = State.SurfaceParameters ?? new TinyCarSurfaceParameters();

            if (State.HitSideStayStatic || State.HitSideStayDynamic)
            {
                float velocityDecrement = Mathf.Clamp01(
                    deltaTime *
                    Car.SideFriction *
                    State.ScaleAdjustment *
                    State.HitSideForce *
                    surface.sideFrictionMultiplier);

                State.WorkingVelocity *= 1f - velocityDecrement;
            }
        }

        public override void CarTriggerStay(Collider other)
        {
            CheckSurfaceParameters(other.gameObject);
        }

        public override void CarCollisionStay(Collision collision)
        {
            if (collision.contacts.Length == 0) return;

            CheckSurfaceParameters(collision.gameObject);

            ContactPoint hit = collision.contacts[0];
            float verticalDot = TinyCarPhysicsMath.GetVerticalDot(collision);
            float collisionXZForce = TinyCarPhysicsMath.GetCollisionForceOnXZ(collision);

            if (verticalDot < 0.1f && collisionXZForce > 0.1f)
            {
                bool isStaticCollider = collision.rigidbody == null || collision.rigidbody.isKinematic;
                if (isStaticCollider)
                {
                    State.HitSideStayStatic = true;
                }
                else
                {
                    State.HitSideStayDynamic = true;
                }

                if (State.HitSideForce < collisionXZForce)
                {
                    State.HitSideForce = collisionXZForce;
                    State.HitSidePosition = hit.point;
                }
            }
        }

        public override void CarCollisionEnter(Collision collision)
        {
            if (collision.contacts.Length == 0) return;

            CheckSurfaceParameters(collision.gameObject);

            ContactPoint hit = collision.contacts[0];
            float verticalDot = TinyCarPhysicsMath.GetVerticalDot(collision);
            float collisionYForce = TinyCarPhysicsMath.GetCollisionForceOnY(collision);

            if (verticalDot > 0.1f && collisionYForce > 0.1f)
            {
                State.HitGround = true;
                State.HitGroundMass = collision.rigidbody == null ? 0f : collision.rigidbody.mass;
                if (State.HitGroundForce < collisionYForce)
                {
                    State.HitGroundForce = collisionYForce;
                }
            }

            float collisionXZForce = TinyCarPhysicsMath.GetCollisionForceOnXZ(collision);
            if (verticalDot < 0.1f && collisionXZForce > 0.1f)
            {
                State.HitSideMass = collision.rigidbody == null ? 0f : collision.rigidbody.mass;
                State.HitSide = true;

                bool isStaticCollider = collision.rigidbody == null || collision.rigidbody.isKinematic;
                if (isStaticCollider)
                {
                    State.HitSideStayStatic = true;
                }
                else
                {
                    State.HitSideStayDynamic = true;
                }

                if (State.HitSideForce < collisionXZForce)
                {
                    State.HitSideForce = collisionXZForce;
                    State.HitSidePosition = hit.point;
                }
            }
        }

        private void CheckSurfaceParameters(GameObject obj)
        {
            TinyCarSurface[] surfaces = obj.GetComponentsInParent<TinyCarSurface>();
            if (surfaces.Length == 0) return;

            State.TriggerSurfaceParameters = new TinyCarSurfaceParameters(0f);
            foreach (TinyCarSurface surface in surfaces)
            {
                State.TriggerSurfaceParameters += surface.GetParameters() / surfaces.Length;
            }
        }
    }
}
