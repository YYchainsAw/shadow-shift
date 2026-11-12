namespace ShadowShift
{
    public abstract class State
    {
        protected State(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public virtual void Enter() { }
        public virtual void Update() { }
        public virtual void Exit() { }
    }
}
