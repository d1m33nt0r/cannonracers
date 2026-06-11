using UnityEngine;

namespace DavidJalbert
{
    [CreateAssetMenu(menuName = "Tiny Car/Body Settings", fileName = "TinyCarBodySettings")]
    public class TinyCarBodySettings : ScriptableObject
    {
        [Tooltip("Radius of the sphere collider.")]
        public float colliderRadius = 2f;
        [Tooltip("Mass of the rigid body.")]
        public float bodyMass = 1f;
        [Tooltip("Scales the mass, velocity, and gravity according to the GameObject's scale.")]
        public bool adjustToScale = false;
    }

    [CreateAssetMenu(menuName = "Tiny Car/Grounding Settings", fileName = "TinyCarGroundingSettings")]
    public class TinyCarGroundingSettings : ScriptableObject
    {
        [Tooltip("Maximum angle of climbable slopes.")]
        public float maxSlopeAngle = 50f;
        [Tooltip("The layers that will be used for ground checks.")]
        public LayerMask collidableLayers = ~0;
    }

    [CreateAssetMenu(menuName = "Tiny Car/Gravity Settings", fileName = "TinyCarGravitySettings")]
    public class TinyCarGravitySettings : ScriptableObject
    {
        [Tooltip("Always Down = Gravity always points straight down.\nTowards Ground = Gravity points to the surface when the car is grounded, otherwise it points straight down.")]
        public TinyCarController.GRAVITY_MODE gravityMode = TinyCarController.GRAVITY_MODE.TowardsGround;
        [Tooltip("Gravity speed.")]
        public float gravityVelocity = 80f;
        [Tooltip("Maximum gravity.")]
        public float maxGravity = 50f;
    }

    [CreateAssetMenu(menuName = "Tiny Car/Engine Settings", fileName = "TinyCarEngineSettings")]
    public class TinyCarEngineSettings : ScriptableObject
    {
        [Tooltip("How much acceleration to apply relative to the speed of the car.")]
        public AnimationCurve accelerationCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 0));
        [Tooltip("Maximum acceleration when going forward.")]
        public float maxAccelerationForward = 100f;
        [Tooltip("Maximum speed when going forward.")]
        public float maxSpeedForward = 40f;
        [Tooltip("Maximum acceleration when going in reverse.")]
        public float maxAccelerationReverse = 50f;
        [Tooltip("Maximum speed when going in reverse.")]
        public float maxSpeedReverse = 20f;
        [Tooltip("How fast the car will brake when the motor goes in the opposite direction.")]
        public float brakeStrength = 200f;
        [Tooltip("How much friction to apply when on a slope. The higher this value, the slower you'll climb up slopes and the faster you'll go down. Setting this to zero adds no additional friction.")]
        public float slopeFriction = 1f;
    }

    [CreateAssetMenu(menuName = "Tiny Car/Steering Settings", fileName = "TinyCarSteeringSettings")]
    public class TinyCarSteeringSettings : ScriptableObject
    {
        [Tooltip("Sharpness of the steering.")]
        public float maxSteering = 200f;
        [Tooltip("Multiplier applied to steering when in the air. Setting this to zero makes the car unsteerable in the air.")]
        public float steeringMultiplierInAir = 0.25f;
        [Tooltip("How much steering to apply relative to the speed of the car.")]
        public AnimationCurve steeringBySpeed = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.25f, 1), new Keyframe(1, 1));
        [Tooltip("How fast the car stops when releasing the gas.")]
        public float forwardFriction = 40f;
        [Tooltip("How much grip the car should have on the road when turning.")]
        public float lateralFriction = 80f;
    }

    [CreateAssetMenu(menuName = "Tiny Car/Collision Settings", fileName = "TinyCarCollisionSettings")]
    public class TinyCarCollisionSettings : ScriptableObject
    {
        [Tooltip("Amount of friction applied when colliding with a wall.")]
        public float sideFriction = 1f;
    }
}
