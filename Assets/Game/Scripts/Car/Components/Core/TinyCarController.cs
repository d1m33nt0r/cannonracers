using System.Collections.Generic;
using UnityEngine;

namespace DavidJalbert
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(SphereCollider))]
    [RequireComponent(typeof(TinyCarBodyModule))]
    [RequireComponent(typeof(TinyCarGroundingModule))]
    [RequireComponent(typeof(TinyCarSteeringModule))]
    [RequireComponent(typeof(TinyCarEngineModule))]
    [RequireComponent(typeof(TinyCarGravityModule))]
    [RequireComponent(typeof(TinyCarCollisionModule))]
    [ExecuteInEditMode]
    public class TinyCarController : MonoBehaviour
    {
        public enum GRAVITY_MODE
        {
            AlwaysDown, TowardsGround
        }

        [Header("Settings Assets")]
        public TinyCarBodySettings bodySettings;
        public TinyCarGroundingSettings groundingSettings;
        public TinyCarGravitySettings gravitySettings;
        public TinyCarEngineSettings engineSettings;
        public TinyCarSteeringSettings steeringSettings;
        public TinyCarCollisionSettings collisionSettings;

        [Header("Physics")]
        [Tooltip("Radius of the sphere collider. Used when Body Settings is not assigned.")]
        public float colliderRadius = 2f;
        [Tooltip("Mass of the rigid body. Used when Body Settings is not assigned.")]
        public float bodyMass = 1f;
        [Tooltip("Always Down = Gravity always points straight down.\nTowards Ground = Gravity points to the surface when the car is grounded, otherwise it points straight down. Used when Gravity Settings is not assigned.")]
        public GRAVITY_MODE gravityMode = GRAVITY_MODE.TowardsGround;
        [Tooltip("Gravity speed. Used when Gravity Settings is not assigned.")]
        public float gravityVelocity = 80f;
        [Tooltip("Maximum gravity. Used when Gravity Settings is not assigned.")]
        public float maxGravity = 50f;
        [Tooltip("Maximum angle of climbable slopes. Used when Grounding Settings is not assigned.")]
        public float maxSlopeAngle = 50f;
        [Tooltip("Amount of friction applied when colliding with a wall. Used when Collision Settings is not assigned.")]
        public float sideFriction = 1f;
        [Tooltip("The layers that will be used for ground checks. Used when Grounding Settings is not assigned.")]
        public LayerMask collidableLayers = ~0;
        [Tooltip("Scales the mass, velocity, and gravity according to the GameObject's scale. Used when Body Settings is not assigned.")]
        public bool adjustToScale = false;

        [Header("Engine")]
        [Tooltip("How much acceleration to apply relative to the speed of the car. Used when Engine Settings is not assigned.")]
        public AnimationCurve accelerationCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 0));
        [Tooltip("Maximum acceleration when going forward. Used when Engine Settings is not assigned.")]
        public float maxAccelerationForward = 100f;
        [Tooltip("Maximum speed when going forward. Used when Engine Settings is not assigned.")]
        public float maxSpeedForward = 40f;
        [Tooltip("Maximum acceleration when going in reverse. Used when Engine Settings is not assigned.")]
        public float maxAccelerationReverse = 50f;
        [Tooltip("Maximum speed when going in reverse. Used when Engine Settings is not assigned.")]
        public float maxSpeedReverse = 20f;
        [Tooltip("How fast the car will brake when the motor goes in the opposite direction. Used when Engine Settings is not assigned.")]
        public float brakeStrength = 200f;
        [Tooltip("How much friction to apply when on a slope. The higher this value, the slower you'll climb up slopes and the faster you'll go down. Setting this to zero adds no additional friction. Used when Engine Settings is not assigned.")]
        public float slopeFriction = 1f;

        [Header("Steering")]
        [Tooltip("Sharpness of the steering. Used when Steering Settings is not assigned.")]
        public float maxSteering = 200f;
        [Tooltip("Multiplier applied to steering when in the air. Setting this to zero makes the car unsteerable in the air. Used when Steering Settings is not assigned.")]
        public float steeringMultiplierInAir = 0.25f;
        [Tooltip("How much steering to apply relative to the speed of the car. Used when Steering Settings is not assigned.")]
        public AnimationCurve steeringBySpeed = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.25f, 1), new Keyframe(1, 1));
        [Tooltip("How fast the car stops when releasing the gas. Used when Steering Settings is not assigned.")]
        public float forwardFriction = 40f;
        [Tooltip("How much grip the car should have on the road when turning. Used when Steering Settings is not assigned.")]
        public float lateralFriction = 80f;

        public Rigidbody body;

        [SerializeField] private float speedMultiplier;
        [SerializeField] private float distanceMultiplier;

        private readonly TinyCarRuntimeState state = new TinyCarRuntimeState();
        private readonly List<TinyCarModule> modules = new List<TinyCarModule>();
        private Vector3 prevPos;

        public TinyCarRuntimeState State
        {
            get { return state; }
        }

        internal float ColliderRadius
        {
            get { return bodySettings != null ? bodySettings.colliderRadius : colliderRadius; }
        }

        internal float BodyMass
        {
            get { return bodySettings != null ? bodySettings.bodyMass : bodyMass; }
        }

        internal bool AdjustToScale
        {
            get { return bodySettings != null ? bodySettings.adjustToScale : adjustToScale; }
        }

        internal float MaxSlopeAngle
        {
            get { return groundingSettings != null ? groundingSettings.maxSlopeAngle : maxSlopeAngle; }
        }

        internal LayerMask CollidableLayers
        {
            get { return groundingSettings != null ? groundingSettings.collidableLayers : collidableLayers; }
        }

        internal GRAVITY_MODE GravityMode
        {
            get { return gravitySettings != null ? gravitySettings.gravityMode : gravityMode; }
        }

        internal float GravityVelocity
        {
            get { return gravitySettings != null ? gravitySettings.gravityVelocity : gravityVelocity; }
        }

        internal float MaxGravity
        {
            get { return gravitySettings != null ? gravitySettings.maxGravity : maxGravity; }
        }

        internal AnimationCurve AccelerationCurve
        {
            get { return engineSettings != null && engineSettings.accelerationCurve != null ? engineSettings.accelerationCurve : accelerationCurve; }
        }

        internal float MaxAccelerationForward
        {
            get { return engineSettings != null ? engineSettings.maxAccelerationForward : maxAccelerationForward; }
        }

        internal float MaxSpeedForward
        {
            get { return engineSettings != null ? engineSettings.maxSpeedForward : maxSpeedForward; }
        }

        internal float MaxAccelerationReverse
        {
            get { return engineSettings != null ? engineSettings.maxAccelerationReverse : maxAccelerationReverse; }
        }

        internal float MaxSpeedReverse
        {
            get { return engineSettings != null ? engineSettings.maxSpeedReverse : maxSpeedReverse; }
        }

        internal float BrakeStrength
        {
            get { return engineSettings != null ? engineSettings.brakeStrength : brakeStrength; }
        }

        internal float SlopeFriction
        {
            get { return engineSettings != null ? engineSettings.slopeFriction : slopeFriction; }
        }

        internal float MaxSteering
        {
            get { return steeringSettings != null ? steeringSettings.maxSteering : maxSteering; }
        }

        internal float SteeringMultiplierInAir
        {
            get { return steeringSettings != null ? steeringSettings.steeringMultiplierInAir : steeringMultiplierInAir; }
        }

        internal AnimationCurve SteeringBySpeed
        {
            get { return steeringSettings != null && steeringSettings.steeringBySpeed != null ? steeringSettings.steeringBySpeed : steeringBySpeed; }
        }

        internal float ForwardFriction
        {
            get { return steeringSettings != null ? steeringSettings.forwardFriction : forwardFriction; }
        }

        internal float LateralFriction
        {
            get { return steeringSettings != null ? steeringSettings.lateralFriction : lateralFriction; }
        }

        internal float SideFriction
        {
            get { return collisionSettings != null ? collisionSettings.sideFriction : sideFriction; }
        }

        protected virtual void Awake()
        {
            EnsureDefaultModules();
            RegisterExistingModules();
        }

        protected virtual void OnEnable()
        {
            EnsureDefaultModules();
            RegisterExistingModules();
        }

        protected virtual void Start()
        {
            EnsureDefaultModules();
            RegisterExistingModules();

            if (!Application.isPlaying) return;

            state.GroundRotation = transform.rotation;
            state.CrossForward = transform.forward;
            state.CrossRight = transform.right;
            state.CrossUp = transform.up;
        }

        protected virtual void Update()
        {
            EnsureDefaultModules();
            RegisterExistingModules();

            for (int i = 0; i < modules.Count; i++)
            {
                if (modules[i] != null && modules[i].isActiveAndEnabled)
                {
                    modules[i].CarUpdate(Time.deltaTime);
                }
            }

            prevPos = transform.position;
        }

        protected virtual void FixedUpdate()
        {
            EnsureDefaultModules();
            RegisterExistingModules();

            float deltaTime = Time.fixedDeltaTime;
            for (int i = 0; i < modules.Count; i++)
            {
                if (modules[i] != null && modules[i].isActiveAndEnabled)
                {
                    modules[i].CarFixedUpdate(deltaTime);
                }
            }

            if (state.Body != null)
            {
                state.Body.linearVelocity = state.WorkingVelocity;
            }

            state.ResetFrameCollisionData();
        }

        protected virtual void OnDisable()
        {
            DetachAllModules();
        }

        protected virtual void OnDestroy()
        {
            if (GetComponent<Rigidbody>() != null) GetComponent<Rigidbody>().hideFlags = HideFlags.None;
            if (GetComponent<SphereCollider>() != null) GetComponent<SphereCollider>().hideFlags = HideFlags.None;
            DetachAllModules();
        }

        public void RegisterModule(TinyCarModule module)
        {
            if (module == null) return;
            if (module.GetComponentInParent<TinyCarController>() != this) return;

            if (!modules.Contains(module))
            {
                modules.Add(module);
                module.Attach(this);
                SortModules();
            }
            else
            {
                module.Attach(this);
            }
        }

        public void UnregisterModule(TinyCarModule module)
        {
            if (module == null) return;

            if (modules.Remove(module))
            {
                module.Detach(this);
            }
        }

        private void EnsureDefaultModules()
        {
            EnsureModule<TinyCarBodyModule>();
            EnsureModule<TinyCarGroundingModule>();
            EnsureModule<TinyCarSteeringModule>();
            EnsureModule<TinyCarEngineModule>();
            EnsureModule<TinyCarGravityModule>();
            EnsureModule<TinyCarCollisionModule>();
        }

        private void EnsureModule<T>() where T : TinyCarModule
        {
            if (GetComponent<T>() == null)
            {
                gameObject.AddComponent<T>();
            }
        }

        private void RegisterExistingModules()
        {
            TinyCarModule[] foundModules = GetComponentsInChildren<TinyCarModule>(true);
            for (int i = 0; i < foundModules.Length; i++)
            {
                TinyCarModule module = foundModules[i];
                if (module != null && module.isActiveAndEnabled && module.GetComponentInParent<TinyCarController>() == this)
                {
                    RegisterModule(module);
                }
            }

            for (int i = modules.Count - 1; i >= 0; i--)
            {
                TinyCarModule module = modules[i];
                if (module == null || !module.isActiveAndEnabled || module.GetComponentInParent<TinyCarController>() != this)
                {
                    if (module != null)
                    {
                        module.Detach(this);
                    }
                    modules.RemoveAt(i);
                }
            }

            SortModules();
        }

        private void SortModules()
        {
            modules.Sort((a, b) => a.ExecutionOrder.CompareTo(b.ExecutionOrder));
        }

        private void DetachAllModules()
        {
            for (int i = modules.Count - 1; i >= 0; i--)
            {
                if (modules[i] != null)
                {
                    modules[i].Detach(this);
                }
            }

            modules.Clear();
        }

        private int getZeroSign(float value)
        {
            return TinyCarPhysicsMath.GetZeroSign(value);
        }

        public void clearVelocity()
        {
            getBody().linearVelocity = Vector3.zero;
            state.WorkingVelocity = Vector3.zero;
        }

        public float getSlopeDelta()
        {
            return state.SlopeDelta;
        }

        internal float GetAccelerationForce(float accelerationMultiplier = 1f, float speedMultiplier = 1f)
        {
            if (isBraking() || speedMultiplier == 0f)
            {
                return BrakeStrength;
            }

            float maxSpeed = getMaxSpeed() * speedMultiplier;
            if (maxSpeed == 0f)
            {
                return 0f;
            }

            float accelerationCurveDelta = Mathf.Abs(state.ForwardVelocity) / maxSpeed;
            if (accelerationCurveDelta > 1f) return -getMaxAcceleration() * accelerationMultiplier;
            if (accelerationCurveDelta < 0f) return getMaxAcceleration() * accelerationMultiplier;
            return AccelerationCurve.Evaluate(accelerationCurveDelta) * getMaxAcceleration() * accelerationMultiplier;
        }

        private float getAcceleration(float accelerationMultiplier = 1f, float speedMultiplier = 1f)
        {
            return GetAccelerationForce(accelerationMultiplier, speedMultiplier);
        }

        public int getVelocityDirection()
        {
            return getZeroSign(state.ForwardVelocity);
        }

        public void setBoostMultiplier(float m)
        {
            state.BoostMultiplier = m;
        }

        public float getBoostMultiplier()
        {
            return state.BoostMultiplier;
        }

        public bool isBraking()
        {
            return getMotor() != 0f && getVelocityDirection() == -getZeroSign(getMotor());
        }

        public float getMaxSpeed()
        {
            return getVelocityDirection() >= 0 ? MaxSpeedForward * state.ScaleAdjustment : MaxSpeedReverse * state.ScaleAdjustment;
        }

        public float getMaxAcceleration()
        {
            return getVelocityDirection() >= 0 ? MaxAccelerationForward * state.ScaleAdjustment : MaxAccelerationReverse * state.ScaleAdjustment;
        }

        public void setSteering(float value)
        {
            state.InputSteering = value;
        }

        public void setMotor(float value)
        {
            state.InputMotor = value;
        }

        public float getSteering()
        {
            return state.InputSteering;
        }

        public float getMotor()
        {
            return state.InputMotor;
        }

        public int getMotorDirection()
        {
            return getZeroSign(getMotor());
        }

        public TinyCarSurfaceParameters getSurfaceParameters()
        {
            return state.SurfaceParameters;
        }

        public Rigidbody getBody()
        {
            if (state.Body == null)
            {
                state.Body = GetComponent<Rigidbody>();
                body = state.Body;
            }

            return state.Body;
        }

        public bool hasHitGround(float minDownwardVelocity = 0f)
        {
            return state.HitGround && state.HitGroundForce >= minDownwardVelocity;
        }

        public bool isHittingSide(bool onlyStatic = false)
        {
            return (onlyStatic && state.HitSideStayStatic) || (!onlyStatic && (state.HitSideStayStatic || state.HitSideStayDynamic));
        }

        public bool hasHitSide(float minForce = 0f)
        {
            return state.HitSide && state.HitSideForce >= minForce;
        }

        public Vector3 getSideHitPosition()
        {
            return state.HitSidePosition;
        }

        public float getSideHitForce()
        {
            return state.HitSideForce;
        }

        public float getSideHitMass()
        {
            return state.HitSideMass;
        }

        public bool isGrounded()
        {
            return state.OnGround;
        }

        public Vector3 getGravityDirection()
        {
            return state.GravityDirection;
        }

        public Vector3 getBodyPosition()
        {
            return getBody().transform.position;
        }

        public Vector3 getGroundPosition()
        {
            return getBodyPosition() + Vector3.down * state.RealColliderRadius;
        }

        public Quaternion getBodyRotation()
        {
            return getBody().transform.rotation;
        }

        public Quaternion getGroundRotation()
        {
            return state.GroundRotation;
        }

        public float getForwardVelocity()
        {
            return state.ForwardVelocity;
        }

        public float getLateralVelocity()
        {
            return state.RightVelocity;
        }

        public float getForwardVelocityDelta()
        {
            float maxSpeed = getMaxSpeed();
            return maxSpeed == 0f ? 0f : Mathf.Abs(getForwardVelocity()) / maxSpeed;
        }

        public float getGroundVelocity()
        {
            return state.GroundVelocity;
        }

        public float getGroundVelocityDelta()
        {
            float maxSpeed = getMaxSpeed();
            return maxSpeed == 0f ? 0f : getGroundVelocity() / maxSpeed;
        }

        public float getGroundHitForce()
        {
            return state.HitGroundForce;
        }

        public float getGroundHitMass()
        {
            return state.HitGroundMass;
        }

        protected virtual void OnTriggerStay(Collider collider)
        {
            for (int i = 0; i < modules.Count; i++)
            {
                if (modules[i] != null && modules[i].isActiveAndEnabled)
                {
                    modules[i].CarTriggerStay(collider);
                }
            }
        }

        protected virtual void OnCollisionStay(Collision collision)
        {
            for (int i = 0; i < modules.Count; i++)
            {
                if (modules[i] != null && modules[i].isActiveAndEnabled)
                {
                    modules[i].CarCollisionStay(collision);
                }
            }
        }

        protected virtual void OnCollisionEnter(Collision collision)
        {
            for (int i = 0; i < modules.Count; i++)
            {
                if (modules[i] != null && modules[i].isActiveAndEnabled)
                {
                    modules[i].CarCollisionEnter(collision);
                }
            }
        }
    }
}
