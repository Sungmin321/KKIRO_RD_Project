using UnityEngine;
using UnityEngine.InputSystem;

namespace TDGame.Utils
{
    public class GameSpeedController : MonoBehaviour
    {
        private float[] speeds = new float[] { 1f, 2f, 4f };
        private int currentSpeedIndex = 0;

        private void Update()
        {
            if (Keyboard.current == null) return;

            // 숫자 키 1, 2, 3 입력 감지
            if (Keyboard.current.digit1Key.wasPressedThisFrame) SetSpeed(1f);
            if (Keyboard.current.digit2Key.wasPressedThisFrame) SetSpeed(2f);
            if (Keyboard.current.digit3Key.wasPressedThisFrame) SetSpeed(4f);

            // Tab 키로 1배 -> 2배 -> 4배 순환
            if (Keyboard.current.tabKey.wasPressedThisFrame)
            {
                currentSpeedIndex = (currentSpeedIndex + 1) % speeds.Length;
                SetSpeed(speeds[currentSpeedIndex]);
            }
        }

        public void SetSpeed(float speed)
        {
            Time.timeScale = speed;
            Debug.Log($"<color=orange>[게임 속도]</color> {speed}x 배속 적용");
        }
    }
}