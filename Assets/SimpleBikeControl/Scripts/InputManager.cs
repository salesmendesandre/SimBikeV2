using UnityEngine;

namespace KikiNgao.SimpleBikeControl
{
    public class InputManager : MonoBehaviour
    {
        public float horizontal, vertical;
        //public KeyCode enterExitKey = KeyCode.F;
        //public KeyCode speedUpKey = KeyCode.LeftShift;

        [SerializeField] private float brakeDuration = 1;
        [HideInInspector]
        public bool enterExitVehicle;
        [HideInInspector]
        public bool speedUp;

        private float timeElapsed = 0;
        private void Update()
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            bool brake = Input.GetButton("Brake");

            this.horizontal = horizontal;

            if (brake)
            {
                this.vertical = Mathf.Lerp(vertical, 0, timeElapsed / brakeDuration);
                timeElapsed += Time.deltaTime;
            }
            else
            {
                this.vertical = vertical;
                timeElapsed = 0;
            }

            //enterExitVehicle = Input.GetKeyDown(enterExitKey);

            //if (Input.GetKeyDown(speedUpKey)) speedUp = true;
            //if (Input.GetKeyUp(speedUpKey)) speedUp = false;
        }

    }
}
