using UnityEngine;

namespace Mock.CharacterControl
{
    public class RootMotionActivator : StateMachineBehaviour
    {
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);

            IRootMotionReciever reciever = GetReciever(animator);
            reciever?.ActiveRootMotion();
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateExit(animator, stateInfo, layerIndex);

            IRootMotionReciever reciever = GetReciever(animator);
            reciever?.InactiveRootMotion();
        }

        private IRootMotionReciever GetReciever(Animator animator)
        {
            if (_reciever == null) 
            {
                _reciever = animator.GetComponent<IRootMotionReciever>(); 
            }

            return _reciever;
        }

        private IRootMotionReciever _reciever;
    }
}
