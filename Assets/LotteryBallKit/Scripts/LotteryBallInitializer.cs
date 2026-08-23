using UnityEngine;

public class LotteryBallInitializer : MonoBehaviour
{
	[SerializeField] private NumberBall ball;
	[SerializeField] private Texture2D texture;
	private void Start()
	{
		ball.Apply();
		ball.SetCharacterTexture(texture);
	}
}
