using UnityEngine;

namespace DavidJalbert
{
    public class TinyCarRuntimeState
    {
        public Rigidbody Body;
        public SphereCollider SphereCollider;
        public PhysicsMaterial PhysicsMaterial;

        public bool OnGround;
        public Vector3 CrossForward = Vector3.zero;
        public Vector3 CrossUp = Vector3.zero;
        public Vector3 CrossRight = Vector3.zero;
        public Vector3 GravityDirection = Vector3.zero;
        public Quaternion GroundRotation;

        public float GroundVelocity;
        public float ForwardVelocity;
        public float RightVelocity;
        public float SlopeDelta;

        public float InputSteering;
        public float InputMotor;
        public float BoostMultiplier = 1f;

        public float AverageScale = 1f;
        public float RealColliderRadius;
        public float ScaleAdjustment = 1f;
        public float CubicScale = 1f;
        public float InverseScaleAdjustment = 1f;

        public Vector3 WorkingVelocity;
        public Vector3 PreviousPosition;

        public TinyCarSurfaceParameters SurfaceParameters;
        public TinyCarSurfaceParameters TriggerSurfaceParameters;

        public bool HitSide;
        public bool HitSideStayStatic;
        public bool HitSideStayDynamic;
        public float HitSideForce;
        public float HitSideMass;
        public Vector3 HitSidePosition = Vector3.zero;

        public bool HitGround;
        public float HitGroundForce;
        public float HitGroundMass;

        public void ResetFrameCollisionData()
        {
            HitSide = false;
            HitSideStayStatic = false;
            HitSideStayDynamic = false;
            HitGround = false;
            HitSideForce = 0f;
            HitGroundForce = 0f;
            HitSidePosition = Vector3.zero;
            TriggerSurfaceParameters = null;
        }
    }

    public static class TinyCarPhysicsMath
    {
        public static int GetZeroSign(float value)
        {
            if (value == 0f) return 0;
            return (int)Mathf.Sign(value);
        }

        public static Vector3 GetTriangleNormal(Vector3 pa, Vector3 pb, Vector3 pc)
        {
            Vector3 u = pb - pa;
            Vector3 v = pc - pa;

            return new Vector3(
                u.y * v.z - u.z * v.y,
                u.z * v.x - u.x * v.z,
                u.x * v.y - u.y * v.x).normalized;
        }

        public static float GetVerticalDot(Collision collision)
        {
            return Vector3.Dot(collision.contacts[0].normal, Vector3.up);
        }

        public static float GetCollisionForceOnXZ(Collision collision)
        {
            Vector3 xzVelocity = new Vector3(collision.relativeVelocity.x, 0f, collision.relativeVelocity.z);
            return collision.relativeVelocity.sqrMagnitude > 0.1f
                ? Vector3.Dot(collision.contacts[0].normal, xzVelocity)
                : 0f;
        }

        public static float GetCollisionForceOnY(Collision collision)
        {
            Vector3 yVelocity = Vector3.up * collision.relativeVelocity.y;
            return collision.relativeVelocity.sqrMagnitude > 0.1f
                ? Vector3.Dot(collision.contacts[0].normal, yVelocity)
                : 0f;
        }
    }
}
