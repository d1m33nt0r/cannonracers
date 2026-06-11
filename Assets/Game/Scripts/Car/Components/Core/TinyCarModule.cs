using UnityEngine;

namespace DavidJalbert
{
    public abstract class TinyCarModule : MonoBehaviour
    {
        public TinyCarController Car { get; private set; }

        public virtual int ExecutionOrder
        {
            get { return 0; }
        }

        protected TinyCarRuntimeState State
        {
            get { return Car != null ? Car.State : null; }
        }

        protected virtual void OnEnable()
        {
            TinyCarController car = GetComponentInParent<TinyCarController>();
            if (car != null)
            {
                car.RegisterModule(this);
            }
        }

        protected virtual void OnDisable()
        {
            if (Car != null)
            {
                Car.UnregisterModule(this);
            }
        }

        internal void Attach(TinyCarController car)
        {
            if (Car == car) return;

            Car = car;
            OnRegistered();
        }

        internal void Detach(TinyCarController car)
        {
            if (Car != car) return;

            OnUnregistered();
            Car = null;
        }

        protected virtual void OnRegistered() { }
        protected virtual void OnUnregistered() { }

        public virtual void CarUpdate(float deltaTime) { }
        public virtual void CarFixedUpdate(float deltaTime) { }
        public virtual void CarTriggerStay(Collider other) { }
        public virtual void CarCollisionStay(Collision collision) { }
        public virtual void CarCollisionEnter(Collision collision) { }
    }
}
