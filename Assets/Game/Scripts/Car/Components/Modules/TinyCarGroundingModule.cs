using UnityEngine;

namespace DavidJalbert
{
    [DisallowMultipleComponent]
    public class TinyCarGroundingModule : TinyCarModule
    {
        private const float GroundCheckDistanceDelta = 0.1f;
        private const float GroundCheckSkinWidthDelta = 0.05f;

        public override int ExecutionOrder
        {
            get { return 10; }
        }

        protected override void OnRegistered()
        {
            State.GroundRotation = transform.rotation;
            State.CrossForward = transform.forward;
            State.CrossRight = transform.right;
            State.CrossUp = transform.up;
        }

        public override void CarFixedUpdate(float deltaTime)
        {
            if (State.Body == null || State.SphereCollider == null) return;

            float groundCheckSkinWidth = State.RealColliderRadius * GroundCheckSkinWidthDelta;
            float groundCheckDistance = State.RealColliderRadius * GroundCheckDistanceDelta;

            State.SurfaceParameters = null;
            State.OnGround = false;
            State.CrossUp = transform.up;

            RaycastHit hitSphere;
            if (Physics.SphereCast(
                    State.SphereCollider.bounds.center,
                    State.RealColliderRadius - groundCheckSkinWidth,
                    Vector3.down,
                    out hitSphere,
                    groundCheckDistance + groundCheckSkinWidth,
                    Car.CollidableLayers,
                    QueryTriggerInteraction.Ignore))
            {
                State.CrossUp = hitSphere.normal;
                if (Vector3.Angle(State.CrossUp, Vector3.up) <= Car.MaxSlopeAngle)
                {
                    State.OnGround = true;

                    TinyCarSurface surface = hitSphere.collider.GetComponentInParent<TinyCarSurface>();
                    if (surface != null)
                    {
                        State.SurfaceParameters = surface.GetParameters();
                    }
                }
            }

            if (State.SurfaceParameters == null)
            {
                State.SurfaceParameters = State.TriggerSurfaceParameters ?? new TinyCarSurfaceParameters();
            }

            Vector3[] groundCheckSource =
            {
                State.SphereCollider.bounds.center + Vector3.forward * State.RealColliderRadius + Vector3.left * State.RealColliderRadius,
                State.SphereCollider.bounds.center + Vector3.forward * State.RealColliderRadius + Vector3.right * State.RealColliderRadius,
                State.SphereCollider.bounds.center + Vector3.back * State.RealColliderRadius + Vector3.left * State.RealColliderRadius,
                State.SphereCollider.bounds.center + Vector3.back * State.RealColliderRadius + Vector3.right * State.RealColliderRadius
            };

            Vector3[] groundCheckHits = new Vector3[groundCheckSource.Length];
            bool[] groundCheckFound = new bool[groundCheckSource.Length];
            RaycastHit rayHit;
            for (int i = 0; i < groundCheckSource.Length; i++)
            {
                groundCheckFound[i] = Physics.Raycast(
                    groundCheckSource[i],
                    Vector3.down,
                    out rayHit,
                    State.RealColliderRadius * 2f,
                    Car.CollidableLayers,
                    QueryTriggerInteraction.Ignore);

                if (groundCheckFound[i])
                {
                    groundCheckHits[i] = rayHit.point;
                }
            }

            Vector3 triFRNormal = Vector3.zero;
            if (groundCheckFound[0] && groundCheckFound[1] && groundCheckFound[2])
            {
                triFRNormal = TinyCarPhysicsMath.GetTriangleNormal(groundCheckHits[0], groundCheckHits[1], groundCheckHits[2]);
            }

            Vector3 triBLNormal = Vector3.zero;
            if (groundCheckFound[1] && groundCheckFound[3] && groundCheckFound[2])
            {
                triBLNormal = TinyCarPhysicsMath.GetTriangleNormal(groundCheckHits[1], groundCheckHits[3], groundCheckHits[2]);
            }

            State.CrossUp = (State.CrossUp + triFRNormal + triBLNormal).normalized;

            State.WorkingVelocity = State.Body.linearVelocity;
            State.GroundVelocity = (State.WorkingVelocity - Vector3.up * State.WorkingVelocity.y).magnitude;
            State.CrossForward = Vector3.Cross(-State.CrossUp, transform.right);
            State.CrossRight = Vector3.Cross(-State.CrossUp, transform.forward);

            State.ForwardVelocity = Vector3.Dot(State.WorkingVelocity, State.CrossForward);
            State.RightVelocity = Vector3.Dot(State.WorkingVelocity, State.CrossRight);

            State.GroundRotation = Quaternion.LookRotation(State.CrossForward, State.CrossUp);
            float groundXAngle = State.GroundRotation.eulerAngles.x;
            State.SlopeDelta = Mathf.Clamp(groundXAngle > 180f ? groundXAngle - 360f : groundXAngle, -90f, 90f) / 90f;
        }
    }
}
